using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MpWpfApp {
    public class MpIcon : MpDbObject {
        public static int TotalIconCount = 0;
        public int IconId { get; set; }
        public Guid IconGuid { get; set; }

        public int DbIconImageId { get; set; }
        public int DbIconBorderImageId { get; set; }
        public int DbIconBorderHighlightImageId { get; set; }
        public int DbIconBorderHighlightSelectedImageId { get; set; }

        public MpDbImage DbIconImage { get; set; }
        public MpDbImage DbIconBorderImage { get; set; }
        public MpDbImage DbIconBorderHighlightImage { get; set; }
        public MpDbImage DbIconBorderHighlightSelectedImage { get; set; }

        public int Color1Id { get; set; }
        public int Color2Id { get; set; }
        public int Color3Id { get; set; }
        public int Color4Id { get; set; }
        public int Color5Id { get; set; }

        public MpColor Color1 { get; set; }
        public MpColor Color2 { get; set; }
        public MpColor Color3 { get; set; }
        public MpColor Color4 { get; set; }
        public MpColor Color5 { get; set; }

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

        public int[] ColorId = new int[5];

        public MpObservableCollection<MpColor> PrimaryIconColorList = new MpObservableCollection<MpColor>();

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
            IconGuid = Guid.NewGuid();
            IconImage = null;
            IconBorderImage = null;
            ++TotalIconCount;
        }
        public MpIcon(BitmapSource iconImage) : base() {
            
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
                PrimaryIconColorList = MpColor.CreatePrimaryColorList(IconImage);
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
            }
            
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
                ColorId[i] = Convert.ToInt32(dr["fk_MpColorId" + (i + 1)].ToString());
                if (ColorId[i] > 0) {
                    PrimaryIconColorList.Add(new MpColor(ColorId[i]));
                }
            }
            //IconImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBlob"]);
            //IconBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["IconBorderBlob"]);
        }
        public override void WriteToDatabase() {
            if (IconImage == null) {
                throw new Exception("Error creating MpIcon Image cannot be null");
            }

            for (int i = 1; i <= PrimaryIconColorList.Count; i++) {
                var c = PrimaryIconColorList[i - 1];
                c.WriteToDatabase();
                ColorId[i - 1] = c.ColorId;
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
                         "insert into MpIcon(MpIconGuid,fk_IconDbImageId,fk_IconBorderDbImageId,fk_IconSelectedHighlightBorderDbImageId,fk_IconHighlightBorderDbImageId,fk_MpColorId1,fk_MpColorId2,fk_MpColorId3,fk_MpColorId4,fk_MpColorId5) " +
                         "values(@ig,@iiid,@ibiid,@ishiid,@ihiid,@c1,@c2,@c3,@c4,@c5)",
                         new Dictionary<string, object> {
                             { "@ig", IconGuid.ToString() },
                            { "@iiid", DbIconImageId },
                            { "@ibiid", DbIconBorderImageId },
                            { "@ishiid", DbIconBorderHighlightSelectedImageId },
                            { "@ihiid", DbIconBorderHighlightImageId },
                            { "@c1", ColorId[0] },
                            { "@c2", ColorId[1] },
                            { "@c3", ColorId[2] },
                            { "@c4", ColorId[3] },
                            { "@c5", ColorId[4] }
                         },IconGuid.ToString());
                IconId = MpDb.Instance.GetLastRowId("MpIcon", "pk_MpIconId");
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpIcon set MpIconGuid=@ig, fk_IconDbImageId=@iiid,fk_IconBorderDbImageId=@ibiid,fk_IconSelectedHighlightBorderDbImageId=@ishiid,fk_IconHighlightBorderDbImageId=@ihiid, fk_MpColorId1=@c1,fk_MpColorId2=@c2,fk_MpColorId3=@c3,fk_MpColorId4=@c4,fk_MpColorId5=@c5 where pk_MpIconId=@iid",
                    new Dictionary<string, object> {
                        { "@ig", IconGuid.ToString() },
                        { "@iiid", DbIconImageId },
                        { "@ibiid", DbIconBorderImageId },
                        { "@ishiid", DbIconBorderHighlightSelectedImageId },
                        { "@ihiid", DbIconBorderHighlightImageId },
                        { "@c1", ColorId[0] },
                        { "@c2", ColorId[1] },
                        { "@c3", ColorId[2] },
                        { "@c4", ColorId[3] },
                        { "@c5", ColorId[4] },
                        { "@iid", IconId }
                    },IconGuid.ToString());
            }
        }

        public void DeleteFromDatabase() {
            if (IconId <= 0) {
                return;
            }

            DbIconImage.DeleteFromDatabase();
            DbIconBorderImage.DeleteFromDatabase();
            DbIconBorderHighlightImage.DeleteFromDatabase();
            DbIconBorderHighlightSelectedImage.DeleteFromDatabase();

            Color1.DeleteFromDatabase();
            Color2.DeleteFromDatabase();
            Color3.DeleteFromDatabase();
            Color4.DeleteFromDatabase();
            Color5.DeleteFromDatabase();

            MpDb.Instance.ExecuteWrite(
                "delete from MpIcon where pk_MpIconId=@cid",
                new Dictionary<string, object> {
                    { "@cid", IconId }
                },IconGuid.ToString());
        }
    }
}
