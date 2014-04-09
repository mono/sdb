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
