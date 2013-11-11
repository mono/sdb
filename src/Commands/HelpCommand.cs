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
