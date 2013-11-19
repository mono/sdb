//
// The MIT License (MIT)
//
// Copyright (c) 2013 Alex Rønne Petersen
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
using System.Collections.Generic;
using System.Linq;
using Mono.Debugging.Client;

namespace Mono.Debugger.Client.Commands
{
    sealed class FrameCommand : MultiCommand
    {
        sealed class FrameArgumentsCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "arguments", "args" }; }
            }

            public override string Summary
            {
                get { return "Show arguments in the current stack frame."; }
            }

            public override string Syntax
            {
                get { return "frame arguments|args"; }
            }

            public override string Help
            {
                get
                {
                    return "Lists all arguments and their values for the current frame.\n" +
                           "\n" +
                           "The 'this' reference is included if available.";
                }
            }

            public override void Process(string args)
            {
                var f = Debugger.ActiveFrame;

                if (f == null)
                {
                    // Not really an error since it's a perfectly
                    // valid state for e.g. the finalizer thread.
                    if (Debugger.State != State.Exited)
                        Log.Info("Backtrace for this thread is unavailable");
                    else
                        Log.Error("No active stack frame");

                    return;
                }

                var vals = new[] { f.GetThisReference() }.Concat(f.GetParameters()).Where(x => x != null);

                if (!vals.Any())
                {
                    Log.Info("No arguments");
                    return;
                }

                foreach (var val in vals)
                {
                    val.WaitHandle.WaitOne();

                    var strErr = Utilities.StringizeValue(val);

                    if (strErr.Item2)
                        Log.Error("{0}<error>{1} {2} = {3}", Color.DarkRed, Color.Reset,
                                  val.Name, strErr.Item1);
                    else
                        Log.Info("{0}{1}{2} {3} = {4}", Color.DarkGreen, val.TypeName,
                                 Color.Reset, val.Name, strErr.Item1);
                }
            }
        }

        sealed class FrameDownCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "down" }; }
            }

            public override string Summary
            {
                get { return "Go down the call stack by one frame."; }
            }

            public override string Syntax
            {
                get { return "frame down"; }
            }

            public override string Help
            {
                get
                {
                    return "Sets the active frame to the frame below the current frame.";
                }
            }

            public override void Process(string args)
            {
                var bt = Debugger.ActiveBacktrace;

                if (bt == null)
                {
                    Log.Error("No active backtrace");
                    return;
                }

                if (bt.FrameCount == 0)
                {
                    Log.Info("Backtrace for this thread is unavailable");
                    return;
                }

                var f = Debugger.ActiveFrame;

                if (f == null)
                {
                    Log.Error("No active stack frame");
                    return;
                }

                var f2 = bt.GetFrame(f.Index - 1);

                if (f2 == f)
                {
                    Log.Error("Cannot go further down the stack");
                    return;
                }

                Debugger.ActiveFrame = f2;

                Log.Emphasis(Utilities.StringizeFrame(f2, true));
            }
        }

        sealed class FrameGetCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "get" }; }
            }

            public override string Summary
            {
                get { return "Show the current stack frame."; }
            }

            public override string Syntax
            {
                get { return "frame get"; }
            }

            public override string Help
            {
                get
                {
                    return "Shows the currently active frame, along with its source location and\n" +
                           "and source line.";
                }
            }

            public override void Process(string args)
            {
                var f = Debugger.ActiveFrame;

                if (f == null)
                {
                    // Not really an error since it's a perfectly
                    // valid state for e.g. the finalizer thread.
                    if (Debugger.State != State.Exited)
                        Log.Info("Backtrace for this thread is unavailable");
                    else
                        Log.Error("No active stack frame");

                    return;
                }

                Log.Emphasis(Utilities.StringizeFrame(f, true));
            }
        }

        sealed class FrameLocalsCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "locals", "variables", "vars" }; }
            }

            public override string Summary
            {
                get { return "Show local variables in the current stack frame."; }
            }

            public override string Syntax
            {
                get { return "frame locals|variables|vars"; }
            }

            public override string Help
            {
                get
                {
                    return "Lists all local variables and their values in the current frame.";
                }
            }

            public override void Process(string args)
            {
                var f = Debugger.ActiveFrame;

                if (f == null)
                {
                    // Not really an error since it's a perfectly
                    // valid state for e.g. the finalizer thread.
                    if (Debugger.State != State.Exited)
                        Log.Info("Backtrace for this thread is unavailable");
                    else
                        Log.Error("No active stack frame");

                    return;
                }

                var vals = f.GetLocalVariables();

                if (vals.Length == 0)
                {
                    Log.Info("No locals");
                    return;
                }

                foreach (var val in vals)
                {
                    val.WaitHandle.WaitOne();

                    var strErr = Utilities.StringizeValue(val);

                    if (strErr.Item2)
                        Log.Error("{0}<error>{1} {2} = {3}", Color.DarkRed, Color.Reset,
                                  val.Name, strErr.Item1);
                    else
                        Log.Info("{0}{1}{2} {3} = {4}", Color.DarkGreen, val.TypeName,
                                 Color.Reset, val.Name, strErr.Item1);
                }
            }
        }

        sealed class FrameSetCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "set" }; }
            }

            public override string Summary
            {
                get { return "Switch to specific stack frame."; }
            }

            public override string Syntax
            {
                get { return "frame set <num>"; }
            }

            public override string Help
            {
                get
                {
                    return "Sets the current frame to the given number." +
                           "\n" +
                           "The numbers shown in a 'backtrace' can be passed to this command.";
                }
            }

            public override void Process(string args)
            {
                var bt = Debugger.ActiveBacktrace;

                if (bt == null)
                {
                    Log.Error("No active backtrace");
                    return;
                }

                if (bt.FrameCount == 0)
                {
                    // Not really an error since it's a perfectly
                    // valid state for e.g. the finalizer thread.
                    if (Debugger.State != State.Exited)
                        Log.Info("Backtrace for this thread is unavailable");
                    else
                        Log.Error("No active stack frame");

                    return;
                }

                int num;

                if (!int.TryParse(args, out num))
                {
                    Log.Error("Invalid frame number");
                    return;
                }

                if (num < 0 || num > bt.FrameCount - 1)
                {
                    Log.Error("Frame number is out of bounds (0 .. {0})", bt.FrameCount - 1);
                    return;
                }

                var f = bt.GetFrame(num);

                Debugger.ActiveFrame = f;

                Log.Emphasis(Utilities.StringizeFrame(f, true));
            }
        }

        sealed class FrameUpCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "up" }; }
            }

            public override string Summary
            {
                get { return "Go up the call stack by one frame."; }
            }

            public override string Syntax
            {
                get { return "frame up"; }
            }

            public override string Help
            {
                get
                {
                    return "Sets the active frame to the frame above the current frame.";
                }
            }

            public override void Process(string args)
            {
                var bt = Debugger.ActiveBacktrace;

                if (bt == null)
                {
                    Log.Error("No active backtrace");
                    return;
                }

                if (bt.FrameCount == 0)
                {
                    Log.Info("Backtrace for this thread is unavailable");
                    return;
                }

                var f = Debugger.ActiveFrame;

                if (f == null)
                {
                    Log.Error("No active stack frame");
                    return;
                }

                var f2 = bt.GetFrame(f.Index + 1);

                if (f2 == f)
                {
                    Log.Error("Cannot go further up the stack");
                    return;
                }

                Debugger.ActiveFrame = f2;

                Log.Emphasis(Utilities.StringizeFrame(f2, true));
            }
        }

        public FrameCommand()
        {
            AddCommand<FrameArgumentsCommand>();
            AddCommand<FrameDownCommand>();
            AddCommand<FrameGetCommand>();
            AddCommand<FrameLocalsCommand>();
            AddCommand<FrameSetCommand>();
            AddCommand<FrameUpCommand>();

            Forward<FrameGetCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "frame" }; }
        }

        public override string Summary
        {
            get { return "Get, set, and inspect stack frames."; }
        }

        public override string Help
        {
            get
            {
                return "Interacts with the call stack.";
            }
        }
    }
}
