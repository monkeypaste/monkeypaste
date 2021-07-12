using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using SQLite;

namespace MonkeyPaste {
    public abstract class MpDbModelBase {
        private static List<Type> _AllDbObjectTypes = new List<Type>();

        public abstract int Id { set; get; }

        public MpDbModelBase(Type subType) {
            if(!_AllDbObjectTypes.Contains(subType)) {
                _AllDbObjectTypes.Add(subType);
            }
        }       

        [Ignore]
        public string Guid { get; set; }

        protected Dictionary<string, string> CheckValue(object a, object b, string colName, Dictionary<string, string> diffLookup, object forceAVal = null) {
            if (a == b) {
                return diffLookup;
            }
            diffLookup.Add(colName, forceAVal == null ? a.ToString() : forceAVal.ToString());
            return diffLookup;
        }
    }
}
