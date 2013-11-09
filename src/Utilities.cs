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
using Mono.Debugging.Client;

namespace Mono.Debugger.Client
{
    public static class Utilities
    {
        public static string StringizeFrame(StackFrame frame, bool includeIndex)
        {
            var loc = string.Empty;

            if (frame.SourceLocation.FileName != null)
            {
                loc = " at " + frame.SourceLocation.FileName;

                if (frame.SourceLocation.Line != -1)
                    loc += ":" + frame.SourceLocation.Line;
            }

            var idx = includeIndex ? string.Format("#{0} ", frame.Index) : string.Empty;

            return string.Format("{0}[0x{1:X8}] {2}{3}", idx, frame.Address, frame.SourceLocation.MethodName, loc);
        }

        public static string StringizeThread(ThreadInfo thread, bool includeFrame)
        {
            var f = includeFrame ? thread.Backtrace.GetFrame(0) : null;

            var fstr = f == null ? string.Empty : Environment.NewLine + StringizeFrame(f, false);
            var tstr = string.Format("Thread #{0} '{1}'", thread.Id, thread.Name);

            return string.Format("{0}{1}", tstr, fstr);
        }

        public static Tuple<string, bool> StringizeValue(ObjectValue value)
        {
            string str;
            bool err;

            if (value.IsError)
            {
                str = value.DisplayValue;
                err = true;
            }
            else if (value.IsUnknown)
            {
                str = "Result is unrepresentable";
                err = true;
            }
            else
            {
                str = value.DisplayValue;
                err = false;
            }

            return Tuple.Create(str, err);
        }
    }
}
