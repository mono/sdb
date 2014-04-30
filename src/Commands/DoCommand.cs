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
using System.Linq;

namespace Mono.Debugger.Client.Commands
{
    sealed class DoCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "do" }; }
        }

        public override string Summary
        {
            get { return "Run several commands separated by a string."; }
        }

        public override string Syntax
        {
            get { return "do <separator> [cmd1] [separator] [cmd2] [...]"; }
        }

        public override string Help
        {
            get
            {
                return "Runs a series of commands separated by a specified string.";
            }
        }

        public override void Process(string args)
        {
            var sep = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();

            if (sep == null)
            {
                Log.Error("No command separator given");
                return;
            }

            var rest = new string(args.Skip(sep.Length).ToArray()).Trim();
            var cmds = rest.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);

            CommandLine.RunCommands(cmds);
        }
    }
}
