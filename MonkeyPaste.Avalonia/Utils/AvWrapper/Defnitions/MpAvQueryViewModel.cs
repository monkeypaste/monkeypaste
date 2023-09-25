using Avalonia.Threading;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvQueryViewModel :
        MpAvViewModelBase,
        MpIQueryResultProvider,
        MpIJsonObject,
        MpIQueryInfo {
        #region Private Variables
        [JsonIgnore]
        private int _requeryCount = 0;
        [JsonIgnore]
        private bool _isRestoringValues = false;
        [JsonIgnore]
        private bool _isRestoringAdvancedValues = false;
        [JsonIgnore]
        private MpQueryPageTools _pageTools;

        #endregion

        #region Constants

        const int OPTIMIZE_PER_REQUERY_COUNT = 5;

        #endregion
        #region Statics

        public static MpIQueryResultProvider Parse(string lastQueryInfoStr) {
            //if(!string.IsNullOrWhiteSpace(lastQueryInfoStr) && 
            //    !lastQueryInfoStr.StartsWith("[") && !lastQueryInfoStr.StartsWith("{")) {
            //    try {
            //        int queryTagId = int.Parse(lastQueryInfoStr);
            //        if(queryTagId != 0) {
            //            return new MpAvSearchCriteriaItemCollectionViewModel(queryTagId);
            //        }
            //    }
            //    catch {

            //    }
            //}
            if (string.IsNullOrWhiteSpace(lastQueryInfoStr)) {
                _instance = new MpAvQueryViewModel();
            } else {
                var result = JsonConvert.DeserializeObject<MpAvQueryViewModel>(lastQueryInfoStr);
                _instance = result;
            }
            return _instance;
        }

        private static MpAvQueryViewModel _instance;
        public static MpAvQueryViewModel Instance => _instance ?? (_instance = new MpAvQueryViewModel());

        #endregion

        #region Interfaces




        #region MpIQueryResultProvider Implementation
        [JsonIgnore]
        public bool IsQuerying { get; private set; }

        [JsonIgnore]
        public bool CanRequery =>
            //!MpAvSearchCriteriaItemCollectionViewModel.Instance.IsAdvSearchActive &&
            !_isRestoringValues &&
            !_isRestoringAdvancedValues &&
            !IsQuerying &&
            MpAvClipTrayViewModel.Instance.QueryCommand.CanExecute(null);

        [JsonIgnore]
        public MpIQueryPageTools PageTools => _pageTools;

        [JsonIgnore]
        public int TotalAvailableItemsInQuery => _pageTools.TotalCount;

        [JsonIgnore]
        public IEnumerable<MpIQueryInfo> Infos {
            get {
                yield return this;
                var next = Next;
                while (next != null) {
                    yield return next;
                    next = next.Next;
                }
            }
        }

        #region Repeat Query
        const bool STORE_QUERY_IDS = true;
        private IEnumerable<int> _allIds = new List<int>();
        public async Task<List<MpCopyItem>> QueryForModelsAsync() {
            int total_count = await MpContentQuery.QueryForTotalCountAsync(this, Mp.Services.ContentQueryTools.GetOmittedContentIds());
            _pageTools.SetTotalCount(total_count);
            _allIds = await MpContentQuery.FetchItemIdsAsync(this, 0, total_count, Mp.Services.ContentQueryTools.GetOmittedContentIds());

            var allItems = await MpDb.GetAsyncTable<MpCopyItem>().ToListAsync();
            var result = _allIds.Select(x => allItems.FirstOrDefault(y => y.Id == x));
            return result.ToList();

        }
        public async Task<List<MpCopyItem>> FetchPageAsync(int offset, int limit) {
            if (_allIds == null) {
                _allIds = await MpContentQuery.FetchItemIdsAsync(this, 0, _pageTools.TotalCount, Mp.Services.ContentQueryTools.GetOmittedContentIds());
            }
            var all = await
                MpDb.GetAsyncTable<MpCopyItem>()
                .ToListAsync();

            var result =
                all
                .Where(y =>
                    _allIds
                        .Skip(offset)
                        .Take(limit)
                        .Contains(y.Id)).ToList();
            return result;
        }
        #endregion

        public async Task QueryForTotalCountAsync(bool isRequery) {
            int total_count = await MpContentQuery.QueryForTotalCountAsync(this, Mp.Services.ContentQueryTools.GetOmittedContentIds());
            _pageTools.SetTotalCount(total_count);
            if (STORE_QUERY_IDS) {
                _allIds = await MpContentQuery.FetchItemIdsAsync(this, 0, total_count, Mp.Services.ContentQueryTools.GetOmittedContentIds());
            }


            if (isRequery) {
                _requeryCount++;
                if (_requeryCount % OPTIMIZE_PER_REQUERY_COUNT == 0) {
                    // this is to improve db perf.
                    // see https://www.sqlite.org/pragma.html#pragma_optimize
                    MpDb.PerformDbOptimizationAsync().FireAndForgetSafeAsync();
                }
            }

        }

        public async Task<List<int>> FetchPageIdsAsync(int offset, int limit) {
            if (STORE_QUERY_IDS) {
                return _allIds.Where(x => !Mp.Services.ContentQueryTools.GetOmittedContentIds().Contains(x)).Skip(offset).Take(limit).ToList();
            }
            var items = await MpContentQuery.FetchItemIdsAsync(this, offset, limit, Mp.Services.ContentQueryTools.GetOmittedContentIds());
            return items;
        }
        public async Task<int> FetchItemOffsetIdxAsync(int ciid) {
            if (STORE_QUERY_IDS) {
                return _allIds.Where(x => !Mp.Services.ContentQueryTools.GetOmittedContentIds().Contains(x)).IndexOf(ciid);
            }
            if (ciid <= 0) {
                return -1;
            }
            int offset_idx = await MpContentQuery.FetchItemOffsetAsync(this, ciid, Mp.Services.ContentQueryTools.GetOmittedContentIds());
            return offset_idx;
        }

        public void RestoreProviderValues() {
            _isRestoringValues = true;
            MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending = IsDescending;
            MpAvClipTileSortFieldViewModel.Instance.SelectedSortType = SortType;
            MpAvTagTrayViewModel.Instance.SelectTagCommand.Execute(TagId);
            MpAvSearchBoxViewModel.Instance.SearchText = MatchValue;
            MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.FilterType = QueryFlags;
            _isRestoringValues = false;
            NotifyQueryChanged(true);
        }


        public void NotifyQueryChanged(bool forceRequery = false) {
            if (!CanRequery) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                IsQuerying = true;
                bool has_query_changed = await RefreshQueryAsync();

                if (has_query_changed || forceRequery) {
                    //MpPrefViewModel.Instance.LastQueryInfoJson = SerializeJsonObject();
                    //MpConsole.WriteLine("Simp requery called");
                    //_pageTools.AllQueryIds.Clear();
                    MpMessenger.SendGlobal(MpMessageType.QueryChanged);
                } else {
                    MpMessenger.SendGlobal(MpMessageType.SubQueryChanged);

                }
            });
        }

        #endregion

        #region MpIQueryInfo Implementation


        public MpQueryType QueryType =>
            MpQueryType.Simple;
        public bool IsDescending { get; set; } = true;
        public MpContentSortType SortType { get; set; } = MpContentSortType.CopyDateTime;
        public int TagId { get; set; } = MpAvTagTrayViewModel.DEFAULT_SELECTED_TAG_ID;

        public MpContentQueryBitFlags QueryFlags { get; set; } = MpContentQueryBitFlags.Content | MpContentQueryBitFlags.TextType | MpContentQueryBitFlags.ImageType | MpContentQueryBitFlags.FileType;

        public MpLogicalQueryType JoinType =>
            MpLogicalQueryType.Or;

        [JsonIgnore] // must be ignored cause criteria opt trees
        public MpIQueryInfo Next =>
            MpAvSearchCriteriaItemCollectionViewModel.Instance.IsAdvSearchActive ?
                MpAvSearchCriteriaItemCollectionViewModel.Instance.HeadItem :
                null;

        public int SortOrderIdx { get; set; } = 0;

        #region MpITextMatchInfo Implementation

        public string MatchValue { get; set; } = string.Empty;
        bool MpITextMatchInfo.CaseSensitive =>
            (this as MpIQueryInfo).QueryFlags.HasFlag(MpContentQueryBitFlags.CaseSensitive);

        bool MpITextMatchInfo.WholeWord =>
            (this as MpIQueryInfo).QueryFlags.HasFlag(MpContentQueryBitFlags.WholeWord);
        bool MpITextMatchInfo.UseRegex =>
            (this as MpIQueryInfo).QueryFlags.HasFlag(MpContentQueryBitFlags.Regex);
        #endregion

        #endregion

        #region MpIJsonObject Implementation

        public string SerializeJsonObject() {
            return MpJsonConverter.SerializeObject(this);
        }

        #endregion

        #endregion

        #region Properties     

        #region State


        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors

        private MpAvQueryViewModel() {
            _pageTools = new MpQueryPageTools();
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }


        #endregion

        #region Public Methods

        public async Task RestoreAdvSearchValuesAsync(MpSearchCriteriaItem simp_ci) {
            if (simp_ci == null) {
                return;
            }
            var query_tag = await MpDataModelProvider.GetItemAsync<MpTag>(simp_ci.QueryTagId);
            if (query_tag == null) {
                // probably shouldn't happen
                MpDebug.Break();
                return;
            }
            _isRestoringAdvancedValues = true;
            IsDescending = query_tag.IsSortDescending.IsTrue();
            SortType = query_tag.SortType.Value;
            TagId = simp_ci.QueryTagId;
            MatchValue = simp_ci.MatchValue;

            try {
                long qf = long.Parse(simp_ci.Options);
                QueryFlags = (MpContentQueryBitFlags)qf;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error converting simple search opts to flags. Opts '{simp_ci.Options}'. Setting to default.", ex);
                QueryFlags = MpSearchCriteriaItem.DefaultSimpleFilters;
            }
            RestoreProviderValues();
            _isRestoringAdvancedValues = false;
            // NOTE requery called from adv init

        }
        #endregion

        #region Private Methods


        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.QueryCompleted:
                    //if (!IsQuerying) {
                    //    MpDebug.Break("Query state mismatch");
                    //}
                    IsQuerying = false;
                    break;
            }
        }

        private async Task<bool> RefreshQueryAsync() {
            // set internal properties to current registered values from bound controls
            bool hasChanged = false;
            if (MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending != IsDescending) {
                hasChanged = true;
            } else if (MpAvClipTileSortFieldViewModel.Instance.SelectedSortType != SortType) {
                hasChanged = true;
            } else if (MpAvTagTrayViewModel.Instance.LastActiveId != TagId) {
                hasChanged = true;
            } else if (MpAvSearchBoxViewModel.Instance.SearchText != MatchValue) {
                hasChanged = true;
            } else if (MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.FilterType != QueryFlags) {
                hasChanged = true;
            }
            if (Next != null &&
                MpAvSearchCriteriaItemCollectionViewModel.Instance.HasAnyCriteriaModelChanged) {
                // NOTE always check criteria to unset change
                hasChanged = true;
                //MpAvSearchCriteriaItemCollectionViewModel.Instance.Items.ForEach(x => x.HasModelChanged = false);
            }

            IsDescending = MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending;
            SortType = MpAvClipTileSortFieldViewModel.Instance.SelectedSortType;
            TagId = MpAvTagTrayViewModel.Instance.LastActiveId;
            MatchValue = MpAvSearchBoxViewModel.Instance.SearchText;
            QueryFlags = MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.FilterType;

            MpAvSearchCriteriaItemCollectionViewModel.Instance.SelectAdvancedSearchCommand.Execute(TagId);
            while (MpAvSearchCriteriaItemCollectionViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }


            return hasChanged;
        }


        #endregion
    }
}
