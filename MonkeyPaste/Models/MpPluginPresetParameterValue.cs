using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
using SQLite;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpParameterHostType {
        None = 0,
        Preset,
        Action
    }
    public class MpPluginPresetParameterValue : 
        MpDbModelBase, 
        MpIClonableDbModel<MpPluginPresetParameterValue> {

        #region Columns
        [Column("pk_MpPluginPresetParameterValueId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpPluginPresetParameterValueGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("e_MpParameterHostType")]
        public string ParameterHostTypeName { get; set; } = MpParameterHostType.None.ToString();

        [Column("fk_ParameterHostId")]
        public int ParameterHostId { get; set; }

        public string ParamId { get; set; }

        public string Value { get; set; } = string.Empty;

        #endregion

        #region Properties

        [Ignore]
        public Guid PluginPresetParameterValueGuid {
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

        [Ignore]
        public MpParameterHostType ParameterHostType {
            get {
                return ParameterHostTypeName.ToEnum<MpParameterHostType>();
            }
            set {
                ParameterHostTypeName = value.ToString();
            }
        }

        #endregion

        public static async Task<MpPluginPresetParameterValue> CreateAsync(
            MpParameterHostType hostType = MpParameterHostType.None,
            int presetId = 0, 
            object paramId = null,
            string value = ""
            //MpPluginParameterFormat format = null
            ) {
            if (hostType == MpParameterHostType.None) {
                throw new Exception("Parameter Value must have a host type");
            }
            if (presetId == 0) {
                throw new Exception("Preset Value must be associated with a preset and parameter");
            }
            if(string.IsNullOrEmpty(paramId.ToString())) {
                throw new Exception("ParamId must cannot be null or empty");
            }

            var dupItem = await MpDataModelProvider.GetPluginPresetValueAsync(hostType, presetId, paramId.ToString());
            if (dupItem != null) {
                MpConsole.WriteLine($"Updating preset Id{presetId} for {paramId}");

                dupItem = await MpDataModelProvider.GetItemAsync<MpPluginPresetParameterValue>(dupItem.Id);
                dupItem.ParameterHostId = presetId;
                dupItem.ParameterHostType = hostType;
                dupItem.ParamId = paramId.ToString();
                dupItem.Value = value;
                //dupItem.ParameterFormat = format;
                await dupItem.WriteToDatabaseAsync();
                return dupItem;
            }

            var newPluginPresetParameterValue = new MpPluginPresetParameterValue() {
                PluginPresetParameterValueGuid = System.Guid.NewGuid(),
                ParameterHostType = hostType,
                ParameterHostId = presetId,
                ParamId = paramId.ToString(),
                Value = value
            };

            await newPluginPresetParameterValue.WriteToDatabaseAsync();

            return newPluginPresetParameterValue;
        }

        #region MpIClonableDbModel Implementation

        public async Task<MpPluginPresetParameterValue> CloneDbModelAsync(bool deepClone = true, bool suppressWrite = false) {
            // NOTE if recreating preset must set PresetId after this method

            var cppv = new MpPluginPresetParameterValue() {
                PluginPresetParameterValueGuid = System.Guid.NewGuid(),
                ParameterHostId = this.ParameterHostId,
                ParameterHostType = this.ParameterHostType,
                ParamId = this.ParamId,
                Value = this.Value,
                //ParameterFormat = this.ParameterFormat
            };
            await cppv.WriteToDatabaseAsync();
            return cppv;
        }

        #endregion

        public MpPluginPresetParameterValue() : base() { }

    }
}
