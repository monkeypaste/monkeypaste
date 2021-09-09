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
using System.Collections.ObjectModel;
using System.Data;

namespace MonkeyPaste {
    public class MpIcon : MpDbModelBase, MpISyncableDbObject {
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
        #endregion

        #region Fk Objects
        [OneToOne(foreignKey:nameof(IconImageId), CascadeOperations = CascadeOperation.All)]
        public MpDbImage IconImage { get; set; }

        [OneToOne(foreignKey: nameof(IconBorderImageId), CascadeOperations = CascadeOperation.All)]
        public MpDbImage IconBorderImage { get; set; }

        [OneToOne(foreignKey: nameof(IconBorderHighlightImageId), CascadeOperations = CascadeOperation.All)]
        public MpDbImage IconBorderHighlightImage { get; set; }

        [OneToOne(foreignKey: nameof(IconBorderHighlightSelectedImageId), CascadeOperations = CascadeOperation.All)]
        public MpDbImage IconBorderHighlightSelectedImage { get; set; }

        public string HexColor1 { get; set; }
        public string HexColor2 { get; set; }
        public string HexColor3 { get; set; }
        public string HexColor4 { get; set; }
        public string HexColor5 { get; set; }
        #endregion
        public static async Task<MpIcon> GetIconByImageStrAsync(string imgStr) {
            var iconImg = await MpDb.Instance.GetItemsAsync<MpDbImage>();
            var img = iconImg.Where(x => x.ImageBase64 == imgStr).FirstOrDefault();
            if(img == null) {
                return null;
            }
            var allicons = await MpDb.Instance.GetItemsAsync<MpIcon>();
            return allicons.Where(x => x.IconImageId == img.Id).FirstOrDefault();
        }

        public static MpIcon GetIconByImageStr(string imgStr) {
            var iconImg = MpDb.Instance.GetItems<MpDbImage>();
            var img = iconImg.Where(x => x.ImageBase64 == imgStr).FirstOrDefault();
            if (img == null) {
                return null;
            }
            var allicons =  MpDb.Instance.GetItems<MpIcon>();
            return allicons.Where(x => x.IconImageId == img.Id).FirstOrDefault();
        }

        public static async Task<MpIcon> GetIconById(int id) {
            var allicons = await MpDb.Instance.GetItemsAsync<MpIcon>();
            return allicons.Where(x => x.Id == id).FirstOrDefault();
        }

        public static MpIcon Create(string iconImgBase64) {
            var iconImage = new MpDbImage() {
                DbImageGuid = System.Guid.NewGuid(),
                ImageBase64 = iconImgBase64
            };

            // TODO add image manipulation stuff like in wpf or get rid of this crap
            var iconBorderImage = new MpDbImage() {
                DbImageGuid = System.Guid.NewGuid(),
                ImageBase64 = iconImgBase64
            };
            var iconBorderHighlightImage = new MpDbImage() {
                DbImageGuid = System.Guid.NewGuid(),
                ImageBase64 = iconImgBase64
            };
            var iconBorderHighlightSelectedImage = new MpDbImage() {
                DbImageGuid = System.Guid.NewGuid(),
                ImageBase64 = iconImgBase64
            };

            MpDb.Instance.AddItem<MpDbImage>(iconImage);
            MpDb.Instance.AddItem<MpDbImage>(iconBorderImage);
            MpDb.Instance.AddItem<MpDbImage>(iconBorderHighlightImage);
            MpDb.Instance.AddItem<MpDbImage>(iconBorderHighlightSelectedImage);

            var iconSkBmp = new MpImageConverter().Convert(iconImgBase64, typeof(SKBitmap)) as SKBitmap;
            var colorList = MpHelpers.Instance.CreatePrimaryColorList(iconSkBmp);
            // TODO create border images here
            var newIcon = new MpIcon() {
                IconGuid = System.Guid.NewGuid(),
                IconImageId = iconImage.Id,
                IconImage = iconImage,
                IconBorderImageId = iconBorderImage.Id,
                IconBorderImage = iconBorderImage,
                IconBorderHighlightImageId = iconBorderHighlightImage.Id,
                IconBorderHighlightImage = iconBorderHighlightImage,
                IconBorderHighlightSelectedImageId = iconBorderHighlightSelectedImage.Id,
                IconBorderHighlightSelectedImage = iconBorderHighlightSelectedImage,
                HexColor1 = colorList[0],
                HexColor2 = colorList[1],
                HexColor3 = colorList[2],
                HexColor4 = colorList[3],
                HexColor5 = colorList[4],
            };
            MpDb.Instance.AddItem<MpIcon>(newIcon);

            return newIcon;
        }

