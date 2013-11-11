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
using System.IO;

namespace Mono.Debugger.Client.Commands
{
    sealed class ArgumentsCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "arguments", "args" }; }
        }

        public override string Summary
        {
            get { return "Get or set the current program arguments."; }
        }

        public override string Syntax
        {
            get { return "arguments|args [args]"; }
        }

        public override string Help
        {
            get
            {
                return "Without any argument, this command prints the arguments to be passed to\n" +
                       "inferior processes launched locally. If an argument is given, it is set\n" +
                       "as the arguments to be passed.";
            }
        }

        public override void Process(string args)
        {
            if (args.Length == 0)
            {
                Log.Info("Program arguments: '{0}'", Debugger.Arguments);
                return;
            }
            else
            {
                var old = Debugger.Arguments;

                Debugger.Arguments = args;

                Log.Info("Program arguments set to '{0}' (were '{1}')", args, old);
            }
        }
    }
}
