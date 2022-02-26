using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MonkeyPaste {
    public static class MpConsole {
        public static double MaxLogFileSizeInMegaBytes = 3.25;

        public static bool LogToFile { get; set; } = false;
        public static bool LogToConsole { get; set; } = true;

        public static string LogFilePath => Path.Combine(Directory.GetCurrentDirectory(), "log.txt");

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
        public static void Write(string str) {
            str = str == null ? string.Empty : str;
            Console.Write(str);
        }

        public static void WriteLine(string line) {
            line = line == null ? string.Empty : line;
            string str = line.ToString();
            str = $"[{DateTime.Now.ToString()}] {str}";
            if (LogToConsole) {
                if (RuntimeInformation.FrameworkDescription.Contains(".NET Framework")) {
                    Console.WriteLine(str);
                    return;
                }
                Console.WriteLine("");
                Console.WriteLine(@"-----------------------------------------------------------------------");
                Console.WriteLine("");
                Console.WriteLine(str);
                Console.WriteLine("");
                Console.Write(@"-----------------------------------------------------------------------");
                Console.WriteLine("");
            }
            if(LogToFile) {
                WriteLogLine(str);
            }
        }

        public static void WriteLine(object line, params object[] args) {
            line = line == null ? string.Empty : line;
            string str = line.ToString();
            str = $"[{DateTime.Now.ToString()}] {str}";
            if (LogToConsole) {
                if (RuntimeInformation.FrameworkDescription.Contains(".NET Framework")) {
                    Console.WriteLine(str);
                    return;
                }
                Console.WriteLine("");
                Console.WriteLine(@"-----------------------------------------------------------------------");
                Console.WriteLine("");
                Console.WriteLine(str);
                Console.WriteLine("");
                Console.Write(@"-----------------------------------------------------------------------");
                Console.WriteLine("");
            }
            if (LogToFile) {
                WriteLogLine(str);
            }
        }

        public static void WriteTraceLine(object line, object args = null, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            line = line == null ? string.Empty: line;
            if(args != null) {
                line += args.ToString();
            }

            line = $"[{DateTime.Now.ToString()}] {line}";
            if (LogToConsole) {
                Console.WriteLine("");
                Console.WriteLine(@"-----------------------------------------------------------------------");
                Console.WriteLine("File: " + callerFilePath);
                Console.WriteLine("Member: " + callerName);
                Console.WriteLine("Line: " + lineNum);
                Console.WriteLine("Msg: " + line);
                Console.WriteLine(@"-----------------------------------------------------------------------");
                Console.WriteLine("");
            }
            if (LogToFile) {
                WriteLogLine(line);
            }
        }

        public static void WriteTraceLine(string format,object args, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath="",[CallerLineNumber] int lineNum = 0) {
            if(args == null || args.GetType() != typeof(Exception)) {
                WriteTraceLine(string.Format(format, args),null,callerName,callerFilePath,lineNum);
            } else {
                WriteTraceLine(format, args, callerName, callerFilePath, lineNum);
            }
            
        }

        public static void WriteLogLine(object line, params object[] args) {
            line = line == null ? string.Empty : line;
            string str = line.ToString();
            str = $"[{DateTime.Now.ToString()}] {str}";
            if (args != null && args.Length > 0) {
                str = string.Format(str, args);
            }
            line = $"[{DateTime.Now.ToString()}] {line}";
            File.AppendAllLines(LogFilePath, new List<string> { line.ToString() });
        }
    }
}
