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
using System.Threading;
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

        static CommandLine()
        {
            Root = new RootCommand();
            ResumeEvent = new AutoResetEvent(false);

            /*
            FIXME: Currently broken: https://bugzilla.novell.com/show_bug.cgi?id=699451
            Also breaks libreadline's history tracker...

            Console.CancelKeyPress += (sender, e) =>
            {
                // Paint the prompt again.
                Log.Info(string.Empty);
                Log.InfoSameLine(GetPrompt());

                Debugger.Pause();

                e.Cancel = true;
            };
            */
        }

        static void Process(string cmd, bool rc)
        {
            if (!rc)
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

        internal static void Run(Version ver, bool batch, IEnumerable<string> commands,
                                 IEnumerable<string> files)
        {
            Configuration.Read();
            Configuration.Apply();

            Log.Notice("Welcome to the Mono soft debugger (sdb {0})", ver);
            Log.Notice("Type 'help' for a list of commands or 'quit' to exit");
            Log.Info(string.Empty);

            Root.AddCommands(Plugins.LoadDefault());

            var rcFile = GetFilePath();

            if (File.Exists(rcFile))
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
                    cmd = LibReadLine.ReadLine(GetPrompt());

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

            // Let's not leave dead Mono processes behind...
            Debugger.Pause();
            Debugger.Kill();

            while (!Debugger.DebuggeeKilled)
                Thread.Sleep(10);

            Log.Notice("Bye");
        }
    }
}
