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

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Debugger.Client.Commands;

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
                string aliasee;

                // If it's an alias, we need to run this method anew.
                if (this is RootCommand && Debugger.Aliases.TryGetValue(name, out aliasee))
                {
                    Process(string.Format(aliasee, args));
                    return;
                }

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
            else if (_forwardTarget == null)
                Log.Error("No '{0}' sub-command specified", Names[0]);
            else
                _forwardTarget.Process(args);
        }
    }
}
