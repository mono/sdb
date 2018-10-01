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

namespace Mono.Debugger.Client.Commands
{
    sealed class JumpCommand : MultiCommand
    {
        sealed class JumpInstructionCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "instruction", "insn" }; }
            }

            public override string Summary
            {
                get { return "Set next instruction to execute."; }
            }

            public override string Syntax
            {
                get { return "jump|next instruction|insn <offset>"; }
            }

            public override string Help
            {
                get
                {
                    return "Sets the next instruction to be executed." +
                           "\n" +
                           "The argument is the IL offset to jump to from the base of the method.";
                }
            }

            public override void Process(string args)
            {
                if (Debugger.State != State.Suspended)
                {
                    Log.Error("No suspended inferior process");
                    return;
                }

                if (Debugger.ActiveFrame == null)
                {
                    Log.Error("No active stack frame");
                    return;
                }

                int num;

                if (!int.TryParse(args, out num) || num < 0)
                {
                    Log.Error("Invalid IL offset");
                    return;
                }

                Debugger.SetInstruction(num);

                Log.Info("Will execute at IL offset '{0}'", num);
            }
        }

        sealed class JumpLineCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "line" }; }
            }

            public override string Summary
            {
                get { return "Set next line to execute."; }
            }

            public override string Syntax
            {
                get { return "jump|next line <line>"; }
            }

            public override string Help
            {
                get
                {
                    return "Sets the next line to be executed.";
                }
            }

            public override void Process(string args)
            {
                if (Debugger.State != State.Suspended)
                {
                    Log.Error("No suspended inferior process");
                    return;
                }

                var f = Debugger.ActiveFrame;

                if (f == null)
                {
                    Log.Error("No active stack frame");
                    return;
                }

                var file = f.SourceLocation.FileName;

                if (file == null)
                {
                    Log.Error("No source information available");
                    return;
                }

                int num;

                if (!int.TryParse(args, out num) || num < 1)
                {
                    Log.Error("Invalid line number");
                    return;
                }

                Debugger.SetLine(file, num);

                Log.Info("Will execute at '{0}:{1}'", file, num);
            }
        }

        public JumpCommand()
        {
            AddCommand<JumpLineCommand>();
            AddCommand<JumpInstructionCommand>();

            Forward<JumpLineCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "jump", "next" }; }
        }

        public override string Summary
        {
            get { return "Set the next line or instruction to execute."; }
        }

        public override string Help
        {
            get
            {
                return "Allows setting the next line or instruction to execute.\n" +
                       "\n" +
                       "Note that jumps can only be made to sequence points. A location is a\n" +
                       "sequence point if one of these conditions is true:\n" +
                       "\n" +
                       "* The IL stack is empty.\n" +
                       "* The location contains a 'nop' instruction.\n" +
                       "* There is a corresponding line number entry in the '.mdb' file.";
            }
        }
    }
}
