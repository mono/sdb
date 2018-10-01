//
// The MIT License (MIT)
//
// Copyright (c) 2018 Alex Rønne Petersen
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

using System.Collections.Generic;

namespace Mono.Debugger.Client.Commands
{
    sealed class ThreadCommand : MultiCommand
    {
        sealed class ThreadBacktraceCommand : Command
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

            public override string Help
            {
                get
                {
                    return "Prints a backtrace for all program threads.\n" +
                           "\n" +
                           "Functionally equivalent to applying 'backtrace' to all threads while\n" +
                           "switching through them with 'thread set'.";
                }
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

        sealed class ThreadGetCommand : Command
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

            public override string Help
            {
                get
                {
                    return "Gets the currently active thread.";
                }
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

        sealed class ThreadListCommand : Command
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

            public override string Help
            {
                get
                {
                    return "Lists all program threads, along with their IDs and names.";
                }
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

        sealed class ThreadSetCommand : Command
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

            public override string Help
            {
                get
                {
                    return "Sets the currently active thread to the given thread ID.\n" +
                           "\n" +
                           "All following commands that somehow interact with the program's call\n" +
                           "stack will happen on the specified thread.";
                }
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

            Forward<ThreadGetCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "thread" }; }
        }

        public override string Summary
        {
            get { return "Get, set, and inspect program threads."; }
        }

        public override string Help
        {
            get
            {
                return "Interacts with program threads.";
            }
        }
    }
}
