using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    [Table("MpCopyItemTag")]
    public class MpCopyItemTag : MpDbModelBase, MpISyncableDbObject {
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

        [ForeignKey(typeof(MpTag))]
        [Column("fk_MpTagId")]
        public int TagId { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        [Column("fk_MpCopyItemId")]
        public int CopyItemId { get; set; }

        public int CopyItemSortIdx { get; set; } = -1;

        #endregion

        #region Fk Models

        //[OneToOne]
        //public MpCopyItem CopyItem { get; set; }

        //[OneToOne]
        //public MpTag Tag { get; set; }
        #endregion

        #region Statics

        public static async Task<MpCopyItemTag> Create(int tagId,int copyItemId, int sortIdx = 0) {
            var dupCheck = await MpDataModelProvider.Instance.GetCopyItemTagForTagAsync(tagId, copyItemId); //MpDb.Instance.GetItemsAsync<MpCopyItemTag>().Where(x => x.TagId == tagId && x.CopyItemId == copyItemId).FirstOrDefault();
            if(dupCheck != null) {
                if(dupCheck.CopyItemSortIdx != sortIdx) {
                    dupCheck.CopyItemSortIdx = sortIdx;
                    await MpDb.Instance.UpdateItemAsync<MpCopyItemTag>(dupCheck);
                }
                return dupCheck;
            }

            var newCopyItemTag = new MpCopyItemTag() {
                CopyItemTagGuid = System.Guid.NewGuid(),
                TagId = tagId,
                CopyItemId = copyItemId,
                CopyItemSortIdx = sortIdx
            };

            await MpDb.Instance.AddOrUpdateAsync<MpCopyItemTag>(newCopyItemTag);

            return newCopyItemTag;
        }

        #endregion
              

        #region Sync

        public async Task<object> CreateFromLogs(string tagGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var citdr = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpCopyItemTag", tagGuid);
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
                        var cidr = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpCopyItem", li.AffectedColumnValue) as MpCopyItem;
                        newCopyItemTag.CopyItemId = cidr.Id;
                        break;
                    case "fk_MpTagId":
                        var tdr = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpTag", li.AffectedColumnValue) as MpTag;
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

        public async Task<object> DeserializeDbObject(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var cit = new MpCopyItemTag() {
                CopyItemTagGuid = System.Guid.Parse(objParts[0])
            };
            var ci = await MpDb.Instance.GetDbObjectByTableGuidAsync<MpCopyItem>(objParts[1]);
            var t = await MpDb.Instance.GetDbObjectByTableGuidAsync<MpTag>(objParts[2]);
            cit.CopyItemId = ci.Id;
            cit.TagId = t.Id;
            return cit;
        }

        public async Task<string> SerializeDbObject() {
            var cit =await  MpDb.Instance.GetItemAsync<MpCopyItem>(CopyItemId);
            var t = await MpDb.Instance.GetItemAsync<MpTag>(TagId);

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

        public async Task<Dictionary<string, string>> DbDiff(object drOrModel) {
            var cit = await MpDb.Instance.GetItemAsync<MpCopyItem>(CopyItemId);
            var t = await MpDb.Instance.GetItemAsync<MpTag>(TagId);

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

        #region Public Methods

        public MpCopyItemTag() { }

        public bool IsSudoTag() {
            return Id != MpTag.AllTagId && Id != MpTag.RecentTagId;
        }
        #endregion
    }
}
