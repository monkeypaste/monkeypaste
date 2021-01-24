using AlphaChiTech.Virtualization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace MpWpfApp {
    public class MpClipTileViewModelPagedSourceProviderAsync : IPagedSourceProviderAsync<MpClipTileViewModel> {
        private int _tagId = 0;
        private string _sortType = string.Empty;
        private bool _isDescending = true;

        public MpClipTileViewModelPagedSourceProviderAsync() {
            SetTag(1);
            SetSort("CopyDateTime", false);
        }

        public void SetTag(int tagId) {
            _tagId = tagId;
        }

        public void SetSort(string sortType,bool isDescending) {
            _sortType = sortType;
            _isDescending = isDescending;
        }

        public void OnReset(int count) {
        }

        public PagedSourceItemsPacket<MpClipTileViewModel> GetItemsAt(int pageoffset, int count, bool usePlaceholder) {
            var clipTileViewModelList = new List<MpClipTileViewModel>();
            var dt = MpDb.Instance.Execute(
                "select * from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid) order by @st limit @mnoi offset @si",
                new Dictionary<string, object> {
                    { "@tid", _tagId },
                    { "@mnoi", count },
                    { "@si", pageoffset },
                    { "@st", _sortType + (_isDescending ? "DESC":"ASC")},
                    //{ "@sd", _isDescending ? "DESC":"ASC" }
                    });
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    clipTileViewModelList.Add(new MpClipTileViewModel(new MpCopyItem(dr)));
                }
            }
            return new PagedSourceItemsPacket<MpClipTileViewModel>() {
                LoadedAt = DateTime.Now,
                Items = clipTileViewModelList
            };
        }

        public async Task<PagedSourceItemsPacket<MpClipTileViewModel>> GetItemsAtAsync(int pageoffset, int count, bool usePlaceholder) {
            var clipTileViewModelList = new List<MpClipTileViewModel>();
            var dt = await MpDb.Instance.ExecuteAsync(
                "select * from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid) order by @st limit @mnoi offset @si",
                new Dictionary<string, object> {
                    { "@tid", _tagId },
                    { "@mnoi", count },
                    { "@si", pageoffset },
                    { "@st", _sortType + (_isDescending ? "DESC":"ASC")},
                    //{ "@sd", _isDescending ? "DESC":"ASC" }
                    });
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    clipTileViewModelList.Add(new MpClipTileViewModel(new MpCopyItem(dr)));
                }
            }
            return new PagedSourceItemsPacket<MpClipTileViewModel>() {
                LoadedAt = DateTime.Now,
                Items = clipTileViewModelList
            };
        }

        public MpClipTileViewModel GetPlaceHolder(int index, int page, int offset) {
            //var dt = MpDb.Instance.Execute(
            //    "select *, ROW_NUMBER() OVER(ORDER BY pk_MpCopyItemId) AS Idx from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid) and Idx=@index order by @st",
            //    new Dictionary<string, object> {
            //        { "@tid", _tagId },
            //        { "@st", _sortType + (_isDescending ? "DESC":"ASC")},
            //        { "@index", index }
            //        //{ "@sd", _isDescending ? "DESC":"ASC" }
            //        });
            //if (dt != null && dt.Rows.Count > 0) {
            //    return new MpClipTileViewModel(new MpCopyItem(dt.Rows[0]));
            //}
            //return null;
            return new MpClipTileViewModel(new MpCopyItem());
        }

        public int Count {
            get {
                var dt = MpDb.Instance.Execute(
                "select pk_MpCopyItemId from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid)",
                new Dictionary<string, object> {
                        { "@tid", _tagId }
                    });
                if (dt != null && dt.Rows.Count > 0) {
                    return dt.Rows.Count;
                }
                return 0;
            }
        }

        public async Task<int> GetCountAsync() {
            var dt = await MpDb.Instance.ExecuteAsync(
                "select pk_MpCopyItemId from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid)",
                new Dictionary<string, object> {
                        { "@tid", _tagId }
                    });
            if (dt != null && dt.Rows.Count > 0) {
                return dt.Rows.Count;
            }
            return 0;
        }

        public int IndexOf(MpClipTileViewModel item) {
            var dt = MpDb.Instance.Execute(
               "select ROW_NUMBER() OVER(ORDER BY pk_MpCopyItemId) AS Idx from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid and fk_MpCopyItemId=@ciid) order by @st",
                new Dictionary<string, object> {
                    { "@tid", _tagId },
                    { @"ciid", item.CopyItemId },
                    { "@st", _sortType + (_isDescending ? "DESC":"ASC")},
                    //{ "@sd", _isDescending ? "DESC":"ASC" }
                    });
            if (dt != null && dt.Rows.Count > 0) {
                return Convert.ToInt32(dt.Rows[0]["Idx"].ToString());
            }
            return -1;
        }

        public async Task<int> IndexOfAsync(MpClipTileViewModel item) {
            var dt = await MpDb.Instance.ExecuteAsync(
                "select ROW_NUMBER() OVER(ORDER BY pk_MpCopyItemId) AS Idx from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid and fk_MpCopyItemId=@ciid) order by @st",
                new Dictionary<string, object> {
                    { "@tid", _tagId },
                    { @"ciid", item.CopyItemId },
                    { "@st", _sortType + (_isDescending ? "DESC":"ASC")},
                    //{ "@sd", _isDescending ? "DESC":"ASC" }
                    });
            if (dt != null && dt.Rows.Count > 0) {
                return Convert.ToInt32(dt.Rows[0]["Idx"].ToString());
            }
            return -1;
        }
    }
}
