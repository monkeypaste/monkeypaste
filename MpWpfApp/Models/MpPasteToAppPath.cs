using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpPasteToAppPath : MpDbObject {
        public int PasteToAppPathId { get; set; }
        public string AppPath { get; set; }
        public string AppName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSilent { get; set; }
        public bool PressEnter { get; set; }
        public string Args { get; set; }
        public string Label { get; set; }
        public BitmapSource Icon { get; set; }
        public WinApi.ShowWindowCommands WindowState { get; set; }

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

        public MpPasteToAppPath(string appPath, string appName, bool isAdmin, bool isSilent = false, string label = "", string args = "", BitmapSource icon = null, WinApi.ShowWindowCommands windowState = WinApi.ShowWindowCommands.Normal, bool pressEnter = false) {
            AppPath = appPath;
            AppName = appName;
            IsAdmin = isAdmin;
            IsSilent = isSilent;
            Label = label;
            Args = args;
            Icon = icon;
            WindowState = windowState;
            PressEnter = pressEnter;
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
            IsSilent = Convert.ToInt32(dr["IsSilent"].ToString()) > 0 ? true : false;
            PressEnter = Convert.ToInt32(dr["PressEnter"].ToString()) > 0 ? true : false;
            WindowState = (WinApi.ShowWindowCommands)Convert.ToInt32(dr["WindowState"].ToString());
            if (dr["IconBlob"] != null && dr["IconBlob"].GetType() != typeof(System.DBNull)) {
                Icon = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBlob"]);
            } else {
                Icon = null;
            }
            Label = dr["Label"].ToString();
            Args = dr["Args"].ToString();
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
                    "select * from MpPasteToAppPath where AppPath=@ap and IsAdmin=@ia and IsSilent=@is and Args=@a",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@ap", AppPath },
                        { "@ia", IsAdmin ? 1:0 },
                        { "@a", Args },
                        { "@is", IsSilent ? 1:0 }
                    });
                if (dt != null && dt.Rows.Count > 0) {
                    PasteToAppPathId = Convert.ToInt32(dt.Rows[0]["pk_MpPasteToAppPathId"].ToString());
                } else {
                    MpDb.Instance.ExecuteWrite(
                        "insert into MpPasteToAppPath(AppPath,AppName,IsAdmin,Label,Args,IconBlob,IsSilent,WindowState,PressEnter) values(@ap,@an,@ia,@l,@a,@ib,@is,@ws,@pe)",
                        new System.Collections.Generic.Dictionary<string, object> {
                        { "@ap", AppPath },
                        { "@an",AppName },
                        { "@ia", IsAdmin ? 1:0 },
                        { "@l", Label },
                        { "@a", Args },
                        { "@ib", MpHelpers.Instance.ConvertBitmapSourceToByteArray(Icon) },
                        { "@is", IsSilent ? 1:0 },
                        { "@ws", (int)WindowState },
                        {"@pe", PressEnter ? 1:0 }
                    });
                    PasteToAppPathId = MpDb.Instance.GetLastRowId("MpPasteToAppPath", "pk_MpPasteToAppPathId");
                }
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpPasteToAppPath set AppPath=@ap, AppName=@an, IsAdmin=@ia, IsSilent=@is, Label=@l, Args=@a, IconBlob=@ib, WindowState=@ws, PressEnter=@pe where pk_MpPasteToAppPathId=@cid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@ap", AppPath },
                        { "@an",AppName },
                        { "@ia", IsAdmin ? 1:0 },
                        { "@cid", PasteToAppPathId },
                        { "@l", Label },
                        { "@a", Args },
                        { "@ib", MpHelpers.Instance.ConvertBitmapSourceToByteArray(Icon) },
                        { "@is", IsSilent ? 1:0 },
                        { "@ws", (int)WindowState },
                        { "@pe", PressEnter ? 1:0 }
                    });
            }
        }
    }
}
