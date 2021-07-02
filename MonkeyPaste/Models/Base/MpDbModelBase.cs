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
        
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Synced { get; set; }
        public DateTime Deleted { get; set; }
    }
}
