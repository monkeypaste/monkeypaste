using Microsoft.Office.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using static System.Data.Entity.Infrastructure.Design.Executor;


namespace MpWpfApp {
    public class MpIcon : MpDbModelBase {
        private static List<MpIcon> _AllIconList = null;
        public static int TotalIconCount = 0;

        public int IconId { get; set; }
        public Guid IconGuid { get; set; }

        public int DbIconImageId { get; set; }
        public int DbIconBorderImageId { get; set; }
        public int DbIconBorderHighlightImageId { get; set; }
        public int DbIconBorderHighlightSelectedImageId { get; set; }

        public MpDbImage DbIconImage { get; set; } = new MpDbImage();
        public MpDbImage DbIconBorderImage { get; set; } = new MpDbImage();
        public MpDbImage DbIconBorderHighlightImage { get; set; } = new MpDbImage();
        public MpDbImage DbIconBorderHighlightSelectedImage { get; set; } = new MpDbImage();

        public string HexColor1 { get; set; }
        public string HexColor2 { get; set; }
        public string HexColor3 { get; set; }
        public string HexColor4 { get; set; }
        public string HexColor5 { get; set; }

        public BitmapSource IconImage {
            get {
                return DbIconImage.DbImage;
            }
            set {
                DbIconImage.DbImage = value;
            }
        }

        public BitmapSource IconBorderImage {
            get {
                return DbIconBorderImage.DbImage;
            }
            set {
                DbIconBorderImage.DbImage = value;
            }
        }

        public BitmapSource IconBorderHighlightImage {
            get {
                return DbIconBorderHighlightImage.DbImage;
            }
            set {
                DbIconBorderHighlightImage.DbImage = value;
            }
        }

        public BitmapSource IconBorderHighlightSelectedImage {
            get {
                return DbIconBorderHighlightSelectedImage.DbImage;
            }
            set {
                DbIconBorderHighlightSelectedImage.DbImage = value;
            }
        }

        public MpObservableCollection<string> PrimaryIconColorList = new MpObservableCollection<string>();

        public static List<MpIcon> GetAllIcons() {
            if(_AllIconList == null) {
                _AllIconList = new List<MpIcon>();
                DataTable dt = MpDb.Instance.Execute("select * from MpIcon", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        _AllIconList.Add(new MpIcon(dr));
                    }
                }
            }
            return _AllIconList;
        }
        