        public static async Task<MpIcon> CreateAsync(string iconImgBase64) {            
            var iconImage = new MpDbImage() {
                //ImageBytes = iconImg
                ImageBase64 = iconImgBase64
            };

            // TODO add image manipulation stuff like in wpf or get rid of this crap
            var iconBorderImage = new MpDbImage() {
                //ImageBytes = iconImg
                ImageBase64 = iconImgBase64
            };
            var iconBorderHighlightImage = new MpDbImage() {
                //ImageBytes = iconImg
                ImageBase64 = iconImgBase64
            };
            var iconBorderHighlightSelectedImage = new MpDbImage() {
                //ImageBytes = iconImg
                ImageBase64 = iconImgBase64
            };

            await MpDb.Instance.AddItemAsync<MpDbImage>(iconImage);
            await MpDb.Instance.AddItemAsync<MpDbImage>(iconBorderImage);
            await MpDb.Instance.AddItemAsync<MpDbImage>(iconBorderHighlightImage);
            await MpDb.Instance.AddItemAsync<MpDbImage>(iconBorderHighlightSelectedImage);

            var iconSkBmp = new MpImageConverter().Convert(iconImgBase64, typeof(SKBitmap)) as SKBitmap;
            var colorList = MpHelpers.Instance.CreatePrimaryColorList(iconSkBmp);
            // TODO create border images here
            var newIcon = new MpIcon() {
                IconGuid = System.Guid.NewGuid(),
                IconImageId = iconImage.Id,
                IconImage = iconImage,
                IconBorderImageId = iconBorderImage.Id,
                IconBorderImage = iconBorderImage,
                IconBorderHighlightImageId = iconBorderHighlightImage.Id,
                IconBorderHighlightImage = iconBorderHighlightImage,
                IconBorderHighlightSelectedImageId = iconBorderHighlightSelectedImage.Id,
                IconBorderHighlightSelectedImage = iconBorderHighlightSelectedImage,
                HexColor1 = colorList[0],
                HexColor2 = colorList[1],
                HexColor3 = colorList[2],
                HexColor4 = colorList[3],
                HexColor5 = colorList[4],
            };
            await MpDb.Instance.AddItemAsync<MpIcon>(newIcon);

            return newIcon;
        }
        public MpIcon() {
        }

