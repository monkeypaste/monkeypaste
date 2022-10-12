using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using SQLiteNetExtensions.Attributes;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using System.Security.Cryptography;

namespace MonkeyPaste {

    public class MpAppPasteShortcut : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpAppPasteShortcutId")]
        public override int Id { get; set; }

        [Column("MpAppPasteShortcutGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpApp))]
        [Column("fk_MpAppId")]
        public int AppId { get; set; }

        public string PasteCmdKeyString { get; set; } = "Control+V";

        [Column("b_EnterAfterPaste")]
        public int EnterAfterPasteVal { get; set; }

        #endregion

        #region Properties

        [Ignore]
        public bool EnterAfterPaste {
            get => EnterAfterPasteVal == 1;
            set => EnterAfterPasteVal = value ? 1 : 0;
        }

        #endregion

        public static async Task<MpAppPasteShortcut> CreateAsync(
            int appId = 0,
            string pasteCmdKeyString = "Control+V",
            bool enterAfterPaste = false) {
            var aps = new MpAppPasteShortcut() {
                Guid = System.Guid.NewGuid().ToString(),
                AppId = appId,
                EnterAfterPaste = enterAfterPaste,
                PasteCmdKeyString = pasteCmdKeyString
            };

            await aps.WriteToDatabaseAsync();
            return aps;
        }
        public MpAppPasteShortcut() { }

    }
}
