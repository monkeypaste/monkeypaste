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
        
        public Guid Guid { get; set; }

        public DateTime LastModifiedDateTime { get; set; }
        public DateTime LastSyncDateTime { get; set; }
    }
}
