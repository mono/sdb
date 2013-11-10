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
    sealed class DatabaseCommand : MultiCommand
    {
        sealed class DatabaseLoadCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "load", "read" }; }
            }

            public override string Summary
            {
                get { return "Read the database for the current inferior."; }
            }

            public override string Syntax
            {
                get { return "database|db load|read <file>"; }
            }

            public override void Process(string args)
            {
                if (args.Length == 0)
                {
                    Log.Error("No file path given");
                    return;
                }

                if (!File.Exists(args))
                {
                    Log.Error("File '{0}' does not exist", args);
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

                Debugger.Read(file);
            }
        }

        sealed class DatabaseSaveCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "save", "write" }; }
            }

            public override string Summary
            {
                get { return "Save the database for the current inferior."; }
            }

            public override string Syntax
            {
                get { return "database|db save|write <file>"; }
            }

            public override void Process(string args)
            {
                if (args.Length == 0)
                {
                    Log.Error("No file path given");
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

                Debugger.Write(file);
            }
        }

        public DatabaseCommand()
        {
            AddCommand<DatabaseLoadCommand>();
            AddCommand<DatabaseSaveCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "database", "db" }; }
        }

        public override string Summary
        {
            get { return "Store and load debugger configuration."; }
        }
    }
}
