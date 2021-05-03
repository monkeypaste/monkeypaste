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

    public class MpApp : MpDbObject {
        private static List<MpApp> _AllAppList = null;
        public static int TotalAppCount = 0;

        public int AppId { get; set; } = 0;
        public string AppPath { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public bool IsAppRejected { get; set; } = false;
        public BitmapSource IconImage { get; set; } = new BitmapImage();
        public BitmapSource IconBorderImage { get; set; } = new BitmapImage();
        public BitmapSource IconHighlightBorderImage { get; set; } = new BitmapImage();
        public BitmapSource IconSelectedHighlightBorderImage { get; set; } = new BitmapImage();

        public int[] ColorId = new int[5];

        public MpObservableCollection<MpColor> PrimaryIconColorList = new MpObservableCollection<MpColor>();

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

        public MpApp(bool isAppRejected, IntPtr hwnd) {
            AppPath = MpHelpers.Instance.GetProcessPath(hwnd);
            AppName = MpHelpers.Instance.GetProcessApplicationName(hwnd);
            IsAppRejected = isAppRejected;
            IconImage = MpHelpers.Instance.GetIconImage(AppPath);
            IconBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio,Colors.White);
            IconHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Yellow);
            IconSelectedHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Pink);
            PrimaryIconColorList = MpColor.CreatePrimaryColorList(IconImage);
        }
        public MpApp(string appPath) {
            //only called when user selects rejected app in settings
            AppPath = appPath;
            AppName = appPath;
            IsAppRejected = true;
            IconImage = MpHelpers.Instance.GetIconImage(AppPath);
            IconBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.White);
            IconHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Yellow);
            IconSelectedHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Pink);
            PrimaryIconColorList = MpColor.CreatePrimaryColorList(IconImage);
        }

        public MpApp(string url, bool isDomainRejected) {
            AppPath = url;
            AppName = MpHelpers.Instance.GetUrlDomain(url);
            IsAppRejected = isDomainRejected;
            IconImage = MpHelpers.Instance.GetUrlFavicon(url);
            IconBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.White);
            IconHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Yellow);
            IconSelectedHighlightBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Pink); 
            PrimaryIconColorList = MpColor.CreatePrimaryColorList(IconImage);
        }

        public MpApp() { }

        public MpApp(DataRow dr) {
            LoadDataRow(dr);
        }
        
        public override void LoadDataRow(DataRow dr) {
            AppId = Convert.ToInt32(dr["pk_MpAppId"].ToString());
            AppPath = dr["SourcePath"].ToString();
            AppName = dr["AppName"].ToString();
            IconImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBlob"]);
            IconBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBorderBlob"]);
            IconSelectedHighlightBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconSelectedHighlightBorderBlob"]);
            IconHighlightBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconHighlightBorderBlob"]);
            IsAppRejected = Convert.ToInt32(dr["IsAppRejected"].ToString()) == 1;

            PrimaryIconColorList.Clear();
            for (int i = 0; i < 5; i++) {
                ColorId[i] = Convert.ToInt32(dr["fk_MpColorId"+(i+1)].ToString());
                if(ColorId[i] > 0) {
                    PrimaryIconColorList.Add(new MpColor(ColorId[i]));
                }
            }      
        }

        public override void WriteToDatabase() {
            for (int i = 1; i <= PrimaryIconColorList.Count; i++) {
                var c = PrimaryIconColorList[i-1];
                c.WriteToDatabase();
                ColorId[i - 1] = c.ColorId;
            }
            if (AppId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpApp(IconBlob,IconBorderBlob,IconSelectedHighlightBorderBlob,IconHighlightBorderBlob,SourcePath,IsAppRejected,AppName,fk_MpColorId1,fk_MpColorId2,fk_MpColorId3,fk_MpColorId4,fk_MpColorId5) " +
                        "values (@ib,@ibb,@ishbb,@ihbb,@sp,@iar,@an,@c1,@c2,@c3,@c4,@c5)",
                        new Dictionary<string, object> {
                            { "@ib", MpHelpers.Instance.ConvertBitmapSourceToByteArray(IconImage) },
                            { "@ibb", MpHelpers.Instance.ConvertBitmapSourceToByteArray(IconBorderImage) },
                            { "@ishbb", MpHelpers.Instance.ConvertBitmapSourceToByteArray(IconSelectedHighlightBorderImage) },
                            { "@ihbb", MpHelpers.Instance.ConvertBitmapSourceToByteArray(IconHighlightBorderImage) },
                            { "@sp", AppPath },
                            { "@iar", Convert.ToInt32(IsAppRejected) },
                            { "@an", AppName },
                            { "@c1", ColorId[0] },
                            { "@c2", ColorId[1] },
                            { "@c3", ColorId[2] },
                            { "@c4", ColorId[3] },
                            { "@c5", ColorId[4] }
                        });
                AppId = MpDb.Instance.GetLastRowId("MpApp", "pk_MpAppId");                
            } else {
                MpDb.Instance.ExecuteWrite(
                    //"update MpApp set IconBlob=@ib, IconBorderBlob=@ibb,IconSelectedHighlightBorderBlob=@ishbb,IconHighlightBorderBlob=@ihbb, IsAppRejected=@iar, SourcePath=@sp, AppName=@an, fk_MpColorId1=@c1,fk_MpColorId2=@c2,fk_MpColorId3=@c3,fk_MpColorId4=@c4,fk_MpColorId5=@c5 where pk_MpAppId=@aid",
                    "update MpApp set IsAppRejected=@iar, SourcePath=@sp, AppName=@an, fk_MpColorId1=@c1,fk_MpColorId2=@c2,fk_MpColorId3=@c3,fk_MpColorId4=@c4,fk_MpColorId5=@c5 where pk_MpAppId=@aid",
                    new Dictionary<string, object> {
                        { "@iar", Convert.ToInt32(IsAppRejected) },
                        { "@sp", AppPath },
                        { "@an", AppName },
                        { "@aid", AppId },
                        { "@c1", ColorId[0] },
                        { "@c2", ColorId[1] },
                        { "@c3", ColorId[2] },
                        { "@c4", ColorId[3] },
                        { "@c5", ColorId[4] }
                    });
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
            MpDb.Instance.ExecuteWrite(
                "delete from MpApp where pk_MpAppId=@aid",
                new Dictionary<string, object> {
                    { "@aid", AppId }
                });

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
