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
        readonly Dictionary<Type, Command> _instantiations = new Dictionary<Type, Command>();

        readonly List<Tuple<string, Command>> _allCommands = new List<Tuple<string, Command>>();

        readonly List<Tuple<string, Command>> _commands = new List<Tuple<string, Command>>();

        Command _forwardTarget;

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

        Command Instantiate(Type type)
        {
            Command cmd;

            if (!_instantiations.TryGetValue(type, out cmd))
            {
                cmd = (Command)Activator.CreateInstance(type);

                _instantiations.Add(type, cmd);
            }

            return cmd;
        }

        protected void AddCommand(Command command)
        {
            foreach (var name in command.Names)
            {
                var tup = Tuple.Create(name, command);

                _allCommands.Add(tup);
                _commands.Add(tup);
            }
        }

        protected void AddCommand(Type type)
        {
            AddCommand(Instantiate(type));
        }

        protected void AddCommand<T>()
            where T : Command
        {
            AddCommand(typeof(T));
        }

        protected void AddCommandWithName(Command command, string name)
        {
            _allCommands.Add(Tuple.Create(name, command));
        }

        protected void AddCommandWithName(Type type, string name)
        {
            AddCommandWithName(Instantiate(type), name);
        }

        protected void AddCommandWithName<T>(string name)
            where T : Command
        {
            AddCommandWithName(typeof(T), name);
        }

        protected void Forward<T>()
        {
            _forwardTarget = Instantiate(typeof(T));
        }

        public Command GetCommand(string name)
        {
            foreach (var cmd in _allCommands)
                if (cmd.Item1.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                    return cmd.Item2;

            return null;
        }

        public override sealed void Process(string args)
        {
            var name = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();

            if (name != null)
            {
                var cmd = GetCommand(name);

                if (cmd != null)
                {
                    cmd.Process(new string(args.Skip(name.Length).ToArray()).Trim());
                    return;
                }
            }

            ProcessFallback(args, name != null);
        }

        protected virtual void ProcessFallback(string args, bool invalidSubCommand)
        {
            if (invalidSubCommand)
                Log.Error("Invalid sub-command given to '{0}'", Names[0]);
            else
            {
                if (_forwardTarget != null)
                    _forwardTarget.Process(args);
                else
                    Log.Error("No '{0}' sub-command specified", Names[0]);
            }
        }
    }
}
