using System;
using System.IO;
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
                WriteLine(@"Error deleting previus log file w/ path: " + LogFilePath + " with exception: " + ex);
            }
        }

        public static void WriteLine(object line) {
            if(!_isLoaded) {
                Init();
            }
            string str = line.ToString();
            Console.WriteLine(str);
            _sb.AppendLine(string.Format(@"[{0}] : {1}", DateTime.Now,str));
        }

        public static void WriteLine(string format,object args) {
            if(args == null || args.GetType() != typeof(Exception)) {
                WriteLine(string.Format(format, args));
            } else {
                WriteLine(format);
                WriteLine(@"With Exception: " + args.ToString());
            }
            
        }
    }
}
