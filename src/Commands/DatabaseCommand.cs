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

            public override string Help
            {
                get
                {
                    return "Loads the debugger state from the given file.\n" +
                           "\n" +
                           "Note that this resets the active debugger state before applying the\n" +
                           "data read from the given file.";
                }
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

                Log.Info("Debugger state initialized from '{0}'", args);
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

            public override string Help
            {
                get
                {
                    return "Saves the active debugger state to the given file.";
                }
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

                Log.Info("Debugger state saved to '{0}'", args);
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
            get { return "Store and load debugger state."; }
        }

        public override string Help
        {
            get
            {
                return "Stores and loads the debugger state.\n" +
                       "\n" +
                       "This is useful for retaining things like breakpoints and environment\n" +
                       "variables across debugger runs.\n" +
                       "\n" +
                       "The following state is saved/loaded:\n" +
                       "\n" +
                       "* Working directory.\n" +
                       "* Program arguments.\n" +
                       "* Environment variables.\n" +
                       "* Watches.\n" +
                       "* Breakpoints and catchpoints.\n" +
                       "* Session options.\n" +
                       "* Evaluation options.\n" +
                       "* Command aliases.";
            }
        }
    }
}
