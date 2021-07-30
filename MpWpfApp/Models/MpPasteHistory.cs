using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MpWpfApp {
    public class MpPasteHistory : MpDbModelBase {
        public static int TotalPasteHistoryCount = 0;

        public int PasteHistoryId { get; set; }
        public Guid SourceClientGuid { get; set; }

        public int CopyItemId { get; set; }
        public int DestAppId { get; set; }
        public DateTime PasteDateTime { get; set; }

        private IntPtr _destHandle;
        //private MpApp _destApp;

        public MpPasteHistory(DataRow dr) {
            LoadDataRow(dr);
        }
        public MpPasteHistory(MpCopyItem ci, IntPtr destHandle) {
            PasteHistoryId = 0;
            SourceClientGuid = Guid.Parse(Properties.Settings.Default.ThisClientGuid);

            CopyItemId = ci.CopyItemId;
            _destHandle = destHandle;
            PasteDateTime = DateTime.Now;

            WriteToDatabase();
        }
        public static List<MpPasteHistory> GetAllPasteHistory() {
            var pasteHistoryList = new List<MpPasteHistory>();
            DataTable dt = MpDb.Instance.Execute("select * from MpPasteHistory", null);
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow r in dt.Rows) {
                    pasteHistoryList.Add(new MpPasteHistory(r));
                }
            }
            return pasteHistoryList;
        }
        public override void LoadDataRow(DataRow dr) {
            PasteHistoryId = Convert.ToInt32(dr["pk_MpPasteHistoryId"], CultureInfo.InvariantCulture);
            SourceClientGuid = Guid.Parse(dr["SourceClientGuid"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"], CultureInfo.InvariantCulture);
            DestAppId = Convert.ToInt32(dr["fk_MpAppId"], CultureInfo.InvariantCulture);
            PasteDateTime = DateTime.Parse(dr["PasteDateTime"].ToString());
        }

        public override void WriteToDatabase() {
            // TODO use _destHandle to find app but this tracking paste history is not critical so leaving DestAppId to null for now
            MpDb.Instance.ExecuteWrite(
                "insert into MpPasteHistory(SourceClientGuid,fk_MpCopyItemId,fk_MpClientId,PasteDateTime) values (@phg,@ciid, @cid, @pdt)",
                new Dictionary<string, object> {
                    { "@phg", SourceClientGuid.ToString() },
                    { "@ciid", CopyItemId },
                    { "@cid", MpDb.Instance.Client.ClientId },
                    { "@pdt", PasteDateTime.ToString("yyyy-MM-dd HH:mm:ss") }
                },SourceClientGuid.ToString());
            PasteHistoryId = MpDb.Instance.GetLastRowId("MpPasteHistory", "pk_MpPasteHistoryId");
        }
    }
}
