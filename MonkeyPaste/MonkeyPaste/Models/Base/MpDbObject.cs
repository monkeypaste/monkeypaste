using System;
using System.Collections.Generic;
using System.Data;
using SQLite;

namespace MonkeyPaste.Models.Base {
    public abstract class MpDbObject : MpObject {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public MpDbObject() { }
    }
}
