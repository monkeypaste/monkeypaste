using System;
using System.Collections.Generic;
using System.Data;
using SQLite;
using SQLiteNetExtensions;
using System.Threading.Tasks;
using System.Linq;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpTag : MpDbObject {
        private static List<MpTag> _AllTags = null;

        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        public int TagSortIdx { get; set; } = 1;

        [ForeignKey(typeof(MpColor))]
        public int ColorId { get; set; }
        [ManyToOne]
        public MpColor TagColor { get; set; }

        public string TagName { get; set; } = "Untitled";

        [ManyToMany(typeof(MpCopyItem))]
        public List<MpCopyItem> CopyItemList { get; set; }

        //unused        
        //public int ParentTagId { get; set; }
        #endregion
        
        public static async Task<List<MpTag>> GetAllTags() {
            if(_AllTags == null) {
                _AllTags = await MpDb.Instance.GetItems<MpTag>();
                RecentTag = _AllTags.Where(x => x.Id == 1).FirstOrDefault();
                AllTag = _AllTags.Where(x => x.Id == 2).FirstOrDefault();
                FavoritesTag = _AllTags.Where(x => x.Id == 3).FirstOrDefault();
                HelpTag = _AllTags.Where(x => x.Id == 4).FirstOrDefault();
            }
            return _AllTags;            
        }

        public static MpTag RecentTag { get; set; }
        public static MpTag AllTag { get; set; }
        public static MpTag FavoritesTag { get; set; }
        public static MpTag HelpTag { get; set; }

        public MpTag() : base(typeof(MpTag)) { }

        //public MpTag(string tagName, string tagColor, int tagCount) {
        //    TagName = tagName;
        //    TagColor = tagColor;
        //    TagSortIdx = tagCount;
        //}
        
        public async Task<bool> IsLinkedWithCopyItemAsync(int cid) {
            if(cid <= 0) {
                return false;
            }
            var citl = await MpCopyItemTag.GetAllCopyItemsForTagId(Id);
            return citl.Any(x => x.CopyItemId == cid);
        }

        public async Task<bool> LinkWithCopyItemAsync(int cid) {
            if(cid <= 0) {
                return false;
            }
            //returns FALSE if copyitem is already linked to maintain correct counts
            bool isLinked = await IsLinkedWithCopyItemAsync(cid);
            if (isLinked) {               
                return false;
            }

            await MpDb.Instance.AddItem<MpCopyItemTag>(new MpCopyItemTag() { CopyItemId = cid, TagId = Id });

            Console.WriteLine("Tag link created between tag " + Id + " with copyitem " + cid);
            return true;
        }
        public async Task UnlinkWithCopyItemAsync(int cid) {
            if(cid <= 0) {
                return;
            }
            bool isLinked = await IsLinkedWithCopyItemAsync(cid);
            if (!isLinked) {
                //Console.WriteLine("MpTag Warning attempting to unlink non-linked tag " + TagId + " with copyitem " + ci.copyItemId + " ignoring...");
                return;
            }
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTag>(@"select * from MpCopyItemTag where TagId=? and CopyItemId=?", Id, cid);
            await MpDb.Instance.DeleteItem<MpCopyItemTag>(result[0]);
            //MpDb.Instance.ExecuteWrite("delete from MpTagCopyItemSortOrder where fk_MpTagId=" + this.TagId);
            Console.WriteLine("Tag link removed between tag " + Id + " with copyitem " + cid + " ignoring...");
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
    }
}
