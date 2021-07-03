using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpDbImage : MpDbObject {
        public int DbImageId { get; set; }
        public Guid DbImageGuid { get; set; }

        public BitmapSource DbImage { get; set; }

        public string DbImageBase64 { get; set; }

        public MpDbImage() { }

        public MpDbImage(BitmapSource img) {
            DbImageGuid = Guid.NewGuid();
            DbImage = img;
            DbImageBase64 = MpHelpers.Instance.ConvertBitmapSourceToBase64String(DbImage);
        }

        public MpDbImage(DataRow dr) {
            LoadDataRow(dr);
        }

        public MpDbImage(int dbImageId) {
            if(dbImageId <= 0) {
                return;
            }
            var dt = MpDb.Instance.Execute(
                "select * from MpDbImage where pk_MpDbImageId=@iid",
                new Dictionary<string, object> {
                    { "@iid", dbImageId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            } else {
                throw new Exception("MpIcon error trying access unknown icon w/ pk: " + dbImageId);
            }
        }

        public override void LoadDataRow(DataRow dr) {
            DbImageId = Convert.ToInt32(dr["pk_MpDbImageId"].ToString());
            DbImageGuid = Guid.Parse(dr["MpDbImageGuid"].ToString());
            //DbImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["ImageBlob"]);
            DbImageBase64 = dr["ImageBase64"].ToString();
            DbImage = MpHelpers.Instance.ConvertStringToBitmapSource(DbImageBase64);
        }

        private bool IsAltered() {
            var dt = MpDb.Instance.Execute(
                @"SELECT pk_MpDbImageId FROM MpDbImage WHERE MpDbImageGuid=@dbig AND ImageBase64=@ib64",
                new Dictionary<string, object> {
                    { "@dbig", DbImageGuid.ToString() },
                    { "@ib64", DbImageBase64 }
                });
            return dt.Rows.Count == 0;
        }

        public override void WriteToDatabase() {
            if(!IsAltered()) {
                return;
            }
            string imgStr = DbImageBase64;//MpHelpers.Instance.ConvertBitmapSourceToBase64String(DbImage);
            if (DbImageId == 0) {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpDbImage(MpDbImageGuid,ImageBase64) values(@dbig,@ib64)",
                    new Dictionary<string, object> {
                        { "@dbig",DbImageGuid.ToString() },
                        { "@ib64", imgStr }
                    },DbImageGuid.ToString());
                DbImageId = MpDb.Instance.GetLastRowId("MpDbImage", "pk_MpDbImageId");
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpDbImage set MpDbImageGuid=@dbig, ImageBase64=@ib64 where pk_MpDbImageId=@dbiid",
                    new Dictionary<string, object> {
                        { "@dbig", DbImageGuid.ToString() },
                        { "@dbiid", DbImageId },
                        { "@ib64", imgStr  }
                    },DbImageGuid.ToString());
            }
        }

        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteWrite(
                "delete from MpDbImage where pk_MpDbImageId=@tid",
                new Dictionary<string, object> {
                    { "@tid", DbImageId }
                },DbImageGuid.ToString());
        }
    }
}
