﻿using MonkeyPaste.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpTransactionSourceType {
        None = 0,
        App,
        Url,
        CopyItem,
        AnalyzerPreset
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
            MpTransactionSourceType sourceType = MpTransactionSourceType.None,
            DateTime? createdDateTime = null,
            bool suppressWrite = false) {
            if(transactionId <= 0) {
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
                } else {
                    // in case source item is deleted (or generally just to make it easier to query)
                    // recursively replicate source item's sources for this link
                    ReplicateCopyItemSourceTreeAsync(transactionId, sourceObjId).FireAndForgetSafeAsync();
                }
            }

            if (!createdDateTime.HasValue) {
                createdDateTime = DateTime.UtcNow;
            }

            var ndio = new MpTransactionSource() {
                CopyItemSourceGuid = System.Guid.NewGuid(),
                TransactionId = transactionId,
                SourceObjId = sourceObjId,
                CopyItemSourceType = sourceType,
                TransactionDateTime = createdDateTime.Value
            };

            if(!suppressWrite) {
                await ndio.WriteToDatabaseAsync();
            }
            return ndio;
        }

        private static async Task ReplicateCopyItemSourceTreeAsync(int targetCopyItemId, int sourceCopyItemId) {
            // get source source's
            var source_cisl = await MpDataModelProvider.GetCopyItemSources(sourceCopyItemId);

            // get recursive tasks (NOTE trying to avoid infinite loop by also filtering out ref's to target, not sure if thats needed)
            var traverse_tasks = 
                source_cisl
                .Where(x => x.CopyItemSourceType == MpTransactionSourceType.CopyItem && x.SourceObjId != targetCopyItemId)
                .Select(x => ReplicateCopyItemSourceTreeAsync(targetCopyItemId, x.SourceObjId));
            
            // get source source's write tasks
            var write_tasks = source_cisl.Select(x => CreateAsync(targetCopyItemId, x.SourceObjId, x.CopyItemSourceType));

            // fire at will
            Task.WhenAll(traverse_tasks).FireAndForgetSafeAsync();
            Task.WhenAll(write_tasks).FireAndForgetSafeAsync();
        }

        public MpTransactionSource() { }

        public override string ToString() {
            return $"[SourceType: {CopyItemSourceType} SourceObjId: {SourceObjId} CopyItemTransactionId: {TransactionId} Created: {TransactionDateTime}]";
        }
    }
}