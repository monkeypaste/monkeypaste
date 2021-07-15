using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace MpWpfApp {
    public abstract class MpDbObject : MpObject {
        public const string ParseToken = @"^(@!@";
        public string TableName = "Unknown";
        public Dictionary<string, object> columnData = new Dictionary<string, object>();

        public abstract void LoadDataRow(DataRow dr);
                

        public int GetByteSize() {
            return 0;
        }

        public DateTime GetCreationDate() {
            return DateTime.Now;
        }

        public int GetOwnerId() {
            return 0;
        }

        public abstract void WriteToDatabase();

        public virtual void WriteToDatabase(string sourceClientGuid,bool ignoreTracking = false) { }

        public virtual void DeleteFromDatabase(string sourceClientGuid) { }
        
        protected Dictionary<string, string> CheckValue(
            object a, object b, string colName, Dictionary<string, string> diffLookup, object forceAVal = null) {
            // a = current model property
            // b = model in db
            // when a != b add a to diffLookup OR substitue with forceVal (so guids are used instead of int keys)
            a = a == null ? string.Empty : a;
            b = b == null ? string.Empty : b;
            if (a.ToString() == b.ToString()) {
                return diffLookup;
            }
            diffLookup.Add(colName, forceAVal == null ? a.ToString() : forceAVal.ToString());
            return diffLookup;
        }

        private void MpDbObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            HasChanged = true;
        }

        public override string ToString() {            
            string outstr = "";
            foreach (KeyValuePair<string, object> cd in columnData) {
                if (cd.Value == null) {
                    continue;
                }
                outstr += "| " + cd.Key.ToString() + ": \n";
                if (cd.Value.GetType() == typeof(Image)) {
                    outstr += "| " + ((Image)cd.Value).Width + " x " + ((Image)cd.Value).Height + " \n";
                } else if (cd.Value.GetType() == typeof(string[])) {
                    foreach (string str in (string[])cd.Value) {
                        outstr += "| " + str + "\n";
                    }
                } else {
                    outstr += "| " + cd.Value.ToString() + "\n";
                }
                outstr += "|-----------------------------------------------------------------------------------------------|\n";
            }
            return outstr;
        }
    }
}
