using MonkeyPaste.Common;
using SQLite;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public class MpAppClipboardShortcuts : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpAppClipboardShortcutsId")]
        public override int Id { get; set; }

        [Column("MpAppClipboardShortcutsGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpAppId")]
        public int AppId { get; set; }

        public string PasteCmdKeyString { get; set; } = string.Empty;
        public string CopyCmdKeyString { get; set; } = string.Empty;

        #endregion

        #region Properties
        #endregion

        public static async Task<MpAppClipboardShortcuts> CreateAsync(
            int appId = 0,
            string pasteCmdKeyString = "",
            string copyCmdKeyString = "") {
            if (appId == 0) {
                throw new Exception("Must have appid");
            }
            var dupCheck = await MpDataModelProvider.GetAppClipboardShortcutsAsync(appId);
            if (dupCheck != null) {
                dupCheck.PasteCmdKeyString = pasteCmdKeyString;
                dupCheck.CopyCmdKeyString = copyCmdKeyString;
                await dupCheck.WriteToDatabaseAsync();
                return dupCheck;
            }
            var aps = new MpAppClipboardShortcuts() {
                Guid = System.Guid.NewGuid().ToString(),
                AppId = appId,
                PasteCmdKeyString = pasteCmdKeyString,
                CopyCmdKeyString = copyCmdKeyString
            };

            await aps.WriteToDatabaseAsync();
            return aps;
        }
        public MpAppClipboardShortcuts() { }


        public override async Task WriteToDatabaseAsync() {
            if (PasteCmdKeyString.ToLower() == Mp.Services.PlatformShorcuts.PasteKeys.ToLower() ||
                CopyCmdKeyString.ToLower() == Mp.Services.PlatformShorcuts.CopyKeys.ToLower()) {
                MpDebug.Break("Redundant assignment error");
            }
            await base.WriteToDatabaseAsync();
        }

    }
}
