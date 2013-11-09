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
    sealed class CatchCommand : MultiCommand
    {
        private sealed class CatchAddCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "add", "set" }; }
            }

            public override string Summary
            {
                get { return "Add a catchpoint for an exception type."; }
            }

            public override string Syntax
            {
                get { return "catch|cp add|set <type>"; }
            }

            public override void Process(string args)
            {
                Log.Error("Catchpoints are not yet implemented.");
            }
        }

        private sealed class CatchDeleteCommand : Command
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

            public override void Process(string args)
            {
                Log.Error("Catchpoints are not yet implemented.");
            }
        }

        private sealed class CatchListCommand : Command
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

            public override void Process(string args)
            {
                Log.Error("Catchpoints are not yet implemented.");
            }
        }

        public CatchCommand()
        {
            AddCommand<CatchAddCommand>();
            AddCommand<CatchDeleteCommand>();
            AddCommand<CatchListCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "catch", "cp" }; }
        }

        public override string Summary
        {
            get { return "Add, delete, and show catchpoints."; }
        }
    }
}
