using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;
using System.Threading.Tasks;
using Xamarin.Forms;
using FFImageLoading.Forms;
using SkiaSharp;
using System.Linq;

namespace MonkeyPaste {
    public class MpIcon : MpDbModelBase {
        #region Columns
        [PrimaryKey,AutoIncrement]
        [Column("pk_MpIconId")]
        public override int Id { get; set; }

        [Column("MpIconGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid IconGuid {
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

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconDbImageId")]
        public int IconImageId { get; set; }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconBorderDbImageId")]
        public int IconBorderImageId { get; set; }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconHighlightBorderDbImageId")]
        public int IconBorderHighlightImageId { get; set; }
        
        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconSelectedHighlightBorderDbImageId")]
        public int IconBorderHighlightSelectedImageId { get; set; }

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId1")]
        public int Color1Id { get; set; }

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId2")]
        public int Color2Id { get; set; }

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId3")]
        public int Color3Id { get; set; }

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId4")]
        public int Color4Id { get; set; }

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId5")]
        public int Color5Id { get; set; }
        #endregion

        #region Fk Objects
        [OneToOne]
        public MpDbImage IconImage { get; set; }
        [OneToOne]
        public MpDbImage IconBorderImage { get; set; }
        [OneToOne]
        public MpDbImage IconBorderHighlightImage { get; set; }
        [OneToOne]
        public MpDbImage IconBorderHighlightSelectedImage { get; set; }

        [ManyToOne]
        public MpColor Color1 { get; set; }
        [ManyToOne]
        public MpColor Color2 { get; set; }
        [ManyToOne]
        public MpColor Color3 { get; set; }
        [ManyToOne]
        public MpColor Color4 { get; set; }
        [ManyToOne]
        public MpColor Color5 { get; set; }
        #endregion

        public static async Task<MpIcon> GetIconById(int id) {
            var allicons = await MpDb.Instance.GetItemsAsync<MpIcon>();
            return allicons.Where(x => x.Id == id).FirstOrDefault();
        }

        public static async Task<MpIcon> Create(string iconImgBase64) {
            
            var newImage = new MpDbImage() {
                //ImageBytes = iconImg
                ImageBase64 = iconImgBase64
            };
            await MpDb.Instance.AddItemAsync<MpDbImage>(newImage);
            
            var iconSkBmp = new MpImageConverter().Convert(iconImgBase64, typeof(SKBitmap)) as SKBitmap;
            var colorList = await MpColor.CreatePrimaryColorList(iconSkBmp);
            // TODO create border images here
            var newIcon = new MpIcon() {
                IconImageId = newImage.Id,
                IconImage = newImage,
                Color1 = colorList[0],
                Color1Id = colorList[0].Id,
                Color2 = colorList[1],
                Color2Id = colorList[1].Id,
                Color3 = colorList[2],
                Color3Id = colorList[2].Id,
                Color4 = colorList[3],
                Color4Id = colorList[3].Id,
                Color5 = colorList[4],
                Color5Id = colorList[4].Id,
            };
            await MpDb.Instance.AddItemAsync<MpIcon>(newIcon);

            return newIcon;
        }
        public MpIcon() {
        }


        //public override void DeleteFromDatabase() {
        //    throw new NotImplementedException();
        //}

        //public override string ToString() {
        //    throw new NotImplementedException();
        //}

        //public override void WriteToDatabase() {
        //    throw new NotImplementedException();
        //}

        //private BitmapSource CreateBorder(BitmapSource img, double scale) {
        //    var border = MpHelpers.Instance.TintBitmapSource(img, Colors.White);
        //    var borderSize = new Size(img.Width * scale, img.Height * scale);
        //    return MpHelpers.Instance.ResizeBitmapSource(border, borderSize);
        //}
    }
}
