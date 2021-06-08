using System;
using System.IO;
using System.Runtime.CompilerServices;
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
                    MpHelpers.Instance.AppendTextToFile(LogFilePath, _sb.ToString());
                    _sb.Clear();
                };

                writeLogTimer.Start();
                _isLoaded = true;
            }
            catch(Exception ex) {
                WriteTraceLine(@"Error deleting previus log file w/ path: " + LogFilePath + " with exception: " + ex);
            }
        }

        public static void WriteLine(object line) {
            Console.WriteLine(line.ToString());
        }

        public static void WriteTraceLine(object line, object args = null, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            if(!_isLoaded) {
                Init();
            }
            line = line == null ? new object(): line;
            args = args == null ? new object() : args;
            string outStr = string.Empty;
            if(args != null && args.GetType() == typeof(Exception)) {
                outStr = string.Format(@"File: {0}\nMember: {1}\nLine: {2}\nMessage: {3}\nException: {4}", callerFilePath, callerName, lineNum, line,args.ToString());
            } else {
                outStr = string.Format(@"File: {0}\nMember: {1}\nLine: {2}\nMessage: {3}", callerFilePath, callerName, lineNum, string.Format(line.ToString(),args.ToString()));
            }
            
            Console.WriteLine(outStr);
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
