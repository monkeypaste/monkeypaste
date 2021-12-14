using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpAnalyticItemParameterType {
        None = 0,
        Button,
        Text,
        ComboBox,
        CheckBox,
        Slider,
        //RuntimeMinOffset,//below are only runtime types        
    }
    public class MpAnalyticItemParameter : MpDbModelBase {
        #region Columns
        [Column("pk_MpAnalyticItemParameterId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAnalyticItemParameterGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpAnalyticItem))]
        [Column("fk_MpAnalyticItemId")]
        public int AnalyticItemId { get; set; }

        [Column("Label")]
        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column("ParameterTypeId")]
        public int ParameterTypeId { get; set; } = 0;

        [Column("ParameterValueTypeId")]
        public int ParameterValueTypeId { get; set; } = 0;

        public int EnumId { get; set; } = 0;

        [Column("SortOrderIdx")]
        public int SortOrderIdx { get; set; } = -1;

        [Column("IsRequired")]
        public int IsRequired { get; set; } = 0;

        [Column("IsReadOnly")]
        public int ReadOnly { get; set; } = 0;

        [Column("FormatInfo")]
        public string FormatInfo { get; set; } = string.Empty;
        #endregion

        #region Fk Models

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpAnalyticItem AnalyticItem { get; set; }


        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeDelete)]
        public List<MpAnalyticItemParameterValue> ValueSeeds { get; set; } = new List<MpAnalyticItemParameterValue>();
        #endregion

        #region Properties

        [Ignore]
        public MpAnalyticItemParameterType ParameterType {
            get {
                return (MpAnalyticItemParameterType)ParameterTypeId;
            }
            set {
                if (ParameterTypeId != (int)value) {
                    ParameterTypeId = (int)value;
                }
            }
        }

        [Ignore]
        public MpAnalyticItemParameterValueUnitType ValueType {
            get {
                return (MpAnalyticItemParameterValueUnitType)ParameterValueTypeId;
            }
            set {
                if (ValueType != value) {
                    ParameterValueTypeId = (int)value;
                }
            }
        }

        [Ignore]
        public bool IsParameterRequired {
            get {
                return IsRequired == 1;
            }
            set {
                if (IsParameterRequired != value) {
                    IsRequired = value ? 1 : 0;
                }
            }
        }

        [Ignore]
        public bool IsReadOnly {
            get {
                return ReadOnly == 1;
            }
            set {
                if (IsReadOnly != value) {
                    ReadOnly = value ? 1 : 0;
                }
            }
        }

        [Ignore]
        public Guid AnalyticItemParameterGuid {
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
        //public bool IsRuntimeParameter => IsExecute || IsResult;


        //[Ignore]
        //public bool IsExecute { get; set; } = false;

        //[Ignore]
        //public bool IsResult { get; set; } = false;

        //[Ignore]
        //public bool IsPreset { get; set; } = false;
        #endregion

        public static async Task<MpAnalyticItemParameter> Create(
            MpAnalyticItem analyticItem,
            string label, 
            int enumId, 
            bool isRequired,
            MpAnalyticItemParameterType paramType,
            MpAnalyticItemParameterValueUnitType valueType,
            bool isReadOnly = false, int sortOrderIdx = -1, string formatInfo = "", string description = "") {
            if(analyticItem == null) {
                throw new Exception("Parameter must be associated with an item");
            }
            var dupItem = await MpDataModelProvider.Instance.GetAnalyticItemParameterByKey(analyticItem.Id,label);
            if (dupItem != null) {
                MpConsole.WriteLine($"Updating parameter {label} for {analyticItem.Title}");

                dupItem = await MpDb.Instance.GetItemAsync<MpAnalyticItemParameter>(dupItem.Id);
                dupItem.IsParameterRequired = isRequired;
                dupItem.ParameterType = paramType;
                dupItem.ValueType = valueType;
                dupItem.Label = label;
                dupItem.Description = description;
                dupItem.SortOrderIdx = sortOrderIdx;
                dupItem.FormatInfo = formatInfo;
                dupItem.IsReadOnly = isReadOnly;
                dupItem.EnumId = enumId;
                await dupItem.WriteToDatabaseAsync();
                return dupItem;
            }

            var newAnalyticItemParameter = new MpAnalyticItemParameter() {
                AnalyticItemParameterGuid = System.Guid.NewGuid(),
                AnalyticItem = analyticItem,
                AnalyticItemId = analyticItem.Id,
                Label = label,
                Description = description,
                EnumId = enumId,
                SortOrderIdx = sortOrderIdx,
                IsParameterRequired = isRequired,
                FormatInfo = formatInfo,
                IsReadOnly = isReadOnly,
                ParameterType = paramType,
                ValueType = valueType
            };

            await newAnalyticItemParameter.WriteToDatabaseAsync();

            return newAnalyticItemParameter;
        }

        public MpAnalyticItemParameter() : base() { }
    }
}
