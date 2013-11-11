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
using System.IO;
using System.Linq;
using Mono.Debugging.Client;

namespace Mono.Debugger.Client.Commands
{
    sealed class BreakpointCommand : MultiCommand
    {
        sealed class BreakpointAddCommand : MultiCommand
        {
            sealed class BreakpointAddLocationCommand : Command
            {
                public override string[] Names
                {
                    get { return new[] { "location", "at" }; }
                }

                public override string Summary
                {
                    get { return "Add a breakpoint at a source location."; }
                }

                public override string Syntax
                {
                    get { return "break|bp add location <file> <line>"; }
                }

                public override void Process(string args)
                {
                    var splitArgs = args.Split(' ').Where(x => x != string.Empty);
                    var count = splitArgs.Count();

                    if (count == 0)
                    {
                        Log.Error("No file name given");
                        return;
                    }

                    if (count == 1)
                    {
                        Log.Error("No line number given");
                        return;
                    }

                    var lineStr = splitArgs.Last();

                    int line;

                    if (!int.TryParse(lineStr, out line))
                    {
                        Log.Error("Invalid line number");
                        return;
                    }

                    var file = new string(args.Take(args.Length - lineStr.Length).ToArray()).Trim();

                    try
                    {
                        file = Path.GetFullPath(file);
                    }
                    catch (Exception ex)
                    {
                        Log.Info("Could not compute absolute path of '{0}':");
                        Log.Info(ex.ToString());

                        return;
                    }

                    foreach (var be in Debugger.Breakpoints)
                    {
                        var bp = be.Value as Breakpoint;

                        if (bp == null)
                            continue;

                        if (bp.FileName == file && bp.Line == line)
                        {
                            Log.Error("A breakpoint at '{0}:{1}' already exists ('{2}')", file, line, be.Key);
                            return;
                        }
                    }

                    var id = Debugger.GetBreakpointId();

                    Debugger.Breakpoints.Add(id, Debugger.BreakEvents.Add(file, line));

                    Log.Info("Breakpoint '{0}' added at '{1}:{2}'", id, file, line);
                }
            }

            sealed class BreakpointAddMethodCommand : Command
            {
                public override string[] Names
                {
                    get { return new[] { "method", "function" }; }
                }

                public override string Summary
                {
                    get { return "Add a breakpoint at a method."; }
                }

                public override string Syntax
                {
                    get { return "break|bp add method|function <name>"; }
                }

                public override void Process(string args)
                {
                    if (args.Length == 0)
                    {
                        Log.Error("No method name given");
                        return;
                    }

                    foreach (var be in Debugger.Breakpoints)
                    {
                        if (!(be.Value is FunctionBreakpoint))
                            continue;

                        if (((FunctionBreakpoint)be.Value).FunctionName == args)
                        {
                            Log.Error("A method breakpoint for '{0}' already exists ('{1}')", args, be.Key);
                            return;
                        }
                    }

                    // TODO: Parameter types too.

                    var id = Debugger.GetBreakpointId();
                    var fbp = new FunctionBreakpoint(args, "C#");

                    Debugger.Breakpoints.Add(id, fbp);
                    Debugger.BreakEvents.Add(fbp);

                    Log.Info("Breakpoint '{0}' added for method '{1}'", id, args);
                }
            }

            public BreakpointAddCommand()
            {
                AddCommand<BreakpointAddLocationCommand>();
                AddCommand<BreakpointAddMethodCommand>();
            }

            public override string[] Names
            {
                get { return new[] { "add" }; }
            }

            public override string Summary
            {
                get { return "Add a breakpoint at a location or method."; }
            }

            public override string Parent
            {
                get { return "break"; }
            }
        }

