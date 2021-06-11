using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace MonkeyPaste {
    public class MpPasteHistory : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpClient))]
        public int ClientId { get; set; }

        [ForeignKey(typeof(MpClip))]
        public int ClipId { get; set; }

        [ForeignKey(typeof(MpSource))]
        public int SourceId { get; set; }
        #endregion

        public MpPasteHistory() : base(typeof(MpPasteHistory)) { }
    }
}
