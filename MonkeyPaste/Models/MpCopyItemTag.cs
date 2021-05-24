using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace MonkeyPaste {
    public class MpCopyItemTag : MpDbObject {
        private static List<MpCopyItemTag> _AllCopyItemTagList = null;
        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpTag))]
        public int TagId { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        public int CopyItemId { get; set; }

        public static async Task<List<MpCopyItemTag>> GetAllCopyItemsForTagId(int tagId) {
            if(_AllCopyItemTagList == null) {
                await GetAllCopyItemsTags();
            }
            return _AllCopyItemTagList.Where(x => x.TagId == tagId).ToList();
        }

        public static async Task<List<MpCopyItemTag>> GetAllCopyItemsTags() {
            if(_AllCopyItemTagList != null) {
                return _AllCopyItemTagList;
            }
            _AllCopyItemTagList = await MpDb.Instance.GetItems<MpCopyItemTag>();
            return _AllCopyItemTagList;
        }

        #endregion
    }
}
