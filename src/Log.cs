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

namespace Mono.Debugger.Client
{
    public static class Log
    {
        static readonly bool _debug = Environment.GetEnvironmentVariable("SDB_DEBUG") == "enable";

        public static object Lock { get; private set; }

        static Log()
        {
            Lock = new object();
        }

        static void Output(bool nl, string color, string format, object[] args)
        {
            var str = color + (args.Length == 0 ? format : string.Format(format, args)) + Color.Reset;

            lock (Lock)
            {
                if (nl)
                    Console.WriteLine(str);
                else
                    Console.Write(str);
            }
        }

        public static void InfoSameLine(string format, params object[] args)
        {
            Output(false, string.Empty, format, args);
        }

        public static void Info(string format, params object[] args)
        {
            Output(true, string.Empty, format, args);
        }

        public static void NoticeSameLine(string format, params object[] args)
        {
            Output(false, Color.DarkCyan, format, args);
        }

        public static void Notice(string format, params object[] args)
        {
            Output(true, Color.DarkCyan, format, args);
        }

        public static void EmphasisSameLine(string format, params object[] args)
        {
            Output(false, Color.DarkGreen, format, args);
        }

        public static void Emphasis(string format, params object[] args)
        {
            Output(true, Color.DarkGreen, format, args);
        }

        public static void ErrorSameLine(string format, params object[] args)
        {
            Output(false, Color.DarkRed, format, args);
        }

        public static void Error(string format, params object[] args)
        {
            Output(true, Color.DarkRed, format, args);
        }

        public static void DebugSameLine(string format, params object[] args)
        {
            if (_debug || Configuration.Current.DebugLogging)
                Output(false, Color.DarkYellow, format, args);
        }

        public static void Debug(string format, params object[] args)
        {
            if (_debug || Configuration.Current.DebugLogging)
                Output(true, Color.DarkYellow, format, args);
        }
    }
}
