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
using System.Linq;
using System.Net;

namespace Mono.Debugger.Client.Commands
{
    sealed class ListenCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "listen", "wait" }; }
        }

        public override string Summary
        {
            get { return "Listen for a remote virtual machine."; }
        }

        public override string Syntax
        {
            get { return "listen|wait <addr> <port>"; }
        }

        public override string Help
        {
            get
            {
                return "Listens for a remote debuggee connection on the given IP address and port.";
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

            Debugger.Listen(addr, port);

            Log.Info("Listening on '{0}:{1}'...", addr, port);
        }
    }
}
