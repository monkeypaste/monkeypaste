using MonkeyPaste.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste {
    [Table("MpCopyItemTag")]
    public class MpCopyItemTag :
        MpDbModelBase,
        MpIClonableDbModel<MpCopyItemTag>,
        MpISyncableDbObject {
        #region Interfaces

        #region MpIClonableDbModel Implementation
        public async Task<MpCopyItemTag> CloneDbModelAsync(
            bool deepClone = true,
            bool suppressWrite = false) {
            // NOTE deepClone and parent are ignored for this model
            // NOTE2 have to ignore duplicate or it will not clone

            var cloned_cit = await MpCopyItemTag.CreateAsync(
                tagId: TagId,
                copyItemId: CopyItemId,
                sortIdx: CopyItemSortIdx,
                ignoreDuplicate: true,
                suppressWrite: suppressWrite);
            return cloned_cit;
        }

        #endregion

        #region MpISyncableDbObject Implementation

        public async Task<object> CreateFromLogsAsync(string tagGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var citdr = await MpDb.GetDbObjectByTableGuidAsync("MpCopyItemTag", tagGuid);
            MpCopyItemTag newCopyItemTag = null;
            if (citdr == null) {
                newCopyItemTag = new MpCopyItemTag();
            } else {
                newCopyItemTag = citdr as MpCopyItemTag;
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpCopyItemTagGuid":
                        newCopyItemTag.CopyItemTagGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "fk_MpCopyItemId":
                        var cidr = await MpDb.GetDbObjectByTableGuidAsync("MpCopyItem", li.AffectedColumnValue) as MpCopyItem;
                        newCopyItemTag.CopyItemId = cidr.Id;
                        break;
                    case "fk_MpTagId":
                        var tdr = await MpDb.GetDbObjectByTableGuidAsync("MpTag", li.AffectedColumnValue) as MpTag;
                        newCopyItemTag.TagId = tdr.Id;
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            //newCopyItemTag.WriteToDatabase(fromClientGuid);
            return newCopyItemTag;
        }

        public async Task<object> DeserializeDbObjectAsync(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var cit = new MpCopyItemTag() {
                CopyItemTagGuid = System.Guid.Parse(objParts[0])
            };
            var ci = await MpDb.GetDbObjectByTableGuidAsync<MpCopyItem>(objParts[1]);
            var t = await MpDb.GetDbObjectByTableGuidAsync<MpTag>(objParts[2]);
            cit.CopyItemId = ci.Id;
            cit.TagId = t.Id;
            return cit;
        }

        public async Task<string> SerializeDbObjectAsync() {
            var cit = await MpDataModelProvider.GetItemAsync<MpCopyItem>(CopyItemId);
            var t = await MpDataModelProvider.GetItemAsync<MpTag>(TagId);

            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}",
                ParseToken,
                CopyItemTagGuid.ToString(),
                (cit == null ? string.Empty : cit.Guid),
                (t == null ? string.Empty : t.Guid));
        }

        public Type GetDbObjectType() {
            return typeof(MpCopyItemTag);
        }

        public async Task<Dictionary<string, string>> DbDiffAsync(object drOrModel) {
            var cit = await MpDataModelProvider.GetItemAsync<MpCopyItem>(CopyItemId);
            var t = await MpDataModelProvider.GetItemAsync<MpTag>(TagId);

            MpCopyItemTag other = null;
            if (drOrModel is MpCopyItemTag) {
                other = drOrModel as MpCopyItemTag;
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpCopyItemTag();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(CopyItemTagGuid, other.CopyItemTagGuid,
                "MpCopyItemTagGuid",
                diffLookup);
            diffLookup = CheckValue(CopyItemId, other.CopyItemId,
                "fk_MpCopyItemId",
                diffLookup,
                (cit == null ? string.Empty : cit.Guid));
            diffLookup = CheckValue(TagId, other.TagId,
                "fk_MpTagId",
                diffLookup,
                (t == null ? string.Empty : t.Guid));

            return diffLookup;
        }

        #endregion

        #endregion

        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemTagId")]
        public override int Id { get; set; }

        [Column("MpCopyItemTagGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid CopyItemTagGuid {
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

        [Column("fk_MpTagId")]
        [Indexed]
        public int TagId { get; set; }

        [Column("fk_MpCopyItemId")]
        [Indexed]
        public int CopyItemId { get; set; }

        public int CopyItemSortIdx { get; set; } = 0;

        public DateTime CreatedDateTime { get; set; }

        #endregion

        #region Statics

        public static async Task<MpCopyItemTag> CreateAsync(
            int tagId = 0,
            int copyItemId = 0,
            int sortIdx = 0,
            bool suppressWrite = false,
            bool ignoreDuplicate = false) {
            // NOTE if no sort specified its ignored since its unimplemented
            // but would allow for a custom user defined sort (dnd?)

            if (tagId == 0) {
                throw new Exception("Must have tag id");
            }
            if (tagId == MpTag.AllTagId) {
                throw new Exception("No physical links to all tag should be made");
            }
            if (copyItemId == 0) {
                throw new Exception("Must have copy item id");
            }

            if (!ignoreDuplicate) {
                var dupCheck = await MpDataModelProvider.GetCopyItemTagForTagAsync(copyItemId, tagId);
                if (dupCheck != null) {
                    dupCheck.WasDupOnCreate = true;
                    return dupCheck;
                }
            }

            var newCopyItemTag = new MpCopyItemTag() {
                CopyItemTagGuid = System.Guid.NewGuid(),
                TagId = tagId,
                CopyItemId = copyItemId,
                CopyItemSortIdx = sortIdx
            };
            if (!suppressWrite) {
                await newCopyItemTag.WriteToDatabaseAsync();
            }

            return newCopyItemTag;
        }

        #endregion

        #region Public Methods

        public MpCopyItemTag() { }

        public bool IsSudoTag() {
            return Id != MpTag.AllTagId;
        }
        public override async Task WriteToDatabaseAsync() {
            if (CopyItemId < 0) {
                // why is this happening? (have a hunch its when dragging tile onto pin tray or onto tag)
                Debugger.Break();
                CopyItemId *= -1;
            }
            if (Id == 0) {
                CreatedDateTime = DateTime.Now;
            }
            await base.WriteToDatabaseAsync();

        }
        #endregion
    }
}
