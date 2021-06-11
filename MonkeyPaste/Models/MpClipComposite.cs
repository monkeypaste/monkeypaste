using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace MonkeyPaste {
    public class MpClipComposite : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpClip))]
        public int ClipId { get; set; }

        [ForeignKey(typeof(MpClip))]
        public int ParentClipId { get; set; }

        public int SortOrderIdx { get; set; }

        #endregion

        public MpClipComposite() : base(typeof(MpClipComposite)) { }
    }
}
