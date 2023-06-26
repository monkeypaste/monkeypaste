using Avalonia.Threading;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvQueryViewModel :
        MpViewModelBase,
        MpIQueryResultProvider,
        MpIJsonObject,
        MpIQueryInfo {
        #region Private Variables

        [JsonIgnore]
        private bool _isRestoringValues = false;
        [JsonIgnore]
        private bool _isRestoringAdvancedValues = false;
        [JsonIgnore]
        private MpQueryPageTools _pageTools;

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

        public async Task QueryForTotalCountAsync(bool isRequery) {
            MpConsole.WriteLine("total count called");
            //int total_count = await MpContentQuery.QueryForTotalCountAsync(this, Mp.Services.ContentQueryTools.GetOmittedContentIds());
            int total_count = await MpContentQuery.QueryForTotalCountAsync(this, null);
            _pageTools.Reset(isRequery);
            _pageTools.SetTotalCount(total_count);

        }

        public async Task<List<MpCopyItem>> FetchPageAsync(int offset, int limit) {
            //var items = await MpContentQuery.FetchItemsAsync(this, offset, limit, Mp.Services.ContentQueryTools.GetOmittedContentIds());
            var items = await MpContentQuery.FetchItemsAsync(this, offset, limit, null);

            return items;
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
                    MpConsole.WriteLine("Simp requery called");
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
        public string MatchValue { get; set; } = string.Empty;

        public MpContentQueryBitFlags QueryFlags { get; set; } = MpContentQueryBitFlags.Content | MpContentQueryBitFlags.TextType | MpContentQueryBitFlags.ImageType | MpContentQueryBitFlags.FileType;

        public MpLogicalQueryType JoinType =>
            MpLogicalQueryType.Or;

        [JsonIgnore] // must be ignored cause criteria opt trees
        public MpIQueryInfo Next =>
            MpAvSearchCriteriaItemCollectionViewModel.Instance.IsAdvSearchActive ?
                MpAvSearchCriteriaItemCollectionViewModel.Instance.HeadItem :
                null;

        public int SortOrderIdx { get; set; } = 0;

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
            PropertyChanged += MpAvQueryInfoViewModel_PropertyChanged;
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
                Debugger.Break();
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

        private void MpAvQueryInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (SupressPropertyChangedNotification) {
                MpConsole.WriteLine("Hey! QueryInfo still receives suppressed property changes");
                return;
            }
        }

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
