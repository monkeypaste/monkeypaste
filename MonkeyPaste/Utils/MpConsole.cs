using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MonkeyPaste {
    public class MpConsole {
        private static readonly Lazy<MpConsole> _Lazy = new Lazy<MpConsole>(() => new MpConsole());
        public static MpConsole Instance { get { return _Lazy.Value; } }

        private StringBuilder _sb = new StringBuilder();
        private MpConsole() {
            string logPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"log.txt";
            try {
                if (File.Exists(logPath)) {
                    File.Delete(logPath);
                }
            }
            catch(Exception ex) {
                WriteLine(@"Error deleting previus log file w/ path: " + logPath + " with exception: " + ex);
            }
            //App.Current.Exit += (s, e) =>
            //{
            //    MpHelpers.Instance.WriteTextToFile(logPath, _sb.ToString(), false);
            //};
        }

        public void WriteLine(object line) {
            string str = line.ToString();
            Console.WriteLine(str);
            _sb.AppendLine(string.Format(@"[{0}]", DateTime.Now));
            _sb.AppendLine(str);
        }

        public void WriteLine(string format,object args) {
            Console.WriteLine(string.Format(format,args));
        }
    }
}
