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
            get { return "connect|remote <name> <addr> <dbg-port> <out-port>"; }
        }

        public override void Process(string args)
        {
            Log.Error("Connect support is not yet implemented.");
        }
    }
}
