using MonkeyPaste.Common;
using SQLite;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpTransactionSourceType {
        None = 0,
        App,
        Url,
        CopyItem,
        AnalyzerPreset,
        UserDevice
    };

    public class MpTransactionSource : MpDbModelBase {
        #region Columns

        [Column("pk_MpTransactionSourceId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;

        [Column("MpTransactionSourceGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        //[Column("fk_MpCopyItemId")]
        ////[Indexed]
        //public int CopyItemId { get; set; }


        [Column("fk_MpCopyItemTransactionId")]
        public int TransactionId { get; set; }

        [Column("e_MpTransactionSourceType")]
        public string SourceTypeStr { get; set; }

        [Column("fk_SourceObjId")]
        public int SourceObjId { get; set; }

        //public string SourceArgs { get; set; }

        public DateTime TransactionDateTime { get; set; }
        #endregion

        #region Properties 

        [Ignore]
        public Guid CopyItemSourceGuid {
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
        public MpTransactionSourceType CopyItemSourceType {
            get => SourceTypeStr.ToEnum<MpTransactionSourceType>();
            set => SourceTypeStr = value.ToString();
        }

        #endregion


        public static async Task<MpTransactionSource> CreateAsync(
            int transactionId = 0,
            int sourceObjId = 0,
            //string sourceArgs = "",
            MpTransactionSourceType sourceType = MpTransactionSourceType.None,
            DateTime? createdDateTime = null,
            bool suppressWrite = false) {
            if (transactionId <= 0) {
                throw new Exception("Must have valid transaction id, id is " + transactionId);
            }
            if (sourceObjId <= 0) {
                throw new Exception("Must have valid sourceObjId, id is " + sourceObjId);
            }
            if (sourceType == MpTransactionSourceType.None) {
                throw new Exception("Must have valid sourceType, sourceType is " + sourceType);
            }

            MpTransactionSource dupCheck = await MpDataModelProvider.GetCopyItemSourceByMembersAsync(transactionId, sourceType, sourceObjId);
            if (dupCheck != null) {
                dupCheck.WasDupOnCreate = true;
                return dupCheck;
            }

            if (sourceType == MpTransactionSourceType.CopyItem) {
                var selfRef_check = await MpDataModelProvider.GetItemAsync<MpCopyItemTransaction>(transactionId);

                if (selfRef_check != null && selfRef_check.CopyItemId == sourceObjId) {
                    // self reference (ole within item), ignore
                    MpConsole.WriteLine($"Self reference detected. Ignoring MpTransactionSource create for ciid: " + sourceObjId);
                    return null;
                }
            }

            if (!createdDateTime.HasValue) {
                createdDateTime = DateTime.UtcNow;
            }

            var ndio = new MpTransactionSource() {
                CopyItemSourceGuid = System.Guid.NewGuid(),
                TransactionId = transactionId,
                SourceObjId = sourceObjId,
                //SourceArgs = sourceArgs,
                CopyItemSourceType = sourceType,
                TransactionDateTime = createdDateTime.Value
            };

            if (!suppressWrite) {
                await ndio.WriteToDatabaseAsync();
            }
            return ndio;
        }


        public MpTransactionSource() { }

        public override string ToString() {
            return $"[SourceType: {CopyItemSourceType} SourceObjId: {SourceObjId} CopyItemTransactionId: {TransactionId} Created: {TransactionDateTime}]";
        }
    }
}
