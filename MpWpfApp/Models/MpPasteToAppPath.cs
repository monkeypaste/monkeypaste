using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpPasteToAppPath : MpDbObject {
        public int PasteToAppPathId { get; set; }
        public string AppPath { get; set; }
        public string AppName { get; set; }
        public bool IsAdmin { get; set; }

        public static List<MpPasteToAppPath> GetAllPasteToAppPaths() {
            var pasteToAppPathList = new List<MpPasteToAppPath>();
            DataTable dt = MpDb.Instance.Execute("select * from MpPasteToAppPath", null);
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    pasteToAppPathList.Add(new MpPasteToAppPath(dr));
                }
            }
            return pasteToAppPathList;
        }

        public MpPasteToAppPath(int pasteToAppPathId) {
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpPasteToAppPath where pk_MpPasteToAppPathId=@cid",
                new System.Collections.Generic.Dictionary<string, object> {
                    { "@cid", pasteToAppPathId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }

        public MpPasteToAppPath(string appPath, string appName, bool isAdmin) {
            AppPath = appPath;
            AppName = appName;
            IsAdmin = isAdmin;
        }
        public MpPasteToAppPath() : this(string.Empty,string.Empty,false) { }

        public MpPasteToAppPath(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            PasteToAppPathId = Convert.ToInt32(dr["pk_MpPasteToAppPathId"].ToString());
            AppPath = dr["AppPath"].ToString();
            AppName = dr["AppName"].ToString();
            IsAdmin = Convert.ToInt32(dr["IsAdmin"].ToString()) > 0 ? true : false;
        }

        public void DeleteFromDatabase() {
            if (PasteToAppPathId <= 0) {
                return;
            }

            MpDb.Instance.ExecuteWrite(
                "delete from MpPasteToAppPath where pk_MpPasteToAppPathId=@cid",
                new Dictionary<string, object> {
                    { "@cid", PasteToAppPathId }
                });
        }

        public override void WriteToDatabase() {
            if (PasteToAppPathId == 0) {
                DataTable dt = MpDb.Instance.Execute(
                    "select * from MpPasteToAppPath where AppPath=@ap and IsAdmin=@ia",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@ap", AppPath },
                        { "@ia", IsAdmin ? 1:0 }
                    });
                if (dt != null && dt.Rows.Count > 0) {
                    PasteToAppPathId = Convert.ToInt32(dt.Rows[0]["pk_MpPasteToAppPathId"].ToString());
                } else {
                    MpDb.Instance.ExecuteWrite(
                        "insert into MpPasteToAppPath(AppPath,AppName,IsAdmin) values(@ap,@an,@ia)",
                        new System.Collections.Generic.Dictionary<string, object> {
                        { "@ap", AppPath },
                        { "@an",AppName },
                        { "@ia", IsAdmin ? 1:0 }
                    });
                    PasteToAppPathId = MpDb.Instance.GetLastRowId("MpPasteToAppPath", "pk_MpPasteToAppPathId");
                }
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpPasteToAppPath set AppPath=@ap, AppName=@an, IsAdmin=@ia where pk_MpPasteToAppPathId=@cid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@ap", AppPath },
                        { "@an",AppName },
                        { "@ia", IsAdmin ? 1:0 },
                        { "@cid", PasteToAppPathId }
                    });
            }
        }
    }
}
