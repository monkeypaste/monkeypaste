using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
using SQLite;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpPluginPresetParameterValue : 
        MpDbModelBase, 
        MpIClonableDbModel<MpPluginPresetParameterValue> {
        #region Columns
        [Column("pk_MpPluginPresetParameterValueId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpPluginPresetParameterValueGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpPluginPresetId")]
        public int PluginPresetId { get; set; }

        public int ParamId { get; set; }

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

        //[Ignore]
        //public MpPluginParameterFormat ParameterFormat { get; set; }

        #endregion

        public static async Task<MpPluginPresetParameterValue> CreateAsync(
            int presetId = 0, 
            int paramEnumId = 0, 
            string value = ""
            //MpPluginParameterFormat format = null
            ) {
            if (presetId == 0) {
                throw new Exception("Preset Value must be associated with a preset and parameter");
            }
            //if(format == null) {
            //    throw new Exception("Must have format");
            //}

            var dupItem = await MpDataModelProvider.GetPluginPresetValueAsync(presetId, paramEnumId);
            if (dupItem != null) {
                MpConsole.WriteLine($"Updating preset Id{presetId} for {paramEnumId}");

                dupItem = await MpDataModelProvider.GetItemAsync<MpPluginPresetParameterValue>(dupItem.Id);
                dupItem.PluginPresetId = presetId;
                dupItem.ParamId = paramEnumId;
                dupItem.Value = value;
                //dupItem.ParameterFormat = format;
                await dupItem.WriteToDatabaseAsync();
                return dupItem;
            }

            var newPluginPresetParameterValue = new MpPluginPresetParameterValue() {
                PluginPresetParameterValueGuid = System.Guid.NewGuid(),
                //ParameterFormat = format,
                PluginPresetId = presetId,
                ParamId = paramEnumId,
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
                PluginPresetId = this.PluginPresetId,
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
