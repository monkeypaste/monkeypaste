using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using System.Linq;

namespace MonkeyPaste {
    public class MpPasteHistory : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpPasteHistoryId")]
        public override int Id { get; set; }

        [Column("MpPasteHistoryGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public DateTime PasteDateTime { get; set; }

        [Ignore]
        public Guid PasteHistoryGuid {
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
        [Column("fk_MpUserDeviceId")]
        public int UserDeviceId { get; set; }

        [Column("fk_MpCopyItemId")]
        public int CopyItemId { get; set; }

        [Column("fk_MpUrlId")]
        public int UrlId { get; set; }

        [Column("fk_MpAppId")]
        public int AppId { get; set; }

        #endregion

        public static async Task<MpPasteHistory> Create(
            int copyItemId,
            int appId = 0,
            int urlId = 0,
            int userDeviceId = 0,
            DateTime pasteDateTime = default) {
            var ph = new MpPasteHistory() {
                PasteHistoryGuid = System.Guid.NewGuid(),
                CopyItemId = copyItemId,
                AppId = appId,
                UrlId = urlId,
                UserDeviceId = userDeviceId == 0 ? MpDefaultDataModelTools.ThisUserDeviceId : userDeviceId,
                PasteDateTime = pasteDateTime == default ? DateTime.Now : pasteDateTime
            };
            await ph.WriteToDatabaseAsync();
            return ph;
        }
        public MpPasteHistory() { }
    }
}