        public MpIcon() {
            IconId = 0;
            IconGuid = Guid.NewGuid();
            IconImage = null;
            IconBorderImage = null;
            ++TotalIconCount;
        }
        public MpIcon(BitmapSource iconImage) : this() {

            MpIcon dupIcon = null;
            //foreach (var i in GetAllIcons()) {
            //    if (i.IconImage.IsEqual(IconImage)) {
            //        dupIcon = i;
            //    }
            //}
            if (dupIcon == null) {
                IconGuid = Guid.NewGuid();
                DbIconImage = new MpDbImage(iconImage);
                DbIconBorderImage = new MpDbImage(MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.White));
                DbIconBorderHighlightImage = new MpDbImage(MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Yellow));
                DbIconBorderHighlightSelectedImage = new MpDbImage(MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Pink));
                //IconImage = iconImage;
                //IconBorderImage = CreateBorder(iconImage, 1.25);
                //IconImage = MpHelpers.Instance.GetIconImage(AppPath);
                //IconBorderImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.White);
                //IconBorderHighlightImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Yellow);
                //IconBorderHighlightSelectedImage = MpHelpers.Instance.CreateBorder(IconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Pink);
                PrimaryIconColorList = CreatePrimaryColorList(IconImage);
                ++TotalIconCount;
            } else {
                IconId = dupIcon.IconId;
                IconGuid = dupIcon.IconGuid;
                DbIconImageId = dupIcon.DbIconImageId;
                DbIconBorderImageId = dupIcon.DbIconBorderImageId;
                DbIconBorderHighlightImageId = dupIcon.DbIconBorderHighlightImageId;
                DbIconBorderHighlightSelectedImageId = dupIcon.DbIconBorderHighlightSelectedImageId;

                DbIconImage = dupIcon.DbIconImage;
                DbIconBorderImage = dupIcon.DbIconBorderImage;
                DbIconBorderHighlightImage = dupIcon.DbIconBorderHighlightImage;
                DbIconBorderHighlightSelectedImage = dupIcon.DbIconBorderHighlightSelectedImage;
            }

        }

        public MpIcon(int iconId) : this() {
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpIcon where pk_MpIconId=@iid",
                new Dictionary<string, object> {
                    { "@iid", iconId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            } else {
                throw new Exception("MpIcon error trying access unknown icon w/ pk: " + iconId);
            }
        }
        public MpIcon(DataRow dr) : this() {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            IconId = Convert.ToInt32(dr["pk_MpIconId"].ToString());
            IconGuid = Guid.Parse(dr["MpIconGuid"].ToString());

            DbIconImageId = Convert.ToInt32(dr["fk_IconDbImageId"].ToString());
            DbIconBorderImageId = Convert.ToInt32(dr["fk_IconBorderDbImageId"].ToString());
            DbIconBorderHighlightImageId = Convert.ToInt32(dr["fk_IconSelectedHighlightBorderDbImageId"].ToString());
            DbIconBorderHighlightSelectedImageId = Convert.ToInt32(dr["fk_IconHighlightBorderDbImageId"].ToString());

            DbIconImage = new MpDbImage(DbIconImageId);
            DbIconBorderImage = new MpDbImage(DbIconBorderImageId);
            DbIconBorderHighlightImage = new MpDbImage(DbIconBorderHighlightImageId);
            DbIconBorderHighlightSelectedImage = new MpDbImage(DbIconBorderHighlightSelectedImageId);

            PrimaryIconColorList.Clear();
            for (int i = 0; i < 5; i++) {
                string hexStr = dr[string.Format(@"HexColor{0}", (i + 1))].ToString();
                if(string.IsNullOrEmpty(hexStr)) {
                    hexStr = MpHelpers.Instance.ConvertColorToHex(MpHelpers.Instance.GetRandomColor());
                }
                PrimaryIconColorList.Add(hexStr);
            }
            //IconImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBlob"]);
            //IconBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBorderBlob"]);
        }
        public override void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisClientGuid);
            }
        }
        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {            
            if (IconImage == null) {
                throw new Exception("Error creating MpIcon Image cannot be null");
            }

            DbIconImage.WriteToDatabase();
            DbIconImageId = DbIconImage.DbImageId;

            DbIconBorderImage.WriteToDatabase();
            DbIconBorderImageId = DbIconBorderImage.DbImageId;

            DbIconBorderHighlightImage.WriteToDatabase();
            DbIconBorderHighlightImageId = DbIconBorderHighlightImage.DbImageId;

            DbIconBorderHighlightSelectedImage.WriteToDatabase();
            DbIconBorderHighlightSelectedImageId = DbIconBorderHighlightSelectedImage.DbImageId;

            if (IconId == 0) {
                MpDb.Instance.ExecuteWrite(
                         "insert into MpIcon(MpIconGuid,fk_IconDbImageId,fk_IconBorderDbImageId,fk_IconSelectedHighlightBorderDbImageId,fk_IconHighlightBorderDbImageId,HexColor1,HexColor2,HexColor3,HexColor4,HexColor5) " +
                         "values(@ig,@iiid,@ibiid,@ishiid,@ihiid,@c1,@c2,@c3,@c4,@c5)",
                         new Dictionary<string, object> {
                             { "@ig", IconGuid.ToString() },
                            { "@iiid", DbIconImageId },
                            { "@ibiid", DbIconBorderImageId },
                            { "@ishiid", DbIconBorderHighlightSelectedImageId },
                            { "@ihiid", DbIconBorderHighlightImageId },
                            { "@c1", PrimaryIconColorList[0] },
                            { "@c2", PrimaryIconColorList[1] },
                            { "@c3", PrimaryIconColorList[2] },
                            { "@c4", PrimaryIconColorList[3] },
                            { "@c5", PrimaryIconColorList[4] }
                         }, IconGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                IconId = MpDb.Instance.GetLastRowId("MpIcon", "pk_MpIconId");
                GetAllIcons().Add(this);
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpIcon set MpIconGuid=@ig, fk_IconDbImageId=@iiid,fk_IconBorderDbImageId=@ibiid,fk_IconSelectedHighlightBorderDbImageId=@ishiid,fk_IconHighlightBorderDbImageId=@ihiid, HexColor1=@c1,HexColor2=@c2,HexColor3=@c3,HexColor4=@c4,HexColor5=@c5 where pk_MpIconId=@iid",
                    new Dictionary<string, object> {
                        { "@ig", IconGuid.ToString() },
                        { "@iiid", DbIconImageId },
                        { "@ibiid", DbIconBorderImageId },
                        { "@ishiid", DbIconBorderHighlightSelectedImageId },
                        { "@ihiid", DbIconBorderHighlightImageId },
                        { "@c1", PrimaryIconColorList[0] },
                        { "@c2", PrimaryIconColorList[1] },
                        { "@c3", PrimaryIconColorList[2] },
                        { "@c4", PrimaryIconColorList[3] },
                        { "@c5", PrimaryIconColorList[4] },
                        { "@iid", IconId }
                    }, IconGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                var cit = GetAllIcons().Where(x => x.IconId == IconId).FirstOrDefault();
                if (cit != null) {
                    _AllIconList[_AllIconList.IndexOf(cit)] = this;
                }
            }
        }

        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(Properties.Settings.Default.ThisClientGuid);
            }
        }

        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (IconId <= 0) {
                return;
            }

            DbIconImage.DeleteFromDatabase();
            DbIconBorderImage.DeleteFromDatabase();
            DbIconBorderHighlightImage.DeleteFromDatabase();
            DbIconBorderHighlightSelectedImage.DeleteFromDatabase();

            MpDb.Instance.ExecuteWrite(
                "delete from MpIcon where pk_MpIconId=@cid",
                new Dictionary<string, object> {
                    { "@cid", IconId }
                }, IconGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);

            GetAllIcons().Remove(this);
        }

        private MpObservableCollection<string> CreatePrimaryColorList(BitmapSource bmpSource) {
            //var sw = new Stopwatch();
            //sw.Start();
            var primaryIconColorList = new MpObservableCollection<string>();
            var hist = MpImageHistogram.Instance.GetStatistics(bmpSource);
            foreach (var kvp in hist) {
                var c = Color.FromArgb(255, kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue);

                //Console.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
                if (primaryIconColorList.Count == 5) {
                    break;
                }
                //between 0-255 where 0 is black 255 is white
                var rgDiff = Math.Abs((int)c.R - (int)c.G);
                var rbDiff = Math.Abs((int)c.R - (int)c.B);
                var gbDiff = Math.Abs((int)c.G - (int)c.B);
                var totalDiff = rgDiff + rbDiff + gbDiff;

                //0-255 0 is black
                var grayScaleValue = 0.2126 * (int)c.R + 0.7152 * (int)c.G + 0.0722 * (int)c.B;
                var relativeDist = primaryIconColorList.Count == 0 ? 1 : MpHelpers.Instance.ColorDistance(MpHelpers.Instance.ConvertHexToColor(primaryIconColorList[primaryIconColorList.Count - 1]), c);
                if (totalDiff > 50 && grayScaleValue < 200 && relativeDist > 0.15) {
                    primaryIconColorList.Add(MpHelpers.Instance.ConvertColorToHex(c));
                }
            }

            //if only 1 color found within threshold make random list
            for (int i = primaryIconColorList.Count; i < 5; i++) {
                primaryIconColorList.Add(MpHelpers.Instance.ConvertColorToHex(MpHelpers.Instance.GetRandomColor()));
            }
            //sw.Stop();
            //Console.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }
    }

    

}
