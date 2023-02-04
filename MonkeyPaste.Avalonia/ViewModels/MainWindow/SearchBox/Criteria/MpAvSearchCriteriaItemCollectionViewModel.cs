using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchCriteriaItemCollectionViewModel : 
        MpViewModelBase,
        MpIExpandableViewModel,
        MpIQueryResultProvider {

        #region Private Variable

        private MpIQueryResultProvider _simpleSearchRef;
        private MpQueryPageTools _pageTools;
        #endregion

        #region Constants
        #endregion

        #region Statics

        private static MpAvSearchCriteriaItemCollectionViewModel _instance;
        public static MpAvSearchCriteriaItemCollectionViewModel Instance => _instance ?? (_instance = new MpAvSearchCriteriaItemCollectionViewModel());

        #endregion

        #region Interfaces

        #region MpIExpandableViewModel Implementation

        private bool _isExpanded;
        public bool IsExpanded { 
            get => _isExpanded;
            set {
                if(IsExpanded != value) {
                    SetIsExpandedAsync(value).FireAndForgetSafeAsync(this);
                }
            }
        }

        #endregion

        #region MpIQueryResultProvider Implementation

        #region MpIJsonObject Implementation

        public string SerializeJsonObject() {
            if(PendingQueryTagId > 0) {
                return (PendingQueryTagId * -1).ToString();
            }
            if(IsSavedQuery) {
                return CurrentQueryTagId.ToString();
            }
            return "0";
        }

        #endregion

        public int TotalAvailableItemsInQuery => _pageTools.AllQueryIds.Count;
        public MpIDbIdCollection PageTools => _pageTools;
        public IEnumerable<MpIQueryInfoValueProvider> ValueProviders => null;
        public void RestoreProviderValues() {
            // NOTE only called in mw init when last load shutdown was adv search
            if (QueryTagId == 0) {
                // shouldn't be called
                Debugger.Break();
                return;
            }
            InitializeAsync(QueryTagId, IsPendingQuery).FireAndForgetSafeAsync(this);
        }

        public void RegisterProvider(MpIQueryInfoValueProvider provider) {
            // NOTE only called in mw init when last load shutdown was adv search
            if(MpAvBootstrapperViewModel.IsPlatformLoaded) {
                // shouldn't happen
                Debugger.Break();
            }
            if(_simpleSearchRef == null) {
                // since simple query won't exist, create for providers to register
                _simpleSearchRef = new MpAvQueryInfoViewModel();
            }
            _simpleSearchRef.RegisterProvider(provider);
        }

        public async Task<List<MpCopyItem>> FetchIdsByQueryIdxListAsync(List<int> copyItemQueryIdxList) {
            var fetchRootIds = _pageTools.AllQueryIds
                                .Select((val, idx) => (val, idx))
                                .Where(x => copyItemQueryIdxList.Contains(x.idx))
                                .Select(x => x.val).ToList();
            var items = await MpDataModelProvider.GetCopyItemsByIdListAsync(fetchRootIds);
            return items;
        }

        public async Task QueryForTotalCountAsync() {
            // NOTE only called on head info

            var result = await MpContentQuery.QueryAllAsync(HeadItem, true);
            _pageTools.AllQueryIds.Clear();
            _pageTools.AllQueryIds.AddRange(result);
        }

        public void NotifyQueryChanged(bool forceRequery = false) {
            if(IsBusy) {
                return;
            }
            var ttrvm = MpAvTagTrayViewModel.Instance;
            if (!IsExpanded && ttrvm.SelectedItem != null &&
                !ttrvm.SelectedItem.IsQueryTag) {
                // treat expanded like is active, so if not active and tag is not query
                // hand back to simple
                InitializeAsync(0, false).FireAndForgetSafeAsync(this);
                return;
            }
            Dispatcher.UIThread.Post(() => {
                // NOTE unlike query vm this treats forceRequery as
                // required since value providers are internal i dunno

                if (forceRequery) {
                    //if(ttrvm.SelectedItemId != SelectedSearchTagId &&
                    //    ttrvm.SelectedItem.IsLinkTag) {
                    //    // ensure tag tree/tray selection updates searchtag when possible
                    //    if(ttrvm.SelectedItemId != AllTagId &&
                    //        ttrvm.SelectedItemId != AvailableNotAllTagId) {
                    //        // shouldn't available tag already be updated?
                    //        Debugger.Break();
                    //    } else {
                    //        SelectedSearchTagId = ttrvm.SelectedItemId;
                    //    }
                    //}
                    _pageTools.AllQueryIds.Clear();
                    MpMessenger.SendGlobal(MpMessageType.QueryChanged);
                } else {
                    MpMessenger.SendGlobal(MpMessageType.SubQueryChanged);
                }

            });
        }

        #endregion

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvSearchCriteriaItemViewModel> Items { get; set; } = new ObservableCollection<MpAvSearchCriteriaItemViewModel>();

        public IEnumerable<MpAvSearchCriteriaItemViewModel> SortedItems =>
            Items.OrderBy(x => x.SortOrderIdx);

        public MpAvSearchCriteriaItemViewModel HeadItem =>
            SortedItems.FirstOrDefault();
        public MpAvSearchCriteriaItemViewModel SelectedItem { get; set; }


        public MpAvTagTileViewModel AvailableNotAllTagViewModel =>
            AvailableNotAllTagId == 0 ? null :
            MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == AvailableNotAllTagId);

        #endregion

        #region State
        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsAnyBusy);
        public bool HasCriteriaItems => Items.Count > 0;

        public bool IsAdvSearchActive =>
            MpPlatform.Services.Query == this;

        public bool IsSavedQuery => CurrentQueryTagId > 0;

        public bool IsPendingQuery => PendingQueryTagId > 0;

        public bool IsAllSelectedSearchTagId =>
            SelectedSearchTagId == AllTagId;
        public int AllTagId =>
            MpTag.AllTagId;
        public int AvailableNotAllTagId {
            get {
                var sttvm = MpAvTagTrayViewModel.Instance.LastSelectedLinkItem;
                if(sttvm == null ||
                    sttvm.IsAllTag ||
                    !sttvm.IsLinkTag) {
                    return 0;
                }
                return sttvm.TagId;
            }
        }
        public int SelectedSearchTagId { get; set; }
        #endregion

        #region Layout

        public double BoundHeaderHeight { get; set; }

        public double BoundCriteriaListBoxScreenHeight { get; set; }

        public double MaxSearchCriteriaListBoxHeight =>
            IsAdvSearchActive ?
                // HEADER + BORDER
                BoundHeaderHeight +
                MpAvSearchCriteriaItemViewModel.CRITERIA_ITEM_BORDER_THICKNESS.Top +
                MpAvSearchCriteriaItemViewModel.CRITERIA_ITEM_BORDER_THICKNESS.Bottom +
                
                // ITEMS W/WO JOIN HEADER + BORDER
                Items.Sum(x => x.CriteriaItemHeight) +
                Items.Sum(x=>
                    MpAvSearchCriteriaItemViewModel.CRITERIA_ITEM_BORDER_THICKNESS.Top + 
                    MpAvSearchCriteriaItemViewModel.CRITERIA_ITEM_BORDER_THICKNESS.Bottom) :
                0;

        #endregion

        #region Model

        #region Global Enumerables

        public IEnumerable<MpUserDevice> UserDevices { get; private set; }
        #endregion

        public int PendingQueryTagId { get; private set; }
        public int CurrentQueryTagId { get; private set;}

        public int QueryTagId {
            get {
                if(IsPendingQuery) {
                    if(IsSavedQuery) {
                        // should only be 1
                        Debugger.Break();
                    }
                    return PendingQueryTagId;
                }
                return CurrentQueryTagId;
            }
        }

        #endregion

        #endregion

        #region Constructors
        public MpAvSearchCriteriaItemCollectionViewModel(int startupQueryTagId) : this() {
            // NOTE only called BEFORE bootstrap in MpAvQueryInfoViewModel.Parse when json is an int (QueryTagId)
            _instance = this;

            if (startupQueryTagId == 0) {
                return;
            }
            if(startupQueryTagId < 0) {
                // shutdown was a pending query
                PendingQueryTagId = -startupQueryTagId;
            } else {
                // shutdown was a saved query
                CurrentQueryTagId = startupQueryTagId;
            }
        }

        public MpAvSearchCriteriaItemCollectionViewModel() : base(null) {
            _pageTools = new MpQueryPageTools();
            PropertyChanged += MpAvSearchCriteriaItemCollectionViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

        }
        #endregion

        #region Public Methods

        public async Task InitializeAsync(int tagId, bool isPending) {
            IsBusy = true;

            if (UserDevices == null) {
                UserDevices = await MpDataModelProvider.GetItemsAsync<MpUserDevice>();
            }

            CurrentQueryTagId = isPending ? 0 : tagId;
            PendingQueryTagId = isPending ? tagId : 0;

            Items.Clear();
            if (QueryTagId > 0) {
                var cil = await MpDataModelProvider.GetCriteriaItemsByTagId(QueryTagId);
                foreach (var ci in cil) {
                    var civm = await CreateCriteriaItemViewModel(ci);
                    Items.Add(civm);
                }
            }

            while(Items.Any(x=>x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            if (HasCriteriaItems) {
                if (_simpleSearchRef == null &&
                    MpPlatform.Services.Query != this) {
                    _simpleSearchRef = MpPlatform.Services.Query;
                }
                MpPlatform.Services.Query = this;
            } else {
                IsExpanded = false;
                if (_simpleSearchRef != null) {
                    MpPlatform.Services.Query = _simpleSearchRef;
                    _simpleSearchRef = null;
                }
            }            

            IsBusy = false;

            while (MpAvClipTrayViewModel.Instance.IsAnyBusy) {
                // wait for tile layouts to update or something that prevents requery
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(AvailableNotAllTagViewModel));
            MpPlatform.Services.Query.NotifyQueryChanged(true);
        }

        public async Task<int> ConvertCurrentSearchToAdvSearchAsync(bool selectConversion) {
            // NOTE select is only when from search plus button
            if(selectConversion) {
                SelectedSearchTagId = AvailableNotAllTagId > 0 ? AvailableNotAllTagId : AllTagId;
            }
            

            bool is_pending = true;
            int converted_query_tag_id;
            if (QueryTagId > 0) {
                // NOTE since this should only be the case from tag add, its assumed 
                // save follows this cmd in add tag

                is_pending = false;

                var cur_query_tag = await MpDataModelProvider.GetItemAsync<MpTag>(QueryTagId);
                bool is_cur_pending = PendingQueryTagId == QueryTagId;

                // clone current adv search
                var clone_of_cur_query_tag = await cur_query_tag.CloneDbModelAsync();

                converted_query_tag_id = clone_of_cur_query_tag.Id;
                if (is_cur_pending) {
                    // if current is pending delete it
                    cur_query_tag.DeleteFromDatabaseAsync().FireAndForgetSafeAsync(this);
                }
            } else {
                // clone current simple search
                converted_query_tag_id = await ConvertCurrentSimpleSearchAsync();
            }
            if(selectConversion) {
                // convert result to criteria rows
                await InitializeAsync(converted_query_tag_id, is_pending);
            }

            return converted_query_tag_id;
        }

        #endregion

        #region Protected Methods

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpTag t && t.Id == QueryTagId) {
                InitializeAsync(0, false).FireAndForgetSafeAsync();
            }
        }

        #endregion

        #region Private Methods

        private async Task<MpAvSearchCriteriaItemViewModel> CreateCriteriaItemViewModel(MpSearchCriteriaItem sci) {
            MpAvSearchCriteriaItemViewModel nscivm = new MpAvSearchCriteriaItemViewModel(this);
            await nscivm.InitializeAsync(sci);
            return nscivm;
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.AdvancedSearchExpanded:
                    double default_visible_row_count = 2.5d;
                    double row_height = MpAvSearchCriteriaItemViewModel.DEFAULT_HEIGHT;
                    double delta_open_height = Math.Min((double)Items.Count, default_visible_row_count) * row_height;

                    BoundHeaderHeight = MpAvSearchCriteriaItemViewModel.DEFAULT_HEIGHT;
                    BoundCriteriaListBoxScreenHeight = delta_open_height;
                    //MpAvResizeExtension.ResizeByDelta(MpAvMainWindow.Instance, 0, delta_open_height,false);

                    if(QueryTagId > 0 && !HasCriteriaItems) {
                        InitializeAsync(QueryTagId, IsPendingQuery).FireAndForgetSafeAsync(this);
                    }
                    break;
                case MpMessageType.AdvancedSearchUnexpanded:
                    double delta_close_height = -BoundCriteriaListBoxScreenHeight;
                    BoundCriteriaListBoxScreenHeight = 0;
                    //MpAvResizeExtension.ResizeByDelta(MpAvMainWindow.Instance, 0, delta_close_height, false);                    
                    break;
                case MpMessageType.RequeryCompleted:
                case MpMessageType.TagSelectionChanged:
                    OnPropertyChanged(nameof(IsSavedQuery));
                    OnPropertyChanged(nameof(AvailableNotAllTagId));
                    OnPropertyChanged(nameof(AvailableNotAllTagViewModel));
                    break;

            }
        }
        private void MpAvSearchCriteriaItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedItem):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    break;
                case nameof(IsExpanded):                    
                    if (IsExpanded) {
                        MpMessenger.SendGlobal(MpMessageType.AdvancedSearchExpanded);
                    } else {
                        MpMessenger.SendGlobal(MpMessageType.AdvancedSearchUnexpanded);
                    }
                    break;
                case nameof(IgnoreHasModelChanged):
                    Items.ForEach(x => x.IgnoreHasModelChanged = IgnoreHasModelChanged);
                    Items.ForEach(x => x.OnPropertyChanged(nameof(HasModelChanged)));
                    break;
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SearchCriteriaItemsChanged);
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SortedItems));
            OnPropertyChanged(nameof(HasCriteriaItems));
            OnPropertyChanged(nameof(MaxSearchCriteriaListBoxHeight));

            UpdateCriteriaSortOrderAsync().FireAndForgetSafeAsync(this);
        }

        private async Task SetIsExpandedAsync(bool newExpandedValue) {
            if (newExpandedValue) {
                if (!IsPendingQuery && !IsSavedQuery) {
                    // plus on search box toggled to checked
                    await ConvertCurrentSearchToAdvSearchAsync(true);
                }
                _isExpanded = true;
            } else {
                _isExpanded = false;
            }
            OnPropertyChanged(nameof(IsExpanded));
        }

        private async Task UpdateCriteriaSortOrderAsync(bool fromModel = false) {
            if (fromModel) {
                Items.Sort(x => x.SortOrderIdx);
            } else {
                Items.ToList().ForEach((x, idx) => x.SortOrderIdx = idx);
                while(Items.ToList().Any(x=>x.IsBusy)) {
                    await Task.Delay(100);
                }
            }
        }       

        private async Task<int> ConvertCurrentSimpleSearchAsync() {
            if(QueryTagId > 0) {
                // not a simple search, check call stack
                Debugger.Break();
                return 0;
            }
            var pending_tag = await MpTag.CreateAsync(
                            tagType: MpTagType.Query,
                            sortType: MpAvClipTileSortFieldViewModel.Instance.SelectedSortType,
                            isSortDescending: MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending);

            // NOTE this is called at end of provider create so sortIdx is seed for these

            string st = MpAvSearchBoxViewModel.Instance.SearchText;

            var all_filters = MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.FilterType;
            List<MpContentQueryBitFlags> true_filters =
                MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.Filters
                .Where(x => !x.IsSeperator && x.IsChecked.IsTrue())
                .Select(x => x.FilterType)
                .ToList();

            List<MpSearchCriteriaItem> items = new List<MpSearchCriteriaItem>();
            foreach (var ctf in true_filters) {
                string opt_path =
                    GetSimpleSearchOptionString(
                        ctf,
                        all_filters,
                        st);

                // NOTE (I think) all simple search filters are OR based..
                var sci = await MpSearchCriteriaItem.CreateAsync(
                    tagId: pending_tag.Id,
                    nextJoinType: MpLogicalQueryType.Or,
                    sortOrderIdx: items.Count,
                    options: opt_path);
                items.Add(sci);
            }

            MpAvSearchBoxViewModel.Instance.ClearTextCommand.Execute("don't notify, chill");
            await Task.Delay(50);
            while (IsAnyBusy) {
                await Task.Delay(100);
            }
            return pending_tag.Id;
        }
        private string GetSimpleSearchOptionString(MpContentQueryBitFlags current_flag, MpContentQueryBitFlags all_flags, string searchText) {
            // format:
            // <opt1>,<opt2>,...

            // opt formats:
            // non-leaf opt:
            // <Enum Module>|<Enum Name>|<MpContentFilterFlag Name>
            // leaf opt:
            // <Enum Module>|<Enum Name>|<base64 search text>|<is case sensitive>|<MpContentFilterFlag Name>

            searchText = searchText == null ? string.Empty : searchText;
            List<object> opts = new List<object>();

            switch (current_flag) {
                // Root.Content
                case MpContentQueryBitFlags.Title:
                    opts.Add(MpRootOptionType.Content);
                    opts.Add(MpContentOptionType.Title);
                    opts.Add(MpTextOptionType.Contains);
                    opts.Add(new object[] { 
                        MpContentQueryBitFlags.Title, 
                        searchText, 
                        all_flags.HasFlag(MpContentQueryBitFlags.CaseSensitive) });
                    break;
                case MpContentQueryBitFlags.Content:
                    opts.Add(MpRootOptionType.Content);
                    opts.Add(MpContentOptionType.AnyText);
                    opts.Add(MpTextOptionType.Contains);
                    opts.Add(new object[] { 
                        MpContentQueryBitFlags.Content, 
                        searchText, 
                        all_flags.HasFlag(MpContentQueryBitFlags.CaseSensitive) });
                    break;
                // Root.ContentType
                case MpContentQueryBitFlags.TextType:
                    opts.Add(MpRootOptionType.ContentType);
                    opts.Add(MpContentTypeOptionType.Text);
                    break;
                case MpContentQueryBitFlags.ImageType:
                    opts.Add(MpRootOptionType.ContentType);
                    opts.Add(MpContentTypeOptionType.Image);
                    break;
                case MpContentQueryBitFlags.FileType:
                    opts.Add(MpRootOptionType.ContentType);
                    opts.Add(MpContentTypeOptionType.Files);
                    break;
                default:
                    return string.Empty;
            }
            return opts.ToOptionPathString(current_flag);
        }
        
        #endregion

        #region Commands


        public ICommand AddSearchCriteriaItemCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsBusy = true;

                int add_idx = Items.Count;
                if (args is MpAvSearchCriteriaItemViewModel scivm) {
                    add_idx = scivm.SortOrderIdx + 1;
                }
                MpSearchCriteriaItem nsci = new MpSearchCriteriaItem() {
                    SortOrderIdx = add_idx
                };
                MpAvSearchCriteriaItemViewModel nscivm = await CreateCriteriaItemViewModel(nsci);
                Items.Add(nscivm);
                OnPropertyChanged(nameof(SortedItems));
                OnPropertyChanged(nameof(HasCriteriaItems));
                while(Items.Any(x=>x.IsAnyBusy)) {
                    await Task.Delay(100);
                }
                // manually write since was busy during init
                await nscivm.SearchCriteriaItem.WriteToDatabaseAsync();

                IsBusy = false;
            });

        public ICommand RemoveSearchCriteriaItemCommand => new MpCommand<object>(
            async (args) => {
                IsBusy = true;
                var scivm = args as MpAvSearchCriteriaItemViewModel;
                int scivmIdx = Items.IndexOf(scivm);
                Items.RemoveAt(scivmIdx);
                if (scivm.SearchCriteriaItem.Id > 0) {
                    await scivm.SearchCriteriaItem.DeleteFromDatabaseAsync();
                }
                await UpdateCriteriaSortOrderAsync();

                while (Items.Any(x => x.IsAnyBusy)) {
                    await Task.Delay(100);
                }
                IsBusy = false;
                NotifyQueryChanged(true);
            },
            (args) => args is MpAvSearchCriteriaItemViewModel);

        public ICommand SavePendingQueryCommand => new MpAsyncCommand(
            async() => {
                // NOTE this should only occur for new searches, onced created saving is by HasModelChanged
                var ttrvm = MpAvTagTrayViewModel.Instance;
                int waitTimeMs = MpAvSidebarItemCollectionViewModel.Instance.SelectedItem == ttrvm ? 0 : 500;
                MpAvSidebarItemCollectionViewModel.Instance.SelectSidebarItemCommand.Execute(ttrvm);
                // wait for panel open
                await Task.Delay(waitTimeMs);

                ttrvm.RootGroupTagViewModel.AddNewChildTagCommand.Execute(PendingQueryTagId);

                // clear pending flag 
                CurrentQueryTagId = PendingQueryTagId;
                PendingQueryTagId = 0;
                OnPropertyChanged(nameof(QueryTagId));
            }, () => IsPendingQuery,this, new[] { this });

        public MpIAsyncCommand RejectPendingCriteriaItemsCommand => new MpAsyncCommand(
            async () => {
                var t = await MpDataModelProvider.GetItemAsync<MpTag>(PendingQueryTagId);
                t.DeleteFromDatabaseAsync().FireAndForgetSafeAsync();
                PendingQueryTagId = 0;
                await InitializeAsync(0, false);
            }, () => IsPendingQuery, this, new[] { this });

        public ICommand SaveQueryCommand => new MpCommand(
             () => {
                 if(IsPendingQuery) {
                     SavePendingQueryCommand.Execute(null);
                 } else {
                     IgnoreHasModelChanged = false;
                 }
             },()=>HasCriteriaItems, this, new[] { this });
        
        public ICommand CancelPendingOrCollapseSavedQueryCommand => new MpAsyncCommand(
             async() => {
                 // NOTE only hooked to adv header minus button
                 if(IsPendingQuery) {
                     await RejectPendingCriteriaItemsCommand.ExecuteAsync();
                 }
                 IsExpanded = false;
             },()=>HasCriteriaItems, this, new[] { this });

        public ICommand SelectSearchTagCommand => new MpCommand<object>(
            (args) => {
                // NOTE only content-linked tags can be search tag
                SelectedSearchTagId = (int)args;
                NotifyQueryChanged(true);
            }, (args) => {
                if(MpPlatform.Services.Query != this) {
                    // shouldn't happend (only called from adv header)
                    Debugger.Break();
                    return false;
                }
                if(args is int tagId) {
                    var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == tagId);
                    if(ttvm == null || ttvm.TagType != MpTagType.Link) {
                        return false;
                    }
                    
                    return true;
                }
                return false;
            });

        public ICommand SelectAdvancedSearchCommand => new MpCommand<object>(
            (args) => {
                // NOTE only called from tag tray when query tag is selected
                // instead of query change ntf
                int queryTagId = 0;
                if(args is int tagId) {
                    queryTagId = tagId;
                }
                IsExpanded = false;
                
                // NOTE since query takes have no linked content
                // but are the selected tag treat search as from
                // all until selected tag is changed
                SelectedSearchTagId = AllTagId;
                InitializeAsync(queryTagId, false).FireAndForgetSafeAsync(this);
            });


        #endregion
    }
}
