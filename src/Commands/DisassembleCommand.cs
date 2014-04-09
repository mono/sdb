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

        public override string Help
        {
            get
            {
                return "Disassembles the current stack frame and highlights the line corresponding\n" +
                       "to the current IL offset.\n" +
                       "\n" +
                       "If arguments are given, they specify how many lines to print before and\n" +
                       "after the line corresponding to the current IL offset.";
            }
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
