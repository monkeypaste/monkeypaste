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
        [ManyToOne]
        public MpColor TagColor { get; set; }

        public string TagName { get; set; } = "Untitled";

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

        public async Task<object> DeserializeDbObject(string objStr, string parseToken = @"^(@!@") {
            var objParts = objStr.Split(new string[] { parseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            var dbLog = new MpTag() {
                TagGuid = System.Guid.Parse(objParts[0]),
                TagName = objParts[1],
                TagSortIdx = Convert.ToInt32(objParts[2]),
                ColorId = Convert.ToInt32(objParts[3])
            };
            return dbLog;
        }

        public string SerializeDbObject(string parseToken = @"^(@!@") {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}",
                parseToken,
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
