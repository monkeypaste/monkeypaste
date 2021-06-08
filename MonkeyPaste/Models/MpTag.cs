using System;
using System.Collections.Generic;
using System.Data;
using SQLite;
using SQLiteNetExtensions;
using System.Threading.Tasks;
using System.Linq;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpTag : MpDbModelBase {
        public const int RecentTagId = 1;
        public const int AllTagId = 2;
        public const int FavoritesTagId = 3;
        public const int HelpTagId = 4;

        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = -1;

        public int TagSortIdx { get; set; } = 1;

        [ForeignKey(typeof(MpColor))]
        public int ColorId { get; set; }
        [ManyToOne]
        public MpColor TagColor { get; set; }

        public string TagName { get; set; } = "Untitled";

        [ManyToMany(typeof(MpClip))]
        public List<MpClip> ClipList { get; set; }

        //unused        
        //public int ParentTagId { get; set; }
        #endregion

        public MpTag() : base(typeof(MpTag)) { }

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
            var result = await MpDb.Instance.QueryAsync<MpClipTag>(@"select * from MpClipTag where TagId=? and ClipId=?", Id, clip.Id);
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
    }
}
