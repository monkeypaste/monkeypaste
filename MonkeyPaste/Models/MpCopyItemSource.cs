using MonkeyPaste.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpCopyItemSourceType {
        None = 0,
        App,
        Url,
        CopyItem
    };

    public class MpCopyItemSource : MpDbModelBase {
        #region Columns

        [Column("pk_MpCopyItemSourceId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;

        [Column("MpCopyItemSourceGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpCopyItemId")]
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

            if(copyItemId == sourceObjId && sourceType == MpCopyItemSourceType.CopyItem) {
                // self reference (ole within item), ignore
                MpConsole.WriteLine($"Self reference detected. Ignoring MpCopyItemSource create for ciid: " + copyItemId);
                return null;
            }
            if(!createdDateTime.HasValue) {
                createdDateTime = DateTime.UtcNow;
            }

            MpCopyItemSource dupCheck = await MpDataModelProvider.GetCopyItemSourceByMembersAsync(copyItemId, sourceType, sourceObjId);
            if (dupCheck != null) {
                dupCheck.WasDupOnCreate = true;
                return dupCheck;
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

        public MpCopyItemSource() { }

        public override string ToString() {
            return $"[SourceType: {CopyItemSourceType} SourceObjId: {SourceObjId} CopyItemId: {CopyItemId} Created: {CreatedDateTime}]";
        }
    }
}
