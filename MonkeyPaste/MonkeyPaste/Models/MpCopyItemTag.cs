using System;
using System.Collections.Generic;
using System.Data;
using SQLite;
using SQLiteNetExtensions;

namespace MonkeyPaste {
    [Table(nameof(MpCopyItemTag))]
    public class MpCopyItemTag : MpDbObject {
        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column(nameof(TagId))]
        public int TagId { get; set; }

        [Column(nameof(CopyItemId))]
        public int CopyItemId { get; set; }
        #endregion
    }
}
