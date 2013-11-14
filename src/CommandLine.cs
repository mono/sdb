/*
 * SDB - Mono Soft Debugger Client
 * Copyright 2013 Alex Rønne Petersen
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Terminal;
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

        static readonly LibC.SignalHandler _interruptHandler;

        static readonly LineEditor _lineEditor;

        static bool _windowsConsoleHandlerSet;

        static volatile bool _inSignalHandler;

        static CommandLine()
        {
            Root = new RootCommand();
            ResumeEvent = new AutoResetEvent(false);

            try
            {
                LibReadLine.Initialize();
            }
            catch (DllNotFoundException)
            {
                // Fall back to `Mono.Terminal.LineEditor`.
                _lineEditor = new LineEditor(null);
            }

            if (!Utilities.IsWindows)
                _interruptHandler = new LibC.SignalHandler(ControlCHandler);
        }

        static void Process(string cmd, bool rc)
        {
            if (!rc && _lineEditor == null)
                LibReadLine.AddHistory(cmd);

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

        static void ControlCHandler(int signal)
        {
            // We need to do this dance because we can get a `SIGINT`
            // while we're inside this handler.
            if (_inSignalHandler)
                return;

            _inSignalHandler = true;

            try
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
            finally
            {
                _inSignalHandler = false;
            }
        }

        static void ConsoleControlCHandler(object sender, ConsoleCancelEventArgs e)
        {
            // FIXME: This is probably not the right way to go about
            // things. We need to actually test this on Windows.
            ControlCHandler(LibC.SignalInterrupt);

            e.Cancel = true;
        }

        internal static void SetControlCHandler()
        {
            if (!Utilities.IsWindows)
            {
                var fptr = Marshal.GetFunctionPointerForDelegate(_interruptHandler);

                LibC.SetSignal(LibC.SignalInterrupt, fptr);
            }
            else if (!_windowsConsoleHandlerSet)
            {
                Console.CancelKeyPress += ConsoleControlCHandler;

                _windowsConsoleHandlerSet = true;
            }
        }

        internal static void UnsetControlCHandler()
        {
            if (!Utilities.IsWindows)
                LibC.SetSignal(LibC.SignalInterrupt, LibC.IgnoreSignal);
        }

        internal static void Run(Version ver, bool batch, bool rc,
                                 IEnumerable<string> commands,
                                 IEnumerable<string> files)
        {
            if (!Configuration.Read())
                Configuration.Defaults();

            Configuration.Apply();

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
                          LibReadLine.ReadLine(GetPrompt());

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

            if (Utilities.IsWindows)
                Console.CancelKeyPress -= ConsoleControlCHandler;

            // Let's not leave dead Mono processes behind...
            Debugger.Pause();
            Debugger.Kill();

            while (!Debugger.DebuggeeKilled)
                Thread.Sleep(10);

            Log.Notice("Bye");
        }
    }
}