        public async Task<object> CreateFromLogs(string iconGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var icon = await MpDb.Instance.GetDbObjectByTableGuidAsync<MpIcon>(iconGuid);

            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpIconGuid":
                        icon.IconGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "fk_IconDbImageId":
                        icon.IconImage = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpDbImage", li.AffectedColumnValue) as MpDbImage;
                        icon.IconImageId = icon.IconImage.Id;
                        break;
                    case "fk_IconBorderDbImageId":
                        icon.IconBorderImage = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpDbImage", li.AffectedColumnValue) as MpDbImage;                        
                        icon.IconBorderImageId = icon.IconBorderImage.Id;
                        break;
                    case "fk_IconSelectedHighlightBorderDbImageId":
                        icon.IconBorderHighlightSelectedImage = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpDbImage", li.AffectedColumnValue) as MpDbImage;                        
                        icon.IconBorderHighlightSelectedImageId = icon.IconBorderHighlightSelectedImage.Id;
                        break;
                    case "fk_IconHighlightBorderDbImageId":
                        icon.IconBorderHighlightImage = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpDbImage", li.AffectedColumnValue) as MpDbImage;                        
                        icon.IconBorderHighlightImageId = icon.IconBorderHighlightImage.Id;
                        break;
                    case "HexColor1":
                        icon.HexColor1 = li.AffectedColumnValue;
                        break;
                    case "HexColor2":
                        icon.HexColor2 = li.AffectedColumnValue;
                        break;
                    case "HexColor3":
                        icon.HexColor3 = li.AffectedColumnValue;
                        break;
                    case "HexColor4":
                        icon.HexColor4 = li.AffectedColumnValue;
                        break;
                    case "HexColor5":
                        icon.HexColor5 = li.AffectedColumnValue;
                        break;
                    default:
                        MonkeyPaste.MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            return icon;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var icon = new MpIcon() {
                IconGuid = System.Guid.Parse(objParts[0])
            };
            icon.IconImage = MpDb.Instance.GetDbObjectByTableGuid("MpDbImage", objParts[1]) as MpDbImage;
            icon.IconImageId = icon.IconImage.Id;

            icon.IconBorderImage = MpDb.Instance.GetDbObjectByTableGuid("MpDbImage", objParts[2]) as MpDbImage;
            icon.IconBorderImageId = icon.IconBorderImage.Id;

            icon.IconBorderHighlightSelectedImage = MpDb.Instance.GetDbObjectByTableGuid("MpDbImage", objParts[3]) as MpDbImage;
            icon.IconBorderHighlightSelectedImageId = icon.IconBorderHighlightSelectedImage.Id;

            icon.IconBorderHighlightImage = MpDb.Instance.GetDbObjectByTableGuid("MpDbImage", objParts[4]) as MpDbImage;
            icon.IconBorderHighlightImageId = icon.IconBorderHighlightImage.Id;

            icon.HexColor1 = objParts[5];
            icon.HexColor2 = objParts[6];
            icon.HexColor3 = objParts[7];
            icon.HexColor4 = objParts[8];
            icon.HexColor5 = objParts[9];

            return icon;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}",
                ParseToken,
                IconGuid.ToString(),
                IconImage.DbImageGuid.ToString(),
                IconBorderImage.DbImageGuid.ToString(),
                IconBorderHighlightSelectedImage.DbImageGuid.ToString(),
                IconBorderHighlightImage.DbImageGuid.ToString(),
                HexColor1,
                HexColor2,
                HexColor3,
                HexColor4,
                HexColor5);
        }

        public Type GetDbObjectType() {
            return typeof(MpIcon);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpIcon other = null;
            if (drOrModel is MpIcon) {
                other = drOrModel as MpIcon;
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpIcon();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(IconGuid, other.IconGuid,
                "MpIconGuid",
                diffLookup,
                IconGuid.ToString());
            diffLookup = CheckValue(IconImageId, other.IconImageId,
                "fk_IconDbImageId",
                diffLookup,
                IconImage.DbImageGuid.ToString());
            diffLookup = CheckValue(IconBorderImageId, other.IconBorderImageId,
                "fk_IconBorderDbImageId",
                diffLookup,
                IconBorderImage.DbImageGuid.ToString());
            diffLookup = CheckValue(IconBorderHighlightSelectedImageId, other.IconBorderHighlightSelectedImageId,
                "fk_IconSelectedHighlightBorderDbImageId",
                diffLookup,
                IconBorderHighlightSelectedImage.DbImageGuid.ToString());
            diffLookup = CheckValue(IconBorderHighlightImageId, other.IconBorderHighlightImageId,
                "fk_IconHighlightBorderDbImageId",
                diffLookup,
                IconBorderHighlightImage.DbImageGuid.ToString());
            diffLookup = CheckValue(HexColor1, other.HexColor1,
                "HexColor1",
                diffLookup);
            diffLookup = CheckValue(HexColor2, other.HexColor2,
                "HexColor2",
                diffLookup);
            diffLookup = CheckValue(HexColor3, other.HexColor3,
                "HexColor3",
                diffLookup);
            diffLookup = CheckValue(HexColor4, other.HexColor4,
                "HexColor4",
                diffLookup);
            diffLookup = CheckValue(HexColor5, other.HexColor5,
                "HexColor5",
                diffLookup);
            return diffLookup;
        }
    }
}
