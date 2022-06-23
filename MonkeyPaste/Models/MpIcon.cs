using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {

    public enum MpIconSize {
        SmallIcon16 = 0x1,
        MediumIcon32 = 0x0,
        LargeIcon48 = 0x2,
        ExtraLargeIcon = 0x4
    }
    public class MpIcon : MpDbModelBase, MpISyncableDbObject, MpIClonableDbModel<MpIcon> {
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

        //[ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconDbImageId")]
        public int IconImageId { get; set; }

        //[ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconBorderDbImageId")]
        public int IconBorderImageId { get; set; }

        [Column("b_IsReadOnly")]
        public int IsReadOnlyValue { get; set; }

        #endregion

        #region Fk Models

        //[OneToOne(foreignKey:nameof(IconImageId), CascadeOperations = CascadeOperation.All)]
        //public MpDbImage IconImage { get; set; }

        //[OneToOne(foreignKey: nameof(IconBorderImageId), CascadeOperations = CascadeOperation.All)]
        //public MpDbImage IconBorderImage { get; set; }
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
        public List<string> HexColors {
            get {
                return new List<string>() {
                            HexColor1,
                            HexColor2,
                            HexColor3,
                            HexColor4,
                            HexColor5
                        };
            }
            set {
                if(value == null) {
                    HexColors.ForEach(x => x = string.Empty);
                } else {
                    int count = Math.Min(HexColors.Count, value.Count);
                    for (int i = 0; i < count; i++) {
                        HexColors[i] = value[i];
                    }
                }
            }
        }

        [Ignore]
        public override bool IsReadOnly {
            get => IsReadOnlyValue == 1;
            set => IsReadOnlyValue = value ? 1 : 0;
        }

        #endregion

        #region Statics

        public static async Task<MpIcon> Create2(
            string iconUrl = "",
            List<string> hexColors = null,
            bool createBorder = true,
            string guid = "",
            bool suppressWrite = false) {
            string base64FavIcon = await MpUrlHelpers.GetUrlFavIconAsync(iconUrl);
            var result = await Create(
                iconImgBase64: base64FavIcon,
                hexColors: hexColors,
                createBorder: createBorder,
                guid: guid,
                suppressWrite: suppressWrite);
            return result;
        }

        public static async Task<MpIcon> Create(
            string iconImgBase64 = "",
            List<string> hexColors = null,
            bool createBorder = true, 
            string guid = "",
            bool suppressWrite = false) {

            if(!string.IsNullOrEmpty(iconImgBase64)) {
                var dupCheck = await MpDataModelProvider.GetIconByImageStr(iconImgBase64);
                if (dupCheck != null) {
                    dupCheck = await MpDb.GetItemAsync<MpIcon>(dupCheck.Id);
                    return dupCheck;
                }
            }

            var iconImage = await MpDbImage.Create(iconImgBase64, suppressWrite);

            var newIcon = new MpIcon() {
                IconGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid():System.Guid.Parse(guid),
                IconImageId = iconImage.Id,
                HexColors = hexColors
            };

            if(createBorder) {
                await newIcon.CreateOrUpdateBorder(string.Empty,suppressWrite);
            } else if(!suppressWrite) {
                await newIcon.WriteToDatabaseAsync();
            }

            return newIcon;
        }

        #endregion

        #region MpIClonableDbModel Implementation

        public async Task<MpIcon> CloneDbModel(bool suppressWrite = false) {
            int cimgId = 0;
            int cbimgId = 0;
            if (IconImageId > 0) {
                var img = await MpDb.GetItemAsync<MpDbImage>(IconImageId);
                var cimg = await img.CloneDbModel(suppressWrite);
                cimgId = cimg.Id;                
            }

            if (IconBorderImageId > 0) {
                var bimg = await MpDb.GetItemAsync<MpDbImage>(IconBorderImageId);
                var cbimg = await bimg.CloneDbModel(suppressWrite);
                cbimgId = cbimg.Id;
            }
            var ci = new MpIcon() {
                IconGuid = System.Guid.NewGuid(),
                HexColors = this.HexColors,
                IsReadOnly = this.IsReadOnly,
                IconImageId = cimgId,
                IconBorderImageId = cbimgId
            };

            
            if(!suppressWrite) {
                await ci.WriteToDatabaseAsync();
            }
            return ci;
        }

        #endregion

        public MpIcon() { }

        public async Task CreateOrUpdateBorder(string forceHexColor = "", bool suppressWrite = false) {
            var iconBuilder = MpPlatformWrapper.Services.IconBuilder;

            var img = await MpDb.GetItemAsync<MpDbImage>(IconImageId);
            if (iconBuilder == null) {
                //make border same as icon if no builder
                var iconBorderImage = await MpDbImage.Create(img.ImageBase64);

                IconBorderImageId = iconBorderImage.Id;                
                HexColor1 = MpHelpers.GetRandomColor().ToHex();
                HexColor2 = MpHelpers.GetRandomColor().ToHex();
                HexColor3 = MpHelpers.GetRandomColor().ToHex();
                HexColor4 = MpHelpers.GetRandomColor().ToHex();
                HexColor5 = MpHelpers.GetRandomColor().ToHex();
            } else  {
                var borderImage64Str = iconBuilder.CreateBorder(img.ImageBase64, 1.25, @"#FFFFFFFF");
                if(IconBorderImageId == 0) {
                    var bimg = await MpDbImage.Create(borderImage64Str);
                    IconBorderImageId = bimg.Id;
                } else {
                    var bimg = await MpDb.GetItemAsync<MpDbImage>(IconBorderImageId);
                    bimg.ImageBase64 = borderImage64Str;
                    await bimg.WriteToDatabaseAsync();
                }

                var colorList = iconBuilder.CreatePrimaryColorList(img.ImageBase64);
                HexColor1 = colorList[0];
                HexColor2 = colorList[1];
                HexColor3 = colorList[2];
                HexColor4 = colorList[3];
                HexColor5 = colorList[4];
            } 
            if(!string.IsNullOrEmpty(forceHexColor)) {
                HexColor1 = forceHexColor;
                HexColor2 = forceHexColor;
                HexColor3 = forceHexColor;
                HexColor4 = forceHexColor;
                HexColor5 = forceHexColor;
            }

            if(!suppressWrite) {
                await WriteToDatabaseAsync();
            }
        }
        #region Sync

        public async Task<object> CreateFromLogs(string iconGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var icon = await MpDb.GetDbObjectByTableGuidAsync<MpIcon>(iconGuid);

            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpIconGuid":
                        icon.IconGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "fk_IconDbImageId":
                        var img = await MpDb.GetDbObjectByTableGuidAsync("MpDbImage", li.AffectedColumnValue) as MpDbImage;
                        if(img != null) {
                            icon.IconImageId = img.Id;
                        }
                        
                        break;
                    case "fk_IconBorderDbImageId":
                        var bimg = await MpDb.GetDbObjectByTableGuidAsync("MpDbImage", li.AffectedColumnValue) as MpDbImage;
                        if (bimg != null) {
                            icon.IconBorderImageId = bimg.Id;
                        }
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
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
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
            var img = await MpDb.GetDbObjectByTableGuidAsync<MpDbImage>(objParts[1]);
            if (img != null) {
                icon.IconImageId = img.Id;
            }
            var bimg = await MpDb.GetDbObjectByTableGuidAsync<MpDbImage>(objParts[2]);
            if (img != null) {
                icon.IconBorderImageId = bimg.Id;
            }
            
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
                IconImageId > 0 ? MpDb.GetItem<MpDbImage>(IconImageId).Guid : String.Empty,
                IconBorderImageId > 0 ? MpDb.GetItem<MpDbImage>(IconBorderImageId).Guid : String.Empty,
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
                MpDb.GetItem<MpDbImage>(IconImageId).Guid);
            if(IconBorderImageId > 0) {
                diffLookup = CheckValue(IconBorderImageId, other.IconBorderImageId,
                "fk_IconBorderDbImageId",
                diffLookup,
                MpDb.GetItem<MpDbImage>(IconBorderImageId).Guid);
            }
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
