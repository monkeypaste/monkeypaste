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
            var tags = await MpDb.Instance.GetItems<MpTag>();
            foreach(var tag in tags) {
                var citl = await MpCopyItemTag.GetAllCopyItemsForTagId(tag.Id);
                tag.CopyItemList = new List<MpCopyItem>();
                foreach(var cit in citl) {
                    var ci = await MpCopyItem.GetCopyItemById(cit.CopyItemId);
                    tag.CopyItemList.Add(ci);
                }
                tag.TagColor = await MpColor.GetColorById(tag.ColorId);
            }
            return tags;
        }

        public MpTag() { }

        //public MpTag(string tagName, string tagColor, int tagCount) {
        //    TagName = tagName;
        //    TagColor = tagColor;
        //    TagSortIdx = tagCount;
        //}
        
        public bool IsLinkedWithCopyItemAsync(MpCopyItem ci) {
            if(ci == null) {
                return false;
            }
            return CopyItemList.Contains(ci);
        }

        public async Task<bool> LinkWithCopyItemAsync(MpCopyItem ci) {
            if(ci == null) {
                return false;
            }
            //returns FALSE if copyitem is already linked to maintain correct counts
            bool isLinked = IsLinkedWithCopyItemAsync(ci);
            if (isLinked) {               
                return false;
            }

            await MpDb.Instance.AddItem<MpCopyItemTag>(new MpCopyItemTag() { CopyItemId = ci.Id, TagId = Id });

            Console.WriteLine("Tag link created between tag " + Id + " with copyitem " + ci.Id);
            return true;
        }
        public async Task UnlinkWithCopyItemAsync(MpCopyItem ci) {
            if(ci == null) {
                return;
            }
            bool isLinked = IsLinkedWithCopyItemAsync(ci);
            if (!isLinked) {
                //Console.WriteLine("MpTag Warning attempting to unlink non-linked tag " + TagId + " with copyitem " + ci.copyItemId + " ignoring...");
                return;
            }
            var result = await MpDb.Instance.QueryAsync<MpCopyItemTag>(@"select * from MpCopyItemTag where TagId=? and CopyItemId=?", Id, ci.Id);
            await MpDb.Instance.DeleteItem<MpCopyItemTag>(result[0]);
            //MpDb.Instance.ExecuteWrite("delete from MpTagCopyItemSortOrder where fk_MpTagId=" + this.TagId);
            Console.WriteLine("Tag link removed between tag " + Id + " with copyitem " + ci.Id + " ignoring...");
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
