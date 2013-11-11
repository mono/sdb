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

using System.IO;

namespace Mono.Debugger.Client.Commands
{
    sealed class PluginCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "plugin", "addin" }; }
        }

        public override string Summary
        {
            get { return "Load a plugin assembly."; }
        }

        public override string Syntax
        {
            get { return "plugin|addin <file>"; }
        }

        public override string Help
        {
            get
            {
                return "Attempts to load the given plugin assembly. All types that are tagged with\n" +
                       "the '[Command]' attribute will be instantiated and made available.";
            }
        }

        public override void Process(string args)
        {
            if (!File.Exists(args))
            {
                Log.Error("Plugin assembly '{0}' does not exist");
                return;
            }

            CommandLine.Root.AddCommands(Plugins.Load(args));
        }
    }
}
