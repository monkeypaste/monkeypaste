using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using SQLite;

namespace MonkeyPaste {
    public abstract class MpDbObject {
        public abstract int Id { set; get; }
                
        //public abstract void WriteToDatabase();
        //public abstract void DeleteFromDatabase();
        //public abstract override string ToString();
    }
}
