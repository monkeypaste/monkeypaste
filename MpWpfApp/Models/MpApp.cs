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

namespace MpWpfApp {

    public class MpApp : MpDbModelBase {
        private static List<MpApp> _AllAppList = null;
        public static int TotalAppCount = 0;

        public int AppId { get; set; } = 0;
        public Guid AppGuid { get; set; }
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
            if (_AllAppList == null) {
                GetAllApps();
            }
            var udbpl = _AllAppList.Where(x => x.AppId == appId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
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
            AppGuid = Guid.NewGuid();
            AppPath = MpHelpers.Instance.GetProcessPath(hwnd);
            AppName = MpHelpers.Instance.GetProcessApplicationName(hwnd);
            IsAppRejected = isAppRejected;
            Icon = new MpIcon(MpHelpers.Instance.GetIconImage(AppPath));

            //IconImage = MpHelpers.Instance.GetIconImage(AppPath);
            //IconBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio,Colors.White);
            //IconHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Yellow);
            //IconSelectedHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Pink);
            //PrimaryIconColorList = MpColor.CreatePrimaryColorList(IconImage);
        }

        public MpApp(string appPath) : this() {
            //only called when user selects rejected app in settings
            AppGuid = Guid.NewGuid();
            AppPath = appPath;
            AppName = appPath;
            IsAppRejected = true;
            Icon = new MpIcon(MpHelpers.Instance.GetIconImage(AppPath));

            //IconImage = MpHelpers.Instance.GetIconImage(AppPath);
            //IconBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.White);
            //IconHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Yellow);
            //IconSelectedHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Pink);
            //PrimaryIconColorList = MpColor.CreatePrimaryColorList(IconImage);
        }

        public MpApp(string url, bool isDomainRejected) : this() {
            AppGuid = Guid.NewGuid();
            AppPath = url;
            AppName = MpHelpers.Instance.GetUrlDomain(url);
            IsAppRejected = isDomainRejected;
            Icon = new MpIcon(MpHelpers.Instance.GetUrlFavicon(url));
            //IconImage = MpHelpers.Instance.GetUrlFavicon(url);
            //IconBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.White);
            //IconHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Yellow);
            //IconSelectedHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Pink); 
            //PrimaryIconColorList = MpColor.CreatePrimaryColorList(IconImage);
        }

        public MpApp() { }

        public MpApp(DataRow dr) : this() {
            LoadDataRow(dr);
        }
        
        public override void LoadDataRow(DataRow dr) {
            AppId = Convert.ToInt32(dr["pk_MpAppId"].ToString());
            AppGuid = Guid.Parse(dr["MpAppGuid"].ToString());
            AppPath = dr["SourcePath"].ToString();
            AppName = dr["AppName"].ToString();
            IconId = Convert.ToInt32(dr["fk_MpIconId"].ToString());
            Icon = new MpIcon(IconId);
            //IconImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBlob"]);
            //IconBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBorderBlob"]);
            //IconSelectedHighlightBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconSelectedHighlightBorderBlob"]);
            //IconHighlightBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconHighlightBorderBlob"]);
            IsAppRejected = Convert.ToInt32(dr["IsAppRejected"].ToString()) == 1;

            //PrimaryIconColorList.Clear();
            //for (int i = 0; i < 5; i++) {
            //    ColorId[i] = Convert.ToInt32(dr["fk_MpColorId"+(i+1)].ToString());
            //    if(ColorId[i] > 0) {
            //        PrimaryIconColorList.Add(new MpColor(ColorId[i]));
            //    }
            //}      
        }

        private bool IsAltered() {
            var dt = MpDb.Instance.Execute(
                @"SELECT pk_MpAppId FROM MpApp WHERE MpAppGuid=@ag AND SourcePath=@sp AND AppName=@an AND IsAppRejected=@iar AND fk_MpIconId=@iid",
                new Dictionary<string, object> {
                    { "@ag", AppGuid.ToString() },
                    { "@sp", AppPath },
                    { "@an", AppName },
                    { "@iar", Convert.ToInt32(IsAppRejected) },
                    { "@iid", IconId },
                });
            return dt.Rows.Count == 0;
        }

        public override void WriteToDatabase() {
            //for (int i = 1; i <= PrimaryIconColorList.Count; i++) {
            //    var c = PrimaryIconColorList[i-1];
            //    c.WriteToDatabase();
            //    ColorId[i - 1] = c.ColorId;
            //}
            if(!HasChanged) {
                return;
            }
            Icon.WriteToDatabase();
            IconId = Icon.IconId;

            if (AppId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpApp(MpAppGuid,fk_MpIconId,SourcePath,IsAppRejected,AppName) " +
                        "values (@ag,@iid,@sp,@iar,@an)",
                        new Dictionary<string, object> {
                            { "@ag", AppGuid.ToString() },
                            { "@iid", IconId },
                            { "@sp", AppPath },
                            { "@iar", Convert.ToInt32(IsAppRejected) },
                            { "@an", AppName },
                        },AppGuid.ToString());
                AppId = MpDb.Instance.GetLastRowId("MpApp", "pk_MpAppId");                
            } else {
                MpDb.Instance.ExecuteWrite(
                    //"update MpApp set IconBlob=@ib, IconBorderBlob=@ibb,IconSelectedHighlightBorderBlob=@ishbb,IconHighlightBorderBlob=@ihbb, IsAppRejected=@iar, SourcePath=@sp, AppName=@an, fk_MpColorId1=@c1,fk_MpColorId2=@c2,fk_MpColorId3=@c3,fk_MpColorId4=@c4,fk_MpColorId5=@c5 where pk_MpAppId=@aid",
                    "update MpApp set MpAppGuid=@ag,fk_MpIconId=@iid, IsAppRejected=@iar, SourcePath=@sp, AppName=@an where pk_MpAppId=@aid",
                    new Dictionary<string, object> {
                        { "@ag", AppGuid.ToString() },
                            { "@iid", IconId },
                        { "@iar", Convert.ToInt32(IsAppRejected) },
                        { "@sp", AppPath },
                        { "@an", AppName },
                        { "@aid", AppId }
                    }, AppGuid.ToString());
            }

            var al = GetAllApps().Where(x => x.AppId == AppId).ToList();
            if (al.Count > 0) {
                _AllAppList[_AllAppList.IndexOf(al[0])] = this;
            } else {
                _AllAppList.Add(this);
            }

            MpAppCollectionViewModel.Instance.Refresh();
        }

        public void DeleteFromDatabase() {
            if (AppId <= 0) {
                return;
            }
            Icon.DeleteFromDatabase();

            MpDb.Instance.ExecuteWrite(
                "delete from MpApp where pk_MpAppId=@aid",
                new Dictionary<string, object> {
                    { "@aid", AppId }
                }, AppGuid.ToString());

            var al = GetAllApps().Where(x => x.AppId == AppId).ToList();
            if (al.Count > 0) {
                _AllAppList.RemoveAt(_AllAppList.IndexOf(al[0]));
            }
        }

        public string GetAppName() {
            return AppPath == null || AppPath == string.Empty ? "None" : Path.GetFileNameWithoutExtension(AppPath);
        }
    }
}
