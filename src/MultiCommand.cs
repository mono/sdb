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
using System.Linq;

namespace Mono.Debugger.Client
{
    public abstract class MultiCommand : Command
    {
        readonly List<Tuple<string, Command>> _commands = new List<Tuple<string, Command>>();

        public IEnumerable<Tuple<string, Command>> Commands
        {
            get { return _commands; }
        }

        public virtual string Parent
        {
            get { return null; }
        }

        public override string Syntax
        {
            get
            {
                var names = string.Join("|", Names);
                var subs = string.Join("|", _commands.Select(x => x.Item2).Distinct().Select(x => x.Names[0]));

                return string.Format("{0}{1} {2} ...", Parent != null ? Parent + " " : string.Empty, names, subs);
            }
        }

        protected void AddCommand(Command command)
        {
            foreach (var name in command.Names)
                _commands.Add(Tuple.Create(name, command));
        }

        protected void AddCommand(Type type)
        {
            var cmd = (Command)Activator.CreateInstance(type);

            AddCommand(cmd);
        }

        protected void AddCommand<T>()
            where T : Command
        {
            AddCommand(typeof(T));
        }

        public override sealed void Process(string args)
        {
            var name = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();
            var found = false;

            if (name != null)
            {
                foreach (var cmd in _commands)
                {
                    if (cmd.Item1.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        cmd.Item2.Process(new string(args.Skip(name.Length).ToArray()).Trim());
                        found = true;

                        break;
                    }
                }

                if (found)
                    return;
            }

            ProcessFallback(args, name != null);
        }

        protected virtual void ProcessFallback(string args, bool invalidSubCommand)
        {
            if (invalidSubCommand)
                Log.Error("Invalid sub-command given to '{0}'", Names[0]);
            else
                Log.Error("No '{0}' sub-command specified", Names[0]);
        }
    }
}
