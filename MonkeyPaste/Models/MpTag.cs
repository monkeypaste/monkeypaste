using System;
using System.Collections.Generic;
using System.Data;
using SQLite;

using System.Threading.Tasks;
using System.Linq;

using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System.Diagnostics;

namespace MonkeyPaste {
    public enum MpTagType {
        None,
        Link,
        Query,
        Group
    }
    public class MpTag : 
        MpDbModelBase,
        MpIIconResource, 
        MpIClonableDbModel<MpTag>,
        MpISyncableDbObject {

        #region Constants

        public const int AllTagId = 1;
        public const int FavoritesTagId = 2;
        public const int HelpTagId = 3;
        public const int RootGroupTagId = 7;

        public const MpContentSortType DEFAULT_QUERY_TAG_SORT_TYPE = MpContentSortType.CopyDateTime;
        public const bool DEFAULT_QUERY_TAG_IS_SORT_DESCENDING = true;

        #endregion

        #region Interfaces

        #region MpIClonableDbModel Implementation


        public async Task<MpTag> CloneDbModelAsync(bool deepClone = true, bool suppressWrite = false) {
            // NOTE deepClone is ignored no current need for content links, hotkeys etc.

            var cloned_tag = await MpTag.CreateAsync(
                tagName: TagName,
                treeSortIdx: TreeSortIdx,
                pinSortIdx: PinSortIdx,
                parentTagId: ParentTagId,
                hexColor: HexColor,
                tagType: TagType,
                sortType: SortType,
                isSortDescending: IsSortDescending,
                suppressWrite: suppressWrite);

            if(TagType == MpTagType.Query) {
                var scil = await MpDataModelProvider.GetCriteriaItemsByTagId(Id);
                foreach(var sci in scil) {
                    var cloned_sci = await MpSearchCriteriaItem.CreateAsync(
                        tagId: cloned_tag.Id,
                        sortOrderIdx: sci.SortOrderIdx,
                        nextJoinType: sci.NextJoinType,
                        options: sci.Options,
                        suppressWrite: suppressWrite);
                }
            }
            return cloned_tag;
        }
        #endregion

        #region MpIIconResource Implementation
        object MpIIconResource.IconResourceObj => HexColor;

        #endregion

        #region MpISyncableDbObject Implementation

        public async Task<object> CreateFromLogsAsync(string tagGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            //await Task.Delay(1);
            //return MpDbModelBase.CreateOrUpdateFromLogs(logs, fromClientGuid);

            var cdr = await MpDb.GetDbObjectByTableGuidAsync("MpTag", tagGuid);
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
            //await MpDb.AddOrUpdate<MpTag>(newTag, fromClientGuid);
            return newTag;
        }

        public async Task<object> DeserializeDbObjectAsync(string objStr) {
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

        public async Task<string> SerializeDbObjectAsync() {
            await Task.Delay(1);

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

        public async Task<Dictionary<string, string>> DbDiffAsync(object drOrModel) {
            await Task.Delay(1);

            MpTag other = null;
            if (drOrModel == null) {
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

        #endregion

        #endregion

        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTagId")]
        public override int Id { get; set; }

        [Column("fk_ParentTagId")]
        public int ParentTagId { get; set; } = 0;

        [Column("MpTagGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }
       

        [Column("TreeSortIdx")]
        public int TreeSortIdx { get; set; } = -1;

        [Column("TraySortIdx")]
        public int PinSortIdx { get; set; } = -1;

        [Column("e_MpTagType")]
        public string TagTypeName { get; set; }

        [Column("HexColor")]
        public string HexColor { get; set; }

        public string TagName { get; set; } = string.Empty;

        public string SortTypeName { get; set; } 

        public string IsSortDescendingName { get; set; } 

        #endregion

        #region Properties

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

        [Ignore]
        public MpTagType TagType {
            get => TagTypeName.ToEnum<MpTagType>();
            set => TagTypeName = value.ToString();
        }

        [Ignore]
        public MpContentSortType? SortType {
            get => string.IsNullOrEmpty(SortTypeName) ? null : SortTypeName.ToEnum<MpContentSortType>();
            set => SortTypeName = value == null ? null : value.ToString();
        }

        [Ignore]
        public bool? IsSortDescending {
            get => string.IsNullOrEmpty(IsSortDescendingName) ? null : bool.Parse(IsSortDescendingName);
            set => IsSortDescendingName = value == null ? null : value.ToString();
        }

        [Ignore]
        public bool CanDelete {
            get {
                if(Id == AllTagId || Id == HelpTagId || Id == RootGroupTagId) {
                    return false;
                }
                return true;
            }
        }


        #endregion

        #region Statics

        public static async Task<MpTag> CreateAsync(
            int id = 0,
            string guid = "",
            string tagName = "Untitled", 
            int treeSortIdx = -1,
            int pinSortIdx = -1,
            int parentTagId = 0, 
            string hexColor = "",
            MpTagType tagType = MpTagType.None,
            MpContentSortType? sortType = null,
            bool? isSortDescending = null,
            bool ignoreTracking = false,
            bool ignoreSyncing = false,
            bool suppressWrite = false) { 
            if(tagType == MpTagType.None) {
                throw new Exception("TagType must be specified");
            }

            hexColor = string.IsNullOrEmpty(hexColor) ? MpHelpers.GetRandomColor().ToHex() : hexColor;
            if(treeSortIdx < 0) {
                if(parentTagId <= 0) {
                    treeSortIdx = 0;
                } else {
                    treeSortIdx = await MpDataModelProvider.GetChildTagCountAsync(parentTagId);
                }
            }
            MpTag newTag = new MpTag() {
                Id = id,
                TagGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                TagName = tagName,
                HexColor = hexColor,
                TreeSortIdx = treeSortIdx,
                PinSortIdx = pinSortIdx,
                ParentTagId = parentTagId,
                TagType = tagType,
                SortType = sortType,
                IsSortDescending = isSortDescending
            };
            if(!suppressWrite) {

                await newTag.WriteToDatabaseAsync(ignoreTracking, ignoreSyncing);
            }
            return newTag;
        }

        #endregion

        public MpTag() { }

        public override async Task DeleteFromDatabaseAsync() {
            if(!CanDelete) {
                // this should be caught in view model
                Debugger.Break();
                return;
            }
            List<Task> deleteTasks = new List<Task>();
            if(TagType == MpTagType.Query) {
                var scil = await MpDataModelProvider.GetCriteriaItemsByTagId(Id);
                deleteTasks.AddRange(scil.Select(x => x.DeleteFromDatabaseAsync()));
            }
            deleteTasks.Add(base.DeleteFromDatabaseAsync());
            await Task.WhenAll(deleteTasks);
        }

    }
}
