using System;
using System.Collections.Generic;
using System.Data;
using SQLite;
using SQLiteNetExtensions;
using System.Threading.Tasks;
using System.Linq;
using SQLiteNetExtensions.Attributes;
using Newtonsoft.Json;
using FFImageLoading.Helpers.Exif;

namespace MonkeyPaste {
    [Table("MpTag")]
    public class MpTag : MpDbModelBase, MpISyncableDbObject {
        public const int RecentTagId = 1;
        public const int AllTagId = 2;
        public const int FavoritesTagId = 3;
        public const int HelpTagId = 4;

        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTagId")]
        public override int Id { get; set; }

        [Column("fk_ParentTagId")]
        [ForeignKey(typeof(MpTag))]
        public int ParentTagId { get; set; } = 0;

        [Column("MpTagGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid TagGuid {
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

        [Column("SortIdx")]
        public int TagSortIdx { get; set; } = 0;

        [Column("HexColor")]
        public string HexColor { get; set; }

        public string TagName { get; set; } = string.Empty;
        #endregion

        #region Fk Models
        [ManyToMany(typeof(MpCopyItemTag))]
        public List<MpCopyItem> CopyItems { get; set; } = new List<MpCopyItem>();

        //[OneToMany]
        //public List<MpShortcut> Shortcuts { get; set; }
        #endregion

        #region Statics
        #endregion

        public MpTag() {
        }


        public async Task<bool> IsLinkedWithCopyItemAsync(MpCopyItem clip) {
            if(clip == null) {
                return false;
            }
            var citl = await MpCopyItemTag.GetAllCopyItemsForTagIdAsync(Id);
            return citl.Any(x => x.CopyItemId == clip.Id);
        }

        public bool IsLinkedWithCopyItem(MpCopyItem clip) {
            if (clip == null) {
                return false;
            }
            return MpDb.Instance.GetItems<MpCopyItemTag>().Where(x => x.TagId == Id && x.CopyItemId == clip.Id).FirstOrDefault() != null;
        }

        public async Task<bool> LinkWithCopyItemAsync(MpCopyItem clip) {
            if(clip == null) {
                return false;
            }
            //returns FALSE if CopyItem is already linked to maintain correct counts
            bool isLinked = await IsLinkedWithCopyItemAsync(clip);
            if (isLinked) {
                return false;
            }

            await MpDb.Instance.AddItemAsync<MpCopyItemTag>(new MpCopyItemTag() { CopyItemId = clip.Id, TagId = Id });

            //CopyItemList.Add(clip);
            Console.WriteLine("Tag link created between tag " + Id + " with CopyItem " + clip.Id);
            return true;
        }

        public bool LinkWithCopyItem(MpCopyItem clip) {
            if (clip == null) {
                return false;
            }
            //returns FALSE if CopyItem is already linked to maintain correct counts
            bool isLinked = IsLinkedWithCopyItem(clip);
            if (isLinked) {
                return false;
            }

            int sortOrderIdx = 1;
            var lastcit = MpDb.Instance.GetItems<MpCopyItemTag>().Where(x => x.TagId == Id).OrderByDescending(y => y.CopyItemSortIdx).FirstOrDefault();
            if(lastcit != null) {
                sortOrderIdx = lastcit.CopyItemSortIdx + 1;
            }
            MpDb.Instance.AddItem<MpCopyItemTag>(new MpCopyItemTag() { CopyItemId = clip.Id, TagId = Id,CopyItemSortIdx = sortOrderIdx});

            //CopyItemList.Add(clip);
            Console.WriteLine("Tag link created between tag " + Id + " with CopyItem " + clip.Id);
            return true;
        }

        public async Task UnlinkWithCopyItemAsync(MpCopyItem clip) {
            if(clip == null) {
                return;
            }
            bool isLinked = await IsLinkedWithCopyItemAsync(clip);
            if (!isLinked) {
                return;
            }
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTag>(@"select * from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?", Id, clip.Id);
            await MpDb.Instance.DeleteItemAsync<MpCopyItemTag>(result[0]);

            //var clipToRemove = CopyItemList.Where(x => x.Id == clip.Id).FirstOrDefault();
            //if(clipToRemove != null) {
            //    CopyItemList.Remove(clipToRemove);
            //}
            Console.WriteLine("Tag link removed between tag " + Id + " with CopyItem " + clip.Id + " ignoring...");
        }

        public void UnlinkWithCopyItem(MpCopyItem clip) {
            if (clip == null) {
                return;
            }
            bool isLinked = IsLinkedWithCopyItem(clip);
            if (!isLinked) {
                return;
            }
            var result = MpDb.Instance.Query<MpCopyItemTag>(@"select * from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?", Id, clip.Id);
            MpDb.Instance.DeleteItem<MpCopyItemTag>(result[0]);

            //var clipToRemove = CopyItemList.Where(x => x.Id == clip.Id).FirstOrDefault();
            //if(clipToRemove != null) {
            //    CopyItemList.Remove(clipToRemove);
            //}
            Console.WriteLine("Tag link removed between tag " + Id + " with CopyItem " + clip.Id + " ignoring...");
        }

        //public async Task DeleteFromDatabaseAsync() {
        //await MpDb.Instance.ExecuteWriteAsync<MpTag>(
        //    "delete from MpTag where Id=@tid",
        //    new Dictionary<string, object> {
        //        { "@tid", Id }
        //    });

        //await MpDb.Instance.ExecuteWriteAsync<MpCopyItemTag>(
        //    "delete from MpCopyItemTag where TagId=@tid",
        //    new Dictionary<string, object> {
        //        { "@tid", Id }
        //    });
        //}

        public async Task<object> CreateFromLogs(string tagGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            //await Task.Delay(1);
            //return MpDbModelBase.CreateOrUpdateFromLogs(logs, fromClientGuid);

            var cdr = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpTag", tagGuid);
            MpTag newTag = null;
            if (cdr == null) {
                newTag = new MpTag();
            } else {
                newTag = cdr as MpTag;
            }

            foreach (var li in logs.OrderBy(x => x.LogActionDateTime)) {
                switch (li.AffectedColumnName) {
                    case "MpTagGuid":
                        newTag.TagGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "TagName":
                        newTag.TagName = li.AffectedColumnValue;
                        break;
                    //case "SortIdx":
                    //    newTag.TagSortIdx = Convert.ToInt32(li.AffectedColumnValue);
                    //    break;
                    case "HexColor":
                        newTag.HexColor = li.AffectedColumnValue;
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            //await MpDb.Instance.AddOrUpdate<MpTag>(newTag, fromClientGuid);
            return newTag;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            var dbLog = new MpTag() {
                TagGuid = System.Guid.Parse(objParts[0]),
                TagName = objParts[1],
                //TagSortIdx = Convert.ToInt32(objParts[2]),
                HexColor = objParts[2]
            };
            return dbLog;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}",
                ParseToken,
                TagGuid.ToString(),
                TagName,
                //TagSortIdx,
                HexColor);
        }

        public Type GetDbObjectType() {
            return typeof(MpTag);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpTag other = null;
            if(drOrModel == null) {
                other = new MpTag();
            } else if (drOrModel is MpTag) {
                other = drOrModel as MpTag;
            } else {
                throw new Exception("Cannot compare xam model to local model");
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(TagGuid, other.TagGuid,
                "MpTagGuid",
                diffLookup);
            diffLookup = CheckValue(ParentTagId, other.ParentTagId,
                "fk_ParentTagId",
                diffLookup);
            diffLookup = CheckValue(
                TagName, other.TagName,
                "TagName",
                diffLookup);
            //diffLookup = CheckValue(
            //    TagSortIdx, other.TagSortIdx,
            //    "SortIdx",
            //    diffLookup);
            //var c = await MpColor.GetColorById(ColorId);
            diffLookup = CheckValue(
                HexColor, other.HexColor,
                "HexColor",
                diffLookup);

            return diffLookup;
        }
    }
}
