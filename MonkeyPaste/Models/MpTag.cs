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
        //public const int HelpTagId = 4;
        //public const int RootTagId = 5;

        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTagId")]
        public override int Id { get; set; }

        [Column("fk_ParentTagId")]
        [ForeignKey(typeof(MpTag))]
        public int ParentTagId { get; set; } = 0;

        [Column("MpTagGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

       

        [Column("SortIdx")]
        public int TagSortIdx { get; set; } = 0;

        [Column("HexColor")]
        public string HexColor { get; set; }

        public string TagName { get; set; } = string.Empty;

        public int Pinned { get; set; } = 0;

        #endregion

        #region Fk Models
        [ManyToMany(typeof(MpCopyItemTag))]
        public List<MpCopyItem> CopyItems { get; set; } = new List<MpCopyItem>();

        //[OneToMany]
        //public List<MpShortcut> Shortcuts { get; set; }
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
        public bool IsPinned {
            get => Pinned == 1;
            set => Pinned = value == true ? 1:0;
        }

        #endregion

        #region Statics

        public static async Task<MpTag> Create(
            string tagName = "Untitled", 
            int sortIdx = 0,
            int parentTagId = 0, 
            string hexColor = "") {
            hexColor = string.IsNullOrEmpty(hexColor) ? MpHelpers.GetRandomColor().ToHex() : hexColor;
            MpTag newTag = new MpTag() {
                TagName = tagName,
                HexColor = hexColor,
                TagSortIdx = sortIdx,
                ParentTagId = parentTagId
            };
            await newTag.WriteToDatabaseAsync();
            return newTag;
        }

        #endregion

        public MpTag() {            
        }


        public async Task<object> CreateFromLogs(string tagGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
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

        public async Task<string> SerializeDbObject() {
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

        public async Task<Dictionary<string, string>> DbDiff(object drOrModel) {
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
