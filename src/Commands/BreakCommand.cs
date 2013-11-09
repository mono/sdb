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
    sealed class BreakCommand : MultiCommand
    {
        sealed class BreakAddCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "add" }; }
            }

            public override string Summary
            {
                get { return "Add a breakpoint at a source line."; }
            }

            public override string Syntax
            {
                get { return "break|bp add <file> <line>"; }
            }

            public override void Process(string args)
            {
                Log.Error("Breakpoints are not yet implemented.");
            }
        }

        sealed class BreakClearCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "clear" }; }
            }

            public override string Summary
            {
                get { return "Clear all breakpoints."; }
            }

            public override string Syntax
            {
                get { return "break|bp clear"; }
            }

            public override void Process(string args)
            {
                Log.Error("Breakpoints are not yet implemented.");
            }
        }

        sealed class BreakDeleteCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "delete", "remove" }; }
            }

            public override string Summary
            {
                get { return "Delete a breakpoint by ID."; }
            }

            public override string Syntax
            {
                get { return "break|bp delete|remove <id>"; }
            }

            public override void Process(string args)
            {
                Log.Error("Breakpoints are not yet implemented.");
            }
        }

        sealed class BreakListCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "list" }; }
            }

            public override string Summary
            {
                get { return "List all set breakpoints and their IDs."; }
            }

            public override string Syntax
            {
                get { return "break|bp list"; }
            }

            public override void Process(string args)
            {
                Log.Error("Breakpoints are not yet implemented.");
            }
        }

        public BreakCommand()
        {
            AddCommand<BreakAddCommand>();
            AddCommand<BreakClearCommand>();
            AddCommand<BreakDeleteCommand>();
            AddCommand<BreakListCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "break", "bp" }; }
        }

        public override string Summary
        {
            get { return "Add, delete, and show breakpoints."; }
        }
    }
}
