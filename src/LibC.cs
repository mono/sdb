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
using System.Runtime.InteropServices;

namespace Mono.Debugger.Client
{
    static class LibC
    {
        // These values are correct for Linux, OS X, and FreeBSD. Might
        // need to be reviewed for other platforms.

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SignalHandler(int signal);

        public static readonly IntPtr ErrorSignal = new IntPtr(-1);

        public static readonly IntPtr IgnoreSignal = new IntPtr(1);

        public static readonly IntPtr DefaultSignal = IntPtr.Zero;

        public const int SignalInterrupt = 2;

        [DllImport("libc", EntryPoint = "signal")]
        public static extern IntPtr SetSignal(int signal, IntPtr handler);
    }
}
