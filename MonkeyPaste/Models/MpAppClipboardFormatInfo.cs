using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    public class MpAppClipboardFormatInfo : MpDbModelBase {
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpAppClipboardFormatInfoId")]
        public override int Id { get; set; }

        [Column("MpAppClipboardFormatInfoGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpAppId")]
        public int AppId { get; set; }


        public string FormatType { get; set; }

        public string FormatInfo { get; set; }

        [Column("b_IgnoreFormatValue")]
        public int IgnoreFormatValue { get; set; }


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
            string format = "",
            string formatInfo = "",
            bool ignoreFormat = false,
            bool suppressWrite = false) {
            if(string.IsNullOrEmpty(format)) {
                throw new Exception("Must have format name");
            }
            if(appId == 0) {
                throw new Exception("Must have app id");
            }
            var dupCheck = await MpDataModelProvider.GetAppClipboardFormatInfosByAppIdAsync(appId);
            if(dupCheck.Any(x=>x.FormatType.ToLower() == format.ToLower())) {
                var dup = dupCheck.FirstOrDefault(x => x.FormatType.ToLower() == format.ToLower());
                dup.WasDupOnCreate = true;
                dup.FormatInfo = formatInfo;
                dup.IgnoreFormat = ignoreFormat;
                if(!suppressWrite) {
                    await dup.WriteToDatabaseAsync();
                }
                return dup;                
            }
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
