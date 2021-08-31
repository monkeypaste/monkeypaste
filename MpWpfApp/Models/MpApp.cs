using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using MonkeyPaste;
using SQLite;
using FFImageLoading.Helpers.Exif;

namespace MpWpfApp {
    public class MpApp : MpDbModelBase, MonkeyPaste.MpISyncableDbObject {
        private static List<MpApp> _AllAppList = null;
        public static int TotalAppCount = 0;

        public int AppId { get; set; } = 0;
        public Guid AppGuid { get; set; }

        public int UserDeviceId { get; set; }

        public int IconId { get; set; }

        public string AppPath { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public bool IsAppRejected { get; set; } = false;

        public MpIcon Icon { get; set; }

        public BitmapSource IconImage { 
            get {
                return Icon.IconImage;
            }
            set {
                Icon.IconImage = value;
            }
        }
        public BitmapSource IconBorderImage {
            get {
                return Icon.IconBorderImage;
            }
            set {
                Icon.IconBorderImage = value;
            }
        }
        public BitmapSource IconHighlightBorderImage {
            get {
                return Icon.IconBorderHighlightImage;
            }
            set {
                Icon.IconBorderHighlightImage = value;
            }
        }
        public BitmapSource IconSelectedHighlightBorderImage {
            get {
                return Icon.IconBorderHighlightSelectedImage;
            }
            set {
                Icon.IconBorderHighlightSelectedImage = value;
            }
        }

        #region Static Methods
        public static List<MpApp> GetAllApps() {            
            if(_AllAppList == null) {
                _AllAppList = new List<MpApp>();
                DataTable dt = MpDb.Instance.Execute("select * from MpApp", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        _AllAppList.Add(new MpApp(dr));
                    }
                }
            }
            return _AllAppList;
        }
        public static MpApp GetAppById(int appId) {
            return GetAllApps().Where(x => x.AppId == appId).FirstOrDefault();
        }

        public static MpApp GetAppByGuid(string appGuid) {
            return GetAllApps().Where(x => x.AppGuid.ToString() == appGuid).FirstOrDefault();
        }

        public static MpApp GetAppByHandle(IntPtr handle) {
            string appPath = MpHelpers.Instance.GetProcessPath(handle);
            return GetAllApps().Where(x => x.AppPath.ToLower() == appPath.ToLower()).FirstOrDefault();
        }

        public static MpApp GetAppByPath(string appPath, string deviceGuid = "") {
            if(string.IsNullOrEmpty(deviceGuid)) {
                deviceGuid = Properties.Settings.Default.ThisDeviceGuid;
            }
            var device = MpUserDevice.GetUserDeviceByGuid(deviceGuid);
            if(device == null) {
                return null;
            }
            return GetAllApps().Where(x => x.AppPath.ToLower() == appPath.ToLower() && x.UserDeviceId == device.UserDeviceId).FirstOrDefault();
        }

