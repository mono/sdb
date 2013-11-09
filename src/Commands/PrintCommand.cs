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

namespace Mono.Debugger.Client.Commands
{
    sealed class PrintCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "print", "evaluate" }; }
        }

        public override string Summary
        {
            get { return "Print (evaluate) a given expression."; }
        }

        public override string Syntax
        {
            get { return "print|evaluate <expr>"; }
        }

        public override void Process(string args)
        {
            var f = Debugger.ActiveFrame;

            if (f == null)
            {
                Log.Error("No active stack frame");
                return;
            }

            if (args.Length == 0)
            {
                Log.Error("No expression given");
                return;
            }

            if (!f.ValidateExpression(args))
            {
                Log.Error("Expression '{0}' is invalid", args);
                return;
            }

            var val = f.GetExpressionValue(args, Debugger.Options.EvaluationOptions);
            var strErr = Utilities.StringizeValue(val);

            if (strErr.Item2)
            {
                Log.Error(strErr.Item1);
                return;
            }

            Log.Info("{0}{1}{2} it = {3}", Color.DarkGreen, val.TypeName, Color.Reset, strErr.Item1);
        }
    }
}
