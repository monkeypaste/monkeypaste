using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;
using SQLiteNetExtensions.Attributes;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using System.Security.Cryptography;

namespace MonkeyPaste {
    public enum MpAppInteropSettingType {
        None = 0,
        ClipboardFormatPriority,
        PasteShortcut
    }

    public class MpAppInteropSetting : MpDbModelBase {
        #region Columns

        [PrimaryKey,AutoIncrement]
        [Column("pk_MpAppInteropSettingId")]
        public override int Id { get; set; }

        [Column("MpAppInteropSettingGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpApp))]
        [Column("fk_MpAppId")]
        public int AppId { get; set; }

        [Column("e_MpAppInteropSettingTypeId")]
        public int SettingTypeId { get; set; }

        public string Arg1 { get; set; }

        public string Arg2 { get; set; }

        public string Arg3 { get; set; }


        [Column("e_MpClipboardFormatTypeId")]
        public int FormatTypeId { get; set; }

        public string FormatInfo { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Properties
        [Ignore]
        public MpAppInteropSettingType SettingType {
            get => (MpAppInteropSettingType)SettingTypeId;
            set => SettingTypeId = (int)value;
        }


        [Ignore]
        public Guid AppInteropSettingGuid {
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

        #endregion


        public static async Task<MpAppInteropSetting> Create(
            int appId = 0, 
            MpAppInteropSettingType settingType = MpAppInteropSettingType.None,
            string arg1 = "",
            string arg2 = "",
            string arg3 = "",
            bool suppressWrite = false) {

            var ais = new MpAppInteropSetting() {
                AppInteropSettingGuid = System.Guid.NewGuid(),
                AppId = appId,
                SettingType = settingType,
                Arg1 = arg1,
                Arg2 = arg2,
                Arg3 = arg3
            };

            if(!suppressWrite) {
                await ais.WriteToDatabaseAsync();
            }
            return ais;
        }
        public MpAppInteropSetting() { }

    }
}
