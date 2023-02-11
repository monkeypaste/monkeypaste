using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using Newtonsoft.Json;

namespace MonkeyPaste.Avalonia {
    public class MpAvQueryViewModel : 
        MpViewModelBase,
        MpIQueryResultProvider,
        MpIJsonObject,
        MpIQueryInfo
        {
        #region Private Variables

        [JsonIgnore]
        private bool _isRestoringValues = false;
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
            var result = JsonConvert.DeserializeObject<MpAvQueryViewModel>(lastQueryInfoStr);

            _instance = result;
             return result;
        }

        private static MpAvQueryViewModel _instance;
        public static MpAvQueryViewModel Instance => _instance ?? (_instance = new MpAvQueryViewModel());

        #endregion

        #region Properties     

        #region MpIQueryResultProvider Implementation

        [JsonIgnore]
        public bool CanRequery =>
            !MpAvSearchCriteriaItemCollectionViewModel.Instance.IsAdvSearchActive &&
            !_isRestoringValues &&
            MpAvClipTrayViewModel.Instance.QueryCommand.CanExecute(null);

        [JsonIgnore]
        public MpIDbIdCollection PageTools => _pageTools;

        [JsonIgnore]
        public int TotalAvailableItemsInQuery => _pageTools.AllQueryIds.Count;

        public async Task QueryForTotalCountAsync() {
            if (MpAvSearchCriteriaItemCollectionViewModel.Instance.IsAdvSearchActive) {
                return;
            }
            MpConsole.WriteLine("Simp total count called");
            var result = await MpContentQuery.QueryAllAsync(this);
            _pageTools.AllQueryIds.Clear();
            _pageTools.AllQueryIds.AddRange(result);
            //_pageTools.AllQueryIds = new ObservableCollection<int>(result);

            MpMessenger.SendGlobal(MpMessageType.TotalQueryCountChanged);
        }

        public async Task<List<MpCopyItem>> FetchItemsByQueryIdxListAsync(IEnumerable<int> copyItemQueryIdxList, IEnumerable<int> idsToOmit) {
            var fetchRootIds = _pageTools.AllQueryIds
                                .Select((val, idx) => (val, idx))
                                .Where(x => copyItemQueryIdxList.Contains(x.idx) && !idsToOmit.Contains(x.val))
                                .Select(x => x.val).ToList();
            var items = await MpDataModelProvider.GetCopyItemsByIdListAsync(fetchRootIds);
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
            if(!CanRequery) {
                return;
            }
            Dispatcher.UIThread.Post(() => {
                bool has_query_changed = RefreshQuery();

                if (has_query_changed || forceRequery) {
                    MpPrefViewModel.Instance.LastQueryInfoJson = SerializeJsonObject();
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
        public int TagId { get; set; } = MpTag.HelpTagId;
        public string MatchValue { get; set; } = string.Empty;

        public MpContentQueryBitFlags QueryFlags { get; set; } = MpContentQueryBitFlags.Content | MpContentQueryBitFlags.TextType | MpContentQueryBitFlags.ImageType | MpContentQueryBitFlags.FileType;

        public MpLogicalQueryType NextJoinType =>
            MpLogicalQueryType.Or;

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

        #region Events
        #endregion

        #region Constructors

        private MpAvQueryViewModel() {
            _pageTools = MpQueryPageTools.Instance;
            PropertyChanged += MpAvQueryInfoViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods
        #endregion

        #region Private Methods

        private void MpAvQueryInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (SupressPropertyChangedNotification) {
                MpConsole.WriteLine("Hey! QueryInfo still receives suppressed property changes");
                return;
            }
        }

        private bool RefreshQuery() {
            // set internal properties to current registered values from bound controls
            bool hasChanged = false;
            if(MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending  != IsDescending) { 
                hasChanged = true; 
            } else if(MpAvClipTileSortFieldViewModel.Instance.SelectedSortType  != SortType) { 
                hasChanged = true; 
            } else if((MpAvTagTrayViewModel.Instance.LastSelectedActiveItem == null && TagId > 0) ||
                (MpAvTagTrayViewModel.Instance.LastSelectedActiveItem != null && 
                MpAvTagTrayViewModel.Instance.LastSelectedActiveItem.TagId != TagId)) { 
                hasChanged = true; 
            } else if(MpAvSearchBoxViewModel.Instance.SearchText  != MatchValue) { 
                hasChanged = true; 
            } else if(MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.FilterType  != QueryFlags) { 
                hasChanged = true; 
            } 
            if(Next != null &&
                MpAvSearchCriteriaItemCollectionViewModel.Instance.HasAnyCriteriaChanged) {
                // NOTE always check criteria to unset change
                hasChanged = true;
                MpAvSearchCriteriaItemCollectionViewModel.Instance.HasAnyCriteriaChanged = false;
            }

            IsDescending = MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending;
            SortType = MpAvClipTileSortFieldViewModel.Instance.SelectedSortType;
            TagId = MpAvTagTrayViewModel.Instance.LastSelectedActiveItem == null ? 0 : 
                    MpAvTagTrayViewModel.Instance.LastSelectedActiveItem.TagId;
            MatchValue = MpAvSearchBoxViewModel.Instance.SearchText;
            QueryFlags = MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.FilterType;

            return hasChanged;
        }

        #endregion
    }
}
