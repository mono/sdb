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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;

namespace Mono.Debugger.Client
{
    public static class Debugger
    {
        static readonly object _lock = new object();

        static Debugger()
        {
            EnsureCreated();
            ResetOptions();
            ResetState();

            _debuggeeKilled = true;

            DebuggerLoggingService.CustomLogger = new CustomLogger();
        }

        public static SoftDebuggerSession Session { get; private set; }

        public static FileInfo CurrentExecutable { get; private set; }

        public static IPAddress CurrentAddress { get; private set; }

        public static int CurrentPort { get; private set; }

        public static DebuggerSessionOptions Options { get; private set; }

        public static string WorkingDirectory { get; set; }

        public static string Arguments { get; set; }

        public static Dictionary<string, string> EnvironmentVariables { get; private set; }

        public static SortedDictionary<long, string> Watches { get; private set; }

        public static SortedDictionary<long, BreakEvent> Breakpoints { get; private set; }

        public static BreakpointStore BreakEvents { get; private set; }

        public static bool DebuggeeKilled
        {
            get { return _debuggeeKilled; }
            set { _debuggeeKilled = value; }
        }

        static volatile bool _debuggeeKilled;

        static long _nextWatchId;

        static long _nextBreakpointId;

        static volatile bool _showResumeMessage;

        public static State State
        {
            get
            {
                if (Session == null || Session.HasExited || !Session.IsConnected)
                    return State.Exited;

                return Session.IsRunning ? State.Running : State.Suspended;
            }
        }

        static ProcessInfo _activeProcess;

        public static ProcessInfo ActiveProcess
        {
            get { return _activeProcess; }
        }

        public static ThreadInfo ActiveThread
        {
            get { return Session == null ? null : Session.ActiveThread; }
        }

        public static Backtrace ActiveBacktrace
        {
            get
            {
                var thr = ActiveThread;

                return thr == null ? null : thr.Backtrace;
            }
        }

        static StackFrame _activeFrame;

        public static StackFrame ActiveFrame
        {
            get
            {
                var f = _activeFrame;

                if (f != null)
                    return f;

                var bt = ActiveBacktrace;

                if (bt != null)
                    return _activeFrame = bt.GetFrame(0);

                return null;
            }
            set { _activeFrame = value; }
        }

        public static ExceptionInfo ActiveException
        {
            get
            {
                var bt = ActiveBacktrace;

                return bt == null ? null : bt.GetFrame(0).GetException();
            }
        }

        static void PrintException(string prefix, ExceptionInfo ex)
        {
            Log.Error("{0}{1}: {2}", prefix, ex.Type, ex.Message);
        }

        static void PrintException(ExceptionInfo ex)
        {
            PrintException(string.Empty, ex);

            var prefix = "> ";
            var inner = ex;

            while ((inner = inner.InnerException) != null)
            {
                PrintException(prefix, inner);

                prefix = "--" + prefix;
            }
        }

        static string StringizeTarget()
        {
            if (CurrentExecutable != null)
                return CurrentExecutable.Name;

            if (CurrentAddress != null)
                return string.Format("{0}:{1}", CurrentAddress, CurrentPort);

            return "<none>";
        }

