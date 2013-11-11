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
    sealed class DirectoryCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "directory", "cd", "cwd" }; }
        }

        public override string Summary
        {
            get { return "Get or set the current working directory."; }
        }

        public override string Syntax
        {
            get { return "directory|cd|cwd [path]"; }
        }

        public override string Help
        {
            get
            {
                return "Without any argument, prints the current working directory for inferior\n" +
                       "processes launched locally. If an argument is given, the working directory\n" +
                       "is set to that argument.";
            }
        }

        public override void Process(string args)
        {
            if (args.Length == 0)
                Log.Info("Working directory: '{0}'", Debugger.WorkingDirectory);
            else if (Directory.Exists(args))
            {
                var old = Debugger.WorkingDirectory;

                Debugger.WorkingDirectory = args;

                Log.Info("Working directory set to '{0}' (was '{1}')", args, old);
            }
            else
                Log.Error("Directory '{0}' does not exist", args);
        }
    }
}
