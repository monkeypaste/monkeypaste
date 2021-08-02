using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FFImageLoading.Forms;
using SkiaSharp;
using SQLite;
using SQLiteNetExtensions.Attributes;
using Xamarin.Forms;

namespace MonkeyPaste {
    [Table("MpColor")]
    public class MpColor : MpDbModelBase, MpISyncableDbObject {                       
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }

        [PrimaryKey,AutoIncrement]
        [Column("pk_MpColorId")]
        public override int Id { get; set; }

        [Column("MpColorGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid ColorGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        [Ignore]
        public Color Color {
            get {
                return Color.FromRgba(R, G, B, A);
            }
            set {   
                R = (int)(value.R * 255);
                G = (int)(value.G * 255);
                B = (int)(value.B * 255);
                A = (int)(value.A * 255);
            }
        }

        [Ignore]
        public Brush ColorBrush {
            get {
                if(Color == null) {
                    return Brush.Red;
                }
                return new SolidColorBrush(Color);
            }
        }
        public MpColor() { }
        
        public MpColor(int colorId) {
            Task.Run(async () => {
                var cl = await MpDb.Instance.GetItemsAsync<MpColor>();
                var c = cl.Where(x => x.Id == colorId).FirstOrDefault();
                if(c != null) {
                    Id = c.Id;
                    R = c.R;
                    G = c.G;
                    B = c.B;
                    A = c.A;
                }
            });
        }

        public MpColor(double r, double g, double b, double a) : this() {
            R = (int)(r * 255);
            G = (int)(g * 255);
            B = (int)(b * 255);
            A = (int)(a * 255);
        }

        public MpColor(Color c) : this(c.R, c.G, c.B, c.A) { 
        }

        public static async Task<MpColor> GetColorByIdAsync(int colorId) {
            var allColors = await MpDb.Instance.GetItemsAsync<MpColor>();
            var udbpl = allColors.Where(x => x.Id == colorId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static MpColor GetColorById(int colorId) {
            var allColors = MpDb.Instance.GetItems<MpColor>();
            var udbpl = allColors.Where(x => x.Id == colorId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static async Task<MpColor> GetColorByGuidAsync(string colorGuid) {
            var allColors = await MpDb.Instance.GetItemsAsync<MpColor>();
            var udbpl = allColors.Where(x => x.ColorGuid.ToString() == colorGuid).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static async Task<List<MpColor>> CreatePrimaryColorList(SKBitmap skbmp, int listCount = 5) {
            //var sw = new Stopwatch();
            //sw.Start();
            
            var primaryIconColorList = new List<MpColor>();
            var hist = await MpImageHistogram.Instance.GetStatistics(skbmp);
            foreach (var kvp in hist) {
                var c = new MpColor(kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, 255);

                //Console.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
                if (primaryIconColorList.Count == listCount) {
                    break;
                }
                //between 0-255 where 0 is black 255 is white
                var rgDiff = Math.Abs((int)c.Color.R - (int)c.Color.G);
                var rbDiff = Math.Abs((int)c.Color.R - (int)c.Color.B);
                var gbDiff = Math.Abs((int)c.Color.G - (int)c.Color.B);
                var totalDiff = rgDiff + rbDiff + gbDiff;

                //0-255 0 is black
                var grayScaleValue = 0.2126 * (int)c.Color.R + 0.7152 * (int)c.Color.G + 0.0722 * (int)c.Color.B;
                var relativeDist = primaryIconColorList.Count == 0 ? 1 : MpHelpers.Instance.ColorDistance(primaryIconColorList[primaryIconColorList.Count - 1].Color, c.Color);
                if (totalDiff > 50 && grayScaleValue < 200 && relativeDist > 0.15) {
                    primaryIconColorList.Add(c);
                }
            }

            //if only 1 color found within threshold make random list
            for (int i = primaryIconColorList.Count; i < listCount; i++) {
                primaryIconColorList.Add(new MpColor(MpHelpers.Instance.GetRandomColor()));
            }

            foreach(var c in primaryIconColorList) {
                await MpDb.Instance.AddItemAsync<MpColor>(c);
            }
            //sw.Stop();
            //Console.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }

        public async Task<object> CreateFromLogs(string colorGuid, List<MonkeyPaste.MpDbLog> rlogs, string fromClientGuid) {
            await Task.Delay(1);
            return MpDbModelBase.CreateOrUpdateFromLogs(rlogs, fromClientGuid);

            //var cdr = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpColor", colorGuid);
            //MpColor newColor = null;
            //if (cdr == null) {
            //    newColor = new MpColor();
            //} else {                
            //    newColor = cdr as MpColor;
            //}
            //foreach (var li in rlogs) {
            //    switch (li.AffectedColumnName) {
            //        case "MpColorGuid":
            //            newColor.ColorGuid = System.Guid.Parse(li.AffectedColumnValue);
            //            break;
            //        case "R":
            //            newColor.R = Convert.ToInt32(li.AffectedColumnValue);
            //            break;
            //        case "G":
            //            newColor.G = Convert.ToInt32(li.AffectedColumnValue);
            //            break;
            //        case "B":
            //            newColor.B = Convert.ToInt32(li.AffectedColumnValue);
            //            break;
            //        case "A":
            //            newColor.A = Convert.ToInt32(li.AffectedColumnValue);
            //            break;
            //        default:
            //            throw new Exception(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
            //    }
            //}
            ////await MpDb.Instance.AddOrUpdateAsync<MpColor>(newColor,fromClientGuid);
            //return newColor;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            var dbLog = new MpColor() {
                Id = Convert.ToInt32(objParts[0]),
                ColorGuid = System.Guid.Parse(objParts[1]),
                R = Convert.ToInt32(objParts[2]),
                G = Convert.ToInt32(objParts[3]),
                B = Convert.ToInt32(objParts[4]),
                A = Convert.ToInt32(objParts[5])
            };
            return dbLog;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}",
                ParseToken,
                Id,
                ColorGuid.ToString(),
                R,
                G,
                B,
                A);
        }

        public Type GetDbObjectType() {
            return typeof(MpColor);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpColor other = null;
            if (drOrModel == null) {
                //this occurs when this model is being added
                //and intended behavior is all values are returned
                other = new MpColor() { R = -1, G = -1, B = -1, A = -1 };
            } else if (drOrModel is MpColor) {
                other = drOrModel as MpColor;
            } else {
                throw new Exception("Cannot compare xam model to local model");
            }
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(ColorGuid, other.ColorGuid,
                "MpColorGuid",
                diffLookup);
            diffLookup = CheckValue(R, other.R,
                "R",
                diffLookup);
            diffLookup = CheckValue(
                G, other.G,
                "G",
                diffLookup);
            diffLookup = CheckValue(
                B, other.B,
                "B",
                diffLookup);
            diffLookup = CheckValue(
                A, other.A,
                "A",
                diffLookup);

            return diffLookup;
        }
    }
}
