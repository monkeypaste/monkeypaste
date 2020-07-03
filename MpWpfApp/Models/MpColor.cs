using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpColor:MpDbObject {
        public int ColorId { get; set; }
        public Color Color{
            get {
                return Color.FromArgb((byte)_a,(byte)_r,(byte)_g,(byte)_b);
            }
            set {
                _r = (byte)value.R;
                _g = (byte)value.G;
                _b = (byte)value.B;
                _a = (byte)value.A;
                WriteToDatabase();
            }
        }

        private int _r, _g, _b,_a;

        public MpColor(int colorId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpColor where pk_MpColorId=" + colorId);
            if(dt != null && dt.Rows.Count > 0) {
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
            if(ColorId == 0) {
                DataTable dt = MpDb.Instance.Execute("select * from MpColor where R=" + _r + " and G=" + _g + " and B=" + _b + " and A=" + _a);
                if(dt != null && dt.Rows.Count > 0) {
                    ColorId = Convert.ToInt32(dt.Rows[0]["pk_MpColorId"].ToString());
                }
                else {
                    MpDb.Instance.ExecuteNonQuery("insert into MpColor(R,G,B,A) values(" + _r + "," + _g + "," + _b + "," + _a + ")");
                    ColorId = MpDb.Instance.GetLastRowId("MpColor","pk_MpColorId");
                }
            }
            else {
                MpDb.Instance.ExecuteNonQuery("update MpColor set R=" + _r + ", G=" + _g + ", B=" + _b + ", A=" + _a + " where pk_MpColorId=" + ColorId);
            }
        }
        private void MapDataToColumns() {
            TableName = "MpColor";
            columnData.Clear();
            columnData.Add("pk_MpColorId",this.ColorId);
            columnData.Add("R",this._r);
            columnData.Add("G",this._g);
            columnData.Add("B",this._b);
            columnData.Add("A",this._a);
        }
    }
}
