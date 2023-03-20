using SQLite;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public class MpAppPasteShortcut : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpAppPasteShortcutId")]
        public override int Id { get; set; }

        [Column("MpAppPasteShortcutGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpAppId")]
        public int AppId { get; set; }

        public string PasteCmdKeyString { get; set; } = string.Empty;

        #endregion

        #region Properties
        #endregion

        public static async Task<MpAppPasteShortcut> CreateAsync(
            int appId = 0,
            string pasteCmdKeyString = "") {
            var aps = new MpAppPasteShortcut() {
                Guid = System.Guid.NewGuid().ToString(),
                AppId = appId,
                PasteCmdKeyString = pasteCmdKeyString
            };

            await aps.WriteToDatabaseAsync();
            return aps;
        }
        public MpAppPasteShortcut() { }

    }
}
