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

using System;
using System.Collections.Generic;
using Mono.Options;

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

            if (!batch && !Utilities.IsWindows)
                LibC.SetSignal(LibC.SignalInterrupt, LibC.IgnoreSignal);

            CommandLine.Run(ver, batch, rc, cmds, files);

            return 0;
        }
    }
}
