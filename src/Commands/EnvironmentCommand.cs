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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Debugger.Client.Commands
{
    sealed class EnvironmentCommand : MultiCommand
    {
        sealed class EnvironmentClearCommand : Command
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

            public override string Help
            {
                get
                {
                    return "Clears all environment variables.";
                }
            }

            public override void Process(string args)
            {
                Debugger.EnvironmentVariables.Clear();

                Log.Info("All environment variables cleared");
            }
        }

        sealed class EnvironmentDeleteCommand : Command
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

            public override string Help
            {
                get
                {
                    return "Deletes the given environment variable, if it exists.";
                }
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

        sealed class EnvironmentGetCommand : Command
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

            public override string Help
            {
                get
                {
                    return "Gets the value of the given environment variable.";
                }
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

        sealed class EnvironmentInheritCommand : Command
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

            public override string Help
            {
                get
                {
                    return "Inherits all environment variables from the debugger host environment.\n" +
                           "\n" +
                           "This simply grabs all environment variables on the machine where the\n" +
                           "debugger is running and inserts them into the debuggee environment.";
                }
            }

            public override void Process(string args)
            {
                foreach (DictionaryEntry pair in Environment.GetEnvironmentVariables())
                    Debugger.EnvironmentVariables[(string)pair.Key] = (string)pair.Value;
            }
        }

        sealed class EnvironmentListCommand : Command
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

            public override string Help
            {
                get
                {
                    return "Lists all environment variables and their values.";
                }
            }

            public override void Process(string args)
            {
                if (Debugger.EnvironmentVariables.Count == 0)
                {
                    Log.Info("No environment variables");
                    return;
                }

                foreach (var pair in Debugger.EnvironmentVariables)
                    Log.Info("'{0}' = '{1}'", pair.Key, pair.Value);
            }
        }

        sealed class EnvironmentSetCommand : Command
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

            public override string Help
            {
                get
                {
                    return "Sets the given environment variable to the given value.";
                }
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

            Forward<EnvironmentListCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "environment" }; }
        }

        public override string Summary
        {
            get { return "Manipulate inferior environment variables."; }
        }

        public override string Help
        {
            get
            {
                return "Manipulates environment variables.\n" +
                       "\n" +
                       "Environment variables set with these commands are passed on to inferior\n" +
                       "processes launched locally.";
            }
        }
    }
}
