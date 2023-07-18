using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace MonkeyPaste.Common {

    public static class MpConsole {
        #region Private Variables
        private static bool _canLogToFile = true;
        private static bool _hasInitialized = false;
        private static StreamWriter _logStream;
        #endregion

        #region Properties

        public static double MaxLogFileSizeInMegaBytes = 3.25;

        public static bool LogToFile { get; set; } = true;
        public static bool LogToConsole { get; set; } = true;

        public static string LogFilePath => Path.Combine(Directory.GetCurrentDirectory(), "consolelog.txt");

        #endregion

        #region Public Methods

        public static void Init() {
            if (_hasInitialized) {
                return;
            }
            _hasInitialized = true;
            if (!LogToFile) {
                return;
            }
            try {
                bool in_use = MpFileIo.IsFileInUse(LogFilePath);
                MpDebug.Assert(!in_use, "Close log file");
                if (File.Exists(LogFilePath)) {
                    File.Delete(LogFilePath);
                }
                _logStream = new StreamWriter(File.Create(LogFilePath));
            }
            catch (Exception ex) {
                _canLogToFile = false;
                WriteTraceLine(@"Error deleting previous log file w/ path: " + LogFilePath + " with exception: " + ex);
            }
        }

        public static void WriteLine(string line, bool pad_pre = false, bool pad_post = false) {
            line = line == null ? string.Empty : line;
            string str = line.ToString();
            str = $"[{DateTime.Now.ToString()}] {str}";
            if (LogToConsole) {
                WriteLineWrapper(str, false, pad_pre, pad_post);
            }
            if (LogToFile) {
                WriteLogLine(str);
            }
        }
        public static void WriteWarningLine(string line, bool pad_pre = false, bool pad_post = false) {
            WriteLine("[WARNING] " + line, pad_pre, pad_post);
        }
        public static void WriteTraceLine(object line, object ex = null, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            line = line == null ? string.Empty : line;

            line = $"[{DateTime.Now.ToString()}] {line}";
            if (LogToConsole) {
                WriteLineWrapper("", true);
                WriteLineWrapper(@"-----------------------------------------------------------------------", true);
                WriteLineWrapper("File: " + callerFilePath, true);
                WriteLineWrapper("Member: " + callerName, true);
                WriteLineWrapper("Line: " + lineNum, true);
                WriteLineWrapper("Msg: " + line, true);
                WriteLineWrapper(@"-----------------------------------------------------------------------", true);
                WriteLineWrapper("", true);
                if (ex != null) {
                    LogException(ex);
                }
            }
            if (LogToFile) {
                WriteLogLine(line);
                if (ex != null) {
                    WriteLogLine("Exception: ");
                    WriteLogLine(ex);
                }
            }
        }

        private static void LogException(object ex, bool isTrace = true, bool recursive = true, int depth = 0) {
            if (ex == null) {
                return;
            }
            if (ex is Exception exObj) {
                string tabs = string.Join(string.Empty, Enumerable.Repeat("\t", depth));
                WriteLineWrapper(tabs + "Exception: ", true);
                WriteLineWrapper(tabs + $"Type: {ex.GetType()}", true);
                WriteLineWrapper(tabs + $"Source: {exObj.Source}", true);
                WriteLineWrapper(tabs + $"StackTrace: {exObj.StackTrace}", true);
                WriteLineWrapper(tabs + $"Message: {exObj.Message}", true);
                WriteLineWrapper(tabs + $"Raw: {ex}");
                if (recursive && exObj.InnerException != null) {
                    LogException(exObj.InnerException, isTrace, recursive, depth + 1);
                }
            } else {
                WriteLineWrapper("Exception: " + ex.ToString(), isTrace);
            }
        }
        private static object _logLock = new object();
        public static void WriteLogLine(object line, params object[] args) {

            if (!_canLogToFile || _logStream == null) {
                return;
            }
            line = line == null ? string.Empty : line;
            string str = line.ToString();
            str = $"<{DateTime.Now}> {str}";
            if (args != null && args.Length > 0) {
                str = string.Format(str, args);
            }
            line = $"<{DateTime.Now}> {line}";

            try {
                lock (_logLock) {
                    _logStream.WriteLine(line.ToString());
                    _logStream.Flush();
                }
            }
            catch {
                _canLogToFile = false;
                //WriteTraceLine($"Error writing to console log file at path '{LogFilePath}'", aex);

            }
        }

        public static void ShutdownLog() {
            if (_logStream == null) {
                return;
            }
            _logStream.Close();
            _logStream.Dispose();
            _logStream = null;
        }
        #endregion

        #region Private Methods

        private static void WriteLineWrapper(string str, bool isTrace = false, bool pad_pre = false, bool pad_post = false) {
            if (RuntimeInformation.FrameworkDescription.ToLower().Contains(".net framework") || isTrace) {
                // wpf
                if (pad_pre) {
                    Console.WriteLine("");
                }
                Console.WriteLine(str);
                if (pad_post) {
                    Console.WriteLine("");
                }
                return;
            } else if (RuntimeInformation.FrameworkDescription.ToLower().Contains(".net 6") ||
                        RuntimeInformation.FrameworkDescription.ToLower().Contains(".net 7")) {
                // avalonia
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    if (pad_pre) {
                        Debug.WriteLine("");
                    }
                    Debug.WriteLine(str);
                    if (pad_post) {
                        Debug.WriteLine("");
                    }
                } else {
                    if (pad_pre) {
                        Console.WriteLine("");
                    }
                    Console.WriteLine(str);
                    if (pad_post) {
                        Console.WriteLine("");
                    }
                }
                return;
            } else {
                // give console space on xamarin

                Console.WriteLine("");
                Console.WriteLine(@"-----------------------------------------------------------------------");
                Console.WriteLine("");
                Console.WriteLine(str);
                Console.WriteLine("");
                Console.WriteLine(@"-----------------------------------------------------------------------");
                Console.WriteLine("");
            }
        }

        #endregion

        private class ExclusiveSynchronizationContext : SynchronizationContext {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state) {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state) {
                lock (items) {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop() {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop() {
                while (!done) {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items) {
                        if (items.Count > 0) {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null) {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("MpAsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    } else {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy() {
                return this;
            }
        }
    }

}
