using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpColor : MpDbObject {
        public int ColorId { get; set; }
        public Color Color {
            get {
                return Color.FromArgb((byte)_a, (byte)_r, (byte)_g, (byte)_b);
            }
            set {   
                _r = (byte)value.R;
                _g = (byte)value.G;
                _b = (byte)value.B;
                _a = (byte)value.A;
                WriteToDatabase();
            }
        }

        private int _r, _g, _b, _a; 

        public static List<MpColor> GetAllColors() {
            var colorList = new List<MpColor>();
            DataTable dt = MpDb.Instance.Execute("select * from MpColor", null);
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    colorList.Add(new MpColor(dr));
                }
            }
            return colorList;
        }

        public MpColor(int colorId) {
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpColor where pk_MpColorId=@cid",
                new System.Collections.Generic.Dictionary<string, object> {
                    { "@cid", colorId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpColor(int r, int g, int b, int a) {
            _r = r;
            _g = g;
            _b = b;
            _a = a;
        }
        public MpColor(Color c) : this(c.R, c.G, c.B, c.A) { }

        public MpColor(DataRow dr) {
            LoadDataRow(dr);
        }


        public override void LoadDataRow(DataRow dr) {
            ColorId = Convert.ToInt32(dr["pk_MpColorId"].ToString());
            _r = Convert.ToInt32(dr["R"].ToString());
            _g = Convert.ToInt32(dr["G"].ToString());
            _b = Convert.ToInt32(dr["B"].ToString());
            _a = Convert.ToInt32(dr["A"].ToString());
        }
        public override void WriteToDatabase() {
            if (ColorId == 0) {
                DataTable dt = MpDb.Instance.Execute(
                    "select * from MpColor where R=@r and G=@g and B=@b and A=@a",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@r", _r },
                        { "@g", _g },
                        { "@b", _b },
                        { "@a", _a }
                    });
                if (dt != null && dt.Rows.Count > 0) {
                    ColorId = Convert.ToInt32(dt.Rows[0]["pk_MpColorId"].ToString());
                } else {
                    MpDb.Instance.ExecuteWrite(
                        "insert into MpColor(R,G,B,A) values(@r,@g,@b,@a)",
                        new System.Collections.Generic.Dictionary<string, object> {
                        { "@r", _r },
                        { "@g", _g },
                        { "@b", _b },
                        { "@a", _a }
                    });
                    ColorId = MpDb.Instance.GetLastRowId("MpColor", "pk_MpColorId");
                }
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpColor set R=@r, G=@g, B=@b, A=@a where pk_MpColorId=@cid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@r", _r },
                        { "@g", _g },
                        { "@b", _b },
                        { "@a", _a },
                        { "@cid", ColorId }
                    });
            }
        }
        private void MapDataToColumns() {
            TableName = "MpColor";
            columnData.Clear();
            columnData.Add("pk_MpColorId", this.ColorId);
            columnData.Add("R", this._r);
            columnData.Add("G", this._g);
            columnData.Add("B", this._b);
            columnData.Add("A", this._a);
        }
    }
}
