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
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpIcon : MpDbModelBase,MonkeyPaste.MpISyncableDbObject {
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

                HexColor1 = PrimaryIconColorList[0];
                HexColor2 = PrimaryIconColorList[1];
                HexColor3 = PrimaryIconColorList[2];
                HexColor4 = PrimaryIconColorList[3];
                HexColor5 = PrimaryIconColorList[4];
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

                PrimaryIconColorList = dupIcon.PrimaryIconColorList;


                HexColor1 = dupIcon.PrimaryIconColorList[0];
                HexColor2 = dupIcon.PrimaryIconColorList[1];
                HexColor3 = dupIcon.PrimaryIconColorList[2];
                HexColor4 = dupIcon.PrimaryIconColorList[3];
                HexColor5 = dupIcon.PrimaryIconColorList[4];
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
            HexColor1 = PrimaryIconColorList[0];
            HexColor2 = PrimaryIconColorList[1];
            HexColor3 = PrimaryIconColorList[2];
            HexColor4 = PrimaryIconColorList[3];
            HexColor5 = PrimaryIconColorList[4];
            //IconImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBlob"]);
            //IconBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBorderBlob"]);
        }
        public override void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid);
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
                DeleteFromDatabase(Properties.Settings.Default.ThisDeviceGuid);
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

        public async Task<object> CreateFromLogs(string iconGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            await Task.Delay(1);
            var iconDr = MpDb.Instance.GetDbDataRowByTableGuid("MpIcon", iconGuid);
            MpIcon icon = null;
            if (iconDr == null) {
                icon = new MpIcon();
            } else {
                icon = new MpIcon(iconDr);
            }
            DataRow tidr = null;
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpIconGuid":
                        icon.IconGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "fk_IconDbImageId":
                        tidr = MpDb.Instance.GetDbDataRowByTableGuid("MpDbImage", li.AffectedColumnValue);
                        icon.DbIconImage = new MpDbImage(tidr);
                        icon.DbIconImageId = icon.DbIconImage.DbImageId;
                        break;
                    case "fk_IconBorderDbImageId":
                        tidr = MpDb.Instance.GetDbDataRowByTableGuid("MpDbImage", li.AffectedColumnValue);
                        icon.DbIconBorderImage = new MpDbImage(tidr);
                        icon.DbIconBorderImageId = icon.DbIconBorderImage.DbImageId;
                        break;
                    case "fk_IconSelectedHighlightBorderDbImageId":
                        tidr = MpDb.Instance.GetDbDataRowByTableGuid("MpDbImage", li.AffectedColumnValue);
                        icon.DbIconBorderHighlightSelectedImage = new MpDbImage(tidr);
                        icon.DbIconBorderHighlightSelectedImageId = icon.DbIconBorderHighlightSelectedImage.DbImageId;
                        break;
                    case "fk_IconHighlightBorderDbImageId":
                        tidr = MpDb.Instance.GetDbDataRowByTableGuid("MpDbImage", li.AffectedColumnValue);
                        icon.DbIconBorderHighlightImage = new MpDbImage(tidr);
                        icon.DbIconBorderHighlightImageId = icon.DbIconBorderHighlightImage.DbImageId;
                        break;
                    case "HexColor1":
                        icon.HexColor1 = li.AffectedColumnValue;
                        break;
                    case "HexColor2":
                        icon.HexColor2 = li.AffectedColumnValue;
                        break;
                    case "HexColor3":
                        icon.HexColor3 = li.AffectedColumnValue;
                        break;
                    case "HexColor4":
                        icon.HexColor4 = li.AffectedColumnValue;
                        break;
                    case "HexColor5":
                        icon.HexColor5 = li.AffectedColumnValue;
                        break;
                    default:
                        MonkeyPaste.MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }

            icon.PrimaryIconColorList = new MpObservableCollection<string>() {
                icon.HexColor1,
                icon.HexColor2,
                icon.HexColor3,
                icon.HexColor4,
                icon.HexColor5
            };
            return icon;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var icon = new MpIcon() {
                IconGuid = System.Guid.Parse(objParts[0])
            };
            icon.DbIconImage = MpDb.Instance.GetDbObjectByTableGuid("MpDbImage", objParts[1]) as MpDbImage;
            icon.DbIconImageId = icon.DbIconImage.DbImageId;

            icon.DbIconBorderImage = MpDb.Instance.GetDbObjectByTableGuid("MpDbImage", objParts[2]) as MpDbImage;
            icon.DbIconBorderImageId = icon.DbIconBorderImage.DbImageId;

            icon.DbIconBorderHighlightSelectedImage = MpDb.Instance.GetDbObjectByTableGuid("MpDbImage", objParts[3]) as MpDbImage;
            icon.DbIconBorderHighlightSelectedImageId = icon.DbIconBorderHighlightSelectedImage.DbImageId;

            icon.DbIconBorderHighlightImage = MpDb.Instance.GetDbObjectByTableGuid("MpDbImage", objParts[4]) as MpDbImage;
            icon.DbIconBorderHighlightImageId = icon.DbIconBorderHighlightImage.DbImageId;

            icon.HexColor1 = objParts[5];
            icon.HexColor2 = objParts[6];
            icon.HexColor3 = objParts[7];
            icon.HexColor4 = objParts[8];
            icon.HexColor5 = objParts[9];

            icon.PrimaryIconColorList = new MpObservableCollection<string>() {
                icon.HexColor1,
                icon.HexColor2,
                icon.HexColor3,
                icon.HexColor4,
                icon.HexColor5
            };
            return icon;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}",
                ParseToken,
                IconGuid.ToString(),
                DbIconImage.DbImageGuid.ToString(),
                DbIconBorderImage.DbImageGuid.ToString(),
                DbIconBorderHighlightSelectedImage.DbImageGuid.ToString(),
                DbIconBorderHighlightImage.DbImageGuid.ToString(),
                HexColor1,
                HexColor2,
                HexColor3,
                HexColor4,
                HexColor5);
        }

        public Type GetDbObjectType() {
            return typeof(MpIcon);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpIcon other = null;
            if (drOrModel is DataRow) {
                other = new MpIcon(drOrModel as DataRow);
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpIcon();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(IconGuid, other.IconGuid,
                "MpIconGuid",
                diffLookup,
                IconGuid.ToString());
            diffLookup = CheckValue(DbIconImageId, other.DbIconImageId,
                "fk_IconDbImageId",
                diffLookup,
                DbIconImage.DbImageGuid.ToString());
            diffLookup = CheckValue(DbIconBorderImageId, other.DbIconBorderImageId,
                "fk_IconBorderDbImageId",
                diffLookup,
                DbIconBorderImage.DbImageGuid.ToString());
            diffLookup = CheckValue(DbIconBorderHighlightSelectedImageId, other.DbIconBorderHighlightSelectedImageId,
                "fk_IconSelectedHighlightBorderDbImageId",
                diffLookup,
                DbIconBorderHighlightSelectedImage.DbImageGuid.ToString());
            diffLookup = CheckValue(DbIconBorderHighlightImageId, other.DbIconBorderHighlightImageId,
                "fk_IconHighlightBorderDbImageId",
                diffLookup,
                DbIconBorderHighlightImage.DbImageGuid.ToString());
            diffLookup = CheckValue(HexColor1, other.HexColor1,
                "HexColor1",
                diffLookup);
            diffLookup = CheckValue(HexColor2, other.HexColor2,
                "HexColor2",
                diffLookup);
            diffLookup = CheckValue(HexColor3, other.HexColor3,
                "HexColor3",
                diffLookup);
            diffLookup = CheckValue(HexColor4, other.HexColor4,
                "HexColor4",
                diffLookup);
            diffLookup = CheckValue(HexColor5, other.HexColor5,
                "HexColor5",
                diffLookup);
            return diffLookup;
        }
    }
}
