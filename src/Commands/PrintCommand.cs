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

        public override string Help
        {
            get
            {
                return "Evaluates the given expression in the context of the active stack frame.\n" +
                       "The expression has access to any local variables and method arguments that\n" +
                       "are in scope.";
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
            val.WaitHandle.WaitOne();

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
