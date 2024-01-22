using SQLite;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpAppOlePreset : MpDbModelBase {
        #region Constants
        #endregion

        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpAppOlePresetId")]
        public override int Id { get; set; }

        [Column("MpAppOlePresetGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpAppId")]
        public int AppId { get; set; }

        [Column("fk_MpPresetId")]
        public int PresetId { get; set; }

        #endregion

        #region Properties
        [Ignore]
        public Guid AppOlePresetGuid {
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

        #endregion

        public static async Task<MpAppOlePreset> CreateAsync(
            int appId = 0,
            int presetId = 0,
            bool suppressWrite = false) {
            if (appId == 0) {
                throw new Exception("Must have app id");
            }
            var dupCheck = await MpDataModelProvider.GetAppOlePresetByMembersAsync(appId, presetId);
            if (dupCheck != null) {
                dupCheck.WasDupOnCreate = true;
                return dupCheck;
            }
            var ais = new MpAppOlePreset() {
                AppOlePresetGuid = System.Guid.NewGuid(),
                AppId = appId,
                PresetId = presetId
            };

            await ais.WriteToDatabaseAsync();
            return ais;
        }
        public MpAppOlePreset() { }

    }
}
