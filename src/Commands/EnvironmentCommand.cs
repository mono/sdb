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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Debugger.Client.Commands
{
    sealed class EnvironmentCommand : MultiCommand
    {
        private sealed class EnvironmentClearCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "clear" }; }
            }

            public override string Summary
            {
                get { return "Clear all environment variables."; }
            }

            public override string Syntax
            {
                get { return "environment clear"; }
            }

            public override void Process(string args)
            {
                Debugger.EnvironmentVariables.Clear();

                Log.Info("All environment variables cleared");
            }
        }

        private sealed class EnvironmentDeleteCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "delete", "remove" }; }
            }

            public override string Summary
            {
                get { return "Delete an environment variable."; }
            }

            public override string Syntax
            {
                get { return "environment delete|remove <name>"; }
            }

            public override void Process(string args)
            {
                string val;

                if (!Debugger.EnvironmentVariables.TryGetValue(args, out val))
                {
                    Log.Error("Environment variable '{0}' not found", args);
                    return;
                }

                Debugger.EnvironmentVariables.Remove(args);

                Log.Info("Environment variable '{0}' (with value '{1}') deleted", args, val);
            }
        }

        private sealed class EnvironmentGetCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "get" }; }
            }

            public override string Summary
            {
                get { return "Get the value of an environment variable."; }
            }

            public override string Syntax
            {
                get { return "environment get <name>"; }
            }

            public override void Process(string args)
            {
                var name = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();

                if (name == null)
                {
                    Log.Error("No environment variable name given");
                    return;
                }

                string val;

                if (Debugger.EnvironmentVariables.TryGetValue(name, out val))
                    Log.Info("'{0}' = '{1}'", name, val);
                else
                    Log.Error("Environment variable '{0}' is not set", name);
            }
        }

        private sealed class EnvironmentInheritCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "inherit" }; }
            }

            public override string Summary
            {
                get { return "Inherit all environment variables from the debugger."; }
            }

            public override string Syntax
            {
                get { return "environment inherit"; }
            }

            public override void Process(string args)
            {
                foreach (DictionaryEntry pair in Environment.GetEnvironmentVariables())
                    Debugger.EnvironmentVariables[(string)pair.Key] = (string)pair.Value;
            }
        }

        private sealed class EnvironmentListCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "list" }; }
            }

            public override string Summary
            {
                get { return "List all environment variables and their values."; }
            }

            public override string Syntax
            {
                get { return "environment list"; }
            }

            public override void Process(string args)
            {
                foreach (var pair in Debugger.EnvironmentVariables)
                    Log.Info("'{0}' = '{1}'", pair.Key, pair.Value);
            }
        }

        private sealed class EnvironmentSetCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "set" }; }
            }

            public override string Summary
            {
                get { return "Set the value of an environment variable."; }
            }

            public override string Syntax
            {
                get { return "environment set <name> <value>"; }
            }

            public override void Process(string args)
            {
                var name = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();

                if (name == null)
                {
                    Log.Error("No environment variable name given");
                    return;
                }

                var rest = new string(args.Skip(name.Length).ToArray()).Trim();

                if (rest.Length == 0)
                {
                    Log.Error("No environment variable value given");
                    return;
                }

                Debugger.EnvironmentVariables[name] = rest;
            }
        }

        public EnvironmentCommand()
        {
            AddCommand<EnvironmentClearCommand>();
            AddCommand<EnvironmentDeleteCommand>();
            AddCommand<EnvironmentGetCommand>();
            AddCommand<EnvironmentInheritCommand>();
            AddCommand<EnvironmentListCommand>();
            AddCommand<EnvironmentSetCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "environment" }; }
        }

        public override string Summary
        {
            get { return "Manipulate inferior environment variables."; }
        }
    }
}
