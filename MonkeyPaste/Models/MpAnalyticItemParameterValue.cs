using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpAnalyticItemParameterValueUnitType {
        None = 0,
        Bool,
        Integer,
        Decimal,
        PlainText,
        RichText,
        Html,
        Image,
        Custom
    }

    public class MpAnalyticItemParameterValue : MpDbModelBase {
        #region Columns

        [Column("pk_MpAnalyticItemParameterValueId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAnalyticItemParameterValueGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpAnalyticItemParameter))]
        [Column("fk_MpAnalyticItemParameterId")]
        public int AnalyticItemParameterValueId { get; set; }

        [Column("Value")]
        public string Value { get; set; } = string.Empty;

        [Column("Label")]
        public string Label { get; set; } = string.Empty;

        [Column("IsDefault")]
        public int Default { get; set; } = 0;

        [Column("IsMinimum")]
        public int Minimum { get; set; } = 0;

        [Column("IsMaximum")]
        public int Maximum { get; set; } = 0;
        #endregion

        #region Fk Models

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpAnalyticItemParameter AnalyticItemParameter { get; set; }

        #endregion

        #region Properties
        [Ignore]
        public bool IsDefault {
            get {
                return Default == 1;
            }
            set {
                if (IsDefault != value) {
                    Default = value ? 1 : 0;
                }
            }
        }

        [Ignore]
        public bool IsMinimum {
            get {
                return Minimum == 1;
            }
            set {
                if (IsMinimum != value) {
                    Minimum = value ? 1 : 0;
                }
            }
        }

        [Ignore]
        public bool IsMaximum {
            get {
                return Maximum == 1;
            }
            set {
                if (IsMaximum != value) {
                    Maximum = value ? 1 : 0;
                }
            }
        }


        [Ignore]
        public Guid AnalyticItemParameterValueGuid {
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

        public static async Task<MpAnalyticItemParameterValue> Create(
            MpAnalyticItemParameter parentItem, 
            string value, 
            string label = "", bool isDefault = false, bool isMin = false, bool isMax = false) {
            if (parentItem == null) {
                throw new Exception("Parameter must be associated with an item");
            }

            var newAnalyticItemParameter = new MpAnalyticItemParameterValue() {
                AnalyticItemParameterValueGuid = System.Guid.NewGuid(),
                AnalyticItemParameter = parentItem,
                AnalyticItemParameterValueId = parentItem.Id,
                IsDefault = isDefault,
                IsMinimum = isMin,
                IsMaximum = isMax,
                Label = string.IsNullOrEmpty(label) ? value : label
            };

            await MpDb.Instance.AddOrUpdateAsync<MpAnalyticItemParameterValue>(newAnalyticItemParameter);

            return newAnalyticItemParameter;
        }

        public MpAnalyticItemParameterValue() : base() { }
    }
}
