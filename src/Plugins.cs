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
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mono.Debugger.Client
{
    static class Plugins
    {
        static string GetFolderPath()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return Path.Combine(home, ".sdb");
        }

        public static IEnumerable<Command> Load(string file)
        {
            Log.Debug("Attempting to load plugin '{0}'", file);

            Assembly asm;

            try
            {
                asm = Assembly.LoadFile(file);
            }
            catch (Exception ex)
            {
                Log.Error("Could not load plugin '{0}':", file);
                Log.Error(ex.ToString());

                yield break;
            }

            Type[] types;

            try
            {
                types = asm.GetTypes();
            }
            catch (Exception ex)
            {
                Log.Error("Could not load types from plugin '{0}':", file);
                Log.Error(ex.ToString());

                yield break;
            }

            foreach (var type in types)
            {
                Command cmd = null;

                try
                {
                    if (type.IsDefined(typeof(CommandAttribute), false))
                        cmd = (Command)Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    Log.Error("Could not load command type '{0}':", type);
                    Log.Error(ex.ToString());
                }

                if (cmd != null)
                    yield return cmd;
            }
        }

        public static IEnumerable<Command> LoadDirectory(string path)
        {
            try
            {
                return Directory.EnumerateFiles(path, "*.dll").Select(x => Load(x)).SelectMany(x => x);
            }
            catch (Exception ex)
            {
                Log.Error("Could not iterate plugin directory '{0}':", path);
                Log.Error(ex.ToString());
            }

            return Enumerable.Empty<Command>();
        }

        public static IEnumerable<Command> LoadDefault()
        {
            var env = Environment.GetEnvironmentVariable("SDB_PATH") ?? string.Empty;
            var paths = new[] { GetFolderPath() }.Concat(env.Split(Path.PathSeparator).Where(x => x != string.Empty));
            var list = new List<Command>();

            foreach (var p in paths)
                if (Directory.Exists(p))
                    list.AddRange(LoadDirectory(p));

            return list;
        }
    }
}
