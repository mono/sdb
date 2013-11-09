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

using System.Collections.Generic;

namespace Mono.Debugger.Client.Commands
{
    sealed class ThreadCommand : MultiCommand
    {
        private sealed class ThreadBacktraceCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "backtrace", "bt" }; }
            }

            public override string Summary
            {
                get { return "Print backtraces for all threads."; }
            }

            public override string Syntax
            {
                get { return "thread backtrace|bt"; }
            }

            public override void Process(string args)
            {
                var p = Debugger.ActiveProcess;

                if (p == null)
                {
                    Log.Error("No active inferior process");
                    return;
                }

                var threads = p.GetThreads();

                for (var i = 0; i < threads.Length; i++)
                {
                    var t = threads[i];
                    var str = Utilities.StringizeThread(t, false);

                    if (t == Debugger.ActiveThread)
                        Log.Emphasis(str);
                    else
                        Log.Info(str);

                    var bt = t.Backtrace;

                    if (bt.FrameCount != 0)
                    {
                        for (var j = 0; j < bt.FrameCount; j++)
                        {
                            var f = bt.GetFrame(j);
                            var fstr = Utilities.StringizeFrame(f, true);

                            if (f == Debugger.ActiveFrame)
                                Log.Emphasis(fstr);
                            else
                                Log.Info(fstr);
                        }
                    }
                    else
                        Log.Info("Backtrace for this thread is unavailable");

                    if (i < threads.Length - 1)
                        Log.Info(string.Empty);
                }
            }
        }

        private sealed class ThreadGetCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "get" }; }
            }

            public override string Summary
            {
                get { return "Show the currently active thread."; }
            }

            public override string Syntax
            {
                get { return "thread get"; }
            }

            public override void Process(string args)
            {
                var t = Debugger.ActiveThread;

                if (t == null)
                {
                    Log.Error("No active thread");
                    return;
                }

                var str = Utilities.StringizeThread(t, true);

                if (t == Debugger.ActiveThread)
                    Log.Emphasis(str);
                else
                    Log.Info(str);
            }
        }

        private sealed class ThreadListCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "list" }; }
            }

            public override string Summary
            {
                get { return "List all program threads."; }
            }

            public override string Syntax
            {
                get { return "thread list"; }
            }

            public override void Process(string args)
            {
                var p = Debugger.ActiveProcess;

                if (p == null)
                {
                    Log.Error("No active inferior process");
                    return;
                }

                var threads = p.GetThreads();

                for (var i = 0; i < threads.Length; i++)
                {
                    var t = threads[i];
                    var str = Utilities.StringizeThread(t, true);

                    if (t == Debugger.ActiveThread)
                        Log.Emphasis(str);
                    else
                        Log.Info(str);

                    if (i < threads.Length - 1)
                        Log.Info(string.Empty);
                }
            }
        }

        private sealed class ThreadSetCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "set" }; }
            }

            public override string Summary
            {
                get { return "Set currently active thread."; }
            }

            public override string Syntax
            {
                get { return "thread set <id>"; }
            }

            public override void Process(string args)
            {
                var p = Debugger.ActiveProcess;

                if (p == null)
                {
                    Log.Error("No active inferior process");
                    return;
                }

                int num;

                if (!int.TryParse(args, out num) || num < 0)
                {
                    Log.Error("Invalid thread ID");
                    return;
                }

                var threads = p.GetThreads();

                foreach (var t in threads)
                {
                    if (t.Id == num)
                    {
                        t.SetActive();
                        Debugger.ActiveFrame = null;

                        Log.Emphasis(Utilities.StringizeThread(t, true));

                        return;
                    }
                }

                Log.Error("Thread ID '{0}' not found", num);
            }
        }

        public ThreadCommand()
        {
            AddCommand<ThreadBacktraceCommand>();
            AddCommand<ThreadGetCommand>();
            AddCommand<ThreadListCommand>();
            AddCommand<ThreadSetCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "thread" }; }
        }

        public override string Summary
        {
            get { return "Get, set, and inspect program threads."; }
        }
    }
}
