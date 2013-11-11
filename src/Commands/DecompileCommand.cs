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
    sealed class DecompileCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "decompile" }; }
        }

        public override string Summary
        {
            get { return "Decompile the current stack frame."; }
        }

        public override string Syntax
        {
            get { return "decompile"; }
        }

        public override string Help
        {
            get
            {
                return "Decompiles the IL code in the active stack frame and attempts to highlight\n" +
                       "the line closest to the current IL offset.\n" +
                       "\n" +
                       "Currently unimplemented.";
            }
        }

        public override void Process(string args)
        {
            Log.Error("Decompilation is not yet implemented.");
        }
    }
}
