using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Media.Imaging;

namespace MpWpfApp {

    public class MpApp : MpDbObject {
        
        public static int TotalAppCount = 0;

        public int AppId { get; set; } = 0;
        public string AppPath { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public bool IsAppRejected { get; set; } = false;

        public BitmapSource IconImage { get; set; }
        //public MpIcon Icon { get; set; }

        #region Static Methods
        public static List<MpApp> GetAllApps() {
            var apps = new List<MpApp>();
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

        public MpApp(bool isAppRejected, IntPtr hwnd) {
            AppPath = MpHelpers.GetProcessPath(hwnd);
            AppName = MpHelpers.GetProcessMainWindowTitle(hwnd);
            IsAppRejected = isAppRejected;
            IconImage = MpHelpers.GetIconImage(AppPath);
        }
        public MpApp(string appPath) {
            //only called when user selects rejected app in settings
            AppPath = appPath;
            AppName = appPath;
            IsAppRejected = true;
            IconImage = MpHelpers.GetIconImage(AppPath);
        }
        public MpApp() : this(false, IntPtr.Zero) { }

        public MpApp(DataRow dr) {
            LoadDataRow(dr);
        }
        
        public override void LoadDataRow(DataRow dr) {
            AppId = Convert.ToInt32(dr["pk_MpAppId"].ToString());
            AppPath = dr["SourcePath"].ToString();
            AppName = dr["AppName"].ToString();
            IconImage = MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["IconBlob"]);
            if (Convert.ToInt32(dr["IsAppRejected"].ToString()) == 0) {
                IsAppRejected = false;
            } else {
                IsAppRejected = true;
            }
        }
        public override void WriteToDatabase() {
            if (AppId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpApp(IconBlob,SourcePath,IsAppRejected,AppName) values (@ib,@sp,@iar,@an)",
                        new Dictionary<string, object> {
                            { "@ib", MpHelpers.ConvertBitmapSourceToByteArray(IconImage) },
                            { "@sp", AppPath },
                            { "@iar", Convert.ToInt32(IsAppRejected) },
                            { "@an", AppName }
                        });
                AppId = MpDb.Instance.GetLastRowId("MpApp", "pk_MpAppId");
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpApp set IconBlob=@ib, IsAppRejected=@iar, SourcePath=@sp, AppName=@an where pk_MpAppId=@aid",
                    new Dictionary<string, object> {
                        { "@ib", MpHelpers.ConvertBitmapSourceToByteArray(IconImage) },
                        { "@iar", Convert.ToInt32(IsAppRejected) },
                        { "@sp", AppPath },
                        { "@an", AppName },
                        { "@aid", AppId }
                    });
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
    }
}
