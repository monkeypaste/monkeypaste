using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MpWpfApp {
    public class MpPasteHistory : MpDbObject {
        public static int TotalPasteHistoryCount = 0;

        public int PasteHistoryId { get; set; }
        public int CopyItemId { get; set; }
        public int DestAppId { get; set; }
        public DateTime PasteDateTime { get; set; }

        private IntPtr _destHandle;
        private MpApp _destApp;

        public MpPasteHistory(DataRow dr) {
            LoadDataRow(dr);
        }
        public MpPasteHistory(MpCopyItem ci, IntPtr destHandle) {
            PasteHistoryId = 0;
            CopyItemId = ci.CopyItemId;
            _destHandle = destHandle;
            PasteDateTime = DateTime.Now;

            WriteToDatabase();
        }
        public static List<MpPasteHistory> GetAllPasteHistory() {
            var pasteHistoryList = new List<MpPasteHistory>();
            DataTable dt = MpDb.Instance.Execute("select * from MpPasteHistory");
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow r in dt.Rows) {
                    pasteHistoryList.Add(new MpPasteHistory(r));
                }
            }
            return pasteHistoryList;
        }
        public override void LoadDataRow(DataRow dr) {
            PasteHistoryId = Convert.ToInt32(dr["pk_MpPasteHistoryId"], CultureInfo.InvariantCulture);
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"], CultureInfo.InvariantCulture);
            DestAppId = Convert.ToInt32(dr["fk_MpAppId"], CultureInfo.InvariantCulture);
            PasteDateTime = DateTime.Parse(dr["PasteDateTime"].ToString());
        }

        public override void WriteToDatabase() {
            //if new paste item (it should always be)
            if (DestAppId == 0) {
                _destApp = new MpApp(0, 0, _destHandle, false);
                DestAppId = _destApp.appId;
                //MpSingletonController.Instance.GetMpData().AddMpApp(_destApp);
            }
            if (MpDb.Instance.NoDb) {
                PasteHistoryId = ++TotalPasteHistoryCount;
                return;
            }
            MpDb.Instance.ExecuteNonQuery("insert into MpPasteHistory(fk_MpCopyItemId,fk_MpClientId,fk_MpAppId,PasteDateTime) values (" + CopyItemId + "," + MpDb.Instance.Client.ClientId + "," + DestAppId + ",'" + PasteDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "');");
            PasteHistoryId = MpDb.Instance.GetLastRowId("MpPasteHistory", "pk_MpPasteHistoryId");
        }
    }
}
