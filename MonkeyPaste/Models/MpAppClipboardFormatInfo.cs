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

        public int Priority { get; set; }

        [Ignore]
        public MpClipboardFormatType FormatType {
            get => FormatTypeName.ToEnum<MpClipboardFormatType>();
            set => FormatTypeName = value.ToString();
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

        public static async Task<MpAppClipboardFormatInfo> Create(
            int appId = 0,
            MpClipboardFormatType format = MpClipboardFormatType.None,
            string formatInfo = "",
            int priority = 0) {
            var ais = new MpAppClipboardFormatInfo() {
                AppClipboardFormatInfoGuid = System.Guid.NewGuid(),
                AppId = appId,
                FormatType = format,
                FormatInfo = formatInfo,
                Priority = priority
            };

            await ais.WriteToDatabaseAsync();
            return ais;
        }
        public MpAppClipboardFormatInfo() { }

    }
}
