//
// The MIT License (MIT)
//
// Copyright (c) 2015 Alex Rønne Petersen
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Terminal;
using Mono.Unix;
using Mono.Unix.Native;
using Mono.Debugger.Client.Commands;

namespace Mono.Debugger.Client
{
    public static class CommandLine
    {
        internal static bool Stop { get; set; }

        internal static RootCommand Root { get; private set; }

        internal static AutoResetEvent ResumeEvent { get; private set; }

        internal static bool InferiorExecuting { get; set; }

        static readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();

        static readonly LineEditor _lineEditor;

        static bool _windowsConsoleHandlerSet;

        static Thread _signalThread;

        static CommandLine()
        {
            Root = new RootCommand();
            ResumeEvent = new AutoResetEvent(false);

            try
            {
                LibEdit.Initialize();
            }
            catch (DllNotFoundException)
            {
                // Fall back to `Mono.Terminal.LineEditor`.
                _lineEditor = new LineEditor(null);
            }
        }

        internal static void SetUnixSignalAction(Signum signal, SignalAction action)
        {
            // This seemingly pointless method adds some
            // indirection so that we don't load `Mono.Posix`
            // unless we absolutely need to.
            Stdlib.SetSignalAction(signal, action);
        }

        static void Process(string cmd, bool rc)
        {
            if (!rc && _lineEditor == null)
                LibEdit.AddHistory(cmd);

            var args = cmd.Trim();

            if (args.Length == 0)
                return;

            try
            {
                Root.Process(args);
            }
            catch (Exception ex)
            {
                Log.Error("Command threw an exception:");
                Log.Error(ex.ToString());
            }
        }

        static string GetPrompt()
        {
            return string.Format("{0}{1}{2} ",
                                 Color.DarkMagenta,
                                 Configuration.Current.InputPrompt,
                                 Color.Reset);
        }

        static string GetFilePath()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return Path.Combine(home, ".sdb.rc");
        }

        public static void RunCommands(IEnumerable<string> commands)
        {
            foreach (var cmd in commands)
                _queue.Enqueue(cmd);
        }

        public static void RunFile(string path, bool swallowAndLog)
        {
            var commands = new List<string>();

            try
            {
                string line;

                using (var reader = new StreamReader(path))
                    while ((line = reader.ReadLine()) != null)
                        commands.Add(line);
            }
            catch (Exception ex)
            {
                if (swallowAndLog)
                {
                    Log.Error("Could not read commands in file '{0}':", path);
                    Log.Error(ex.ToString());

                    return;
                }
                else
                    throw;
            }

            foreach (var cmd in commands)
                _queue.Enqueue(cmd);
        }

        static void ControlCHandler()
        {
            Log.Info(string.Empty);

            switch (Debugger.State)
            {
                case State.Running:
                    Debugger.Pause();

                    break;
                case State.Suspended:
                    Log.Error("Inferior is already suspended");
                    Log.InfoSameLine(GetPrompt());

                    break;
                case State.Exited:
                    // If `InferiorExecuting` is set while the state is
                    // `Exited`, it means that we were listening or
                    // connecting. So cancel.
                    if (InferiorExecuting)
                        Debugger.Kill();
                    else
                    {
                        Log.Error("No inferior process");
                        Log.InfoSameLine(GetPrompt());
                    }

                    break;
            }
        }

        static void ConsoleControlCHandler(object sender, ConsoleCancelEventArgs e)
        {
            // This method is only fired on Windows.
            ControlCHandler();

            e.Cancel = true;
        }

        internal static void SetControlCHandler()
        {
            if (Utilities.IsWindows)
            {
                if (!_windowsConsoleHandlerSet)
                    Console.CancelKeyPress += ConsoleControlCHandler;

                _windowsConsoleHandlerSet = true;
            }
            else if (_signalThread == null)
            {
                SetUnixSignalAction(Signum.SIGINT, SignalAction.Default);

                _signalThread = new Thread(() =>
                {
                    try
                    {
                        using (var sig = new UnixSignal(Signum.SIGINT))
                        {
                            while (true)
                            {
                                sig.WaitOne();

                                ControlCHandler();
                            }
                        }
                    }
                    catch (ThreadAbortException)
                    {
                    }
                });

                _signalThread.Start();
            }
        }

        internal static void UnsetControlCHandler()
        {
            if (Utilities.IsWindows)
            {
                Console.CancelKeyPress -= ConsoleControlCHandler;

                _windowsConsoleHandlerSet = false;
            }
            else if (_signalThread != null)
            {
                _signalThread.Abort();
                _signalThread.Join();

                _signalThread = null;

                SetUnixSignalAction(Signum.SIGINT, SignalAction.Ignore);
            }
        }

        internal static void Run(Version ver, bool batch, bool rc,
                                 IEnumerable<string> commands,
                                 IEnumerable<string> files)
        {
            if (!Configuration.Read())
                Configuration.Defaults();

            Configuration.Apply();

            var dbFile = Configuration.Current.DefaultDatabaseFile;

            if (Configuration.Current.LoadDatabaseAutomatically &&
                !string.IsNullOrWhiteSpace(dbFile))
            {
                FileInfo file;

                try
                {
                    file = new FileInfo(dbFile);
                }
                catch (Exception ex)
                {
                    Log.Error("Could not open database file '{0}':", dbFile);
                    Log.Error(ex.ToString());

                    return;
                }

                Debugger.Read(file);
            }

            Log.Notice("Welcome to the Mono soft debugger (sdb {0})", ver);
            Log.Notice("Type 'help' for a list of commands or 'quit' to exit");
            Log.Info(string.Empty);

            Root.AddCommands(Plugins.LoadDefault());

            var rcFile = GetFilePath();

            if (rc && File.Exists(rcFile))
                RunFile(rcFile, true);

            foreach (var file in files)
                if (File.Exists(file))
                    RunFile(file, true);

            RunCommands(commands);

            while (!Stop)
            {
                // If the command caused the debuggee to start
                // or resume execution, wait for it to suspend.
                if (InferiorExecuting)
                {
                    ResumeEvent.WaitOne();
                    InferiorExecuting = false;
                }

                string cmd;

                // We use a queue here so that batch commands are
                // also subject to the suspension check above. It
                // also makes things easier since everything gets
                // executed in one thread.
                if (_queue.TryDequeue(out cmd))
                    Process(cmd, true);
                else if (batch)
                    Stop = true;
                else
                {
                    cmd = _lineEditor != null ?
                          _lineEditor.Edit(GetPrompt(), string.Empty) :
                          LibEdit.ReadLine(GetPrompt());

                    // Did we get EOF?
                    if (cmd == null)
                    {
                        Log.Info(string.Empty);

                        if (Debugger.State != State.Exited)
                            Log.Error("An inferior process is active");
                        else
                            Stop = true;
                    }
                    else
                        Process(cmd, false);
                }
            }

            // Clean up just in case.
            UnsetControlCHandler();

            dbFile = Configuration.Current.DefaultDatabaseFile;

            if (Configuration.Current.SaveDatabaseAutomatically &&
                !string.IsNullOrWhiteSpace(dbFile))
            {
                FileInfo file;

                try
                {
                    file = new FileInfo(dbFile);
                }
                catch (Exception ex)
                {
                    Log.Error("Could not open database file '{0}':", dbFile);
                    Log.Error(ex.ToString());

                    return;
                }

                Debugger.Write(file);
            }

            // Let's not leave dead Mono processes behind...
            Debugger.Pause();
            Debugger.Kill();

            while (!Debugger.DebuggeeKilled)
                Thread.Sleep(10);

            Log.Notice("Bye");
        }
    }
}
