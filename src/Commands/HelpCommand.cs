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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Debugger.Client.Commands
{
    sealed class HelpCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "help" }; }
        }

        public override string Summary
        {
            get { return "Describe available commands."; }
        }

        public override string Syntax
        {
            get { return "help [cmd] [sub-cmd] [...]"; }
        }

        public override string Help
        {
            get
            {
                return "Prints detailed help for the given (sub-)command.\n" +
                       "\n" +
                       "If the command is a multi-command, its sub-commands are printed in addition\n" +
                       "to its detailed help text.";
            }
        }

        public override void Process(string args)
        {
            var names = args.Split(' ').Where(x => x != string.Empty).ToArray();
            var fullName = string.Empty;
            Command cmd = CommandLine.Root;

            foreach (var name in names)
            {
                var mcmd = cmd as MultiCommand;

                if (mcmd == null)
                    break;

                var scmd = mcmd.GetCommand(name);

                if (scmd == null)
                {
                    if (cmd is RootCommand)
                        Log.Error("'{0}' is not a known command", name);
                    else
                        Log.Error("No sub-command '{0}' under '{1}'", name, fullName);

                    return;
                }
                else
                {
                    cmd = scmd;

                    if (fullName != string.Empty)
                        fullName += ' ';

                    fullName += name;
                }
            }

            if (!(cmd is RootCommand))
            {
                Log.Info(string.Empty);
                Log.Emphasis("  {0}", cmd.Syntax);
            }

            Log.Info(string.Empty);
            Log.Info(cmd.Help);
            Log.Info(string.Empty);

            var mcmd2 = cmd as MultiCommand;

            if (mcmd2 != null)
            {

                foreach (var sub in mcmd2.Commands.Select(x => x.Item2).Distinct())
                {
                    Log.Emphasis("  {0}", string.Join("|", sub.Names));
                    Log.Info("    {0}", sub.Summary);
                    Log.Info(string.Empty);
                }
            }
        }
    }
}
