using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MonkeyPaste.Common {
    public static class MpConsole {
        #region Private Variables
        private static bool _canLogToFile = true;
        private static StreamWriter _logStream;

        private static object _logLock = new object();

        #endregion

        #region Properties

        public static MpLogLevel MinLogLevel =>
#if DEBUG
            MpLogLevel.Debug;
#else
            MpLogLevel.Error;
#endif

        public static bool HasInitialized { get; private set; } = false;

        public static double MaxLogFileSizeInMegaBytes = 3.25;

        public static bool LogToFile { get; set; } = true;
        public static bool LogToConsole { get; set; } = true;

        public static string LogFilePath => Path.Combine(Directory.GetCurrentDirectory(), "consolelog.txt");

        #endregion

        #region Public Methods

        public static void Init() {
            if (HasInitialized) {
                return;
            }
            if (MpFileIo.IsFileInUse(LogFilePath)) {
                return;
            }
            HasInitialized = true;
            if (!LogToFile) {
                return;
            }
            try {
                if (File.Exists(LogFilePath)) {
                    File.Delete(LogFilePath);
                }
                _logStream = new StreamWriter(File.Open(LogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None));
            }
            catch (Exception ex) {
                _canLogToFile = false;
                WriteTraceLine(@"Error deleting previous log file w/ path: " + LogFilePath + " with exception: " + ex);
            }
        }


        public static void WriteLine(string line, bool pad_pre = false, bool pad_post = false, bool stampless = false, MpLogLevel level = MpLogLevel.Debug) {
            if (!CanLog(level)) {
                return;
            }
            var sb = new StringBuilder();
            if (!stampless) {
                sb.Append($"{GetLogStamp(level)}");
            }
            sb.Append(line.ToStringOrEmpty());
            if (LogToConsole) {
                WriteLineWrapper(sb.ToString(), false, pad_pre, pad_post, level);
            }
            if (LogToFile) {
                WriteLineToFile(sb.ToString());
            }
        }

        public static void WriteTraceLine(object line, object ex = null, MpLogLevel level = MpLogLevel.Error, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            if (!CanLog(level)) {
                return;
            }
            line = line == null ? string.Empty : line;

            line = $"{GetLogStamp(level)} {line}";
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
                WriteLineToFile(line);
                if (ex != null) {
                    WriteLineToFile("Exception: ");
                    WriteLineToFile(ex);
                }
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

        private static string GetLogStamp(MpLogLevel level) {
            return $"[{DateTime.Now.ToShortTimeString()}-{level}] ";
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
        private static void WriteLineToFile(object line, params object[] args) {
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
            }
        }
        private static void WriteLineWrapper(string str, bool isTrace = false, bool pad_pre = false, bool pad_post = false, MpLogLevel level = MpLogLevel.Debug) {
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

        private static bool CanLog(MpLogLevel level) {
            return (int)level <= (int)MinLogLevel;
        }
        #endregion
    }

}
