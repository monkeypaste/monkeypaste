using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpSyncHistory : MpDbObject {
        private static List<MpSyncHistory> _AllSyncHistoryList = null;
        public static int TotalSyncHistoryCount = 0;

        public int SyncHistoryId { get; set; }
        public Guid OtherClientGuid { get; set; }
        public DateTime SyncDateTime { get; set; }

        public static List<MpSyncHistory> GetAllSyncHistorys() {
            if(_AllSyncHistoryList == null) {
                _AllSyncHistoryList = new List<MpSyncHistory>();
                DataTable dt = MpDb.Instance.Execute("select * from MpSyncHistory", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        _AllSyncHistoryList.Add(new MpSyncHistory(dr));
                    }
                }
            }
            return _AllSyncHistoryList;
        }
        public static MpSyncHistory GetSyncHistoryById(int SyncHistoryId) {
            if (_AllSyncHistoryList == null) {
                GetAllSyncHistorys();
            }
            var udbpl = _AllSyncHistoryList.Where(x => x.SyncHistoryId == SyncHistoryId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static MpSyncHistory GetSyncHistoryByGuid(string guid) {
            if (_AllSyncHistoryList == null) {
                GetAllSyncHistorys();
            }
            var udbpl = _AllSyncHistoryList
                            .Where(x => x.OtherClientGuid.ToString() == guid)
                            .ToList().OrderByDescending(x=>x.SyncDateTime)
                            .FirstOrDefault();
            return udbpl;
        }

        public MpSyncHistory() {
            TrackHasChanged(true);
        }

        public MpSyncHistory(int SyncHistoryId) : this() {
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpSyncHistory where pk_MpSyncHistoryId=@cid",
                new System.Collections.Generic.Dictionary<string, object> {
                    { "@cid", SyncHistoryId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }

        public MpSyncHistory(DataRow dr) : this() {
            LoadDataRow(dr);
        }

        public override void LoadDataRow(DataRow dr) {
            SyncHistoryId = Convert.ToInt32(dr["pk_MpSyncHistoryId"].ToString());
            OtherClientGuid = Guid.Parse(dr["OtherClientGuid"].ToString());
            SyncDateTime = DateTime.Parse(dr["SyncDateTime"].ToString());
        }


        public override void WriteToDatabase() {
            if (OtherClientGuid == Guid.Empty) {
                OtherClientGuid = Guid.NewGuid();
            }

            if (SyncHistoryId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpSyncHistory(OtherClientGuid,SyncDateTime) values(@ocg,@sdt)",
                        new System.Collections.Generic.Dictionary<string, object> {
                            { "@ocg",OtherClientGuid.ToString() },
                        { "@sdt", SyncDateTime.ToString("yyyy-MM-dd HH:mm:ss") }
                    });
                SyncHistoryId = MpDb.Instance.GetLastRowId("MpSyncHistory", "pk_MpSyncHistoryId");
                GetAllSyncHistorys().Add(this);
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpSyncHistory set OtherClientGuid=@ocg, SyncDateTime=@sdt where pk_MpSyncHistoryId=@shid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@ocg",OtherClientGuid.ToString() },
                        { "@sdt", SyncDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                        { "@shid", SyncHistoryId }
                    });
                var c = _AllSyncHistoryList.Where(x => x.SyncHistoryId == SyncHistoryId).FirstOrDefault();
                if(c != null) {
                    _AllSyncHistoryList[_AllSyncHistoryList.IndexOf(c)] = this;
                }
            }                  
        }
    }
}
