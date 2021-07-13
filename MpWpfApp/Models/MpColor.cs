using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpColor : MpDbObject, MonkeyPaste.MpISyncableDbObject {
        private static List<MpColor> _AllColorList = null;
        public static int TotalColorCount = 0;

        public int ColorId { get; set; }
        public Guid ColorGuid { get; set; }

        public Color Color {
            get {
                return Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B);
            }
            set {   
                R = (byte)value.R;
                G = (byte)value.G;
                B = (byte)value.B;
                A = (byte)value.A;
                WriteToDatabase();
            }
        }

        public Brush ColorBrush {
            get {
                if(Color == null) {
                    return Brushes.Pink;
                }
                return new SolidColorBrush(Color);
            }

        }
        public int R { get; private set; }
         public int G { get; private set; }
         public int B { get; private set; }
         public int A { get; private set; } 

        public static List<MpColor> GetAllColors() {
            if(_AllColorList == null) {
                _AllColorList = new List<MpColor>();
                DataTable dt = MpDb.Instance.Execute("select * from MpColor", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        _AllColorList.Add(new MpColor(dr));
                    }
                }
            }
            return _AllColorList;
        }
        public static MpColor GetColorById(int colorId) {
            if (_AllColorList == null) {
                GetAllColors();
            }
            var udbpl = _AllColorList.Where(x => x.ColorId == colorId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static MpColor GetColorByGuid(string colorGuid) {
            if (_AllColorList == null) {
                GetAllColors();
            }
            var udbpl = _AllColorList.Where(x => x.ColorGuid.ToString() == colorGuid).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static MpObservableCollection<MpColor> CreatePrimaryColorList(BitmapSource bmpSource) {
            //var sw = new Stopwatch();
            //sw.Start();
            var primaryIconColorList = new MpObservableCollection<MpColor>();
            var hist = MpImageHistogram.Instance.GetStatistics(bmpSource);
            foreach (var kvp in hist) {
                var c = new MpColor(kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, 255);

                //Console.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
                if (primaryIconColorList.Count == 5) {
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
            for (int i = primaryIconColorList.Count; i < 5; i++) {
                primaryIconColorList.Add(new MpColor(MpHelpers.Instance.GetRandomColor()));
            }
            //sw.Stop();
            //Console.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }

        public MpColor() { }

        public MpColor(int colorId) : this() {
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpColor where pk_MpColorId=@cid",
                new System.Collections.Generic.Dictionary<string, object> {
                    { "@cid", colorId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpColor(int r, int g, int b, int a) : this() {
            ColorGuid = Guid.NewGuid();
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public MpColor(Color c) : this(c.R, c.G, c.B, c.A) { }

        public MpColor(DataRow dr) : this() {
            LoadDataRow(dr);
        }

        public override void LoadDataRow(DataRow dr) {
            ColorId = Convert.ToInt32(dr["pk_MpColorId"].ToString());
            if (dr["MpColorGuid"] == null || dr["MpColorGuid"].GetType() == typeof(System.DBNull)) {
                ColorGuid = Guid.NewGuid();
            } else {
                ColorGuid = Guid.Parse(dr["MpColorGuid"].ToString());
            }
            R = Convert.ToInt32(dr["R"].ToString());
            G = Convert.ToInt32(dr["G"].ToString());
            B = Convert.ToInt32(dr["B"].ToString());
            A = Convert.ToInt32(dr["A"].ToString());
        }
        //public void DeleteFromDatabase() {
        //    if (ColorId <= 0) {
        //        return;
        //    }

        //    MpDb.Instance.ExecuteWrite(
        //        "delete from MpColor where pk_MpColorId=@cid",
        //        new Dictionary<string, object> {
        //            { "@cid", ColorId }
        //        });
        //}

        private bool IsAltered() {
            var dt = MpDb.Instance.Execute(
                @"SELECT pk_MpColorId FROM MpColor WHERE MpColorGuid=@cg AND R=@r AND G=@g AND B=@b AND A=@a",
                new Dictionary<string, object> {
                    { "@cg", ColorGuid.ToString() },
                    { "@r", R },
                    { "@g", G },
                    { "@b", B },
                    { "@a", A }
                });
            return dt.Rows.Count == 0;
        }

        public override void WriteToDatabase(string sourceClientGuid) {
            if (ColorGuid == Guid.Empty) {
                ColorGuid = Guid.NewGuid();
            }
            //if (!IsAltered()) {
            //    return;
            //}


            if (ColorId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpColor(MpColorGuid,R,G,B,A) values(@cg,@r,@g,@b,@a)",
                        new System.Collections.Generic.Dictionary<string, object> {
                            { "@cg",ColorGuid.ToString() },
                        { "@r", R },
                        { "@g", G },
                        { "@b", B },
                        { "@a", A }
                    }, ColorGuid.ToString(), sourceClientGuid,this);
                ColorId = MpDb.Instance.GetLastRowId("MpColor", "pk_MpColorId");
                GetAllColors().Add(this);
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpColor set MpColorGuid=@cg, R=@r, G=@g, B=@b, A=@a where pk_MpColorId=@cid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@cg",ColorGuid.ToString() },
                        { "@r", R },
                        { "@g", G },
                        { "@b", B },
                        { "@a", A },
                        { "@cid", ColorId }
                    }, ColorGuid.ToString(),sourceClientGuid,this);
                var c = _AllColorList.Where(x => x.ColorId == ColorId).FirstOrDefault();
                if (c != null) {
                    _AllColorList[_AllColorList.IndexOf(c)] = this;
                }
            }
        }
        public override void WriteToDatabase() {
            WriteToDatabase(Properties.Settings.Default.ThisClientGuid);      
        }

        public override void DeleteFromDatabase(string sourceClientGuid) {
            if (ColorId <= 0) {
                return;
            }

            MpDb.Instance.ExecuteWrite(
                "delete from MpColor where pk_MpColorId=@cid",
                new Dictionary<string, object> {
                    { "@cid", ColorId }
                }, ColorGuid.ToString(),sourceClientGuid,this);
        }
        public void DeleteFromDatabase() {
            DeleteFromDatabase(Properties.Settings.Default.ThisClientGuid);
        }

        public async Task<object> DeserializeDbObject(string objStr, string parseToken = @"^(@!@") {
            var objParts = objStr.Split(new string[] { parseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            var dbLog = new MpColor() {
                ColorId = Convert.ToInt32(objParts[0]),
                ColorGuid = System.Guid.Parse(objParts[1]),
                R = Convert.ToInt32(objParts[2]),
                G = Convert.ToInt32(objParts[3]),
                B = Convert.ToInt32(objParts[4]),
                A = Convert.ToInt32(objParts[5])
            };
            return dbLog;
        }

        public string SerializeDbObject(string parseToken = @"^(@!@") {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}",
                parseToken,
                ColorId,
                ColorGuid.ToString(),
                R,
                G,
                B,
                A);
        }

        public Type GetDbObjectType() {
            return typeof(MpColor);
        }

        public Dictionary<string,string> DbDiff(object drOrModel) {
            MpColor other = null;
            if(drOrModel == null) {
                //this occurs when this model is being added
                //and intended behavior is all values are returned
                other = new MpColor() { R = -1, G = -1, B = -1, A = -1 };
            } else if (drOrModel is DataRow) {
                other = new MpColor(drOrModel as DataRow);
            } else {
                throw new Exception("Cannot compare xam model to local model");
            }
            var diffLookup = new Dictionary<string, string>();
            //if(ColorId > 0) {
            //    diffLookup = CheckValue(ColorId, other.ColorId,
            //    "pk_MpColorId",
            //    diffLookup);
            //}
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
