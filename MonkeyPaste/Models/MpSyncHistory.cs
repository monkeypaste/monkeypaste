using SQLite;
using System;

namespace MonkeyPaste {
    public class MpSyncHistory : MpDbModelBase {
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpSyncHistoryId")]
        public override int Id { get; set; } = 0;

        public string OtherClientGuid { get; set; }
        public DateTime SyncDateTime { get; set; }

        public MpSyncHistory() { }
    }
}
