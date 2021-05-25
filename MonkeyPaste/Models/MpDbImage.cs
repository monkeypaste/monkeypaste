using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpDbImage : MpDbObject {
        [PrimaryKey,AutoIncrement]
        public override int Id { get; set; }

        public byte[] ImageBytes { get; set; }

        public MpDbImage() : base(typeof(MpDbImage)) { }
    }
}
