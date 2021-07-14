using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using SQLite;

namespace MonkeyPaste {
    public abstract class MpDbModelBase {
        public const string ParseToken = @"^(@!@";
        public abstract int Id { set; get; }    

        [Ignore]
        public string Guid { get; set; }

        protected Dictionary<string, string> CheckValue(object a, object b, string colName, Dictionary<string, string> diffLookup, object forceAVal = null) {
            a = a == null ? string.Empty : a;
            b = b == null ? string.Empty : b;
            if (a.ToString() == b.ToString()) {
                return diffLookup;
            }
            diffLookup.Add(colName, forceAVal == null ? a.ToString() : forceAVal.ToString());
            return diffLookup;
        }
    }
}
