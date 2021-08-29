using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MonkeyPaste {
    public class MpSyncHistory : MpDbModelBase {
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpSyncHistoryId")]
        public override int Id { get; set; } = 0;

        public string OtherClientGuid { get; set; }
        public DateTime SyncDateTime { get; set; }

        public static MpSyncHistory GetSyncHistoryByDeviceGuid(string dg) {
            return MpDb.Instance.GetItems<MpSyncHistory>().Where(x => x.OtherClientGuid == dg).FirstOrDefault();
        }

        public MpSyncHistory() { }
    }
}
