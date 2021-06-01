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

        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

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

        //public MpTag(string tagName, string tagColor, int tagCount) {
        //    TagName = tagName;
        //    TagColor = tagColor;
        //    TagSortIdx = tagCount;
        //}
        
        public async Task<bool> IsLinkedWithClipAsync(int cid) {
            if(cid <= 0) {
                return false;
            }
            var citl = await MpClipTag.GetAllClipsForTagId(Id);
            return citl.Any(x => x.ClipId == cid);
        }

        public async Task<bool> LinkWithClipAsync(int cid) {
            if(cid <= 0) {
                return false;
            }
            //returns FALSE if Clip is already linked to maintain correct counts
            bool isLinked = await IsLinkedWithClipAsync(cid);
            if (isLinked) {               
                return false;
            }

            await MpDb.Instance.AddItem<MpClipTag>(new MpClipTag() { ClipId = cid, TagId = Id });

            Console.WriteLine("Tag link created between tag " + Id + " with Clip " + cid);
            return true;
        }
        public async Task UnlinkWithClipAsync(int cid) {
            if(cid <= 0) {
                return;
            }
            bool isLinked = await IsLinkedWithClipAsync(cid);
            if (!isLinked) {
                //Console.WriteLine("MpTag Warning attempting to unlink non-linked tag " + TagId + " with Clip " + ci.ClipId + " ignoring...");
                return;
            }
            var result = await MpDb.Instance.QueryAsync<MpClipTag>(@"select * from MpClipTag where TagId=? and ClipId=?", Id, cid);
            await MpDb.Instance.DeleteItem<MpClipTag>(result[0]);
            //MpDb.Instance.ExecuteWrite("delete from MpTagClipSortOrder where fk_MpTagId=" + this.TagId);
            Console.WriteLine("Tag link removed between tag " + Id + " with Clip " + cid + " ignoring...");
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
