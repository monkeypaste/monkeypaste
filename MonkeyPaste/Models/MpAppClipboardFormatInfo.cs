using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpAppClipboardFormatInfo : MpDbModelBase {
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpAppClipboardFormatInfoId")]
        public override int Id { get; set; }

        [Column("MpAppClipboardFormatInfoGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpApp))]
        [Column("fk_MpAppId")]
        public int AppId { get; set; }


        public string FormatTypeName { get; set; }

        public string FormatInfo { get; set; }

        [Column("b_IgnoreFormatValue")]
        public int IgnoreFormatValue { get; set; }

        [Ignore]
        public MpClipboardFormatType FormatType {
            get => FormatTypeName.ToEnum<MpClipboardFormatType>();
            set => FormatTypeName = value.ToString();
        }

        [Ignore]
        public bool IgnoreFormat {
            get => IgnoreFormatValue == 1;
            set => IgnoreFormatValue = value ? 1 : 0;
        }

        [Ignore]
        public Guid AppClipboardFormatInfoGuid {
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

        public static async Task<MpAppClipboardFormatInfo> CreateAsync(
            int appId = 0,
            MpClipboardFormatType format = MpClipboardFormatType.None,
            string formatInfo = "",
            bool ignoreFormat = false) {
            var ais = new MpAppClipboardFormatInfo() {
                AppClipboardFormatInfoGuid = System.Guid.NewGuid(),
                AppId = appId,
                FormatType = format,
                FormatInfo = formatInfo,
                IgnoreFormat = ignoreFormat
            };

            await ais.WriteToDatabaseAsync();
            return ais;
        }
        public MpAppClipboardFormatInfo() { }

    }
}
