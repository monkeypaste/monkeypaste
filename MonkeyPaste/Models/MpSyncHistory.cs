using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpSyncHistory : MpDbModelBase {
        [PrimaryKey, AutoIncrement]
        [Column("pk_SyncHistoryId")]
        public override int Id { get; set; }
        public Guid OtherClientGuid { get; set; }
        public DateTime SyncDateTime { get; set; }

        public MpSyncHistory() : base(typeof(MpSyncHistory)) { }
    }
}
