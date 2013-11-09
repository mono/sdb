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

namespace Mono.Debugger.Client
{
    public static class Color
    {
        // We need this class because the color sequences emitted by
        // the ForegroundColor, BackgroundColor, and ResetColor helpers
        // on System.Console mess with libreadline's input and history
        // tracker.

        static readonly bool _disableColors;

        static Color()
        {
            _disableColors = Console.IsOutputRedirected ||
                             new[] { null, "dumb" }.Contains(Environment.GetEnvironmentVariable("TERM")) ||
                             Environment.GetEnvironmentVariable("SDB_COLORS") == "disable" ||
                             Configuration.Current.DisableColors;
        }

        static string GetColor(string modifier, string color)
        {
            return _disableColors ? string.Empty : modifier + color;
        }

        public static string Red
        {
            get { return GetColor("\x1b[1m", "\x1b[31m"); }
        }

        public static string DarkRed
        {
            get { return GetColor(string.Empty, "\x1b[31m"); }
        }

        public static string Green
        {
            get { return GetColor("\x1b[1m", "\x1b[32m"); }
        }

        public static string DarkGreen
        {
            get { return GetColor(string.Empty, "\x1b[32m"); }
        }

        public static string Yellow
        {
            get { return GetColor("\x1b[1m", "\x1b[33m"); }
        }

        public static string DarkYellow
        {
            get { return GetColor(string.Empty, "\x1b[33m"); }
        }

        public static string Blue
        {
            get { return GetColor("\x1b[1m", "\x1b[34m"); }
        }

        public static string DarkBlue
        {
            get { return GetColor(string.Empty, "\x1b[34m"); }
        }

        public static string Magenta
        {
            get { return GetColor("\x1b[1m", "\x1b[35m"); }
        }

        public static string DarkMagenta
        {
            get { return GetColor(string.Empty, "\x1b[35m"); }
        }

        public static string Cyan
        {
            get { return GetColor("\x1b[1m", "\x1b[36m"); }
        }

        public static string DarkCyan
        {
            get { return GetColor(string.Empty, "\x1b[36m"); }
        }

        public static string Reset
        {
            get { return GetColor("\x1b[0m", string.Empty); }
        }
    }
}
