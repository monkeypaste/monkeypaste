using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
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
            DataTable dt = MpApplication.Instance.DataModel.Db.Execute("select * from MpColor where pk_MpColorId=" + colorId);
            if(dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }


        public MpColor(DataRow dr) {
            LoadDataRow(dr);
        }
        public MpColor(int r,int g,int b,int a) {
            _r = r;
            _g = g;
            _b = b;
            _a = a;
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
                DataTable dt = MpApplication.Instance.DataModel.Db.Execute("select * from MpColor where R=" + _r + " and G=" + _g + " and B=" + _b + " and A=" + _a);
                if(dt != null && dt.Rows.Count > 0) {
                    ColorId = Convert.ToInt32(dt.Rows[0]["pk_MpColorId"].ToString());
                }
                else {
                    MpApplication.Instance.DataModel.Db.ExecuteNonQuery("insert into MpColor(R,G,B,A) values(" + _r + "," + _g + "," + _b + "," + _a + ")");
                    ColorId = MpApplication.Instance.DataModel.Db.GetLastRowId("MpColor","pk_MpColorId");
                }
            }
            else {
                MpApplication.Instance.DataModel.Db.ExecuteNonQuery("update MpColor set R=" + _r + ", G=" + _g + ", B=" + _b + ", A=" + _a + " where pk_MpColorId=" + ColorId);
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
