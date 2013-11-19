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

using System.Collections.Generic;

namespace Mono.Debugger.Client.Commands
{
    sealed class StepCommand : MultiCommand
    {
        sealed class StepOverCommand : MultiCommand
        {
            sealed class StepOverLineCommand : Command
            {
                public override string[] Names
                {
                    get { return new[] { "line" }; }
                }

                public override string Summary
                {
                    get { return "Step over a line."; }
                }

                public override string Syntax
                {
                    get { return "step over line"; }
                }

                public override string Help
                {
                    get
                    {
                        return "Steps over a line.";
                    }
                }

                public override void Process(string args)
                {
                    if (Debugger.State == State.Suspended)
                        Debugger.StepOverLine();
                    else
                        Log.Error("No suspended inferior process");
                }
            }

            sealed class StepOverInstructionCommand : Command
            {
                public override string[] Names
                {
                    get { return new[] { "instruction", "insn" }; }
                }

                public override string Summary
                {
                    get { return "Step over an instruction."; }
                }

                public override string Syntax
                {
                    get { return "step over instruction|insn"; }
                }

                public override string Help
                {
                    get
                    {
                        return "Steps over an instruction.";
                    }
                }

                public override void Process(string args)
                {
                    if (Debugger.State == State.Suspended)
                        Debugger.StepOverInstruction();
                    else
                        Log.Error("No suspended inferior process");
                }
            }

            public StepOverCommand()
            {
                AddCommand<StepOverLineCommand>();
                AddCommand<StepOverInstructionCommand>();

                Forward<StepOverLineCommand>();
            }

            public override string[] Names
            {
                get { return new[] { "over" }; }
            }

            public override string Summary
            {
                get { return "Step over a line or an instruction."; }
            }

            public override string Help
            {
                get
                {
                    return "Steps over a line or an instruction.\n" +
                           "\n" +
                           "This executes the line or instruction that's next in the source code or\n" +
                           "IL stream. If the line or instruction contains a call, the debugger\n" +
                           "will treat it as a single unit, as opposed to 'step into'.";
                }
            }

            public override string Parent
            {
                get { return "step"; }
            }
        }

        sealed class StepIntoCommand : MultiCommand
        {
            sealed class StepIntoLineCommand : Command
            {
                public override string[] Names
                {
                    get { return new[] { "line" }; }
                }

                public override string Summary
                {
                    get { return "Step into a line."; }
                }

                public override string Syntax
                {
                    get { return "step into line"; }
                }

                public override string Help
                {
                    get
                    {
                        return "Steps into a line.";
                    }
                }

                public override void Process(string args)
                {
                    if (Debugger.State == State.Suspended)
                        Debugger.StepIntoLine();
                    else
                        Log.Error("No suspended inferior process");
                }
            }

            sealed class StepIntoInstructionCommand : Command
            {
                public override string[] Names
                {
                    get { return new[] { "instruction", "insn" }; }
                }

                public override string Summary
                {
                    get { return "Step into an instruction."; }
                }

                public override string Syntax
                {
                    get { return "step into instruction|insn"; }
                }

                public override string Help
                {
                    get
                    {
                        return "Steps into an instruction.";
                    }
                }

                public override void Process(string args)
                {
                    if (Debugger.State == State.Suspended)
                        Debugger.StepIntoInstruction();
                    else
                        Log.Error("No suspended inferior process");
                }
            }

            public StepIntoCommand()
            {
                AddCommand<StepIntoLineCommand>();
                AddCommand<StepIntoInstructionCommand>();

                Forward<StepIntoLineCommand>();
            }

            public override string[] Names
            {
                get { return new[] { "into" }; }
            }

            public override string Summary
            {
                get { return "Step into a line or an instruction."; }
            }

            public override string Help
            {
                get
                {
                    return "Steps into a line or an instruction.\n" +
                           "\n" +
                           "This executes the line or instruction that's next in the source code or\n" +
                           "IL stream. If the line or instruction contains a call, the debugger\n" +
                           "will pause at the first line or instruction inside the callee.";
                }
            }

            public override string Parent
            {
                get { return "step"; }
            }
        }

        sealed class StepOutCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "out" }; }
            }

            public override string Summary
            {
                get { return "Step out of the current method."; }
            }

            public override string Syntax
            {
                get { return "step out"; }
            }

            public override string Help
            {
                get
                {
                    return "Steps out of the current method.\n" +
                           "\n" +
                           "This resumes the debuggee until it exits the method it's currently in.";
                }
            }

            public override void Process(string args)
            {
                if (Debugger.State == State.Suspended)
                    Debugger.StepOutOfMethod();
                else
                    Log.Error("No suspended inferior process");
            }
        }

        public StepCommand()
        {
            AddCommandWithName<StepOverCommand>("o");
            AddCommand<StepOverCommand>();
            AddCommand<StepIntoCommand>();
            AddCommand<StepOutCommand>();

            Forward<StepOverCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "step" }; }
        }

        public override string Summary
        {
            get { return "Single-step through lines/instructions/methods."; }
        }

        public override string Help
        {
            get
            {
                return "Single-steps through lines, instructions, and methods.";
            }
        }
    }
}
