using MonkeyPaste.Common;
using SQLite;
using System;
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
            if (appId == 0) {
                throw new Exception("Must have appid");
            }
            var dupCheck = await MpDataModelProvider.GetAppPasteShortcutAsync(appId);
            if (dupCheck != null) {
                dupCheck.PasteCmdKeyString = pasteCmdKeyString;
                await dupCheck.WriteToDatabaseAsync();
                return dupCheck;
            }
            var aps = new MpAppPasteShortcut() {
                Guid = System.Guid.NewGuid().ToString(),
                AppId = appId,
                PasteCmdKeyString = pasteCmdKeyString
            };

            await aps.WriteToDatabaseAsync();
            return aps;
        }
        public MpAppPasteShortcut() { }


        public override async Task WriteToDatabaseAsync() {
            if (string.IsNullOrWhiteSpace(PasteCmdKeyString)) {
                MpDebug.Break("Null assignment error");
            }
            if (PasteCmdKeyString.ToLower() == Mp.Services.PlatformShorcuts.PasteKeys.ToLower()) {
                MpDebug.Break("Redundant assignment error");
            }
            await base.WriteToDatabaseAsync();
        }

    }
}
