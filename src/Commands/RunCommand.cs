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
using System.IO;

namespace Mono.Debugger.Client.Commands
{
    sealed class RunCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "run", "execute", "launch" }; }
        }

        public override string Summary
        {
            get { return "Run a virtual machine locally."; }
        }

        public override string Syntax
        {
            get { return "run|execute|launch <path>"; }
        }

        public override string Help
        {
            get
            {
                return "Runs an inferior process locally. The argument must be the path to the\n" +
                       "executable file (i.e. '.exe').\n" +
                       "\n" +
                       "The following debugger state is taken into account:\n" +
                       "\n" +
                       "* Working directory.\n" +
                       "* Program arguments.\n" +
                       "* Environment variables.";
            }
        }

        public override void Process(string args)
        {
            if (Debugger.State != State.Exited)
            {
                Log.Error("An inferior process is already being debugged");
                return;
            }

            if (args.Length == 0)
            {
                Log.Error("No program path given");
                return;
            }

            if (!File.Exists(args))
            {
                Log.Error("Program executable '{0}' does not exist", args);
                return;
            }

            FileInfo file;

            try
            {
                file = new FileInfo(args);
            }
            catch (Exception ex)
            {
                Log.Error("Could not open file '{0}':", args);
                Log.Error(ex.ToString());

                return;
            }

            Debugger.Run(file);
        }
    }
}
