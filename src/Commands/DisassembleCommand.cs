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

namespace Mono.Debugger.Client.Commands
{
    sealed class DisassembleCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "disassemble", "disasm", "dasm" }; }
        }

        public override string Summary
        {
            get { return "Disassemble the current stack frame."; }
        }

        public override string Syntax
        {
            get { return "disassemble|disasm|dasm [lower] [upper]"; }
        }

        public override void Process(string args)
        {
            var f = Debugger.ActiveFrame;

            if (f == null)
            {
                Log.Error("No active stack frame");
                return;
            }

            var lower = -10;
            var upper = 20;

            var lowerStr = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();

            if (lowerStr != null)
            {
                if (!int.TryParse(lowerStr, out lower))
                {
                    Log.Error("Invalid lower bound value");
                    return;
                }

                lower = -lower;

                var upperStr = new string(args.Skip(lowerStr.Length).ToArray()).Trim();

                if (upperStr.Length != 0)
                {
                    if (!int.TryParse(upperStr, out upper))
                    {
                        Log.Error("Invalid upper bound value");
                        return;
                    }

                    upper += System.Math.Abs(lower) + 1;
                }
            }

            var asm = f.Disassemble(lower, upper);

            foreach (var line in asm)
            {
                if (line.IsOutOfRange)
                    continue;

                var str = string.Format("0x{0:X8}    {1}", line.Address, line.Code);

                if (line.Address == f.Address)
                    Log.Emphasis(str);
                else
                    Log.Info(str);
            }
        }
    }
}
