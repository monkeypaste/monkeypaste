using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpCopyItemFetchResult {
        //[Column("pk_MpCopyItemFetchResultId")]
        //[PrimaryKey, AutoIncrement]
        //public override int Id { get; set; }

        //[Column("RootId")]
        public int RootId { get; set; }

        //[Column("pk_MpCopyItemId")]
        public int PrimaryItemId { get; set; }

        public MpCopyItemFetchResult() : base() { }
    }
    
    public class MpCopyItemFetchResultComparer : IEqualityComparer<MpCopyItemFetchResult> {
        public bool Equals(MpCopyItemFetchResult x, MpCopyItemFetchResult y) {
            return x.RootId == y.RootId;
        }

        public int GetHashCode(MpCopyItemFetchResult codeh) {
            return codeh.GetHashCode();
        }
    }
}
