using MonkeyPaste.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpCopyItemSourceType {
        None = 0,
        App,
        Url,
        CopyItem,
        Plugin
    };

    public class MpCopyItemSource : MpDbModelBase {
        #region Columns

        [Column("pk_MpCopyItemSourceId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;

        [Column("MpCopyItemSourceGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpCopyItemId")]
        [Indexed]
        public int CopyItemId { get; set; }

        [Column("fk_SourceObjId")]
        public int SourceObjId { get; set; }

        [Column("e_MpCopyItemSourceType")]
        public string CopyItemSourceTypeStr { get; set; }

        public DateTime CreatedDateTime { get; set; }
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
        public MpCopyItemSourceType CopyItemSourceType {
            get => CopyItemSourceTypeStr.ToEnum<MpCopyItemSourceType>();
            set => CopyItemSourceTypeStr = value.ToString();
        }

        #endregion


        public static async Task<MpCopyItemSource> CreateAsync(
            int copyItemId = 0,
            int sourceObjId = 0,
            MpCopyItemSourceType sourceType = MpCopyItemSourceType.None,
            DateTime? createdDateTime = null,
            bool suppressWrite = false) {
            if(copyItemId <= 0) {
                throw new Exception("Must have valid copyitem id, id is " + copyItemId);
            }
            if (sourceObjId <= 0) {
                throw new Exception("Must have valid sourceObjId, id is " + sourceObjId);
            }
            if (sourceType == MpCopyItemSourceType.None) {
                throw new Exception("Must have valid sourceType, sourceType is " + sourceType);
            }

            MpCopyItemSource dupCheck = await MpDataModelProvider.GetCopyItemSourceByMembersAsync(copyItemId, sourceType, sourceObjId);
            if (dupCheck != null) {
                dupCheck.WasDupOnCreate = true;
                return dupCheck;
            }

            if (sourceType == MpCopyItemSourceType.CopyItem) {
                if (copyItemId == sourceObjId) {
                    // self reference (ole within item), ignore
                    MpConsole.WriteLine($"Self reference detected. Ignoring MpCopyItemSource create for ciid: " + copyItemId);
                    return null;
                } else {
                    // in case source item is deleted (or generally just to make it easier to query)
                    // recursively replicate source item's sources for this link
                    ReplicateCopyItemSourceTreeAsync(copyItemId, sourceObjId).FireAndForgetSafeAsync();
                }
            }
            
            if(!createdDateTime.HasValue) {
                createdDateTime = DateTime.UtcNow;
            }

            var ndio = new MpCopyItemSource() {
                CopyItemSourceGuid = System.Guid.NewGuid(),
                CopyItemId = copyItemId,
                SourceObjId = sourceObjId,
                CopyItemSourceType = sourceType,
                CreatedDateTime = createdDateTime.Value
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
                .Where(x => x.CopyItemSourceType == MpCopyItemSourceType.CopyItem && x.SourceObjId != targetCopyItemId)
                .Select(x => ReplicateCopyItemSourceTreeAsync(targetCopyItemId, x.SourceObjId));
            
            // get source source's write tasks
            var write_tasks = source_cisl.Select(x => CreateAsync(targetCopyItemId, x.SourceObjId, x.CopyItemSourceType));

            // fire at will
            Task.WhenAll(traverse_tasks).FireAndForgetSafeAsync();
            Task.WhenAll(write_tasks).FireAndForgetSafeAsync();
        }

        public MpCopyItemSource() { }

        public override string ToString() {
            return $"[SourceType: {CopyItemSourceType} SourceObjId: {SourceObjId} CopyItemId: {CopyItemId} Created: {CreatedDateTime}]";
        }
    }
}
