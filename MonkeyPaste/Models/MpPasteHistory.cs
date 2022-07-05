using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
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

        [ForeignKey(typeof(MpUserDevice))]
        [Column("fk_MpUserDeviceId")]
        public int UserDeviceId { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        [Column("fk_MpCopyItemId")]
        public int CopyItemId { get; set; }

        [ForeignKey(typeof(MpUrl))]
        [Column("fk_MpUrlId")]
        public int UrlId { get; set; }

        [ForeignKey(typeof(MpApp))]
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
                UserDeviceId = userDeviceId == 0 ? MpPrefViewModel.Instance.ThisUserDevice.Id : userDeviceId,
                PasteDateTime = pasteDateTime == default ? DateTime.Now : pasteDateTime
            };
            await ph.WriteToDatabaseAsync();
            return ph;
        }
        public MpPasteHistory() { }
    }
}
