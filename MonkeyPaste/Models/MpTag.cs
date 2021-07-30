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
        public int TagSortIdx { get; set; } = -1;

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId")]
        public int ColorId { get; set; }

        private MpColor _color;
        [Ignore]
        public MpColor Color {
            get {
                if (_color == null && ColorId > 0) {
                    _color = MpColor.GetColorById(ColorId);
                } else if (_color != null && _color.Id != ColorId) {
                    if (ColorId == 0) {
                        ColorId = _color.Id;
                    } else if (ColorId > 0) {
                        _color = MpColor.GetColorById(ColorId);
                    } else {
                        _color = new MpColor();
                    }
                }
                return _color;
            }
            set {
                if (_color != value) {
                    _color = value;
                    if (_color != null) {
                        ColorId = _color.Id;
                    } else {
                        ColorId = 0;
                    }
                }
            }
        }

        public string TagName { get; set; }

        [Ignore]
        [ManyToMany(typeof(MpCopyItem))]
        public List<MpCopyItem> CopyItemList { get; set; }

        //unused        
        //public int ParentTagId { get; set; }
        #endregion

        public MpTag() {
        }

        public async Task<MpColor> GetColor() {
            if (ColorId > 0) {
                var tc = await MpColor.GetColorByIdAsync(ColorId);
                return tc;
            }

            return null;
        }

        public void SetColor(int colorId) {
            ColorId = colorId;
        }

        public async Task<bool> IsLinkedWithCopyItemAsync(MpCopyItem clip) {
            if(clip == null) {
                return false;
            }
            var citl = await MpCopyItemTag.GetAllCopyItemsForTagId(Id);
            return citl.Any(x => x.CopyItemId == clip.Id);
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

        public async Task DeleteFromDatabaseAsync() {
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
        }

        public async Task<object> CreateFromLogs(string tagGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            await Task.Delay(1);
            return MpDbModelBase.CreateOrUpdateFromLogs(logs, fromClientGuid);

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
                    case "fk_MpColorId":
                        //var color = await MpDb.Instance.GetObjDbRow("MpColor", li.AffectedColumnValue);
                        var color = await MpColor.GetColorByGuidAsync(li.AffectedColumnValue);
                        newTag.ColorId = (color as MpColor).Id;
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
                ColorId = Convert.ToInt32(objParts[2])
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
                ColorId, other.ColorId,
                "fk_MpColorId",
                diffLookup,
                Color.ColorGuid
                );

            return diffLookup;
        }
    }
}
