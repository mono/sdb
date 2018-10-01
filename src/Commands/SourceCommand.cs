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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mono.Debugger.Client.Commands
{
    sealed class SourceCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "source", "src" }; }
        }

        public override string Summary
        {
            get { return "Show the source for the current stack frame."; }
        }

        public override string Syntax
        {
            get { return "source|src [lower] [upper]"; }
        }

        public override string Help
        {
            get
            {
                return "Prints the source for the current stack frame and highlights the current\n" +
                       "line.\n" +
                       "\n" +
                       "If arguments are given, they specify how many lines to print before and\n" +
                       "after the current line.";
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

            var lower = 10;
            var upper = 10;

            var lowerStr = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();

            if (lowerStr != null)
            {
                if (!int.TryParse(lowerStr, out lower))
                {
                    Log.Error("Invalid lower bound value");
                    return;
                }

                lower = System.Math.Abs(lower);

                var upperStr = new string(args.Skip(lowerStr.Length).ToArray()).Trim();

                if (upperStr.Length != 0)
                {
                    if (!int.TryParse(upperStr, out upper))
                    {
                        Log.Error("Invalid upper bound value");
                        return;
                    }
                }
            }

            var loc = f.SourceLocation;
            var file = loc.FileName;
            var line = loc.Line;

            if (file != null && line != -1)
            {
                if (!File.Exists(file))
                {
                    Log.Error("Source file '{0}' not found", file);
                    return;
                }

                StreamReader reader;

                try
                {
                    reader = File.OpenText(file);
                }
                catch (Exception ex)
                {
                    Log.Error("Could not open source file '{0}'", file);
                    Log.Error(ex.ToString());

                    return;
                }

                try
                {
                    var exec = Debugger.CurrentExecutable;

                    if (exec != null && File.GetLastWriteTime(file) > exec.LastWriteTime)
                        Log.Notice("Source file '{0}' is newer than the debuggee executable", file);

                    var cur = 0;

                    while (!reader.EndOfStream)
                    {
                        var str = reader.ReadLine();

                        var i = line - cur;
                        var j = cur - line;

                        if (i > 0 && i < lower + 2 || j >= 0 && j < upper)
                        {
                            var lineStr = string.Format("{0,8}:    {1}", cur + 1, str);

                            if (cur == line - 1)
                                Log.Emphasis(lineStr);
                            else
                                Log.Info(lineStr);
                        }

                        cur++;
                    }
                }
                finally
                {
                    reader.Dispose();
                }
            }
            else
                Log.Error("No source information available");
        }
    }
}
