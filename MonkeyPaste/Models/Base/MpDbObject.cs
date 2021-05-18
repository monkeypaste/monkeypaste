using System;
using System.Collections.Generic;
using System.Data;
using SQLite;

namespace MonkeyPaste {
    public abstract class MpDbObject {
        public abstract int Id { get; set; }
    }
}
