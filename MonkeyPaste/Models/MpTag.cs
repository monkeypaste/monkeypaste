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
        public int TagSortIdx { get; set; } = 1;

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId")]
        public int ColorId { get; set; }


        private MpColor _tagColor;
        [Ignore]
        public MpColor TagColor { 
            get {
                if(_tagColor == null && ColorId > 0) {
                    _tagColor = MpColor.GetColorById(ColorId).Result;
                } else if(_tagColor != null && _tagColor.Id != ColorId) {
                    if(ColorId == 0) {
                        ColorId = _tagColor.Id;
                    } else {
                        _tagColor = MpColor.GetColorById(ColorId).Result;
                    }
                }
                return _tagColor;
            }
            set {
                if(_tagColor != value) {
                    _tagColor = value;
                    if(_tagColor != null) {
                        ColorId = _tagColor.Id;
                    } else {
                        ColorId = 0;
                    }
                }
            }
        }

        public string TagName { get; set; } = "Untitled";

        [Ignore]
        [ManyToMany(typeof(MpClip))]
        public List<MpClip> ClipList { get; set; }

        //unused        
        //public int ParentTagId { get; set; }
        #endregion

        public MpTag() {
        }

        public async Task<bool> IsLinkedWithClipAsync(MpClip clip) {
            if(clip == null) {
                return false;
            }
            var citl = await MpClipTag.GetAllClipsForTagId(Id);
            return citl.Any(x => x.ClipId == clip.Id);
        }

        public async Task<bool> LinkWithClipAsync(MpClip clip) {
            if(clip == null) {
                return false;
            }
            //returns FALSE if Clip is already linked to maintain correct counts
            bool isLinked = await IsLinkedWithClipAsync(clip);
            if (isLinked) {
                return false;
            }

            await MpDb.Instance.AddItem<MpClipTag>(new MpClipTag() { ClipId = clip.Id, TagId = Id });

            //ClipList.Add(clip);
            Console.WriteLine("Tag link created between tag " + Id + " with Clip " + clip.Id);
            return true;
        }
        public async Task UnlinkWithClipAsync(MpClip clip) {
            if(clip == null) {
                return;
            }
            bool isLinked = await IsLinkedWithClipAsync(clip);
            if (!isLinked) {
                return;
            }
            var result = await MpDb.Instance.QueryAsync<MpClipTag>(@"select * from MpCopyItemTag where fk_MpTagId=? and fk_MpCopyItemId=?", Id, clip.Id);
            await MpDb.Instance.DeleteItem<MpClipTag>(result[0]);

            //var clipToRemove = ClipList.Where(x => x.Id == clip.Id).FirstOrDefault();
            //if(clipToRemove != null) {
            //    ClipList.Remove(clipToRemove);
            //}
            Console.WriteLine("Tag link removed between tag " + Id + " with Clip " + clip.Id + " ignoring...");
        }

        public async Task DeleteFromDatabaseAsync() {
            //await MpDb.Instance.ExecuteWriteAsync<MpTag>(
            //    "delete from MpTag where Id=@tid",
            //    new Dictionary<string, object> {
            //        { "@tid", Id }
            //    });

            //await MpDb.Instance.ExecuteWriteAsync<MpClipTag>(
            //    "delete from MpClipTag where TagId=@tid",
            //    new Dictionary<string, object> {
            //        { "@tid", Id }
            //    });
        }

        public async Task<MpTag> CreateFromLogs(string tagGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var cdr = await MpDb.Instance.GetObjDbRow("MpTag", tagGuid);
            MpTag newTag = null;
            if (cdr == null) {
                newTag = new MpTag();
            } else {
                newTag = cdr as MpTag;
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpTagGuid":
                        newTag.TagGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "TagName":
                        newTag.TagName = li.AffectedColumnValue;
                        break;
                    case "SortIdx":
                        newTag.TagSortIdx = Convert.ToInt32(li.AffectedColumnValue);
                        break;
                    case "fk_MpColorId":
                        //var color = await MpDb.Instance.GetObjDbRow("MpColor", li.AffectedColumnValue);
                        var color = await MpColor.GetColorByGuid(li.AffectedColumnValue);
                        newTag.ColorId = (color as MpColor).Id;
                        break;
                    default:
                        throw new Exception(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                }
            }
            await MpDb.Instance.AddOrUpdate<MpTag>(newTag,fromClientGuid);
            return newTag;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            var dbLog = new MpTag() {
                TagGuid = System.Guid.Parse(objParts[0]),
                TagName = objParts[1],
                TagSortIdx = Convert.ToInt32(objParts[2]),
                ColorId = Convert.ToInt32(objParts[3])
            };
            return dbLog;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}",
                ParseToken,
                TagGuid.ToString(),
                TagName,
                TagSortIdx,
                ColorId);
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
            //if(Id > 0) {
            //    diffLookup = CheckValue(Id, other.Id,
            //        "pk_MpTagId",
            //        diffLookup);
            //}
            diffLookup = CheckValue(TagGuid, other.TagGuid,
                "MpTagGuid",
                diffLookup);
            diffLookup = CheckValue(ParentTagId, other.ParentTagId,
                "fk_ParentTagId",
                diffLookup);
            diffLookup = CheckValue(
                TagName, other.TagName,
                "G",
                diffLookup);
            diffLookup = CheckValue(
                TagSortIdx, other.TagSortIdx,
                "SortIdx",
                diffLookup);
            diffLookup = CheckValue(
                ColorId, other.ColorId,
                "fk_MpColorId",
                diffLookup,
                TagColor.ColorGuid
                );

            return diffLookup;
        }
    }
}
