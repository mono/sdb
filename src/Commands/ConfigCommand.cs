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
using System.Linq;
using System.Reflection;

namespace Mono.Debugger.Client.Commands
{
    sealed class ConfigCommand : MultiCommand
    {
        sealed class ConfigGetCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "get" }; }
            }

            public override string Summary
            {
                get { return "Get the value of a configuration element."; }
            }

            public override string Syntax
            {
                get { return "config|cfg get <name>"; }
            }

            public override string Help
            {
                get
                {
                    return "Gets the value of the given configuration element.\n" +
                           "\n" +
                           "This is either the default value or the value in '~/.sdb.cfg'.";
                }
            }

            public override void Process(string args)
            {
                var name = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();

                if (name == null)
                {
                    Log.Error("No configuration element name given");
                    return;
                }

                var props = typeof(Configuration).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in props)
                {
                    if (prop.Name.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Log.Info("'{0}' = '{1}'", prop.Name, prop.GetValue(Configuration.Current));
                        return;
                    }
                }

                Log.Error("Configuration element '{0}' not found", name);
            }
        }

        sealed class ConfigListCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "list" }; }
            }

            public override string Summary
            {
                get { return "List all configuration elements and their values."; }
            }

            public override string Syntax
            {
                get { return "config|cfg list"; }
            }

            public override string Help
            {
                get
                {
                    return "Lists all configuration elements and their values.\n" +
                           "\n" +
                           "These are either the default values or the values in '~/.sdb.cfg'.";
                }
            }

            public override void Process(string args)
            {
                var props = typeof(Configuration).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in props)
                    Log.Info("'{0}' = '{1}'", prop.Name, prop.GetValue(Configuration.Current));
            }
        }

        sealed class ConfigResetCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "reset" }; }
            }

            public override string Summary
            {
                get { return "Reset configuration values to their defaults."; }
            }

            public override string Syntax
            {
                get { return "config|cfg reset"; }
            }

            public override string Help
            {
                get
                {
                    return "Resets all configuration elements to their default values.\n" +
                           "\n" +
                           "Note that this overwrites '~/.sdb.cfg' too.";
                }
            }

            public override void Process(string args)
            {
                Configuration.Defaults();

                Log.Info("All configuration values reset");
            }
        }

        sealed class ConfigSetCommand : Command
        {
            public override string[] Names
            {
                get { return new[] { "set" }; }
            }

            public override string Summary
            {
                get { return "Set the value of a configuration element."; }
            }

            public override string Syntax
            {
                get { return "config|cfg set <name> <value>"; }
            }

            public override string Help
            {
                get
                {
                    return "Sets the value of the given configuration element.\n" +
                           "\n" +
                           "If '~/.sdb.cfg' doesn't exist, this command causes it to be created.";
                }
            }

            public override void Process(string args)
            {
                var name = args.Split(' ').Where(x => x != string.Empty).FirstOrDefault();

                if (name == null)
                {
                    Log.Error("No configuration element name given");
                    return;
                }

                var rest = new string(args.Skip(name.Length).ToArray()).Trim();

                if (rest.Length == 0)
                {
                    Log.Error("No configuration value given");
                    return;
                }

                var props = typeof(Configuration).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in props)
                {
                    if (prop.Name.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var was = prop.GetValue(Configuration.Current);

                        object value = null;

                        try
                        {
                            if (prop.PropertyType == typeof(bool))
                                value = bool.Parse(rest);
                            else if (prop.PropertyType == typeof(int))
                                value = int.Parse(rest);
                            else if (prop.PropertyType == typeof(string))
                                value = rest;
                        }
                        catch (Exception ex)
                        {
                            if (ex is FormatException || ex is OverflowException)
                            {
                                Log.Error("Invalid configuration value");
                                return;
                            }

                            throw;
                        }

                        prop.SetValue(Configuration.Current, value);

                        Configuration.Write();
                        Configuration.Apply();

                        Log.Info("'{0}' = '{1}' (was '{2}')", prop.Name, value, was);

                        return;
                    }
                }

                Log.Error("Configuration element '{0}' not found", name);
            }
        }

        public ConfigCommand()
        {
            AddCommand<ConfigGetCommand>();
            AddCommand<ConfigListCommand>();
            AddCommand<ConfigResetCommand>();
            AddCommand<ConfigSetCommand>();

            Forward<ConfigListCommand>();
        }

        public override string[] Names
        {
            get { return new[] { "config", "cfg" }; }
        }

        public override string Summary
        {
            get { return "Manipulate the debugger configuration."; }
        }

        public override string Help
        {
            get
            {
                return "Manipulates the debugger configuration.\n" +
                       "\n" +
                       "Configuration values are stored in '~/.sdb.cfg' and are loaded on debugger\n" +
                       "startup. The file is only created when a configuration value is set.";
            }
        }
    }
}