        static void EnsureCreated()
        {
            lock (_lock)
            {
                if (Session != null)
                    return;

                Session = new SoftDebuggerSession();
                Session.Breakpoints = BreakEvents;

                Session.ExceptionHandler = ex =>
                {
                    if (Configuration.Current.LogInternalErrors)
                    {
                        Log.Error("Internal debugger error:", ex.GetType());
                        Log.Error(ex.ToString());
                    }

                    return true;
                };

                Session.LogWriter = (isStdErr, text) =>
                {
                    if (Configuration.Current.LogRuntimeSpew)
                        Log.NoticeSameLine("[Mono] {0}", text); // The string already has a line feed.
                };

                Session.OutputWriter = (isStdErr, text) =>
                {
                    lock (Log.Lock)
                    {
                        if (isStdErr)
                            Console.Error.Write(text);
                        else
                            Console.Write(text);
                    }
                };

                Session.TypeResolverHandler += (identifier, location) =>
                {
                    // I honestly have no idea how correct this is. I suspect you
                    // could probably break it in some corner cases. It does make
                    // something like `p Android.Runtime.JNIEnv.Handle` work,
                    // though, which would otherwise have required `global::` to
                    // be explicitly prepended.

                    if (identifier == "__EXCEPTION_OBJECT__")
                        return null;

                    foreach (var loc in ActiveFrame.GetAllLocals())
                        if (loc.Name == identifier)
                            return null;

                    return identifier;
                };

                Session.TargetEvent += (sender, e) =>
                {
                    Log.Debug("Event: '{0}'", e.Type);
                };

                Session.TargetStarted += (sender, e) =>
                {
                    _activeFrame = null;

                    if (_showResumeMessage)
                        Log.Notice("Inferior process '{0}' ('{1}') resumed",
                                   ActiveProcess.Id, StringizeTarget());
                };

                Session.TargetReady += (sender, e) =>
                {
                    _showResumeMessage = true;
                    _activeProcess = Session.GetProcesses().SingleOrDefault();

                    CommandLine.SetControlCHandler();

                    Log.Notice("Inferior process '{0}' ('{1}') started",
                               ActiveProcess.Id, StringizeTarget());
                };

                Session.TargetStopped += (sender, e) =>
                {
                    Log.Notice("Inferior process '{0}' ('{1}') suspended",
                               ActiveProcess.Id, StringizeTarget());
                    Log.Emphasis(Utilities.StringizeFrame(ActiveFrame, true));

                    CommandLine.ResumeEvent.Set();
                };

                Session.TargetInterrupted += (sender, e) =>
                {
                    Log.Notice("Inferior process '{0}' ('{1}') interrupted",
                               ActiveProcess.Id, StringizeTarget());
                    Log.Emphasis(Utilities.StringizeFrame(ActiveFrame, true));

                    CommandLine.ResumeEvent.Set();
                };

                Session.TargetHitBreakpoint += (sender, e) =>
                {
                    var bp = e.BreakEvent as Breakpoint;
                    var fbp = e.BreakEvent as FunctionBreakpoint;

                    if (fbp != null)
                        Log.Notice("Hit method breakpoint on '{0}'", fbp.FunctionName);
                    else
                    {
                        var cond = bp.ConditionExpression != null ?
                                   string.Format(" (condition '{0}' met)", bp.ConditionExpression) :
                                   string.Empty;

                        Log.Notice("Hit breakpoint at '{0}:{1}'{2}", bp.FileName, bp.Line, cond);
                    }

                    Log.Emphasis(Utilities.StringizeFrame(ActiveFrame, true));

                    CommandLine.ResumeEvent.Set();
                };

                Session.TargetExited += (sender, e) =>
                {
                    var p = ActiveProcess;

                    // Can happen when a remote connection attempt fails.
                    if (p != null)
                        Log.Notice("Inferior process '{0}' ('{1}') exited", ActiveProcess.Id, StringizeTarget());
                    else
                        Log.Notice("Failed to connect to '{0}'", StringizeTarget());

                    CommandLine.UnsetControlCHandler();

                    // Make sure we clean everything up on a normal exit.
                    Kill();

                    _debuggeeKilled = true;

                    CommandLine.ResumeEvent.Set();
                };

                Session.TargetExceptionThrown += (sender, e) =>
                {
                    var ex = ActiveException;

                    Log.Notice("Trapped first-chance exception of type '{0}'", ex.Type);
                    Log.Emphasis(Utilities.StringizeFrame(ActiveFrame, true));

                    PrintException(ex);

                    CommandLine.ResumeEvent.Set();
                };

                Session.TargetUnhandledException += (sender, e) =>
                {
                    var ex = ActiveException;

                    Log.Notice("Trapped unhandled exception of type '{0}'", ex.Type);
                    Log.Emphasis(Utilities.StringizeFrame(ActiveFrame, true));

                    PrintException(ex);

                    CommandLine.ResumeEvent.Set();
                };

                Session.TargetThreadStarted += (sender, e) =>
                {
                    Log.Notice("Inferior thread '{0}' ('{1}') started",
                               e.Thread.Id, e.Thread.Name);
                };

                Session.TargetThreadStopped += (sender, e) =>
                {
                    Log.Notice("Inferior thread '{0}' ('{1}') exited",
                               e.Thread.Id, e.Thread.Name);
                };
            }
        }

