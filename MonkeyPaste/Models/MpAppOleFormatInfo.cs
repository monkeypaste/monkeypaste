using SQLite;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpAppOleFormatInfo : MpDbModelBase {
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpAppOleFormatInfoId")]
        public override int Id { get; set; }

        [Column("MpAppOleFormatInfoGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpAppId")]
        public int AppId { get; set; }


        public string FormatName { get; set; }

        public string FormatInfo { get; set; }

        [Column("b_IgnoreFormatValue")]
        public int IgnoreFormatValue { get; set; }


        [Ignore]
        public bool IgnoreFormat {
            get => IgnoreFormatValue == 1;
            set => IgnoreFormatValue = value ? 1 : 0;
        }

        [Ignore]
        public Guid AppOleFormatInfoGuid {
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

        public static async Task<MpAppOleFormatInfo> CreateAsync(
            int appId = 0,
            string format = "",
            string formatInfo = "",
            bool ignoreFormat = false,
            bool suppressWrite = false) {
            if (string.IsNullOrEmpty(format)) {
                throw new Exception("Must have format name");
            }
            if (appId == 0) {
                throw new Exception("Must have app id");
            }
            var dupCheck = await MpDataModelProvider.GetAppOleFormatInfosByAppIdAsync(appId);
            if (dupCheck.Any(x => x.FormatName.ToLower() == format.ToLower())) {
                var dup = dupCheck.FirstOrDefault(x => x.FormatName.ToLower() == format.ToLower());
                dup.WasDupOnCreate = true;
                dup.FormatInfo = formatInfo;
                dup.IgnoreFormat = ignoreFormat;
                if (!suppressWrite) {
                    await dup.WriteToDatabaseAsync();
                }
                return dup;
            }
            var ais = new MpAppOleFormatInfo() {
                AppOleFormatInfoGuid = System.Guid.NewGuid(),
                AppId = appId,
                FormatName = format,
                FormatInfo = formatInfo,
                IgnoreFormat = ignoreFormat
            };

            await ais.WriteToDatabaseAsync();
            return ais;
        }
        public MpAppOleFormatInfo() { }

    }
}
