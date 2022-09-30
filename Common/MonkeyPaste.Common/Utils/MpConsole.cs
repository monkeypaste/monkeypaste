using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
            catch(Exception ex) {
                WriteTraceLine(@"Error deleting previus log file w/ path: " + LogFilePath + " with exception: " + ex);
            }
        }

        public static void WriteLine(string line) {
            line = line == null ? string.Empty : line;
            string str = line.ToString();
            str = $"[{DateTime.Now.ToString()}] {str}";
            if (LogToConsole) {
                WriteLineWrapper(str);
            }
            if(LogToFile) {
                WriteLogLine(str);
            }
        }

        public static void WriteLines(params object[] lines) {
            if(lines == null) {
                return;
            }
            foreach(var l in lines) {
                WriteLine(l);
            }
        }

        public static void WriteLine(object line, params object[] args) {
            line = line == null ? string.Empty : line;
            string str = line.ToString();
            str = $"<{DateTime.Now}> {str}";
            if (LogToConsole) {
                WriteLineWrapper(str);
            }
            if (LogToFile) {
                WriteLogLine(str);
            }
        }

        public static void WriteTraceLine(object line, object ex = null, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            line = line == null ? string.Empty: line;

            line = $"[{DateTime.Now.ToString()}] {line}";
            if (LogToConsole) {
                WriteLineWrapper("",true);
                WriteLineWrapper(@"-----------------------------------------------------------------------",true);
                WriteLineWrapper("File: " + callerFilePath,true);
                WriteLineWrapper("Member: " + callerName,true);
                WriteLineWrapper("Line: " + lineNum,true);
                WriteLineWrapper("Msg: " + line,true);
                WriteLineWrapper(@"-----------------------------------------------------------------------",true);
                WriteLineWrapper("",true);
                if(ex != null) {
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
            if(ex == null) {
                return;
            }
            if(ex is Exception exObj) {
                string tabs = string.Join(string.Empty,Enumerable.Repeat("\t", depth));
                WriteLineWrapper(tabs+"Exception: ", true);
                WriteLineWrapper(tabs + $"Type: {ex.GetType()}", true);
                WriteLineWrapper(tabs + $"Source: {exObj.Source}", true);
                WriteLineWrapper(tabs + $"StackTrace: {exObj.StackTrace}", true);
                WriteLineWrapper(tabs + $"Message: {exObj.Message}", true);
                WriteLineWrapper(tabs + $"Raw: {ex}");
                if (recursive && exObj.InnerException != null) {
                    LogException(exObj.InnerException, isTrace, recursive, depth + 1);
                }
            } else {
                WriteLineWrapper("Exception: "+ex.ToString(),isTrace);
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

        private static void WriteLineWrapper(string str, bool isTrace = false) {
            if (RuntimeInformation.FrameworkDescription.ToLower().Contains(".net framework") || isTrace) {
                // wpf
                Console.WriteLine(str);
                return;
            } else if (RuntimeInformation.FrameworkDescription.ToLower().Contains(".net 6")) {
                // avalonia
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    Debug.WriteLine(str);
                } else {
                    Console.WriteLine(str);
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
