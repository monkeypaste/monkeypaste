using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpAppInteropSetting : MpDbModelBase {
        [PrimaryKey,AutoIncrement]
        [Column("pk_MpAppInteropSettingId")]
        public override int Id { get; set; }

        [Column("MpAppInteropSettingGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpApp))]
        [Column("fk_MpAppId")]
        public int AppId { get; set; }

        [Column("e_MpClipboardFormatTypeId")]
        public int FormatTypeId { get; set; }

        public string FormatInfo { get; set; }

        public int Priority { get; set; } 

        [Ignore]
        public MpClipboardFormatType FormatType {
            get => (MpClipboardFormatType)FormatTypeId;
            set => FormatTypeId = (int)value; 
        }

        [Ignore]
        public bool IsFormatIgnored => Priority < 0;

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

        public static async Task<MpAppInteropSetting> Create(
            int appId = 0, 
            MpClipboardFormatType format = MpClipboardFormatType.None, 
            string formatInfo = "",
            int priority = 0) {
            MpAppInteropSetting ais = null;

            var dupCheck = await MpDataModelProvider.GetInteropSettingsByAppId(appId);
            if(dupCheck != null) {                
                ais = dupCheck.FirstOrDefault(x => x.FormatType == format);
            }
            if(ais == null) {
                ais = new MpAppInteropSetting() {
                    AppInteropSettingGuid = System.Guid.NewGuid(),
                    FormatType = format,
                    FormatInfo = formatInfo,
                    Priority = priority
                };
            } else {
                MpConsole.WriteTraceLine("Duplicate app setting detected, overwriting: " + ais);
                ais.FormatInfo = formatInfo;
                ais.Priority = priority;
            }

            await ais.WriteToDatabaseAsync();
            return ais;
        }
        public MpAppInteropSetting() { }

    }
}
