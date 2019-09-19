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
using System.Net;

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
            get { return "attach <address>:<port>"; }
        }

        public override string Help
        {
            get
            {
                return "Attempts to attach to the specified process.\n";
            }
        }

        public override void Process(string args)
        {
            IPAddress address;
            int port;

            if (Debugger.State != State.Exited)
            {
                Log.Error("An inferior process is already being debugged");
                return;
            }

            if (args.Length == 0)
            {
                if (Debugger.CurrentAddress == null)
                {
                    Log.Error("No process address and/or port given (and no process attached previously)");
                    return;
                }

                address = Debugger.CurrentAddress;
                port = Debugger.CurrentPort;
            }
            else
            {
                var split = args.Split(':', 2);
                if (split.Length < 2)
                {
                    Log.Error("The argument does not include a semicolon.");
                    return;
                }

                if (!IPAddress.TryParse(split[0], out address))
                {
                    Log.Error("Failed to parse address");
                    return;
                }

                if (!int.TryParse(split[1], out port))
                {
                    Log.Error("Failed to parse port");
                    return;
                }
            }

            Debugger.Connect(address, port);
        }
    }
}
