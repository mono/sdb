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

using System.Linq;

namespace Mono.Debugger.Client.Commands
{
    sealed class AliasCommand : MultiCommand
    {
        sealed class AliasAddCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "add" }; }
            }

            public override string Summary
            {
                get { return "Add a command alias."; }
            }

            public override string Syntax
            {
                get { return "alias add <name> <command>"; }
            }

            public override string Help
            {
                get
                {
                    return "Adds a command alias with the given aliased command.\n" +
                           "\n" +
                           "Please note that command aliases can override built-in commands and plugins.\n" +
                           "\n" +
                           "Command aliases and their meanings can be shown with the 'alias list' command.";
                }
            }

            public override void Process(string args)
            {
                var name = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();

                if (name == null)
                {
                    Log.Error("No command alias name given");
                    return;
                }

                var rest = new string(args.Skip(name.Length).ToArray()).Trim();

                if (rest.Length == 0)
                {
                    Log.Error("No command alias meaning given");
                    return;
                }

                Debugger.Aliases.Add(name, rest);

                Log.Info("Added command alias '{0}' with meaning '{1}'", name, rest);
            }
        }

        sealed class AliasClearCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "clear" }; }
            }

            public override string Summary
            {
                get { return "Delete all command aliases."; }
            }

            public override string Syntax
            {
                get { return "alias clear"; }
            }

            public override string Help
            {
                get
                {
                    return "Clears all command aliases.";
                }
            }

            public override void Process(string args)
            {
                Debugger.Aliases.Clear();

                Log.Info("All command aliases cleared");
            }
        }

        sealed class AliasDeleteCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "delete", "remove" }; }
            }

            public override string Summary
            {
                get { return "Delete a command alias by name."; }
            }

            public override string Syntax
            {
                get { return "alias delete|remove <name>"; }
            }

            public override string Help
            {
                get
                {
                    return "Deletes the command alias with the specified name, if it exists.";
                }
            }

            public override void Process(string args)
            {
                string meaning;

                if (!Debugger.Aliases.TryGetValue(args, out meaning))
                {
                    Log.Error("Command alias '{0}' not found", args);
                    return;
                }

                Debugger.Aliases.Remove(args);

                Log.Info("Command alias '{0}' (with meaning '{1}') deleted", args, meaning);
            }
        }

        sealed class AliasListCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "list" }; }
            }

            public override string Summary
            {
                get { return "List all command aliases and their meanings."; }
            }

            public override string Syntax
            {
                get { return "alias list"; }
            }

            public override string Help
            {
                get
                {
                    return "Lists all command aliases along with their aliased commands.";
                }
            }

            public override void Process(string args)
            {
                if (Debugger.Aliases.Count == 0)
                {
                    Log.Info("No command aliases");
                    return;
                }

                foreach (var pair in Debugger.Aliases)
                    Log.Info("'{0}' = '{1}'", pair.Key, pair.Value);
            }
        }

        public AliasCommand()
        {
            AddCommand<AliasAddCommand>();
            AddCommand<AliasClearCommand>();
            AddCommand<AliasDeleteCommand>();
            AddCommand<AliasListCommand>();

            Forward<AliasListCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "alias" }; }
        }

        public override string Summary
        {
            get { return "Add, delete, and show command aliases."; }
        }

        public override string Help
        {
            get
            {
                return "Manipulates command aliases.\n" +
                       "\n" +
                       "A command alias can be used to execute a more elaborate command, optionally\n" +
                       "containing the '{0}' string formatting specifier which will be replaced with\n" +
                       "any arguments given to the alias at invocation time.";
            }
        }
    }
}
