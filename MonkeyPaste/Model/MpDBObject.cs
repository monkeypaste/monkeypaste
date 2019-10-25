using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public abstract class MpDBObject : ICloneable {
        [DllImport("user32.dll",CharSet = CharSet.Auto,SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle,out uint processId);

        public string databaseName = "mp.db";
        public string tableName = "abstract";
        public Dictionary<string,object> columnData = new Dictionary<string,object>();
  
        public abstract void LoadDataRow(DataRow dr);

        /*public int GetByteSize() {
            return 0;
        }
        public DateTime GetCreationDate() {
            return DateTime.Now;
        }
        public int GetOwnerId() {
            return 0;
        }*/
        
        public abstract void WriteToDatabase();
        public object Clone() {
            return this.MemberwiseClone();
        }
        protected string GetProcessPath(IntPtr hwnd) {
            uint pid = 0;
            GetWindowThreadProcessId(hwnd,out pid);
            Process proc = Process.GetProcessById((int)pid);
            return proc.MainModule.FileName.ToString();
        }
        public override string ToString() {
            string outstr = "| "+tableName + "\n";
            foreach(KeyValuePair<string,object> cd in columnData) {
                if(cd.Value == null) {
                    continue;
                }
                outstr += "| " + cd.Key.ToString() + ": \n";
                if(cd.Value.GetType() == typeof(Image)) {
                    outstr += "| " + ((Image)cd.Value).Width + " x " + ((Image)cd.Value).Height + " \n";
                }
                else if(cd.Value.GetType() == typeof(string[])) {
                    foreach(string str in (string[])cd.Value) {
                        outstr += "| " + str + "\n";
                    }
                }
                else {
                    outstr += "| " + cd.Value.ToString() + "\n";
                }
                outstr += "|-----------------------------------------------------------------------------------------------|\n";
            }
            return outstr;
        }
    }
}
