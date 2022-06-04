using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpAnalyticItemPresetParameterValue : 
        MpDbModelBase, 
        MpIClonableDbModel<MpAnalyticItemPresetParameterValue> {
        #region Columns
        [Column("pk_MpAnalyticItemPresetParameterValueId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAnalyticItemPresetParameterValueGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpAnalyticItemPreset))]
        [Column("fk_MpAnalyticItemPresetId")]
        public int AnalyticItemPresetId { get; set; }

        public int ParamId { get; set; }

        public string Value { get; set; } = string.Empty;

        #endregion

        #region Fk Models

        #endregion

        #region Properties

        [Ignore]
        public Guid AnalyticItemPresetParameterValueGuid {
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
        public MpPluginParameterFormat ParameterFormat { get; set; }

        #endregion

        public static async Task<MpAnalyticItemPresetParameterValue> Create(
            int presetId = 0, 
            int paramEnumId = 0, 
            string value = "",
            MpPluginParameterFormat format = null) {
            if (presetId == 0) {
                throw new Exception("Preset Value must be associated with a preset and parameter");
            }
            if(format == null) {
                throw new Exception("Must have format");
            }

            var dupItem = await MpDataModelProvider.GetAnalyticItemPresetValue(presetId, paramEnumId);
            if (dupItem != null) {
                MpConsole.WriteLine($"Updating preset Id{presetId} for {paramEnumId}");

                dupItem = await MpDb.GetItemAsync<MpAnalyticItemPresetParameterValue>(dupItem.Id);
                dupItem.AnalyticItemPresetId = presetId;
                dupItem.ParamId = paramEnumId;
                dupItem.Value = value;
                dupItem.ParameterFormat = format;
                await dupItem.WriteToDatabaseAsync();
                return dupItem;
            }

            var newAnalyticItemPresetParameterValue = new MpAnalyticItemPresetParameterValue() {
                AnalyticItemPresetParameterValueGuid = System.Guid.NewGuid(),
                ParameterFormat = format,
                AnalyticItemPresetId = presetId,
                ParamId = paramEnumId,
                Value = value
            };

            await newAnalyticItemPresetParameterValue.WriteToDatabaseAsync();

            return newAnalyticItemPresetParameterValue;
        }

        #region MpIClonableDbModel Implementation

        public async Task<MpAnalyticItemPresetParameterValue> CloneDbModel(bool suppressWrite = false) {
            // NOTE if recreating preset must set PresetId after this method

            var cppv = new MpAnalyticItemPresetParameterValue() {
                AnalyticItemPresetParameterValueGuid = System.Guid.NewGuid(),
                AnalyticItemPresetId = this.AnalyticItemPresetId,
                ParamId = this.ParamId,
                Value = this.Value,
                ParameterFormat = this.ParameterFormat
            };
            await cppv.WriteToDatabaseAsync();
            return cppv;
        }

        #endregion

        public MpAnalyticItemPresetParameterValue() : base() { }

    }
}
