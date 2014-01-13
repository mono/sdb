//
// The MIT License (MIT)
//
// Copyright (c) 2013 Alex Rønne Petersen
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
using Mono.Options;
using Mono.Unix.Native;

namespace Mono.Debugger.Client
{
    static class Program
    {
        static void Help(OptionSet set)
        {
            Console.WriteLine("This is the Mono soft debugger.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("  sdb [options]");
            Console.WriteLine("  sdb [options] \"run prog.exe\"");
            Console.WriteLine("  sdb [options] \"args --foo --bar baz\" \"run prog.exe\"");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine();

            set.WriteOptionDescriptions(Console.Out);

            Console.WriteLine();
            Console.WriteLine("All non-option arguments are treated as commands that are executed");
            Console.WriteLine("at startup. Files are executed before individual commands.");
        }

        static int Main(string[] args)
        {
            var version = false;
            var help = false;
            var batch = false;
            var rc = true;

            var files = new List<string>();

            var p = new OptionSet()
            {
                {"v|version", "Show version information and exit.", v => version = v != null},
                {"h|help", "Show this help message and exit.", v => help = v != null},
                {"b|batch", "Exit after running commands.", v => batch = v != null},
                {"n|norc", "Don't run commands in '~/.sdb.rc'.", v => rc = v == null},
                {"f|file=", "Execute commands in the given file at startup.", f => files.Add(f)}
            };

            List<string> cmds;

            try
            {
                cmds = p.Parse(args);
            }
            catch (OptionException ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }

            var ver = typeof(Program).Assembly.GetName().Version;

            if (version)
            {
                Console.WriteLine("Mono soft debugger (sdb) {0}", ver);
                return 0;
            }

            if (help)
            {
                Help(p);
                return 0;
            }

            // This ugly hack is necessary because we don't want the
            // debuggee to die if the user hits Ctrl-C. We set our signal
            // disposition to `SIG_IGN` so that the debuggee inherits
            // this and doesn't die.
            if (!Utilities.IsWindows && !batch)
                Stdlib.SetSignalAction(Signum.SIGINT, SignalAction.Ignore);

            CommandLine.Run(ver, batch, rc, cmds, files);

            return 0;
        }
    }
}
