using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MpWpfApp {
    public class MpIcon : MpDbObject {
        public static int TotalIconCount = 0;
        public int iconId { get; set; }
        public BitmapSource IconImage { get; set; }

        public MpIcon() {
            iconId = 0;
            IconImage = null;
            ++TotalIconCount;
        }
        public MpIcon(BitmapSource iconImage) : base() {
            this.iconId = 0;
            this.IconImage = iconImage;
            ++TotalIconCount;
        }
        public MpIcon(int iconId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpIcon where pk_MpIconId=" + iconId);
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
            this.iconId = Convert.ToInt32(dr["pk_MpIconId"].ToString());
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
            if (iconId == 0) {
                if (MpDb.Instance.NoDb) {
                    this.iconId = ++TotalIconCount;
                    MapDataToColumns();
                    return;
                }
                DataTable dt = MpDb.Instance.Execute("select * from MpIcon where IconBlob=@0", new List<string>() { "@0" }, new List<object>() { MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)this.IconImage) });
                if (dt.Rows.Count > 0) {
                    this.iconId = Convert.ToInt32(dt.Rows[0]["pk_MpIconId"]);
                    MpDb.Instance.ExecuteNonQuery("update MpIcon set IconBlob=@0 where pk_MpIconId=" + this.iconId, new List<string>() { "@0" }, new List<object>() { MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)this.IconImage) });
                    isNew = false;
                } else {
                    MpDb.Instance.ExecuteNonQuery("insert into MpIcon(IconBlob) values(@0)", new List<string>() { "@0" }, new List<object>() { MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)this.IconImage) });
                    this.iconId = MpDb.Instance.GetLastRowId("MpIcon", "pk_MpIconId");
                    isNew = true;
                }
            } else {
                MpDb.Instance.ExecuteNonQuery("update MpIcon set IconBlob=@0 where pk_MpIconId=" + this.iconId, new List<string>() { "@0" }, new List<object>() { MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)this.IconImage) });
            }
            if (isNew) {
                MapDataToColumns();
            }
            Console.WriteLine(isNew ? "Created " : "Updated " + " MpIcon");
            Console.WriteLine(ToString());
        }

        private void MapDataToColumns() {
            TableName = "MpIcon";
            columnData.Add("pk_MpIconId", this.iconId);
            columnData.Add("IconBlob", this.IconImage);
        }
    }
}
