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
        sealed class WatchAddCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "add" }; }
            }

            public override string Summary
            {
                get { return "Add a watch expression."; }
            }

            public override string Syntax
            {
                get { return "watch add <expr>"; }
            }

            public override void Process(string args)
            {
                var id = Debugger.GetWatchId();

                Debugger.Watches.Add(id, args);

                Log.Info("Added watch '{0}' with expression '{1}'", id, args);
            }
        }

        sealed class WatchClearCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "clear" }; }
            }

            public override string Summary
            {
                get { return "Delete all watches."; }
            }

            public override string Syntax
            {
                get { return "watch clear"; }
            }

            public override void Process(string args)
            {
                Debugger.Watches.Clear();

                Log.Info("All watches cleared");
            }
        }

        sealed class WatchDeleteCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "delete", "remove" }; }
            }

            public override string Summary
            {
                get { return "Delete a watch by ID."; }
            }

            public override string Syntax
            {
                get { return "watch delete|remove <id>"; }
            }

            public override void Process(string args)
            {
                long num;

                if (!long.TryParse(args, out num))
                {
                    Log.Error("Invalid watch ID");
                    return;
                }

                string expr;

                if (!Debugger.Watches.TryGetValue(num, out expr))
                {
                    Log.Error("Watch '{0}' not found", num);
                    return;
                }

                Debugger.Watches.Remove(num);

                Log.Info("Watch '{0}' (with expression '{1}') deleted", num, expr);
            }
        }

        sealed class WatchListCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "list" }; }
            }

            public override string Summary
            {
                get { return "List all set watches and their IDs."; }
            }

            public override string Syntax
            {
                get { return "watch list"; }
            }

            public override void Process(string args)
            {
                var f = Debugger.ActiveFrame;

                if (f == null)
                {
                    Log.Error("No active stack frame");
                    return;
                }

                if (Debugger.Watches.Count == 0)
                {
                    Log.Info("No watches");
                    return;
                }

                foreach (var pair in Debugger.Watches)
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
                        obj.WaitHandle.WaitOne();

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

            Forward<WatchListCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "watch" }; }
        }

        public override string Summary
        {
            get { return "Add, delete, and show watches."; }
        }
    }
}
