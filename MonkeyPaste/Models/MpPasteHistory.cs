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

        [ForeignKey(typeof(MpClient))]
        [Column("fk_MpClientId")]
        public int ClientId { get; set; }

        [ForeignKey(typeof(MpClip))]
        [Column("fk_MpCopyItemId")]
        public int ClipId { get; set; }

        [ForeignKey(typeof(MpUrl))]
        [Column("fk_MpUrlId")]
        public int UrlId { get; set; }

        [ForeignKey(typeof(MpApp))]
        [Column("fk_MpAppId")]
        public int AppId { get; set; }
        #endregion

        public MpPasteHistory() : base(typeof(MpPasteHistory)) { }
    }
}
