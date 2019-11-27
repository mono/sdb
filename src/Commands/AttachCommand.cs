//
// The MIT License (MIT)
//
// Copyright (c) 2018 Alex Rønne Petersen
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
using System.Threading;
using Mono.Unix.Native;

namespace Mono.Debugger.Client.Commands
{
    sealed class AttachCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "attach" }; }
        }

        public override string Summary
        {
            get { return "Attach to a running process."; }
        }

        public override string Syntax
        {
            get { return "attach <id>"; }
        }

        public override string Help
        {
            get
            {
                return "Attempts to attach to the given process ID.";
            }
        }

        public override void Process(string args)
        {
            int pid;
            if (Debugger.State != State.Exited)
            {
                Log.Error("An inferior process is already being debugged");
                return;
            }


            if (!int.TryParse(args, out pid))
            {
                Log.Error("Invalid process id");
                return;
            }
            Debugger.Attach(pid);
        }
    }
}
