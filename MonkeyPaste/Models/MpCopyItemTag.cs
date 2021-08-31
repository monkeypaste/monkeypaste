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

        #endregion

        public static async Task<List<MpCopyItemTag>> GetAllCopyItemsForTagId(int tagId) {
            var allCopyItemTagList = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();
            return allCopyItemTagList.Where(x => x.TagId == tagId).ToList();
        }

        public static async Task DeleteAllCopyItemTagsForCopyItemId(int CopyItemId) {
            var allCopyItemTagList = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();
            var citl = allCopyItemTagList.Where(x => x.CopyItemId == CopyItemId).ToList();
            foreach (var cit in citl) {
                await MpDb.Instance.DeleteItemAsync<MpCopyItemTag>(cit);
            }
        }

        public static async Task DeleteAllCopyItemTagsForTagId(int tagId) {
            var allCopyItemTagList = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();
            var citl = allCopyItemTagList.Where(x => x.TagId == tagId).ToList();
            foreach (var cit in citl) {
                await MpDb.Instance.DeleteItemAsync<MpCopyItemTag>(cit);
            }
        }

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
            var ci = MpDb.Instance.GetDbObjectByTableGuid("MpCopyItem", objParts[1]) as MpCopyItem;
            var t = MpDb.Instance.GetDbObjectByTableGuid("MpTag", objParts[2]) as MpTag;
            cit.CopyItemId = ci.Id;
            cit.TagId = t.Id;
            return cit;
        }

        public string SerializeDbObject() {
            var cig = MpCopyItem.GetCopyItemById(CopyItemId)?.CopyItemGuid;
            var tg = MpTag.GetTagById(TagId)?.TagGuid;
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}",
                ParseToken,
                CopyItemTagGuid.ToString(),
                cig,
                tg);
        }

        public Type GetDbObjectType() {
            return typeof(MpCopyItemTag);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            var cig = MpCopyItem.GetCopyItemById(CopyItemId)?.CopyItemGuid;
            var tg = MpTag.GetTagById(TagId)?.TagGuid;
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
                cig);
            diffLookup = CheckValue(TagId, other.TagId,
                "fk_MpTagId",
                diffLookup,
                tg);

            return diffLookup;
        }


        public MpCopyItemTag() { }
    }
}
