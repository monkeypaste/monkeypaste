using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpAnalyticItemPresetParameterValue : MpDbModelBase, ICloneable {
        #region Columns
        [Column("pk_MpAnalyticItemPresetParameterValueId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAnalyticItemPresetParameterValueGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpAnalyticItemPreset))]
        [Column("fk_MpAnalyticItemPresetId")]
        public int AnalyticItemPresetId { get; set; }

        public int ParameterEnumId { get; set; }

        public string Value { get; set; } = string.Empty;

        #endregion

        #region Fk Models

        [ManyToOne]
        public MpAnalyticItemPreset AnalyticItemPreset { get; set; }

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

        #endregion

        public static async Task<MpAnalyticItemPresetParameterValue> Create(
            MpAnalyticItemPreset parentItem, 
            int paramEnumId, 
            string value) {
            if (parentItem == null) {
                throw new Exception("Preset Value must be associated with a preset and parameter");
            }
            var dupItem = await MpDataModelProvider.Instance.GetAnalyticItemPresetValue(parentItem.Id, paramEnumId);
            if (dupItem != null) {
                MpConsole.WriteLine($"Updating preset {parentItem.Label} for {paramEnumId}");

                dupItem = await MpDb.Instance.GetItemAsync<MpAnalyticItemPresetParameterValue>(dupItem.Id);
                dupItem.AnalyticItemPresetId = parentItem.Id;
                dupItem.ParameterEnumId = paramEnumId;
                dupItem.Value = value;
                await dupItem.WriteToDatabaseAsync();
                return dupItem;
            }

            var newAnalyticItemPresetParameterValue = new MpAnalyticItemPresetParameterValue() {
                AnalyticItemPresetParameterValueGuid = System.Guid.NewGuid(),
                AnalyticItemPreset = parentItem,
                AnalyticItemPresetId = parentItem.Id,
                ParameterEnumId = paramEnumId,
                Value = value
            };

            await newAnalyticItemPresetParameterValue.WriteToDatabaseAsync();

            return newAnalyticItemPresetParameterValue;
        }

        public MpAnalyticItemPresetParameterValue() : base() { }

        public object Clone() {
            return new MpAnalyticItemPresetParameterValue() {
                AnalyticItemPresetId = this.AnalyticItemPresetId,
                ParameterEnumId = this.ParameterEnumId,
                Value = this.Value
            };
        }
    }
}
