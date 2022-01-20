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

        public string HexColor1 { get; set; }
        public string HexColor2 { get; set; }
        public string HexColor3 { get; set; }
        public string HexColor4 { get; set; }
        public string HexColor5 { get; set; }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconDbImageId")]
        public int IconImageId { get; set; }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconBorderDbImageId")]
        public int IconBorderImageId { get; set; }
        #endregion

        #region Fk Models

        [OneToOne(foreignKey:nameof(IconImageId), CascadeOperations = CascadeOperation.All)]
        public MpDbImage IconImage { get; set; }

        [OneToOne(foreignKey: nameof(IconBorderImageId), CascadeOperations = CascadeOperation.All)]
        public MpDbImage IconBorderImage { get; set; }
        #endregion

        #region Properties

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

        [Ignore]
        public List<string> HexColors => new List<string>() { 
            HexColor1,
            HexColor2,
            HexColor3,
            HexColor4,
            HexColor5        
        };

        #endregion

        #region Statics

        public static async Task<MpIcon> Create(string iconImgBase64, bool createBorder = true) {
            var dupCheck = await MpDataModelProvider.Instance.GetIconByImageStr(iconImgBase64);
            if(dupCheck != null) {
                dupCheck = await MpDb.Instance.GetItemAsync<MpIcon>(dupCheck.Id);
                return dupCheck;
            }

            var iconImage = await MpDbImage.Create(iconImgBase64);

            var newIcon = new MpIcon() {
                IconGuid = System.Guid.NewGuid(),
                IconImageId = iconImage.Id,
                IconImage = iconImage
            };

            var iconBuilder = createBorder ? MpNativeWrapper.Instance.GetIconBuilder() : null;
            if (iconBuilder != null) {
                var borderImage64Str = iconBuilder.CreateBorder(iconImgBase64, 1.25, @"#FFFFFFFF");
                var iconBorderImage = await MpDbImage.Create(borderImage64Str);

                var colorList = iconBuilder.CreatePrimaryColorList(iconImgBase64);
                newIcon.IconBorderImageId = iconBorderImage.Id;
                newIcon.IconBorderImage = iconBorderImage;
                newIcon.HexColor1 = colorList[0];
                newIcon.HexColor2 = colorList[1];
                newIcon.HexColor3 = colorList[2];
                newIcon.HexColor4 = colorList[3];
                newIcon.HexColor5 = colorList[4];
            } else {
                var iconBorderImage = await MpDbImage.Create(iconImgBase64);

                newIcon.IconBorderImageId = iconBorderImage.Id;
                newIcon.IconBorderImage = iconBorderImage;
                newIcon.HexColor1 = MpHelpers.GetRandomColor().ToHex();
                newIcon.HexColor2 = MpHelpers.GetRandomColor().ToHex();
                newIcon.HexColor3 = MpHelpers.GetRandomColor().ToHex();
                newIcon.HexColor4 = MpHelpers.GetRandomColor().ToHex();
                newIcon.HexColor5 = MpHelpers.GetRandomColor().ToHex();
            }
            
            await newIcon.WriteToDatabaseAsync();

            return newIcon;
        }

        #endregion

        public MpIcon() { }

        #region Sync

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
            icon.IconImage = await MpDb.Instance.GetDbObjectByTableGuidAsync<MpDbImage>(objParts[1]);
            icon.IconImageId = icon.IconImage.Id;
            icon.IconBorderImage = await MpDb.Instance.GetDbObjectByTableGuidAsync<MpDbImage>(objParts[2]);
            icon.IconBorderImageId = icon.IconBorderImage.Id;
            icon.HexColor1 = objParts[5];
            icon.HexColor2 = objParts[6];
            icon.HexColor3 = objParts[7];
            icon.HexColor4 = objParts[8];
            icon.HexColor5 = objParts[9];

            return icon;
        }

        public async Task<string> SerializeDbObject() {
            await Task.Delay(1);
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}",
                ParseToken,
                IconGuid.ToString(),
                IconImage.DbImageGuid.ToString(),
                IconBorderImage.DbImageGuid.ToString(),
                HexColor1,
                HexColor2,
                HexColor3,
                HexColor4,
                HexColor5);
        }

        public Type GetDbObjectType() {
            return typeof(MpIcon);
        }

        public async Task<Dictionary<string, string>> DbDiff(object drOrModel) {
            await Task.Delay(1);

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

        #endregion
    }
}
