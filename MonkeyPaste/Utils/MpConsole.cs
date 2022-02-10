using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MonkeyPaste {
    public static class MpConsole {
        public static double MaxLogFileSizeInMegaBytes = 3.25;

        public static int LogWriteToFileIntervalInMs = 1000 * 60;

        private static bool _logToFile = false;
        public static bool LogToFile {
            get {
                return _logToFile;
            }
            set {
                if (_logToFile != value) {
                    _logToFile = value;
                    if (LogToFile) {
                        Init();
                    }
                }
            }
        }

        public static string LogFilePath {
            get {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"log.txt";
            }
        }

        private static StringBuilder _sb = new StringBuilder();

        private static bool _isLoaded = false;

        public static void Init() {
            if(!LogToFile) {
                _isLoaded = true;
                return;
            }
            if(_isLoaded) {
                return;
            }
            try {
                if (File.Exists(LogFilePath)) {
                    File.Delete(LogFilePath);                 
                }

                var writeLogTimer = new System.Timers.Timer() {
                    Interval = LogWriteToFileIntervalInMs,
                    AutoReset = true
                };

                writeLogTimer.Elapsed += (s, e) => {
                    MpFileIo.AppendTextToFile(LogFilePath, _sb.ToString());
                    _sb.Clear();
                };

                writeLogTimer.Start();
                _isLoaded = true;
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
            string test = RuntimeInformation.FrameworkDescription;
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

        public static void WriteLine(object line, params object[] args) {
            line = line == null ? string.Empty : line;
            string str = line.ToString();
            str = $"[{DateTime.Now.ToString()}] {str}";
            if (args != null && args.Length > 0) {
                str = string.Format(str, args);
            }
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

        public static void WriteTraceLine(object line, object args = null, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            if(!_isLoaded) {
                Init();
            }
            line = line == null ? string.Empty: line;
            if(args != null) {
                line += args.ToString();
            }

            line = $"[{DateTime.Now.ToString()}] {line}";
            //args = args == null ? string.Empty : args;
            string outStr = string.Empty;
            Console.WriteLine("");
            Console.WriteLine(@"-----------------------------------------------------------------------");
            Console.WriteLine("File: "+callerFilePath);
            Console.WriteLine("Member: " + callerName);
            Console.WriteLine("Line: " + lineNum);
            Console.WriteLine("Msg: " + line);
            Console.WriteLine(@"-----------------------------------------------------------------------");
            Console.WriteLine("");

            _sb.AppendLine(string.Format(@"[{0}] : {1}", DateTime.Now, outStr));
        }

        public static void WriteTraceLine(string format,object args, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath="",[CallerLineNumber] int lineNum = 0) {
            if(args == null || args.GetType() != typeof(Exception)) {
                WriteTraceLine(string.Format(format, args),null,callerName,callerFilePath,lineNum);
            } else {
                WriteTraceLine(format, args, callerName, callerFilePath, lineNum);
            }
            
        }
    }
}
