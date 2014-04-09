//
// The MIT License (MIT)
//
// Copyright (c) 2014 Alex Rønne Petersen
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
using System.Linq;

namespace Mono.Debugger.Client.Commands
{
    sealed class CatchpointCommand : MultiCommand
    {
        sealed class CatchpointAddCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "add" }; }
            }

            public override string Summary
            {
                get { return "Add a catchpoint for an exception type."; }
            }

            public override string Syntax
            {
                get { return "catch|cp add <type>"; }
            }

            public override string Help
            {
                get
                {
                    return "Adds a catchpoint for the given exception type.";
                }
            }

            public override void Process(string args)
            {
                foreach (var cp in Debugger.BreakEvents.GetCatchpoints())
                {
                    if (cp.ExceptionName == args)
                    {
                        Log.Error("Catchpoint for '{0}' already exists");
                        return;
                    }
                }

                Debugger.BreakEvents.AddCatchpoint(args);

                Log.Info("Catchpoint for '{0}' added", args);
            }
        }

        sealed class CatchpointClearCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "clear" }; }
            }

            public override string Summary
            {
                get { return "Clear all catchpoints."; }
            }

            public override string Syntax
            {
                get { return "catch|cp clear"; }
            }

            public override string Help
            {
                get
                {
                    return "Removes all active catchpoints.";
                }
            }

            public override void Process(string args)
            {
                Debugger.BreakEvents.ClearCatchpoints();

                Log.Info("All catchpoints cleared");
            }
        }

        sealed class CatchpointDeleteCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "delete", "remove" }; }
            }

            public override string Summary
            {
                get { return "Delete a catchpoint by type."; }
            }

            public override string Syntax
            {
                get { return "catch|cp delete|remove <type>"; }
            }

            public override string Help
            {
                get
                {
                    return "Deletes the catchpoint for the given exception type, if it exists.";
                }
            }

            public override void Process(string args)
            {
                if (!Debugger.BreakEvents.GetCatchpoints().Any(x => x.ExceptionName == args))
                {
                    Log.Error("No catchpoint for '{0}' found", args);
                    return;
                }

                Debugger.BreakEvents.RemoveCatchpoint(args);

                Log.Info("Catchpoint for '{0}' deleted", args);
            }
        }

        sealed class CatchpointListCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "list" }; }
            }

            public override string Summary
            {
                get { return "List all set catchpoints."; }
            }

            public override string Syntax
            {
                get { return "catch|cp list"; }
            }

            public override string Help
            {
                get
                {
                    return "Lists all currently active catchpoints.";
                }
            }

            public override void Process(string args)
            {
                var cps = Debugger.BreakEvents.GetCatchpoints();

                if (cps.Count == 0)
                {
                    Log.Info("No catchpoints");
                    return;
                }

                foreach (var cp in cps)
                    Log.Info("'{0}'", cp.ExceptionName);
            }
        }

        public CatchpointCommand()
        {
            AddCommand<CatchpointAddCommand>();
            AddCommand<CatchpointClearCommand>();
            AddCommand<CatchpointDeleteCommand>();
            AddCommand<CatchpointListCommand>();

            Forward<CatchpointListCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "catchpoint", "cp" }; }
        }

        public override string Summary
        {
            get { return "Add, delete, and show catchpoints."; }
        }

        public override string Help
        {
            get
            {
                return "Manipulates catchpoints.\n" +
                       "\n" +
                       "When a catchpoint is added for an exception type and the debuggee code\n" +
                       "actively catches it, the debugger will break on the 'throw' site of the\n" +
                       "exception.";
            }
        }
    }
}
