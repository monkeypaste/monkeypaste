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
        public static int TotalAppCount = 0;

        public int AppId { get; set; } = 0;
        public string AppPath { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public bool IsAppRejected { get; set; } = false;
        public BitmapSource IconImage { get; set; }
        public BitmapSource IconBorderImage { get; set; }

        public int[] ColorId = new int[5];

        public List<MpColor> PrimaryIconColorList = new List<MpColor>();

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
            AppName = MpHelpers.Instance.GetProcessMainWindowTitle(hwnd);
            IsAppRejected = isAppRejected;
            IconImage = MpHelpers.Instance.GetIconImage(AppPath);
            IconBorderImage = CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio);
            PrimaryIconColorList = CreatePrimaryColorList(IconImage);
        }
        public MpApp(string appPath) {
            //only called when user selects rejected app in settings
            AppPath = appPath;
            AppName = appPath;
            IsAppRejected = true;
            IconImage = MpHelpers.Instance.GetIconImage(AppPath);
            IconBorderImage = CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio);
            PrimaryIconColorList = CreatePrimaryColorList(IconImage);
        }

        public MpApp() : this(false, IntPtr.Zero) { }

        public MpApp(DataRow dr) {
            LoadDataRow(dr);
        }
        
        public override void LoadDataRow(DataRow dr) {
            AppId = Convert.ToInt32(dr["pk_MpAppId"].ToString());
            AppPath = dr["SourcePath"].ToString();
            AppName = dr["AppName"].ToString();
            IconImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBlob"]);
            IconBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBorderBlob"]);

            PrimaryIconColorList.Clear();
            for (int i = 0; i < 5; i++) {
                ColorId[i] = Convert.ToInt32(dr["fk_MpColorId"+(i+1)].ToString());
                if(ColorId[i] > 0) {
                    PrimaryIconColorList.Add(new MpColor(ColorId[i]));
                }
            }

            if (Convert.ToInt32(dr["IsAppRejected"].ToString()) == 0) {
                IsAppRejected = false;
            } else {
                IsAppRejected = true;
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
                        "insert into MpApp(IconBlob,IconBorderBlob,SourcePath,IsAppRejected,AppName,fk_MpColorId1,fk_MpColorId2,fk_MpColorId3,fk_MpColorId4,fk_MpColorId5) values (@ib,@ibb,@sp,@iar,@an,@c1,@c2,@c3,@c4,@c5)",
                        new Dictionary<string, object> {
                            { "@ib", MpHelpers.Instance.ConvertBitmapSourceToByteArray(IconImage) },
                            { "@ibb", MpHelpers.Instance.ConvertBitmapSourceToByteArray(IconBorderImage) },
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
                    "update MpApp set IconBlob=@ib, IconBorderBlob=@ibb, IsAppRejected=@iar, SourcePath=@sp, AppName=@an, fk_MpColorId1=@c1,fk_MpColorId2=@c2,fk_MpColorId3=@c3,fk_MpColorId4=@c4,fk_MpColorId5=@c5 where pk_MpAppId=@aid",
                    new Dictionary<string, object> {
                        { "@ib", MpHelpers.Instance.ConvertBitmapSourceToByteArray(IconImage) },
                        { "@ibb", MpHelpers.Instance.ConvertBitmapSourceToByteArray(IconBorderImage) },
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
        }

        public void DeleteFromDatabase() {
            if (AppId <= 0) {
                return;
            }
            
            // NOTE: Colors not deleted since they may be referenced by another app

            MpDb.Instance.ExecuteWrite(
                "delete from MpApp where pk_MpAppId=@aid",
                new Dictionary<string, object> {
                    { "@aid", AppId }
                });
        }

        private List<MpColor> CreatePrimaryColorList(BitmapSource bmpSource) {
            //var sw = new Stopwatch();
            //sw.Start();
            double distThreshold = 0;
            PrimaryIconColorList.Clear();
            var hist = MpImageHistogram.Instance.GetStatistics(IconImage);
            foreach (var kvp in hist) {
                var c = new MpColor(kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue,255);

                //Console.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
                if(PrimaryIconColorList.Count == 5) {
                    break;
                }
                if(PrimaryIconColorList.Count == 0) {
                    PrimaryIconColorList.Add(c);
                    continue;
                }
                var dist = MpHelpers.Instance.ColorDistance(PrimaryIconColorList[PrimaryIconColorList.Count - 1].Color, c.Color);
                if(dist >= distThreshold) {
                    PrimaryIconColorList.Add(c);
                }
            }
            //sw.Stop();
            //Console.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return PrimaryIconColorList;
        }
        private BitmapSource CreateBorder(BitmapSource img, double scale) {
            return MpHelpers.Instance.TintBitmapSource(img, Colors.White);
            //var borderSize = new Size(img.Width * scale, img.Height * scale);
            //return MpHelpers.Instance.ResizeBitmapSource(border, new Size(scale,scale));
        }

        public string GetAppName() {
            return AppPath == null || AppPath == string.Empty ? "None" : Path.GetFileNameWithoutExtension(AppPath);
        }
    }
}
