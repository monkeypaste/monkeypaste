using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpPasteHistory:MpDbObject {
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
        public MpPasteHistory(MpCopyItem ci,IntPtr destHandle) {
            PasteHistoryId = 0;
            CopyItemId = ci.CopyItemId;
            _destHandle = destHandle;
            PasteDateTime = DateTime.Now;

            WriteToDatabase();
        }
        public override void LoadDataRow(DataRow dr) {
            PasteHistoryId = Convert.ToInt32(dr["pk_MpPasteHistoryId"]);
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"]);
            DestAppId = Convert.ToInt32(dr["fk_MpAppId"]);
            PasteDateTime = DateTime.Parse(dr["PasteDateTime"].ToString());
        }

        public override void WriteToDatabase() {
            //if new paste item (it should always be)
            if(DestAppId == 0) {
                _destApp = new MpApp(0,0,_destHandle,false);
                DestAppId = _destApp.appId;
                //MpSingletonController.Instance.GetMpData().AddMpApp(_destApp);
            }
            if(MpApplication.Instance.DataModel.Db.NoDb) {
                PasteHistoryId = ++TotalPasteHistoryCount;
                return;
            }
            MpApplication.Instance.DataModel.Db.ExecuteNonQuery("insert into MpPasteHistory(fk_MpCopyItemId,fk_MpClientId,fk_MpAppId,PasteDateTime) values (" + CopyItemId + "," + MpApplication.Instance.DataModel.Db.Client.ClientId +","+DestAppId+ ",'" + PasteDateTime.ToString("yyyy-MM-dd HH:mm:ss") +"');");
            PasteHistoryId = MpApplication.Instance.DataModel.Db.GetLastRowId("MpPasteHistory","pk_MpPasteHistoryId");
        }

    }
}