        public static void Run(FileInfo file)
        {
            lock (_lock)
            {
                EnsureCreated();

                CurrentExecutable = file;
                CurrentAddress = null;
                CurrentPort = -1;

                _showResumeMessage = false;
                _debuggeeKilled = false;

                var info = new SoftDebuggerStartInfo(Configuration.Current.RuntimePrefix,
                                                     EnvironmentVariables)
                {
                    Command = file.FullName,
                    Arguments = Arguments,
                    WorkingDirectory = WorkingDirectory,
                    StartArgs =
                    {
                        MaxConnectionAttempts = Configuration.Current.MaxConnectionAttempts,
                        TimeBetweenConnectionAttempts = Configuration.Current.ConnectionAttemptInterval
                    }
                };

                Session.Run(info, Options);

                CommandLine.InferiorExecuting = true;
            }
        }

        public static void Connect(IPAddress address, int port)
        {
            lock (_lock)
            {
                EnsureCreated();

                CurrentExecutable = null;
                CurrentAddress = address;
                CurrentPort = port;

                _showResumeMessage = false;
                _debuggeeKilled = false;

                var args = new SoftDebuggerConnectArgs(string.Empty, address, port)
                {
                    MaxConnectionAttempts = Configuration.Current.MaxConnectionAttempts,
                    TimeBetweenConnectionAttempts = Configuration.Current.ConnectionAttemptInterval
                };

                Session.Run(new SoftDebuggerStartInfo(args), Options);

                CommandLine.InferiorExecuting = true;
            }
        }

        public static void Listen(IPAddress address, int port)
        {
            lock (_lock)
            {
                EnsureCreated();

                CurrentExecutable = null;
                CurrentAddress = address;
                CurrentPort = port;

                _showResumeMessage = false;
                _debuggeeKilled = false;

                var args = new SoftDebuggerListenArgs(string.Empty, address, port);

                Session.Run(new SoftDebuggerStartInfo(args), Options);

                CommandLine.InferiorExecuting = true;
            }
        }

        public static void Pause()
        {
            lock (_lock)
                if (Session != null && Session.IsRunning)
                    Session.Stop();
        }

        public static void Continue()
        {
            lock (_lock)
            {
                if (Session != null && !Session.IsRunning && !Session.HasExited)
                {
                    Session.Continue();

                    CommandLine.InferiorExecuting = true;
                }
            }
        }

        public static void Kill()
        {
            lock (_lock)
            {
                if (Session == null)
                    return;

                CommandLine.InferiorExecuting = true;

                if (!Session.HasExited)
                    Session.Exit();

                Session.Dispose();
                Session = null;
            }
        }

        public static void StepOverLine()
        {
            lock (_lock)
            {
                if (Session != null && !Session.IsRunning && !Session.HasExited)
                {
                    Session.StepLine();

                    CommandLine.InferiorExecuting = true;
                }
            }
        }

        public static void StepOverInstruction()
        {
            lock (_lock)
            {
                if (Session != null && !Session.IsRunning && !Session.HasExited)
                {
                    Session.StepInstruction();

                    CommandLine.InferiorExecuting = true;
                }
            }
        }

        public static void StepIntoLine()
        {
            lock (_lock)
            {
                if (Session != null && !Session.IsRunning && !Session.HasExited)
                {
                    Session.NextLine();

                    CommandLine.InferiorExecuting = true;
                }
            }
        }

