using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
using SQLite;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace MonkeyPaste {
    public enum MpParameterHostType {
        None = 0,
        Preset,
        Action
    }
    public class MpParameterValue : 
        MpDbModelBase, MpIParamterValueProvider,
        MpIClonableDbModel<MpParameterValue> {

        #region Columns
        [Column("pk_MpParameterValueId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpParameterValueGuid")]
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

        public static async Task<MpParameterValue> CreateAsync(
            MpParameterHostType hostType = MpParameterHostType.None,
            int hostId = 0, 
            object paramId = null,
            string value = "") {
            if (hostType == MpParameterHostType.None) {
                throw new Exception("Parameter Value must have a host type");
            }
            if (hostId == 0) {
                throw new Exception("Preset Value must be associated with a preset and parameter");
            }
            if(string.IsNullOrEmpty(paramId.ToString())) {
                throw new Exception("ParamId must cannot be null or empty");
            }
            var dup_check = await MpDataModelProvider.GetParameterValueAsync(hostType, hostId, paramId.ToString());


            if (dup_check != null) {
                MpConsole.WriteLine($"Updating preset Id{hostId} for {paramId}");
                // when does this happen?
                //Debugger.Break();

                dup_check = await MpDataModelProvider.GetItemAsync<MpParameterValue>(dup_check.Id);
                dup_check.ParameterHostId = hostId;
                dup_check.ParameterHostType = hostType;
                dup_check.ParamId = paramId.ToString();
                dup_check.Value = value;
                //dupItem.ParameterFormat = format;
                await dup_check.WriteToDatabaseAsync();
                return dup_check;
            }

            var newPluginPresetParameterValue = new MpParameterValue() {
                PluginPresetParameterValueGuid = System.Guid.NewGuid(),
                ParameterHostType = hostType,
                ParameterHostId = hostId,
                ParamId = paramId.ToString(),
                Value = value
            };

            await newPluginPresetParameterValue.WriteToDatabaseAsync();

            return newPluginPresetParameterValue;
        }

        #region MpIClonableDbModel Implementation

        public async Task<MpParameterValue> CloneDbModelAsync(bool deepClone = true, bool suppressWrite = false) {
            // NOTE if recreating preset must set PresetId after this method

            var cppv = new MpParameterValue() {
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

        public MpParameterValue() : base() { }

    }
}
