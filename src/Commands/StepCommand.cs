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
    sealed class StepCommand : MultiCommand
    {
        private sealed class StepOverCommand : MultiCommand
        {
            private sealed class StepOverLineCommand : Command
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

                public override void Process(string args)
                {
                    if (Debugger.State == State.Suspended)
                        Debugger.StepOverLine();
                    else
                        Log.Error("No suspended inferior process");
                }
            }

            private sealed class StepOverInstructionCommand : Command
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
            }

            public override string[] Names
            {
                get { return new[] { "over" }; }
            }

            public override string Summary
            {
                get { return "Step over a line or an instruction."; }
            }

            public override string Parent
            {
                get { return "step"; }
            }
        }

        private sealed class StepIntoCommand : MultiCommand
        {
            private sealed class StepIntoLineCommand : Command
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

                public override void Process(string args)
                {
                    if (Debugger.State == State.Suspended)
                        Debugger.StepIntoLine();
                    else
                        Log.Error("No suspended inferior process");
                }
            }

            private sealed class StepIntoInstructionCommand : Command
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
            }

            public override string[] Names
            {
                get { return new[] { "into" }; }
            }

            public override string Summary
            {
                get { return "Step into a line or an instruction."; }
            }

            public override string Parent
            {
                get { return "step"; }
            }
        }

        private sealed class StepOutCommand : Command
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
            AddCommand<StepOverCommand>();
            AddCommand<StepIntoCommand>();
            AddCommand<StepOutCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "step" }; }
        }

        public override string Summary
        {
            get { return "Single-step through lines/instructions/methods."; }
        }
    }
}