        public static void StepIntoInstruction()
        {
            lock (_lock)
            {
                if (Session != null && !Session.IsRunning && !Session.HasExited)
                {
                    Session.NextInstruction();

                    CommandLine.InferiorExecuting = true;
                }
            }
        }

        public static void StepOutOfMethod()
        {
            lock (_lock)
            {
                if (Session != null && !Session.IsRunning && !Session.HasExited)
                {
                    Session.Finish();

                    CommandLine.InferiorExecuting = true;
                }
            }
        }

        public static long GetWatchId()
        {
            return _nextWatchId++;
        }

        public static long GetBreakpointId()
        {
            return _nextBreakpointId++;
        }

        public static void ResetState()
        {
            // No need to lock on this data.

            WorkingDirectory = Environment.CurrentDirectory;
            Arguments = string.Empty;
            EnvironmentVariables = new Dictionary<string, string>();
            Watches = new SortedDictionary<long, string>();
            _nextWatchId = 0;
            Breakpoints = new SortedDictionary<long, BreakEvent>();
            BreakEvents = new BreakpointStore();

            // Make sure breakpoints/catchpoints take effect.
            if (Session != null)
                Session.Breakpoints = BreakEvents;
        }

        public static void ResetOptions()
        {
            // No need to lock on this data.

            Options = new DebuggerSessionOptions
            {
                EvaluationOptions = EvaluationOptions.DefaultOptions
            };

            Options.EvaluationOptions.UseExternalTypeResolver = true;
        }

        [Serializable]
        sealed class DebuggerState
        {
            public string WorkingDirectory { get; set; }

            public string Arguments { get; set; }

            public Dictionary<string, string> EnvironmentVariables { get; set; }

            public SortedDictionary<long, string> Watches { get; set; }

            public long NextWatchId { get; set; }

            public Dictionary<long, Tuple<BreakEvent, bool>> Breakpoints { get; set; }

            public long NextBreakpointId { get; set; }

            public ReadOnlyCollection<Catchpoint> Catchpoints { get; set; }
        }

        public static void Write(FileInfo file)
        {
            var state = new DebuggerState
            {
                WorkingDirectory = WorkingDirectory,
                Arguments = Arguments,
                EnvironmentVariables = EnvironmentVariables,
                Watches = Watches,
                NextWatchId = _nextWatchId,
                Breakpoints = Breakpoints.Select(x => Tuple.Create(x.Key, x.Value, BreakEvents.Contains(x.Value)))
                                         .ToDictionary(x => x.Item1, x => Tuple.Create(x.Item2, x.Item3)),
                NextBreakpointId = _nextBreakpointId,
                Catchpoints = BreakEvents.GetCatchpoints()
            };

            try
            {
                using (var stream = file.Open(FileMode.Create, FileAccess.Write))
                    new BinaryFormatter().Serialize(stream, state);
            }
            catch (Exception ex)
            {
                Log.Error("Could not write database file '{0}':", file);
                Log.Error(ex.ToString());
            }
        }

        public static void Read(FileInfo file)
        {
            DebuggerState state;

            try
            {
                using (var stream = file.Open(FileMode.Open, FileAccess.Read))
                    state = (DebuggerState)new BinaryFormatter().Deserialize(stream);
            }
            catch (Exception ex)
            {
                Log.Error("Could not read database file '{0}':", file);
                Log.Error(ex.ToString());

                return;
            }

            ResetState();

            WorkingDirectory = state.WorkingDirectory;
            Arguments = state.Arguments;
            EnvironmentVariables = state.EnvironmentVariables;
            Watches = state.Watches;

            foreach (var kvp in state.Breakpoints)
            {
                Breakpoints.Add(kvp.Key, kvp.Value.Item1);

                if (kvp.Value.Item2)
                    BreakEvents.Add(kvp.Value.Item1);
            }

            foreach (var cp in state.Catchpoints)
                BreakEvents.Add(cp);
        }
    }
}
