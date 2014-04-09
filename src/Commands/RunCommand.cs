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
