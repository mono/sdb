//
// The MIT License (MIT)
//
// Copyright (c) 2015 Alex Rønne Petersen
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
using System.Linq;
using System.Net;

namespace Mono.Debugger.Client.Commands
{
    sealed class ConnectCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "connect", "remote" }; }
        }

        public override string Summary
        {
            get { return "Connect to a remote virtual machine."; }
        }

        public override string Syntax
        {
            get { return "connect|remote <addr> <port>"; }
        }

        public override string Help
        {
            get
            {
                return "Attempts to connect to a debuggee running at the specified IP address and\n" +
                       "port.\n" +
                       "\n" +
                       "Respects the MaxConnectionAttempts and ConnectionAttemptInterval settings.";
            }
        }

        public override void Process(string args)
        {
            if (Debugger.State != State.Exited)
            {
                Log.Error("An inferior process is already being debugged");
                return;
            }

            var ip = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();

            if (ip == null)
            {
                Log.Error("No IP address given");
                return;
            }

            IPAddress addr;

            if (!IPAddress.TryParse(ip, out addr))
            {
                Log.Error("Invalid IP address");
                return;
            }

            var rest = new string(args.Skip(ip.Length).ToArray()).Trim();

            if (rest.Length == 0)
            {
                Log.Error("No port number given");
                return;
            }

            int port;

            if (!int.TryParse(rest, out port) || port <= 0)
            {
                Log.Error("Invalid port number");
                return;
            }

            Log.Info("Connecting to '{0}:{1}'...", addr, port);

            Debugger.Connect(addr, port);
        }
    }
}
