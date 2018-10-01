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
using System.Linq;
using System.Threading;
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

            public override string Help
            {
                get
                {
                    return "Adds a watch with the given expression.\n" +
                           "\n" +
                           "Watches and their values can be shown with the 'watch list' command.";
                }
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

            public override string Help
            {
                get
                {
                    return "Clears all active watches.";
                }
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

            public override string Help
            {
                get
                {
                    return "Deletes the watch with the specified ID, if it exists.";
                }
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

            public override string Help
            {
                get
                {
                    return "Lists all watches, along with their IDs, expressions, and values.";
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

                if (Debugger.Watches.Count == 0)
                {
                    Log.Info("No watches");
                    return;
                }

                var vals = Debugger.Watches.Select(pair =>
                {
                    ObjectValue obj;
                    bool validated;

                    if (f.ValidateExpression(pair.Value))
                    {
                        obj = f.GetExpressionValue(pair.Value, Debugger.Options.EvaluationOptions);
                        validated = true;
                    }
                    else
                    {
                        obj = null;
                        validated = false;
                    }

                    return Tuple.Create(pair.Key, pair.Value, obj, validated);
                });

                // We do things somewhat awkwardly here so that we can
                // wait for all the values at once, which should display
                // everything faster overall.
                WaitHandle.WaitAll(vals.Where(x => x.Item4).Select(x => x.Item3.WaitHandle).ToArray());

                foreach (var val in vals)
                {
                    string value;
                    bool error;

                    if (val.Item4)
                    {
                        var strErr = Utilities.StringizeValue(val.Item3);

                        value = strErr.Item1;
                        error = strErr.Item2;
                    }
                    else
                    {
                        value = "Expression is invalid";
                        error = true;
                    }

                    var prefix = string.Format("#{0} '{1}': ", val.Item1, val.Item2);

                    if (error)
                        Log.Error("{0}{1}", prefix, value);
                    else
                        Log.Info("{0}{1}{2}{3} it = {4}", prefix, Color.DarkGreen,
                                 val.Item3.TypeName, Color.Reset, value);
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

        public override string Help
        {
            get
            {
                return "Manipulates watches.\n" +
                       "\n" +
                       "Watches are simply expressions that can be saved and queried at any point\n" +
                       "when the debuggee is paused. They are useful for easily observing global\n" +
                       "program state.";
            }
        }
    }
}
