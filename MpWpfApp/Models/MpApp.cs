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

namespace MpWpfApp {
    public class MpApp : MpDbModelBase {
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
        //public BitmapSource IconBorderImage { get; set; } = new BitmapImage();
        //public BitmapSource IconHighlightBorderImage { get; set; } = new BitmapImage();
        //public BitmapSource IconSelectedHighlightBorderImage { get; set; } = new BitmapImage();

        //public int[] ColorId = new int[5];

        //public MpObservableCollection<MpColor> PrimaryIconColorList = new MpObservableCollection<MpColor>();

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

        public static MpApp GetAppByHandle(IntPtr handle) {
            string appPath = MpHelpers.Instance.GetProcessPath(handle);
            return GetAllApps().Where(x => x.AppPath.ToLower() == appPath.ToLower()).FirstOrDefault();
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
        #endregion

        public MpApp(bool isAppRejected, IntPtr hwnd) : this() {
            //used for for new app sources when copy item added from clipboard
            AppGuid = Guid.NewGuid();
            AppPath = MpHelpers.Instance.GetProcessPath(hwnd);
            AppName = MpHelpers.Instance.GetProcessApplicationName(hwnd);
            IsAppRejected = isAppRejected;
            Icon = new MpIcon(MpHelpers.Instance.GetIconImage(AppPath));
            UserDeviceId = MpUserDevice.GetUserDeviceByGuid(Properties.Settings.Default.ThisClientGuid).UserDeviceId;
        }

        public MpApp(string appPath) : this() {
            //only called when user selects rejected app in settings
            AppGuid = Guid.NewGuid();
            AppPath = appPath;
            AppName = appPath;
            IsAppRejected = true;
            Icon = new MpIcon(MpHelpers.Instance.GetIconImage(AppPath)); 
            UserDeviceId = MpUserDevice.GetUserDeviceByGuid(Properties.Settings.Default.ThisClientGuid).UserDeviceId;
        }

        public MpApp(string url, bool isDomainRejected) : this() {
            //experimental, will be used to block url domains 
            AppGuid = Guid.NewGuid();
            AppPath = url;
            AppName = MpHelpers.Instance.GetUrlDomain(url);
            IsAppRejected = isDomainRejected;
            Icon = new MpIcon(MpHelpers.Instance.GetUrlFavicon(url));
        }

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
                WriteToDatabase(Properties.Settings.Default.ThisClientGuid);
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
                DeleteFromDatabase(Properties.Settings.Default.ThisClientGuid);
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
    }
}
