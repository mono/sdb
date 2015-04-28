//
// The MIT License (MIT)
//
// Copyright (c) 2015 Alex Rønne Petersen
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
