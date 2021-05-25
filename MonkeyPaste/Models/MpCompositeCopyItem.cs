using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace MonkeyPaste {
    public class MpCompositeCopyItem : MpDbObject {
        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        public int CopyItemId { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        public int ParentCopyItemId { get; set; }

        public int SortOrderIdx { get; set; }

        #endregion

        public MpCompositeCopyItem() : base(typeof(MpCompositeCopyItem)) { }
    }
}