        public static bool IsAppRejectedByHandle(IntPtr hwnd) {
            string appPath = MpHelpers.Instance.GetProcessPath(hwnd);
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

        public static MpApp Create(object source, bool isAppRejected = false) {
            MpApp newApp = new MpApp();

            if (source == null) {
                newApp.AppPath = MpHelpers.Instance.GetApplicationProcessPath();
                newApp.AppName = Properties.Settings.Default.ApplicationName;
            } else if (source is string) {
                newApp.AppPath = source as string;
                if (MpHelpers.Instance.IsValidUrl(newApp.AppPath)) {
                    newApp.AppName = MpHelpers.Instance.GetUrlDomain(newApp.AppPath);
                    newApp.Icon = new MpIcon(MpHelpers.Instance.GetUrlFavicon(newApp.AppPath));
                } else {
                    newApp.AppName = Path.GetFileNameWithoutExtension(newApp.AppPath);
                }
            } else if (source is IntPtr hwnd) {
                newApp.AppPath = MpHelpers.Instance.GetProcessPath(hwnd);
                newApp.AppName = MpHelpers.Instance.GetProcessApplicationName(hwnd);
            }

            var dupApp = MpApp.GetAppByPath(newApp.AppPath);
            if(dupApp != null) {
                if(dupApp.IsAppRejected != isAppRejected) {
                    dupApp.IsAppRejected = isAppRejected;
                }
                return dupApp;
            }
            //used for for new app sources when copy item added from clipboard
            newApp.AppGuid = Guid.NewGuid();
            newApp.IsAppRejected = isAppRejected;
            if (newApp.Icon == null) {
                newApp.Icon = new MpIcon(MpHelpers.Instance.GetIconImage(newApp.AppPath));
            }
            newApp.UserDeviceId = MpUserDevice.GetUserDeviceByGuid(Properties.Settings.Default.ThisDeviceGuid).UserDeviceId;

            return newApp;
        }
        #endregion

        public MpApp() { }

        public MpApp(DataRow dr) : this() {
            LoadDataRow(dr);
        }
        
        public override void LoadDataRow(DataRow dr) {
            AppId = Convert.ToInt32(dr["pk_MpAppId"].ToString());
            UserDeviceId = Convert.ToInt32(dr["fk_MpUserDeviceId"].ToString());
            AppGuid = Guid.Parse(dr["MpAppGuid"].ToString());
            AppPath = dr["SourcePath"].ToString();
            AppName = dr["AppName"].ToString();
            IconId = Convert.ToInt32(dr["fk_MpIconId"].ToString());
            Icon = new MpIcon(IconId);
            IsAppRejected = Convert.ToInt32(dr["IsAppRejected"].ToString()) == 1;
        }

        public override void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }
        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            Icon.WriteToDatabase();
            IconId = Icon.IconId;

            if (AppId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpApp(fk_MpUserDeviceId,MpAppGuid,fk_MpIconId,SourcePath,IsAppRejected,AppName) " +
                        "values (@udid,@ag,@iid,@sp,@iar,@an)",
                        new Dictionary<string, object> {
                            { "@ag", AppGuid.ToString() },
                            { "@iid", IconId },
                            { "@udid", UserDeviceId },
                            { "@sp", AppPath },
                            { "@iar", Convert.ToInt32(IsAppRejected) },
                            { "@an", AppName },
                        },AppGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                AppId = MpDb.Instance.GetLastRowId("MpApp", "pk_MpAppId");
                GetAllApps().Add(this);
            } else {
                MpDb.Instance.ExecuteWrite(
                    //"update MpApp set IconBlob=@ib, IconBorderBlob=@ibb,IconSelectedHighlightBorderBlob=@ishbb,IconHighlightBorderBlob=@ihbb, IsAppRejected=@iar, SourcePath=@sp, AppName=@an, fk_MpColorId1=@c1,fk_MpColorId2=@c2,fk_MpColorId3=@c3,fk_MpColorId4=@c4,fk_MpColorId5=@c5 where pk_MpAppId=@aid",
                    "update MpApp set fk_MpUserDeviceId=@udid, MpAppGuid=@ag,fk_MpIconId=@iid, IsAppRejected=@iar, SourcePath=@sp, AppName=@an where pk_MpAppId=@aid",
                    new Dictionary<string, object> {
                            { "@ag", AppGuid.ToString() },
                            { "@iid", IconId },
                            { "@udid", UserDeviceId },
                            { "@iar", Convert.ToInt32(IsAppRejected) },
                            { "@sp", AppPath },
                            { "@an", AppName },
                            { "@aid", AppId }
                    }, AppGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                var cit = GetAllApps().Where(x => x.AppId == AppId).FirstOrDefault();
                if (cit != null) {
                    _AllAppList[_AllAppList.IndexOf(cit)] = this;
                }
            }
            MpAppCollectionViewModel.Instance.Refresh();
        }
        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }

        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (AppId <= 0) {
                return;
            }
            Icon.DeleteFromDatabase();

