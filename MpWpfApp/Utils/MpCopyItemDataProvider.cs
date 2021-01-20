using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace MpWpfApp {
    public class MpCopyItemDataProvider {
        private int _tagId = 0;

        public MpCopyItemDataProvider(int tagId) {
            SetTagId(tagId);
        }
        public void SetTagId(int tagId) {
            _tagId = tagId;
        }

        public async Task<int> GetCopyItemsByTagIdCountAsync() {
            var copyItemList = new List<MpCopyItem>();
            var dt = await MpDb.Instance.ExecuteAsync(
                "select pk_MpCopyItemId from MpCopyItem where pk_MpCopyItemId=(select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid)",
                new Dictionary<string, object> {
                        { "@tid", _tagId }
                    });
            if (dt != null && dt.Rows.Count > 0) {
                return dt.Rows.Count;
            }
            return 0;
        }

        public IAsyncOperation<List<MpCopyItem>> GetCopyItemsByTagIdAsync(uint startIndex, uint maxNumberOfItems) {
            var copyItemList = new List<MpCopyItem>();
            var dt = MpDb.Instance.ExecuteAsync(
                "select * from MpCopyItem where pk_MpCopyItemId=(select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid) order by pk_MpCopyItemId limit @mnoi offset @si",
                new Dictionary<string, object> {
                        { "@tid", _tagId },
                        { "@mnoi", maxNumberOfItems },
                        { "@si", startIndex }
                    }).Result;
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    copyItemList.Add(new MpCopyItem(dr));
                }
            }
            return (IAsyncOperation<List<MpCopyItem>>)copyItemList;
        }

        public async Task<List<MpCopyItem>> GetCopyItemsByTagIdAsync() {
            var copyItemList = new List<MpCopyItem>();
            var dt = await MpDb.Instance.ExecuteAsync(
                "select * from MpCopyItem where pk_MpCopyItemId=(select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid)",
                new Dictionary<string, object> {
                        { "@tid", _tagId }
                    });
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    copyItemList.Add(new MpCopyItem(dr));
                }
            }
            return copyItemList;
        }

        public event EventHandler CopyItemChanged;
        public virtual void OnCopyItemChanged(MpCopyItem ci) => CopyItemChanged?.Invoke(ci, EventArgs.Empty);

    }
}
