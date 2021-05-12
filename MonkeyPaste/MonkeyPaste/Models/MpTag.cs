using System;
using System.Collections.Generic;
using System.Data;
using SQLite;
using SQLiteNetExtensions;
using System.Threading.Tasks;
using System.Linq;

namespace MonkeyPaste {
    [Table(nameof(MpTag))]
    public class MpTag : MpDbObject {
        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column(nameof(TagSortIdx))]
        public int TagSortIdx { get; set; } = 1;

        [Column(nameof(TagColor))]
        public string TagColor { get; set; }

        [Column(nameof(TagName))]
        public string TagName { get; set; } = "Untitled";

        //unused        
        //public int ParentTagId { get; set; }
        #endregion

        public MpTag() { }

        public MpTag(string tagName, string tagColor, int tagCount) {
            TagName = tagName;
            TagColor = tagColor;
            TagSortIdx = tagCount;
        }
        
        public async Task<bool> IsLinkedWithCopyItemAsync(MpCopyItem ci) {
            if(ci == null) {
                return false;
            }

            var result = await MpDb.Instance.ExecuteAsync<MpCopyItemTag>(
                "select * from MpCopyItemTag where TagId=@tid and CopyItemId=@ciid",
                new Dictionary<string, object> {
                    { "@tid", Id },
                    { "@ciid", ci.Id }
                });

            return result != null && result.Count > 0;
        }
        public async Task<bool> LinkWithCopyItemAsync(MpCopyItem ci) {
            if(ci == null) {
                return false;
            }
            //returns FALSE if copyitem is already linked to maintain correct counts
            bool isLinked = await IsLinkedWithCopyItemAsync(ci);
            if (isLinked) {               
                return false;
            }

            var result = await MpDb.Instance.ExecuteAsync<MpCopyItemTag>(
                "select * from MpCopyItemTag where TagId=@tid",
                new Dictionary<string, object> {
                    { "@tid", Id }
                });

            int SortOrderIdx =result.Count + 1;
            await MpDb.Instance.ExecuteWriteAsync<MpCopyItemTag>(
                "insert into MpCopyItemTag(CopyItemId,TagId) values(@ciid,@tid)",
                new Dictionary<string, object> {
                    { "@ciid", ci.Id },
                    { "@tid", Id }
                });

            Console.WriteLine("Tag link created between tag " + Id + " with copyitem " + ci.Id);
            return true;
        }
        public async Task UnlinkWithCopyItemAsync(MpCopyItem ci) {
            if(ci == null) {
                return;
            }
            bool isLinked = await IsLinkedWithCopyItemAsync(ci);
            if (!isLinked) {
                //Console.WriteLine("MpTag Warning attempting to unlink non-linked tag " + TagId + " with copyitem " + ci.copyItemId + " ignoring...");
                return;
            }
            await MpDb.Instance.ExecuteWriteAsync<MpCopyItemTag>(
                "delete from MpCopyItemTag where CopyItemId=@ciid and TagId=@tid",
                new Dictionary<string, object> {
                    { "@ciid", ci.Id },
                    { "@tid", Id }
                });
            //MpDb.Instance.ExecuteWrite("delete from MpTagCopyItemSortOrder where fk_MpTagId=" + this.TagId);
            Console.WriteLine("Tag link removed between tag " + Id + " with copyitem " + ci.Id + " ignoring...");
        }
        public async Task DeleteFromDatabaseAsync() {
            await MpDb.Instance.ExecuteWriteAsync<MpTag>(
                "delete from MpTag where Id=@tid",
                new Dictionary<string, object> {
                    { "@tid", Id }
                });

            await MpDb.Instance.ExecuteWriteAsync<MpCopyItemTag>(
                "delete from MpCopyItemTag where TagId=@tid",
                new Dictionary<string, object> {
                    { "@tid", Id }
                });
        }
    }
}
