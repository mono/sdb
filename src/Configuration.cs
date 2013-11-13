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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Mono.Debugging.Client;

namespace Mono.Debugger.Client
{
    [Serializable]
    public sealed class Configuration
    {
        public bool AllowMethodEvaluation { get; set; }

        public bool AllowTargetInvoke { get; set; }

        public bool AllowToStringCalls { get; set; }

        public bool ChunkRawStrings { get; set; }

        public int ConnectionAttemptInterval { get; set; }

        public bool DebugLogging { get; set; }

        public bool DisableColors { get; set; }

        public bool EllipsizeStrings { get; set; }

        public int EllipsizeThreshold { get; set; }

        public bool EnableControlC { get; set; }

        public int EvaluationTimeout { get; set; }

        public string ExceptionIdentifier { get; set; }

        public bool FlattenHierarchy { get; set; }

        public bool HexadecimalIntegers { get; set; }

        public string InputPrompt { get; set; }

        public bool LogInternalErrors { get; set; }

        public bool LogRuntimeSpew { get; set; }

        public int MaxConnectionAttempts { get; set; }

        public int MemberEvaluationTimeout { get; set; }

        public string RuntimePrefix { get; set; }

        public bool StepOverPropertiesAndOperators { get; set; }

        public static Configuration Current { get; private set; }

        static Configuration()
        {
            Current = new Configuration();
        }

        static string GetFilePath()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return Path.Combine(home, ".sdb.cfg");
        }

        public static void Write()
        {
            var file = GetFilePath();

            try
            {
                using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write))
                    new BinaryFormatter().Serialize(stream, Current);
            }
            catch (Exception ex)
            {
                Log.Error("Could not write configuration file '{0}':", file);
                Log.Error(ex.ToString());
            }
        }

        public static void Read()
        {
            var file = GetFilePath();

            try
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    Current = (Configuration)new BinaryFormatter().Deserialize(stream);
            }
            catch (Exception ex)
            {
                // If it's an FNFE, chances are the file just
                // hasn't been written yet.
                if (!(ex is FileNotFoundException))
                {
                    Log.Error("Could not read configuration file '{0}':", file);
                    Log.Error(ex.ToString());
                }

                // Set some sane defaults...
                Defaults();
            }
        }

        public static void Defaults()
        {
            // Cute hack to set all properties to their default values.
            foreach (var prop in typeof(Configuration).GetProperties(BindingFlags.Public |
                                                                     BindingFlags.Instance))
                prop.SetValue(Configuration.Current, null);

            Current.AllowMethodEvaluation = true;
            Current.AllowTargetInvoke = true;
            Current.AllowToStringCalls = true;
            Current.ConnectionAttemptInterval = 500;
            Current.EllipsizeStrings = true;
            Current.EllipsizeThreshold = 100;
            Current.EnableControlC = true;
            Current.EvaluationTimeout = 1000;
            Current.ExceptionIdentifier = "$exception";
            Current.FlattenHierarchy = true;
            Current.InputPrompt = "(sdb)";
            Current.MaxConnectionAttempts = 1;
            Current.MemberEvaluationTimeout = 5000;
            Current.RuntimePrefix = "/usr";
            Current.StepOverPropertiesAndOperators = true;
        }

        public static void Apply()
        {
            // We can only apply a limited set of options here since some
            // are set at session creation time.

            var opt = Debugger.Options;

            opt.StepOverPropertiesAndOperators = Current.StepOverPropertiesAndOperators;

            var eval = opt.EvaluationOptions;

            eval.AllowMethodEvaluation = Current.AllowMethodEvaluation;
            eval.AllowTargetInvoke = Current.AllowTargetInvoke;
            eval.AllowToStringCalls = Current.AllowToStringCalls;
            eval.ChunkRawStrings = Current.ChunkRawStrings;
            eval.CurrentExceptionTag = Current.ExceptionIdentifier;
            eval.EllipsizeStrings = Current.EllipsizeStrings;
            eval.EllipsizedLength = Current.EllipsizeThreshold;
            eval.EvaluationTimeout = Current.EvaluationTimeout;
            eval.FlattenHierarchy = Current.FlattenHierarchy;
            eval.IntegerDisplayFormat = Current.HexadecimalIntegers ?
                                        IntegerDisplayFormat.Hexadecimal :
                                        IntegerDisplayFormat.Decimal;
            eval.MemberEvaluationTimeout = Current.MemberEvaluationTimeout;
        }
    }
}
