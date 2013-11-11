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
    sealed class RootCommand : MultiCommand
    {
        public RootCommand()
        {
            AddCommandWithName<PrintCommand>("p");
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
            AddCommand<EnvironmentCommand>();
            AddCommand<FrameCommand>();
            AddCommand<HelpCommand>();
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