        sealed class BreakpointClearCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "clear" }; }
            }

            public override string Summary
            {
                get { return "Clear all breakpoints."; }
            }

            public override string Syntax
            {
                get { return "break|bp clear"; }
            }

            public override void Process(string args)
            {
                Debugger.Breakpoints.Clear();
                Debugger.BreakEvents.ClearBreakpoints();

                Log.Info("All breakpoints cleared");
            }
        }

        sealed class BreakpointConditionCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "condition", "expression" }; }
            }

            public override string Summary
            {
                get { return "Set a conditional expression for a breakpoint."; }
            }

            public override string Syntax
            {
                get { return "break|bp condition|expression <id> [expr]"; }
            }

            public override void Process(string args)
            {
                var id = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();

                if (id == null)
                {
                    Log.Error("No breakpoint ID given");
                    return;
                }

                long num;

                if (!long.TryParse(id, out num))
                {
                    Log.Error("Invalid breakpoint ID");
                    return;
                }

                BreakEvent b;

                if (!Debugger.Breakpoints.TryGetValue(num, out b))
                {
                    Log.Error("Breakpoint '{0}' not found", num);
                    return;
                }

                if (b is FunctionBreakpoint)
                {
                    Log.Error("Breakpoint '{0}' is a method breakpoint", num);
                    return;
                }

                var expr = new string(args.Skip(id.Length).ToArray()).Trim();

                var bp = (Breakpoint)b;
                var was = bp.ConditionExpression != null ?
                          string.Format(" (was '{0}')", bp.ConditionExpression) :
                          string.Empty;

                if (expr.Length == 0)
                {
                    bp.ConditionExpression = null;

                    Log.Info("Condition for breakpoint '{0}' unset{1}", num, was);
                }
                else
                {
                    bp.ConditionExpression = expr;

                    Log.Info("Condition for breakpoint '{0}' set to '{1}'{2}", num, expr, was);
                }
            }
        }

        sealed class BreakpointDeleteCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "delete", "remove" }; }
            }

            public override string Summary
            {
                get { return "Delete a breakpoint by ID."; }
            }

            public override string Syntax
            {
                get { return "break|bp delete|remove <id>"; }
            }

            public override void Process(string args)
            {
                long num;

                if (!long.TryParse(args, out num))
                {
                    Log.Error("Invalid breakpoint ID");
                    return;
                }

                BreakEvent b;

                if (!Debugger.Breakpoints.TryGetValue(num, out b))
                {
                    Log.Error("Breakpoint '{0}' not found", num);
                    return;
                }

                Debugger.Breakpoints.Remove(num);
                Debugger.BreakEvents.Remove(b);

                Log.Info("Breakpoint '{0}' deleted", num);
            }
        }

        sealed class BreakpointListCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "list" }; }
            }

            public override string Summary
            {
                get { return "List all set breakpoints and their IDs."; }
            }

            public override string Syntax
            {
                get { return "break|bp list"; }
            }

            public override void Process(string args)
            {
                if (Debugger.Breakpoints.Count == 0)
                {
                    Log.Info("No breakpoints");
                    return;
                }

                foreach (var pair in Debugger.Breakpoints)
                {
                    var bp = pair.Value as Breakpoint;
                    var fbp = pair.Value as FunctionBreakpoint;

                    var at = fbp != null ? fbp.FunctionName : string.Format("{0}:{1}", bp.FileName, bp.Line);
                    var expr = bp != null && bp.ConditionExpression != null ?
                               string.Format(" '{0}'", bp.ConditionExpression) :
                               string.Empty;

                    // TODO: Parameter types too.

                    Log.Info("#{0} '{1}'{2}", pair.Key, at, expr);
                }
            }
        }

        sealed class BreakpointToggleCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "toggle" }; }
            }

            public override string Summary
            {
                get { return "Toggle a breakpoint on/off."; }
            }

            public override string Syntax
            {
                get { return "break|bp toggle <id>"; }
            }

            public override void Process(string args)
            {
                long num;

                if (!long.TryParse(args, out num))
                {
                    Log.Error("Invalid breakpoint ID");
                    return;
                }

                BreakEvent b;

                if (!Debugger.Breakpoints.TryGetValue(num, out b))
                {
                    Log.Error("Breakpoint '{0}' not found", num);
                    return;
                }

                if (Debugger.BreakEvents.Contains(b))
                {
                    Debugger.BreakEvents.Remove(b);

                    Log.Info("Breakpoint '{0}' disabled", num);
                }
                else
                {
                    Debugger.BreakEvents.Add(b);

                    Log.Info("Breakpoint '{0}' enabled", num);
                }
            }
        }

        public BreakpointCommand()
        {
            AddCommand<BreakpointAddCommand>();
            AddCommand<BreakpointClearCommand>();
            AddCommand<BreakpointConditionCommand>();
            AddCommand<BreakpointDeleteCommand>();
            AddCommand<BreakpointListCommand>();
            AddCommand<BreakpointToggleCommand>();

            Forward<BreakpointListCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "break", "bp" }; }
        }

        public override string Summary
        {
            get { return "Add, delete, and show breakpoints."; }
        }
    }
}
