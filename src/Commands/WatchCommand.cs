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

using System.Collections.Generic;
using Mono.Debugging.Client;

namespace Mono.Debugger.Client.Commands
{
    sealed class WatchCommand : MultiCommand
    {
        private sealed class WatchAddCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "add", "set" }; }
            }

            public override string Summary
            {
                get { return "Add a watchpoint expression."; }
            }

            public override string Syntax
            {
                get { return "watch|wp add|set <expr>"; }
            }

            public override void Process(string args)
            {
                var id = Debugger.GetWatchId();

                Debugger.Watchpoints.Add(id, args);

                Log.Info("Added watchpoint '{0}' with expression '{1}'", id, args);
            }
        }

        private sealed class WatchClearCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "clear" }; }
            }

            public override string Summary
            {
                get { return "Delete all watchpoints."; }
            }

            public override string Syntax
            {
                get { return "watch|wp clear"; }
            }

            public override void Process(string args)
            {
                Debugger.Watchpoints.Clear();

                Log.Info("All watchpoints cleared");
            }
        }

        private sealed class WatchDeleteCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "delete", "remove" }; }
            }

            public override string Summary
            {
                get { return "Delete a watchpoint by ID."; }
            }

            public override string Syntax
            {
                get { return "watch|wp delete|remove <id>"; }
            }

            public override void Process(string args)
            {
                long num;

                if (!long.TryParse(args, out num))
                {
                    Log.Error("Invalid watchpoint ID");
                    return;
                }

                string expr;

                if (!Debugger.Watchpoints.TryGetValue(num, out expr))
                {
                    Log.Error("Watchpoint '{0}' not found", num);
                    return;
                }

                Debugger.Watchpoints.Remove(num);

                Log.Info("Watchpoint '{0}' (with expression '{1}') deleted", num, expr);
            }
        }

        private sealed class WatchListCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "list" }; }
            }

            public override string Summary
            {
                get { return "List all set watchpoints and their IDs."; }
            }

            public override string Syntax
            {
                get { return "watch|wp list"; }
            }

            public override void Process(string args)
            {
                var f = Debugger.ActiveFrame;

                if (f == null)
                {
                    Log.Error("No active stack frame");
                    return;
                }

                if (Debugger.Watchpoints.Count == 0)
                {
                    Log.Info("No watchpoints");
                    return;
                }

                foreach (var pair in Debugger.Watchpoints)
                {
                    ObjectValue obj = null;
                    string value;
                    bool error;

                    if (!f.ValidateExpression(pair.Value))
                    {
                        value = "Expression is invalid";
                        error = true;
                    }
                    else
                    {
                        obj = f.GetExpressionValue(pair.Value, Debugger.Options.EvaluationOptions);
                        var strErr = Utilities.StringizeValue(obj);

                        value = strErr.Item1;
                        error = strErr.Item2;
                    }

                    var prefix = string.Format("#{0} '{1}': ", pair.Key, pair.Value);

                    if (error)
                        Log.Error("{0}{1}", prefix, value);
                    else
                        Log.Info("{0}{1}{2}{3} it = {4}", prefix, Color.DarkGreen, obj.TypeName, Color.Reset, value);
                }
            }
        }

        public WatchCommand()
        {
            AddCommand<WatchAddCommand>();
            AddCommand<WatchClearCommand>();
            AddCommand<WatchDeleteCommand>();
            AddCommand<WatchListCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "watch", "wp" }; }
        }

        public override string Summary
        {
            get { return "Add, delete, and show watchpoints."; }
        }
    }
}
