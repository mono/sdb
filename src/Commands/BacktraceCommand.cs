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
    sealed class BacktraceCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "backtrace", "bt" }; }
        }

        public override string Summary
        {
            get { return "Print a backtrace of the call stack."; }
        }

        public override string Syntax
        {
            get { return "backtrace|bt"; }
        }

        public override string Help
        {
            get
            {
                return "Prints a backtrace of the call stack for the active thread. Includes\n" +
                       "IL offsets, source locations, and source lines.";
            }
        }

        public override void Process(string args)
        {
            var bt = Debugger.ActiveBacktrace;

            if (bt == null)
            {
                Log.Error("No active backtrace");
                return;
            }

            if (bt.FrameCount == 0)
            {
                Log.Info("Backtrace for this thread is unavailable");
                return;
            }

            for (var i = 0; i < bt.FrameCount; i++)
            {
                var f = bt.GetFrame(i);
                var str = Utilities.StringizeFrame(f, true);

                if (f == Debugger.ActiveFrame)
                    Log.Emphasis(str);
                else
                    Log.Info(str);
            }
        }
    }
}
