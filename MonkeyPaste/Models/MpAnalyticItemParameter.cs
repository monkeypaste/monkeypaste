using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
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

        [Column("Key")]
        public string Key { get; set; } = string.Empty;

        [Column("ValueCsv")]
        public string ValueCsv { get; set; } = string.Empty;

        [Column("SortOrderIdx")]
        public int SortOrderIdx { get; set; } = 0;

        [Column("IsRequired")]
        public int IsRequired { get; set; } = 0;

        [Column("IsHeaderParameter")]
        public int IsHeader { get; set; } = 0;

        [Column("IsRequestParameter")]
        public int IsRequest { get; set; } = 0;
        #endregion

        #region Fk Models

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpAnalyticItem AnalyticItem { get; set; }


        #endregion

        #region Properties

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
        public bool IsRequestParameter {
            get {
                return IsRequest == 1;
            }
            set {
                if (IsRequestParameter != value) {
                    IsRequest = value ? 1 : 0;
                }
            }
        }

        [Ignore]
        public bool IsResponseParameter {
            get {
                return !IsRequestParameter;
            }
        }

        [Ignore]
        public bool IsHeaderParameter {
            get {
                return IsHeader == 1;
            }
            set {
                if (IsHeaderParameter != value) {
                    IsHeader = value ? 1 : 0;
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

        #endregion

        public static async Task<MpAnalyticItemParameter> Create(MpAnalyticItem parentItem,string key, string value, bool isRequired, bool isHeader, bool isRequest, int sortOrderIdx) {
            if(parentItem == null) {
                throw new Exception("Parameter must be associated with an item");
            }
            var dupItem = await MpDataModelProvider.Instance.GetAnalyticItemParameterByKey(parentItem.Id,key);
            if (dupItem != null) {
                dupItem = await MpDb.Instance.GetItemAsync<MpAnalyticItemParameter>(dupItem.Id);
                if(dupItem.ValueCsv != value || dupItem.SortOrderIdx != sortOrderIdx || dupItem.IsParameterRequired != isRequired) {
                    MpConsole.WriteLine($"Updating parameter {key} for {parentItem.Title}");
                    dupItem.ValueCsv = value;
                    dupItem.SortOrderIdx = sortOrderIdx;
                    dupItem.IsParameterRequired = isRequired;
                    await MpDb.Instance.AddOrUpdateAsync<MpAnalyticItemParameter>(dupItem);
                }
                return dupItem;
            }

            var newAnalyticItemParameter = new MpAnalyticItemParameter() {
                AnalyticItemParameterGuid = System.Guid.NewGuid(),
                Key = key,
                ValueCsv = value,
                AnalyticItemId = parentItem.Id,
                SortOrderIdx = sortOrderIdx,
                IsParameterRequired = isRequired,
                IsHeaderParameter = isHeader,
                IsRequestParameter = isRequest
            };

            await MpDb.Instance.AddOrUpdateAsync<MpAnalyticItemParameter>(newAnalyticItemParameter);

            return newAnalyticItemParameter;
        }

        public MpAnalyticItemParameter() { }
    }
}
