using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MpWpfApp {

    public class MpApp : MpDbObject {
        public static int TotalAppCount = 0;

        public int AppId { get; set; }
        public int IconId { get; set; }
        public string AppPath { get; set; }
        public string AppName { get; set; }
        public bool IsAppRejected { get; set; }

        public MpIcon Icon { get; set; }

        #region Static Methods
        public static List<MpApp> GetAllApps() {
            List<MpApp> apps = new List<MpApp>();
            DataTable dt = MpDb.Instance.Execute("select * from MpApp", null);
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    apps.Add(new MpApp(dr));
                }
            }
            return apps;
        }
        public static bool IsAppRejectedByHandle(IntPtr hwnd) {
            string appPath = MpHelpers.GetProcessPath(hwnd);
            foreach(MpApp app in GetAllApps()) {
                if(app.AppPath == appPath && app.IsAppRejected) {
                    return true;
                }
            }
            return false;
        }
        public static List<MpApp> GetAllRejectedApps() {
            return GetAllApps().Where(x => x.IsAppRejected == true).ToList();
        }
        #endregion
        public MpApp(String sourcePath, bool isAppRejected, string appName) {
            this.IconId = this.AppId = 0;
            this.AppPath = sourcePath;
            this.AppName = appName;
            this.IsAppRejected = isAppRejected;
            this.Icon = new MpIcon(MpHelpers.GetIconImage(this.AppPath));
        }
        //for new MpApp's set appId and iconId to 0
        public MpApp(String sourcePath, bool isAppRejected, IntPtr hwnd) {
            this.IconId = this.AppId = 0;
            this.AppPath = sourcePath;
            this.AppName = MpHelpers.GetProcessMainWindowTitle(hwnd);
            this.IsAppRejected = isAppRejected;
            this.Icon = new MpIcon(MpHelpers.GetIconImage(this.AppPath));
        }
        public MpApp(int appId, int iconId, IntPtr sourceHandle, bool isAppRejected) {
            this.AppId = appId;
            this.IconId = iconId;
            this.AppPath = MpHelpers.GetProcessPath(sourceHandle);
            this.AppName = MpHelpers.GetProcessApplicationName(sourceHandle);
            this.IsAppRejected = isAppRejected;
        }
        public MpApp() : this(string.Empty, true, IntPtr.Zero) { }

        public MpApp(DataRow dr) {
            LoadDataRow(dr);
        }
        
        public override void LoadDataRow(DataRow dr) {
            this.AppId = Convert.ToInt32(dr["pk_MpAppId"].ToString());
            this.IconId = Convert.ToInt32(dr["fk_MpIconId"].ToString());
            Icon = new MpIcon(IconId);
            this.AppPath = dr["SourcePath"].ToString();
            this.AppName = dr["AppName"].ToString();
            if (Convert.ToInt32(dr["IsAppRejected"].ToString()) == 0) {
                this.IsAppRejected = false;
            } else {
                this.IsAppRejected = true;
            }
            MapDataToColumns();
        }
        public override void WriteToDatabase() {
            bool isNew = false;
            if (this.IconId == 0) {
                Icon = new MpIcon(MpHelpers.GetIconImage(this.AppPath));
            }
            Icon.WriteToDatabase();
            this.IconId = Icon.IconId;
            if (this.AppId == 0) {
                DataTable dt = MpDb.Instance.Execute(
                    "select * from MpApp where SourcePath=@ap",
                    new Dictionary<string, object> {
                        { "@ap", AppPath }
                    });
                if (dt.Rows.Count > 0) {
                    this.AppId = Convert.ToInt32(dt.Rows[0]["pk_MpAppId"]);
                    this.IconId = Convert.ToInt32(dt.Rows[0]["fk_MpIconId"]);
                    isNew = false;
                } else {
                    MpDb.Instance.ExecuteWrite(
                        "insert into MpApp(fk_MpIconId,SourcePath,IsAppRejected,AppName) values (@iId,@sp,@iar,@an)",
                        new Dictionary<string, object> {
                            { "@iId", IconId },
                            { "@sp", AppPath },
                            { "@iar", Convert.ToInt32(IsAppRejected) },
                            { "@an", AppName }
                        });
                    this.AppId = MpDb.Instance.GetLastRowId("MpApp", "pk_MpAppId");
                    isNew = false;
                }
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpApp set fk_MpIconId=@iid, IsAppRejected=@iar, SourcePath=@sp, AppName=@an where pk_MpAppId=@aid",
                    new Dictionary<string, object> {
                        { "@iid", IconId },
                        { "@iar", Convert.ToInt32(IsAppRejected) },
                        { "@sp", AppPath },
                        { "@an", AppName },
                        { "@aid", AppId }
                    });
            }
            if (isNew) {
                MapDataToColumns();
            }
        }
        public void DeleteFromDatabase() {
            if (AppId <= 0) {
                return;
            }

            MpDb.Instance.ExecuteWrite(
                "delete from MpApp where pk_MpAppId=@aid",
                new Dictionary<string, object> {
                    { "@aid", AppId }
                });
        }

        public string GetAppName() {
            return AppPath == null || AppPath == string.Empty ? "None" : Path.GetFileNameWithoutExtension(AppPath);
        }
        private void MapDataToColumns() {
            TableName = "MpApp";
            columnData.Add("pk_MpAppId", this.AppId);
            columnData.Add("fk_MpIconId", this.IconId);
            columnData.Add("SourcePath", this.AppPath);
            columnData.Add("IsAppRejected", this.IsAppRejected);
        }
    }
}
