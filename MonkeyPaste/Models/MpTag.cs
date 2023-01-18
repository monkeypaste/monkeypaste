using System;
using System.Collections.Generic;
using System.Data;
using SQLite;

using System.Threading.Tasks;
using System.Linq;

using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {
    public class MpTag : MpDbModelBase,
        MpIIconResource, MpISyncableDbObject {
        //public const int RecentTagId = 1;
        public static int AllTagId = 1;
        public static int FavoritesTagId = 2;
        public static int HelpTagId = 3;
        //public const int RootTagId = 5;
        #region MpIIconResource Implementation
        object MpIIconResource.IconResourceObj => HexColor;

        #endregion
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTagId")]
        public override int Id { get; set; }

        [Column("fk_ParentTagId")]
        //[ForeignKey(typeof(MpTag))]
        public int ParentTagId { get; set; } = 0;

        [Column("MpTagGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

       

        [Column("TreeSortIdx")]
        public int TreeSortIdx { get; set; } = -1;

        [Column("TraySortIdx")]
        public int PinSortIdx { get; set; } = -1;


        [Column("HexColor")]
        public string HexColor { get; set; }

        public string TagName { get; set; } = string.Empty;

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
            bool ignoreTracking = false,
            bool ignoreSyncing = false) { 
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
                ParentTagId = parentTagId
            };
            await newTag.WriteToDatabaseAsync(ignoreTracking,ignoreSyncing);
            return newTag;
        }

        #endregion

        public MpTag() {            
        }


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
