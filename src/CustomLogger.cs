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
    sealed class CustomLogger : ICustomLogger
    {
        public void LogError(string message, Exception ex)
        {
            Log.Error(message);

            if (ex != null)
                Log.Error(ex.ToString());
        }

        public void LogAndShowException(string message, Exception ex)
        {
            LogError(message, ex);
        }

        public void LogMessage(string format, params object[] args)
        {
            Log.Info(format, args);
        }
    }
}
