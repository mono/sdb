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
            Reset();

            // Have to special case this initalization
            // during startup, since `Session` and
            // `BreakEvents` initially depend on each
            // other.
            Session.Breakpoints = BreakEvents;

            DebuggerLoggingService.CustomLogger = new CustomLogger();
        }

        public static SoftDebuggerSession Session { get; private set; }

        public static FileInfo CurrentExecutable { get; private set; }

        public static DebuggerSessionOptions Options { get; private set; }

        public static string WorkingDirectory { get; set; }

        public static string Arguments { get; set; }

        public static Dictionary<string, string> EnvironmentVariables { get; private set; }

        public static SortedDictionary<long, string> Watchpoints { get; private set; }

        public static BreakpointStore BreakEvents { get; private set; }

        static long _nextWatchId;

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

                Session.TargetEvent += (sender, e) =>
                {
                    Log.Debug("Event: '{0}'", e.Type);
                };

                Session.TargetStarted += (sender, e) =>
                {
                    _activeFrame = null;

                    if (_showResumeMessage)
                        Log.Notice("Inferior process '{0}' ('{1}') resumed",
                                   ActiveProcess.Id, CurrentExecutable.Name);
                };

                Session.TargetReady += (sender, e) =>
                {
                    _showResumeMessage = true;
                    _activeProcess = Session.GetProcesses().SingleOrDefault();

                    Log.Notice("Inferior process '{0}' ('{1}') started",
                               ActiveProcess.Id, CurrentExecutable.Name);
                };

                Session.TargetStopped += (sender, e) =>
                {
                    Log.Notice("Inferior process '{0}' ('{1}') suspended",
                               ActiveProcess.Id, CurrentExecutable.Name);
                    Log.Emphasis(Utilities.StringizeFrame(ActiveFrame, true));

                    CommandLine.ResumeEvent.Set();
                };

                Session.TargetInterrupted += (sender, e) =>
                {
                    Log.Notice("Inferior process '{0}' ('{1}') interrupted",
                               ActiveProcess.Id, CurrentExecutable.Name);
                    Log.Emphasis(Utilities.StringizeFrame(ActiveFrame, true));

                    CommandLine.ResumeEvent.Set();
                };

                Session.TargetHitBreakpoint += (sender, e) =>
                {
                    Log.Emphasis(Utilities.StringizeFrame(ActiveFrame, true));

                    CommandLine.ResumeEvent.Set();
                };

                Session.TargetExited += (sender, e) =>
                {
                    Log.Notice("Inferior process '{0}' ('{1}') exited",
                               ActiveProcess.Id, CurrentExecutable.Name);

                    // Make sure we clean everything up on a normal exit.
                    Kill();

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

                var info = new SoftDebuggerStartInfo(Configuration.Current.RuntimePrefix,
                                                     EnvironmentVariables)
                {
                    Command = file.Name,
                    Arguments = Arguments,
                    WorkingDirectory = WorkingDirectory
                };

                CurrentExecutable = file;
                _showResumeMessage = false;

                Session.Run(info, Options);

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

        public static void Reset()
        {
            // No need to lock on this data.

            Options = new DebuggerSessionOptions
            {
                EvaluationOptions = EvaluationOptions.DefaultOptions
            };
            WorkingDirectory = Environment.CurrentDirectory;
            Arguments = string.Empty;
            EnvironmentVariables = new Dictionary<string, string>();
            Watchpoints = new SortedDictionary<long, string>();
            _nextWatchId = 0;
            BreakEvents = new BreakpointStore();

            // Make sure breakpoints/catchpoints take effect.
            if (Session != null)
                Session.Breakpoints = BreakEvents;
        }
    }
}
