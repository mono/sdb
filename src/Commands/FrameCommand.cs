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
using System.Collections.Generic;
using System.Linq;
using Mono.Debugging.Client;

namespace Mono.Debugger.Client.Commands
{
    sealed class FrameCommand : MultiCommand
    {
        private sealed class FrameArgumentsCommand : Command
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

                var vals = new[] { f.GetThisReference() }.Concat(f.GetParameters());

                if (!vals.Any())
                {
                    Log.Info("No arguments");
                    return;
                }

                foreach (var val in vals)
                    if (val != null)
                        Log.Info("{0}{1}{2} {3} = {4}", Color.DarkGreen, val.TypeName,
                                 Color.Reset, val.Name, val.DisplayValue);
            }
        }

        private sealed class FrameDownCommand : Command
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

        private sealed class FrameGetCommand : Command
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

        private sealed class FrameLocalsCommand : Command
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
                    Log.Info("{0}{1}{2} {3} = {4}", Color.DarkGreen, val.TypeName,
                             Color.Reset, val.Name, val.DisplayValue);
            }
        }

        private sealed class FrameSetCommand : Command
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

        private sealed class FrameUpCommand : Command
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
    }
}
