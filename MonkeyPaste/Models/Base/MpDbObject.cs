using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using SQLite;

namespace MonkeyPaste {
    public abstract class MpDbObject {
        private static List<Type> _AllDbObjectTypes = new List<Type>();

        public abstract int Id { set; get; }


        public MpDbObject(Type subType) {
            if(!_AllDbObjectTypes.Contains(subType)) {
                _AllDbObjectTypes.Add(subType);
            }
        }       
        //public abstract void WriteToDatabase();
        //public abstract void DeleteFromDatabase();
        //public abstract override string ToString();
    }
}