            MpDb.Instance.ExecuteWrite(
                "delete from MpApp where pk_MpAppId=@aid",
                new Dictionary<string, object> {
                    { "@aid", AppId }
                }, AppGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);

            GetAllApps().Remove(this);
        }

        public string GetAppName() {
            return AppPath == null || AppPath == string.Empty ? "None" : Path.GetFileNameWithoutExtension(AppPath);
        }

        public async Task<object> CreateFromLogs(string appGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            await Task.Delay(1);
            var adr = MpDb.Instance.GetDbDataRowByTableGuid("MpApp", appGuid);
            MpApp appFromLog = null;
            if (adr == null) {
                appFromLog = new MpApp();
            } else {
                appFromLog = new MpApp(adr);
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpAppGuid":
                        appFromLog.AppGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "fk_MpUserDeviceId":
                        var uddr = MpDb.Instance.GetDbDataRowByTableGuid("MpUserDevice", li.AffectedColumnValue);
                        appFromLog.UserDeviceId = new MpUserDevice(uddr).UserDeviceId;
                        break;
                    case "fk_MpIconId":
                        var idr = MpDb.Instance.GetDbDataRowByTableGuid("MpIcon", li.AffectedColumnValue);
                        appFromLog.Icon = new MpIcon(idr);
                        appFromLog.IconId = appFromLog.Icon.IconId;
                        break;
                    case "SourcePath":
                        appFromLog.AppPath = li.AffectedColumnValue;
                        break;
                    case "AppName":
                        appFromLog.AppName = li.AffectedColumnValue;
                        break;
                    case "IsAppRejected":
                        appFromLog.IsAppRejected = li.AffectedColumnValue == "1";
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            //newCopyItemTag.WriteToDatabase(fromClientGuid);
            return appFromLog;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var a = new MpApp() {
                AppGuid = System.Guid.Parse(objParts[0])
            };
            var ud = MpDb.Instance.GetDbObjectByTableGuid("MpUserDevice", objParts[1]) as MpUserDevice;
            a.Icon = MpDb.Instance.GetDbObjectByTableGuid("MpIcon", objParts[2]) as MpIcon;
            a.UserDeviceId = ud.UserDeviceId;
            a.IconId = a.Icon.IconId;

            a.AppPath = objParts[3];
            a.AppName = objParts[4];
            a.IsAppRejected = objParts[5] == "1";
            return a;
        }

        public string SerializeDbObject() {
            var udg = MpUserDevice.GetUserDeviceById(UserDeviceId).UserDeviceGuid.ToString();
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}",
                ParseToken,
                AppGuid.ToString(),
                udg,
                Icon.IconGuid.ToString(),
                AppPath,
                AppName,
                IsAppRejected ? "1":"0");
        }

        public Type GetDbObjectType() {
            return typeof(MpApp);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            var udg = MpUserDevice.GetUserDeviceById(UserDeviceId).UserDeviceGuid.ToString();
            MpApp other = null;
            if (drOrModel is DataRow) {
                other = new MpApp(drOrModel as DataRow);
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpApp();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(AppGuid, other.AppGuid,
                "MpAppGuid",
                diffLookup,
                AppGuid.ToString());
            diffLookup = CheckValue(UserDeviceId, other.UserDeviceId,
                "fk_MpUserDeviceId",
                diffLookup,
                udg);
            diffLookup = CheckValue(IconId, other.IconId,
                "fk_MpIconId",
                diffLookup,
                Icon.IconGuid.ToString());
            diffLookup = CheckValue(AppPath, other.AppPath,
                "SourcePath",
                diffLookup);
            diffLookup = CheckValue(AppName, other.AppName,
                "AppName",
                diffLookup);
            diffLookup = CheckValue(IsAppRejected, other.IsAppRejected,
                "IsAppRejected",
                diffLookup,
                IsAppRejected ? "1" : "0");
            return diffLookup;
        }
    }
}
