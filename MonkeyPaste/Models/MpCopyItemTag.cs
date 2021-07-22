using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace MonkeyPaste {
    [Table("MpCopyItemTag")]
    public class MpCopyItemTag : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemTagId")]
        public override int Id { get; set; }

        [Column("MpCopyItemTagGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid CopyItemTagGuid {
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

        [ForeignKey(typeof(MpTag))]
        [Column("fk_MpTagId")]
        public int TagId { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        [Column("fk_MpCopyItemId")]
        public int CopyItemId { get; set; }

        public static async Task<List<MpCopyItemTag>> GetAllCopyItemsForTagId(int tagId) {
            var allCopyItemTagList = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();
            return allCopyItemTagList.Where(x => x.TagId == tagId).ToList();
        }

        public static async Task DeleteAllCopyItemTagsForCopyItemId(int CopyItemId) {
            var allCopyItemTagList = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();
            var citl = allCopyItemTagList.Where(x => x.CopyItemId == CopyItemId).ToList();
            foreach(var cit in citl) {
                await MpDb.Instance.DeleteItemAsync<MpCopyItemTag>(cit);
            }
        }

        public static async Task DeleteAllCopyItemTagsForTagId(int tagId) {
            var allCopyItemTagList = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();
            var citl = allCopyItemTagList.Where(x => x.TagId == tagId).ToList();
            foreach (var cit in citl) {
                await MpDb.Instance.DeleteItemAsync<MpCopyItemTag>(cit);
            }
        }

        #endregion

        public MpCopyItemTag() {
        }
    }
}
