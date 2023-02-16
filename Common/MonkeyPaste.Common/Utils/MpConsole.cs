using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Common {
    public static class MpConsole {
        #region Private Variables

        #endregion

        #region Properties

        public static double MaxLogFileSizeInMegaBytes = 3.25;

        public static bool LogToFile { get; set; } = false;
        public static bool LogToConsole { get; set; } = true;

        public static string LogFilePath => Path.Combine(Directory.GetCurrentDirectory(), "log.txt");

        #endregion

        #region Public Methods

        public static void Init() {
            try {
                if (File.Exists(LogFilePath)) {
                    File.Delete(LogFilePath);
                }
            }
            catch (Exception ex) {
                WriteTraceLine(@"Error deleting previus log file w/ path: " + LogFilePath + " with exception: " + ex);
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

        //public static void WriteTraceLine(string format,object args, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath="",[CallerLineNumber] int lineNum = 0) {
        //    if(args == null || args.GetType() != typeof(Exception)) {
        //        WriteTraceLine(string.Format(format, args),null,callerName,callerFilePath,lineNum);
        //    } else {
        //        WriteTraceLine(format, args, callerName, callerFilePath, lineNum);
        //    }

        //}

        public static void WriteLogLine(object line, params object[] args) {
            line = line == null ? string.Empty : line;
            string str = line.ToString();
            str = $"<{DateTime.Now}> {str}";
            if (args != null && args.Length > 0) {
                str = string.Format(str, args);
            }
            line = $"<{DateTime.Now}> {line}";
            File.AppendAllLines(LogFilePath, new List<string> { line.ToString() });
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
    }
}
