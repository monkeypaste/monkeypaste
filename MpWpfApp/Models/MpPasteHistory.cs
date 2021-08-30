using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace MpWpfApp {
    public class MpPasteHistory : MpDbModelBase {
        private static List<MpPasteHistory> _AllPasteHistoryList = null;

        public int PasteHistoryId { get; set; }
        public Guid PasteHistoryGuid { get; set; }


        public int CopyItemId { get; set; }
        public int UserDeviceId { get; set; }
        public int DestAppId { get; set; }
        public int DestUrlId { get; set; }

        public DateTime PasteDateTime { get; set; }

        public static List<MpPasteHistory> GetAllPasteHistory() {
            if (_AllPasteHistoryList == null) {
                _AllPasteHistoryList = new List<MpPasteHistory>();
                DataTable dt = MpDb.Instance.Execute("select * from MpPasteHistory", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        _AllPasteHistoryList.Add(new MpPasteHistory(dr));
                    }
                }
            }
            return _AllPasteHistoryList;
        }

        public static List<MpPasteHistory> GetPasteHistoryByCopyItemId(int copyItemId) {
            return GetAllPasteHistory().Where(x => x.CopyItemId == copyItemId).ToList();
        }

        public MpPasteHistory(DataRow dr) {
            LoadDataRow(dr);
        }
        public MpPasteHistory(MpCopyItem ci, int appId = 0, int urlId = 0) {
            PasteHistoryId = 0;
            PasteHistoryGuid = Guid.NewGuid();

            CopyItemId = ci.CopyItemId;
            PasteDateTime = DateTime.Now;

            WriteToDatabase();
        }

        public override void LoadDataRow(DataRow dr) {
            PasteHistoryId = Convert.ToInt32(dr["pk_MpPasteHistoryId"], CultureInfo.InvariantCulture);
            PasteHistoryGuid = System.Guid.Parse(dr["MpPasteHistoryGuid"].ToString());

            UserDeviceId = Convert.ToInt32(dr["fk_MpUserDeviceId"].ToString());

            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"], CultureInfo.InvariantCulture);

            DestAppId = Convert.ToInt32(dr["fk_MpAppId"], CultureInfo.InvariantCulture);
            DestUrlId = Convert.ToInt32(dr["fk_MpUrlId"], CultureInfo.InvariantCulture);

            PasteDateTime = DateTime.Parse(dr["PasteDateTime"].ToString());
        }

        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            // TODO use _destHandle to find app but this tracking paste history is not critical so leaving DestAppId to null for now
            MpDb.Instance.ExecuteWrite(
                "insert into MpPasteHistory(MpPasteHistoryGuid,fk_MpCopyItemId,fk_MpUserDeviceId,fk_MpAppId,fk_MpUrlId,PasteDateTime) values (@phg,@ciid, @udid, @aid,@uid,@pdt)",
                new Dictionary<string, object> {
                    { "@phg", PasteHistoryGuid.ToString() },
                    { "@ciid", CopyItemId },
                    { "@udid", UserDeviceId },
                    { "@udid", UserDeviceId },
                    { "@aid", DestAppId },
                    { "@uid", DestUrlId },
                    { "@pdt", PasteDateTime.ToString("yyyy-MM-dd HH:mm:ss") }
                }, PasteHistoryGuid.ToString(), sourceClientGuid.ToString(),this,ignoreTracking,ignoreSyncing);
            PasteHistoryId = MpDb.Instance.GetLastRowId("MpPasteHistory", "pk_MpPasteHistoryId");

            GetAllPasteHistory().Add(this);
        }
        public override void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }
        public void WriteToDatabase(bool ignoreTracking, bool ignoreSyncing) {
            WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid, ignoreTracking, ignoreSyncing);
        }

        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (string.IsNullOrEmpty(sourceClientGuid)) {
                sourceClientGuid = Properties.Settings.Default.ThisDeviceGuid;
            }

            MpDb.Instance.ExecuteWrite(
                "delete from MpPasteHistory where pk_MpPasteHistoryId=@phid",
                new Dictionary<string, object> {
                    { "@phid", PasteHistoryId }
                    }, PasteHistoryGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);

            GetAllPasteHistory().Remove(this);
        }

        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }
    }
}
