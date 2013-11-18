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
