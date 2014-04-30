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

using System.Collections.Generic;

namespace Mono.Debugger.Client.Commands
{
    sealed class RootCommand : MultiCommand
    {
        public RootCommand()
        {
            AddCommandWithName<PrintCommand>("p");
            AddCommand<AliasCommand>();
            AddCommand<ArgumentsCommand>();
            AddCommand<AttachCommand>();
            AddCommandWithName<BreakpointCommand>("b");
            AddCommand<BacktraceCommand>();
            AddCommand<BreakpointCommand>();
            AddCommandWithName<ContinueCommand>("c");
            AddCommand<CatchpointCommand>();
            AddCommand<ConfigCommand>();
            AddCommandWithName<RunCommand>("r");
            AddCommand<ConnectCommand>();
            AddCommand<ContinueCommand>();
            AddCommand<DatabaseCommand>();
            AddCommand<DecompileCommand>();
            AddCommand<DirectoryCommand>();
            AddCommand<DisassembleCommand>();
            AddCommand<DoCommand>();
            AddCommand<EnvironmentCommand>();
            AddCommand<FrameCommand>();
            AddCommand<HelpCommand>();
            AddCommand<JumpCommand>();
            AddCommand<KillCommand>();
            AddCommand<ListenCommand>();
            AddCommand<PluginCommand>();
            AddCommandWithName<PrintCommand>("p");
            AddCommand<PrintCommand>();
            AddCommand<QuitCommand>();
            AddCommandWithName<RunCommand>("r");
            AddCommand<ResetCommand>();
            AddCommand<RunCommand>();
            AddCommandWithName<StepCommand>("s");
            AddCommand<SourceCommand>();
            AddCommand<StepCommand>();
            AddCommand<ThreadCommand>();
            AddCommand<WatchCommand>();
        }

        public void AddCommands(IEnumerable<Command> commands)
        {
            foreach (var cmd in commands)
                AddCommand(cmd);
        }

        public override string[] Names
        {
            get { return new[] { "root" }; }
        }

        public override string Summary
        {
            get { return "N/A"; }
        }

        public override string Syntax
        {
            get { return "N/A"; }
        }

        public override string Help
        {
            get { return "All commands (and sub-commands) can be abbreviated.\n" +
                         "For example, 'exe' means 'execute' and 'co s' means 'config set'."; }
        }

        protected override void ProcessFallback(string args, bool invalidSubCommand)
        {
            Log.Error("'{0}' is not a known command", args);
        }
    }
}
