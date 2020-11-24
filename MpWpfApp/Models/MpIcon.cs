using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MpWpfApp {
    public class MpIcon : MpDbObject {
        public static int TotalIconCount = 0;
        public int IconId { get; set; }
        public BitmapSource IconImage { get; set; }

        public MpIcon() {
            IconId = 0;
            IconImage = null;
            ++TotalIconCount;
        }
        public MpIcon(BitmapSource iconImage) : base() {
            this.IconId = 0;
            this.IconImage = iconImage;
            ++TotalIconCount;
        }
        public MpIcon(int iconId) {
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
        public MpIcon(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            this.IconId = Convert.ToInt32(dr["pk_MpIconId"].ToString());
            this.IconImage = MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["IconBlob"]);
            //MapDataToColumns();
            //Console.WriteLine("Loaded MpIcon");
            //Console.WriteLine(ToString());
        }
        public override void WriteToDatabase() {
            bool isNew = false;
            if (IconImage == null) {
                throw new Exception("Error creating MpIcon Image cannot be null");
            }
            if (IconId == 0) {
                DataTable dt = MpDb.Instance.Execute(
                    "select * from MpIcon where IconBlob=@ib",
                    new Dictionary<string, object> {
                        { "@ib", MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)this.IconImage) }
                    });
                if (dt.Rows.Count > 0) {
                    this.IconId = Convert.ToInt32(dt.Rows[0]["pk_MpIconId"]);
                    MpDb.Instance.ExecuteWrite(
                        "update MpIcon set IconBlob=@ib where pk_MpIconId=@iid",
                        new Dictionary<string, object> {
                            { "@ib", MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)this.IconImage) },
                            { "@iid" , IconId}
                        });
                    isNew = false;
                } else {
                    MpDb.Instance.ExecuteWrite(
                        "insert into MpIcon(IconBlob) values(@ib)",
                        new Dictionary<string, object> {
                            { "@ib", MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)this.IconImage) }
                        });
                    this.IconId = MpDb.Instance.GetLastRowId("MpIcon", "pk_MpIconId");
                    isNew = true;
                }
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpIcon set IconBlob=@ib where pk_MpIconId=@iid",
                    new Dictionary<string, object> {
                        { "@ib", MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)this.IconImage) },
                        { "@iid", IconId }
                    });
            }
            if (isNew) {
                MapDataToColumns();
            }
            Console.WriteLine(isNew ? "Created " : "Updated " + " MpIcon");
            Console.WriteLine(ToString());
        }

        private void MapDataToColumns() {
            TableName = "MpIcon";
            columnData.Add("pk_MpIconId", this.IconId);
            columnData.Add("IconBlob", this.IconImage);
        }
    }
}
