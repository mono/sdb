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
