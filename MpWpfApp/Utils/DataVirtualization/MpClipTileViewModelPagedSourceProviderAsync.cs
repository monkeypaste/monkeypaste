using AlphaChiTech.Virtualization;
using DataGridAsyncDemoMVVM.filtersort;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace MpWpfApp {
    public class MpClipTileViewModelPagedSourceProviderAsync : IPagedSourceProviderAsync<MpClipTileViewModel>, IFilteredSortedSourceProviderAsync {
        #region Private Variables
        private int _tagId = 0;
        private string _sortType = string.Empty;
        private bool _isDescending = true;
        private readonly MpClipTileViewModelDataSource _data;
        #endregion

        #region Properties
        public FilterDescriptionList FilterDescriptionList => _data.FilterDescriptionList;

        public SortDescriptionList SortDescriptionList => _data.SortDescriptionList;

        public bool IsSynchronized { get; }

        public object SyncRoot { get; }
        #endregion

        #region Public Methods
        public MpClipTileViewModelPagedSourceProviderAsync(MpClipTileViewModelDataSource data) {
            _data = data;
            SetTag(1);
            SetSort("CopyDateTime", false);
        }

        public void SetTag(int tagId) {
            _tagId = tagId;
        }

        public void SetSort(string sortType, bool isDescending) {
            _sortType = sortType;
            _isDescending = isDescending;
        }
        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion

        #region IPagedSourceProvider Implementation
        public void OnReset(int count) {
        }
        //public int IndexOf(MpClipTileViewModel item) {
        //    var dt = MpDb.Instance.Execute(
        //       "select ROW_NUMBER() OVER(ORDER BY pk_MpCopyItemId) AS Idx from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid and fk_MpCopyItemId=@ciid) order by @st",
        //        new Dictionary<string, object> {
        //            { "@tid", _tagId },
        //            { @"ciid", item.CopyItemId },
        //            { "@st", _sortType + (_isDescending ? "DESC":"ASC")},
        //            //{ "@sd", _isDescending ? "DESC":"ASC" }
        //            });
        //    if (dt != null && dt.Rows.Count > 0) {
        //        return Convert.ToInt32(dt.Rows[0]["Idx"].ToString());
        //    }
        //    return -1;
        //}

        int IPagedSourceProvider<MpClipTileViewModel>.IndexOf(MpClipTileViewModel item) {
            return _data.FilteredOrderedItems.IndexOf(item);
        }

        public bool Contains(MpClipTileViewModel item) {
            return _data.Contains(item);
        }

        //public PagedSourceItemsPacket<MpClipTileViewModel> GetItemsAt(int pageoffset, int count, bool usePlaceholder) {
        //    var clipTileViewModelList = new List<MpClipTileViewModel>();
        //    var dt = MpDb.Instance.Execute(
        //        "select * from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid) order by @st limit @mnoi offset @si",
        //        new Dictionary<string, object> {
        //            { "@tid", _tagId },
        //            { "@mnoi", count },
        //            { "@si", pageoffset },
        //            { "@st", _sortType + (_isDescending ? "DESC":"ASC")},
        //            //{ "@sd", _isDescending ? "DESC":"ASC" }
        //            });
        //    if (dt != null && dt.Rows.Count > 0) {
        //        foreach (DataRow dr in dt.Rows) {
        //            clipTileViewModelList.Add(new MpClipTileViewModel(new MpCopyItem(dr)));
        //        }
        //    }
        //    return new PagedSourceItemsPacket<MpClipTileViewModel>() {
        //        LoadedAt = DateTime.Now,
        //        Items = clipTileViewModelList
        //    };
        //}

        public PagedSourceItemsPacket<MpClipTileViewModel> GetItemsAt(int pageoffset, int count, bool usePlaceholder) {
            return new PagedSourceItemsPacket<MpClipTileViewModel> {
                LoadedAt = DateTime.Now,
                Items = (from items in _data.FilteredOrderedItems select items).Skip(pageoffset).Take(count)
            };
        }

        //public int Count {
        //    get {
        //        var dt = MpDb.Instance.Execute(
        //        "select pk_MpCopyItemId from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid)",
        //        new Dictionary<string, object> {
        //                { "@tid", _tagId }
        //            });
        //        if (dt != null && dt.Rows.Count > 0) {
        //            return dt.Rows.Count;
        //        }
        //        return 0;
        //    }
        //}

        public int Count {
            get {
                return _data.FilteredOrderedItems.Count;
                ;
            }
        }
        #endregion

        #region IPagedSourceProviderAsync Implementation
        public Task<bool> ContainsAsync(MpClipTileViewModel item) {
            throw new NotImplementedException();
        }

        //public async Task<int> GetCountAsync() {
        //    var dt = await MpDb.Instance.ExecuteAsync(
        //        "select pk_MpCopyItemId from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid)",
        //        new Dictionary<string, object> {
        //                { "@tid", _tagId }
        //            });
        //    if (dt != null && dt.Rows.Count > 0) {
        //        return dt.Rows.Count;
        //    }
        //    return 0;
        //}

        public Task<int> GetCountAsync() {
            return Task.Run(() => {
                return _data.FilteredOrderedItems.Count;
            });
        }

        //public async Task<PagedSourceItemsPacket<MpClipTileViewModel>> GetItemsAtAsync(int pageoffset, int count, bool usePlaceholder) {
        //    var clipTileViewModelList = new List<MpClipTileViewModel>();
        //    var dt = await MpDb.Instance.ExecuteAsync(
        //        "select * from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid) order by @st limit @mnoi offset @si",
        //        new Dictionary<string, object> {
        //            { "@tid", _tagId },
        //            { "@mnoi", count },
        //            { "@si", pageoffset },
        //            { "@st", _sortType + (_isDescending ? "DESC":"ASC")},
        //            //{ "@sd", _isDescending ? "DESC":"ASC" }
        //            });
        //    if (dt != null && dt.Rows.Count > 0) {
        //        foreach (DataRow dr in dt.Rows) {
        //            clipTileViewModelList.Add(new MpClipTileViewModel(new MpCopyItem(dr)));
        //        }
        //    }
        //    return new PagedSourceItemsPacket<MpClipTileViewModel>() {
        //        LoadedAt = DateTime.Now,
        //        Items = clipTileViewModelList
        //    };
        //}

        public Task<PagedSourceItemsPacket<MpClipTileViewModel>> GetItemsAtAsync(int pageoffset, int count,bool usePlaceholder) {
            //Console.WriteLine("Get");
            return Task.Run(() => {
                return new PagedSourceItemsPacket<MpClipTileViewModel> {
                    LoadedAt = DateTime.Now,
                    Items = (from items in _data.FilteredOrderedItems select items).Skip(pageoffset)
                        .Take(count)
                };
            });
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
            return new MpClipTileViewModel();
        }

        //public async Task<int> IndexOfAsync(MpClipTileViewModel item) {
        //    var dt = await MpDb.Instance.ExecuteAsync(
        //        "select ROW_NUMBER() OVER(ORDER BY pk_MpCopyItemId) AS Idx from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid and fk_MpCopyItemId=@ciid) order by @st",
        //        new Dictionary<string, object> {
        //            { "@tid", _tagId },
        //            { @"ciid", item.CopyItemId },
        //            { "@st", _sortType + (_isDescending ? "DESC":"ASC")},
        //            //{ "@sd", _isDescending ? "DESC":"ASC" }
        //            });
        //    if (dt != null && dt.Rows.Count > 0) {
        //        return Convert.ToInt32(dt.Rows[0]["Idx"].ToString());
        //    }
        //    return -1;
        //}

        /// <summary>
        ///     This returns the index of a specific item. This method is optional – you can just return -1 if you
        ///     don’t need to use IndexOf. It’s not strictly required if don’t need to be able to seeking to a
        ///     specific item, but if you are selecting items implementing this method is recommended.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Task<int> IndexOfAsync(MpClipTileViewModel item) {
            return Task.Run(() => _data.FilteredOrderedItems.IndexOf(item));
        }
        #endregion
    }
}
