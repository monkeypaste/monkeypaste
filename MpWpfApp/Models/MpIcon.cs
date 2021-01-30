using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MpWpfApp {
    public class MpIcon : MpDbObject {
        public static int TotalIconCount = 0;
        public int IconId { get; set; }
        public BitmapSource IconImage { get; set; }
        public BitmapSource IconBorderImage { get; set; }

        public static List<MpIcon> GetAllIcons() {
            var iconList = new List<MpIcon>();
            DataTable dt = MpDb.Instance.Execute("select * from MpIcon", null);
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    iconList.Add(new MpIcon(dr));
                }
            }
            return iconList;
        }
        public MpIcon() {
            IconId = 0;
            IconImage = null;
            IconBorderImage = null;
            ++TotalIconCount;
        }
        public MpIcon(BitmapSource iconImage) : base() {
            IconId = 0;
            IconImage = iconImage;
            IconBorderImage = CreateBorder(iconImage, 1.25);
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
            IconId = Convert.ToInt32(dr["pk_MpIconId"].ToString());
            IconImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBlob"]);
            IconBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBorderBlob"]);
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
                        { "@ib", MpHelpers.Instance.ConvertBitmapSourceToByteArray((BitmapSource)IconImage) }
                    });
                if (dt.Rows.Count > 0) {
                    IconId = Convert.ToInt32(dt.Rows[0]["pk_MpIconId"]);
                    MpDb.Instance.ExecuteWrite(
                        "update MpIcon set IconBlob=@ib where pk_MpIconId=@iid",
                        new Dictionary<string, object> {
                            { "@ib", MpHelpers.Instance.ConvertBitmapSourceToByteArray((BitmapSource)IconImage) },
                            { "@iid" , IconId}
                        });
                    isNew = false;
                } else {
                    MpDb.Instance.ExecuteWrite(
                        "insert into MpIcon(IconBlob) values(@ib)",
                        new Dictionary<string, object> {
                            { "@ib", MpHelpers.Instance.ConvertBitmapSourceToByteArray((BitmapSource)IconImage) }
                        });
                    IconId = MpDb.Instance.GetLastRowId("MpIcon", "pk_MpIconId");
                    isNew = true;
                }
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpIcon set IconBlob=@ib where pk_MpIconId=@iid",
                    new Dictionary<string, object> {
                        { "@ib", MpHelpers.Instance.ConvertBitmapSourceToByteArray((BitmapSource)IconImage) },
                        { "@iid", IconId }
                    });
            }
        }

        private BitmapSource CreateBorder(BitmapSource img, double scale) {
            var border = MpHelpers.Instance.TintBitmapSource(img, Colors.White);
            var borderSize = new Size(img.Width * scale, img.Height * scale);
            return MpHelpers.Instance.ResizeBitmapSource(border, borderSize);
        }
    }
}
