using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

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

        //[ForeignKey(typeof(MpTag))]
        [Column("fk_MpTagId")]
        [Indexed]
        public int TagId { get; set; }

        //[ForeignKey(typeof(MpCopyItem))]
        [Column("fk_MpCopyItemId")]
        [Indexed]
        public int CopyItemId { get; set; }

        public int CopyItemSortIdx { get; set; } = 0;

        #endregion

        #region Fk Models

        //[OneToOne]
        //public MpCopyItem CopyItem { get; set; }

        //[OneToOne]
        //public MpTag Tag { get; set; }
        #endregion

        #region Statics

        public static async Task<MpCopyItemTag> Create(int tagId,int copyItemId, int sortIdx = 0) {
            var dupCheck = await MpDataModelProvider.GetCopyItemTagForTagAsync(copyItemId,tagId);
            if(dupCheck != null) {
                return dupCheck;
            }

            var newCopyItemTag = new MpCopyItemTag() {
                CopyItemTagGuid = System.Guid.NewGuid(),
                TagId = tagId,
                CopyItemId = copyItemId,
                CopyItemSortIdx = sortIdx
            };

            await newCopyItemTag.WriteToDatabaseAsync();

            return newCopyItemTag;
        }

        #endregion              

        #region Sync

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
            var cit =await  MpDb.GetItemAsync<MpCopyItem>(CopyItemId);
            var t = await MpDb.GetItemAsync<MpTag>(TagId);

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
            var cit = await MpDb.GetItemAsync<MpCopyItem>(CopyItemId);
            var t = await MpDb.GetItemAsync<MpTag>(TagId);

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
            return Id != MpTag.AllTagId;
        }
        #endregion
    }
}
