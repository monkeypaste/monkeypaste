using Avalonia.Controls;
using Avalonia.Threading;
using Gtk;
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
        //MpIQueryResultProvider, 
        MpIExpandableViewModel
        {

        #region Private Variable

        private bool _isRestoringValues = false;
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

        //#region MpIQueryResultProvider Implementation

        //#region MpIJsonObject Implementation

        //public string SerializeJsonObject() {
        //    if(PendingQueryTagId > 0) {
        //        return (PendingQueryTagId * -1).ToString();
        //    }
        //    if(IsSavedQuery) {
        //        return CurrentQueryTagId.ToString();
        //    }
        //    return "0";
        //}

        //#endregion

        //public bool CanRequery =>
        //    IsAdvSearchActive &&
        //    !_isRestoringValues &&
        //    MpAvClipTrayViewModel.Instance.QueryCommand.CanExecute(null);

        //public int TotalAvailableItemsInQuery => _pageTools.AllQueryIds.Count;
        //public MpIDbIdCollection PageTools => _pageTools;

        //public void RestoreProviderValues() {
        //    // NOTE only called in mw init when last load shutdown was adv search
        //    //InitializeAsync(QueryTagId, IsPendingQuery).FireAndForgetSafeAsync(this);
        //    if (MpPlatform.Services.Query != this) {
        //        // mistake
        //        Debugger.Break();
        //        return;
        //    }
        //    var qttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == QueryTagId);
        //    if (qttvm == null) {
        //        // mistake
        //        Debugger.Break();
        //        return;
        //    }
        //    _isRestoringValues = true;
            
        //    MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending = qttvm.IsSortDescending;
        //    MpAvClipTileSortFieldViewModel.Instance.SelectedSortType = qttvm.SortType;
        //    MpAvSearchBoxViewModel.Instance.SearchText = string.Empty;
        //    MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.FilterType = 
        //        MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.DefaultFilters;
        //    _isRestoringValues = false;

        //    if(MpAvTagTrayViewModel.Instance.LastActiveId == QueryTagId) {
        //        NotifyQueryChanged(true);
        //    } else {
        //        MpAvTagTrayViewModel.Instance.SelectTagCommand.Execute(QueryTagId);
        //    }
        //}

        //public async Task<List<MpCopyItem>> FetchItemsByQueryIdxListAsync(IEnumerable<int> copyItemQueryIdxList, IEnumerable<int> idsToOmit) {
        //    var fetchRootIds = _pageTools.AllQueryIds
        //                        .Select((val, idx) => (val, idx))
        //                        .Where(x => copyItemQueryIdxList.Contains(x.idx) && !idsToOmit.Contains(x.val))
        //                        .Select(x => x.val).ToList();
        //    var items = await MpDataModelProvider.GetCopyItemsByIdListAsync(fetchRootIds);
        //    return items;
        //}

        //public async Task QueryForTotalCountAsync() {

        //    MpConsole.WriteLine("Adv total count called");
        //    // NOTE only called on head info

        //    var result = await MpContentQuery.QueryAllAsync(HeadItem);
        //    _pageTools.AllQueryIds.Clear();
        //    _pageTools.AllQueryIds.AddRange(result);
        //    //_pageTools.AllQueryIds = new ObservableCollection<int>(result);

        //    MpMessenger.SendGlobal(MpMessageType.TotalQueryCountChanged);
        //}

        //public void NotifyQueryChanged(bool forceRequery = false) {
        //    //if(IsBusy) {
        //    //    return;
        //    //}
        //    var ttrvm = MpAvTagTrayViewModel.Instance;
        //    if (!IsExpanded && 
        //        !IsConvertingQueryToSelect &&
        //        ttrvm.SelectedItem != null &&
        //        !ttrvm.SelectedItem.IsQueryTag) {
        //        // treat expanded like is active, so if not active and tag is not query
        //        // hand back to simple
        //        InitializeAsync(0, false).FireAndForgetSafeAsync(this);
        //        return;
        //    }

        //    if (!CanRequery) {
        //        return;
        //    }
        //    Dispatcher.UIThread.Post(() => {
        //        // NOTE unlike query vm this treats forceRequery as
        //        // required since value providers are internal i dunno

        //        if (forceRequery) {
        //            MpConsole.WriteLine("Adv requery called");
        //            //_pageTools.AllQueryIds.Clear();
        //            //MpMessenger.SendGlobal(MpMessageType.TotalQueryCountChanged);
        //            MpMessenger.SendGlobal(MpMessageType.QueryChanged);
        //        } else {
        //            MpMessenger.SendGlobal(MpMessageType.SubQueryChanged);
        //        }

        //    });
        //}

        //#endregion

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvSearchCriteriaItemViewModel> Items { get; set; } = new ObservableCollection<MpAvSearchCriteriaItemViewModel>();

        public IEnumerable<MpAvSearchCriteriaItemViewModel> SortedItems =>
            Items.OrderBy(x => x.SortOrderIdx);

        public MpAvSearchCriteriaItemViewModel HeadItem =>
            SortedItems.FirstOrDefault();
        public MpAvSearchCriteriaItemViewModel SelectedItem { get; set; }

        #endregion

        #region State

        public bool HasAnyCriteriaChanged {
            get => Items.Any(x => x.HasCriteriaChanged);
            set => Items.ForEach(x => x.HasCriteriaChanged = value);
        }
        public bool IsConvertingQueryToSelect { get; set; }
        public bool IsAnyBusy => 
            IsBusy || Items.Any(x => x.IsAnyBusy);
        public bool HasCriteriaItems => 
            Items.Count > 0;

        public bool IsAdvSearchActive =>
            QueryTagId > 0;

        public bool IsSavedQuery => 
            CurrentQueryTagId > 0;

        public bool IsPendingQuery => 
            PendingQueryTagId > 0;

        #endregion

        #region Layout

        public double BoundHeaderHeight { get; set; }

        public double BoundCriteriaListBoxScreenHeight { get; set; }

        public double MaxSearchCriteriaListBoxHeight {
            get {
                if(!IsAdvSearchActive) {
                    return 0;
                }
                double h = 0;
                if(IsPendingQuery) {
                    // HEADER + BORDER
                    h +=
                        BoundHeaderHeight +
                            MpAvSearchCriteriaItemViewModel.CRITERIA_ITEM_BORDER_THICKNESS.Top +
                            MpAvSearchCriteriaItemViewModel.CRITERIA_ITEM_BORDER_THICKNESS.Bottom;
                }
                // ITEMS W/WO JOIN HEADER + BORDER
                h += Items.Sum(x => x.CriteriaItemHeight) +
                Items.Sum(x =>
                    MpAvSearchCriteriaItemViewModel.CRITERIA_ITEM_BORDER_THICKNESS.Top +
                    MpAvSearchCriteriaItemViewModel.CRITERIA_ITEM_BORDER_THICKNESS.Bottom);
                return h;
            }
        }

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
            _pageTools = MpQueryPageTools.Instance;
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
                    var civm = await CreateCriteriaItemViewModelAsync(ci);
                    Items.Add(civm);
                }
            } 
            
            if(!HasCriteriaItems) {
                // create empty criteria item
                var empty_civm = await CreateCriteriaItemViewModelAsync(null);
                Items.Add(empty_civm);
            }

            while(Items.Any(x=>x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            //if (HasCriteriaItems) {
            //    if (_simpleSearchRef == null &&
            //        MpPlatform.Services.Query != this) {
            //        _simpleSearchRef = MpPlatform.Services.Query;
            //    }
            //    MpPlatform.Services.Query = this;
            //    if(QueryTagId > 0 && !isPending) {
            //        // restore will either:
            //        // 1. select the query tag (which will re-initialize this and call restore again)
            //        // 2. call notify query change if query tag is already selected
            //        IsBusy = false;
            //        RestoreProviderValues();
            //        return;
            //    }
            //} else {
            //    IsExpanded = false;
            //    if (_simpleSearchRef != null) {
            //        MpPlatform.Services.Query = _simpleSearchRef;
            //        _simpleSearchRef = null;
            //    }
            //}            
            

            IsBusy = false;

            //if (QueryTagId > 0 && !isPending) {
            //    // restore will either:
            //    // 1. select the query tag (which will re-initialize this and call restore again)
            //    // 2. call notify query change if query tag is already selected
            //    RestoreProviderValues();
            //} else {
            //    MpPlatform.Services.Query.NotifyQueryChanged(true);
            //}
            MpPlatform.Services.Query.NotifyQueryChanged(true);
        }


        public async Task<int> ConvertCurrentSearchToAdvSearchAsync(bool selectConversion) {
            // NOTE select is only when from search plus button
            if(selectConversion) {
                IsConvertingQueryToSelect = true;
                //SelectedSearchTagId = AvailableNotAllTagId > 0 ? AvailableNotAllTagId : AllTagId;
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
            IsConvertingQueryToSelect = false;

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

        private async Task<MpAvSearchCriteriaItemViewModel> CreateCriteriaItemViewModelAsync(MpSearchCriteriaItem sci) {
            MpAvSearchCriteriaItemViewModel nscivm = new MpAvSearchCriteriaItemViewModel(this);
            await nscivm.InitializeAsync(sci);
            return nscivm;
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.AdvancedSearchExpanded:
                    Dispatcher.UIThread.Post(async () => {
                        double default_visible_row_count = 2.5d;
                        double row_height = MpAvSearchCriteriaItemViewModel.DEFAULT_HEIGHT;
                        double delta_open_height = Math.Min((double)Items.Count, default_visible_row_count) * row_height;

                        BoundHeaderHeight = MpAvSearchCriteriaItemViewModel.DEFAULT_HEIGHT;
                        BoundCriteriaListBoxScreenHeight = delta_open_height;
                        //MpAvResizeExtension.ResizeByDelta(MpAvSearchCriteriaListBoxView.Instance, 0, delta_open_height, false);

                        if (QueryTagId > 0 && !HasCriteriaItems) {
                            InitializeAsync(QueryTagId, IsPendingQuery).FireAndForgetSafeAsync(this);
                        }
                    });
                    break;
                case MpMessageType.AdvancedSearchUnexpanded:
                    double delta_close_height = -BoundCriteriaListBoxScreenHeight;
                    BoundCriteriaListBoxScreenHeight = 0;
                    //MpAvResizeExtension.ResizeByDelta(MpAvMainWindow.Instance, 0, delta_close_height, false);                    
                    break;
                case MpMessageType.RequeryCompleted:
                    if(IsAdvSearchActive) {
                        //MpAvClipTrayViewModel.Instance.Items.ForEach(x => x.UpdateQueryOffset());
                        // BUG on requery w/ adv query offset isn't updating
                        // i think its like the ref in clip tray to total count from
                        // the interface or somehow not reevaluating query info 
                        // but sub query fixes the offsets...
                        //Dispatcher.UIThread.Post(async () => {
                        //    while (MpAvClipTrayViewModel.Instance.IsAnyBusy) {
                        //        // wait for tile layouts to update or something that prevents requery
                        //        await Task.Delay(100);
                        //    }
                        //    MpAvClipTrayViewModel.Instance.QueryCommand.Execute(MpAvClipTrayViewModel.Instance.ScrollOffset);
                        //});
                    }
                    break;
                case MpMessageType.TagSelectionChanged:
                    OnPropertyChanged(nameof(IsSavedQuery));
                    //OnPropertyChanged(nameof(AvailableNotAllTagId));
                    //OnPropertyChanged(nameof(AvailableNotAllTagViewModel));
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
                    //await ConvertCurrentSearchToAdvSearchAsync(true);
                    await InitializeAsync(0, true);
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

            if(!MpAvTagTrayViewModel.Instance.LastSelectedActiveItem.IsLinkTag) {
                // simple conversion shouldn't of been called
                Debugger.Break();
            }
            int tagId = MpAvTagTrayViewModel.Instance.LastSelectedActiveItem.TagId;
            var opt_value_tuples = GetSimpleCriteriaOptions(all_filters, st, tagId);
            List<MpSearchCriteriaItem> items = new List<MpSearchCriteriaItem>();
            foreach (var opt_val_tuple in opt_value_tuples) {

                var sci = await MpSearchCriteriaItem.CreateAsync(
                    tagId: pending_tag.Id,
                    nextJoinType: MpLogicalQueryType.Or,
                    sortOrderIdx: items.Count,
                    options: opt_val_tuple.Item1,
                    matchValue: opt_val_tuple.Item2,
                    isCaseSensitive: all_filters.HasFlag(MpContentQueryBitFlags.CaseSensitive),
                    isWholeWord: all_filters.HasFlag(MpContentQueryBitFlags.WholeWord));
                items.Add(sci);
            }

            MpAvSearchBoxViewModel.Instance.ClearTextCommand.Execute("don't notify, chill");
            await Task.Delay(50);
            while (IsAnyBusy) {
                await Task.Delay(100);
            }
            return pending_tag.Id;
        }
        private IEnumerable<Tuple<string,string>> GetSimpleCriteriaOptions(MpContentQueryBitFlags sqf, string st, int tagId) {
            // mv: <case sensitive>,<whole word>,<regex>,<base64 search text>
            List<Tuple<string, string>> opts = new List<Tuple<string, string>>();
            if(tagId != MpTag.AllTagId) {
                // NOTE since db value is being stored in option text field
                // must reference it by guid or the reference will
                // break if search criteria is sync'd from another device
                var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == tagId);
                if (ttvm == null) {
                    // should have found, is tagId a pending query id?
                    Debugger.Break();
                } else {
                    opts.Add(new Tuple<string,string>($"{(int)MpRootOptionType.Collection}", ttvm.Tag.Guid));
                }
            }

            if (!sqf.HasFlag(MpContentQueryBitFlags.TextType) ||
               !sqf.HasFlag(MpContentQueryBitFlags.ImageType) ||
               !sqf.HasFlag(MpContentQueryBitFlags.FileType)) {
                // NOTE if all types are specified, ignore type flags
                if (sqf.HasFlag(MpContentQueryBitFlags.TextType)) {
                    opts.Add(new Tuple<string,string>($"{(int)MpRootOptionType.ContentType}",$"{(int)MpContentTypeOptionType.Text}"));
                }
                if (sqf.HasFlag(MpContentQueryBitFlags.ImageType)) {
                    opts.Add(new Tuple<string,string>($"{(int)MpRootOptionType.ContentType}",$"{(int)MpContentTypeOptionType.Image}"));
                }
                if (sqf.HasFlag(MpContentQueryBitFlags.FileType)) {
                    opts.Add(new Tuple<string,string>($"{(int)MpRootOptionType.ContentType}",$"{(int)MpContentTypeOptionType.Files}"));
                }
            }

            MpTextOptionType tot = sqf.HasFlag(MpContentQueryBitFlags.Regex) ? MpTextOptionType.RegEx : MpTextOptionType.Contains;
            if(sqf.HasFlag(MpContentQueryBitFlags.Title)) {
                opts.Add(new Tuple<string,string>($"{(int)MpRootOptionType.Content},{(int)MpContentOptionType.Title},{(int)tot}",st));
            }
            if(sqf.HasFlag(MpContentQueryBitFlags.Content)) {
                opts.Add(new Tuple<string,string>($"{(int)MpRootOptionType.Content},{(int)MpContentOptionType.AnyText},{(int)tot}",st));
            }
            if (sqf.HasFlag(MpContentQueryBitFlags.Annotations)) {
                opts.Add(new Tuple<string,string>($"{(int)MpRootOptionType.Content},{(int)MpContentOptionType.Annotation},{(int)tot}",st));
            }
            if (sqf.HasFlag(MpContentQueryBitFlags.Url)) {
                opts.Add(new Tuple<string,string>($"{(int)MpRootOptionType.Source},{(int)MpSourceOptionType.Website},{(int)MpWebsiteOptionType.Url},{(int)tot}",st));
            }
            if(sqf.HasFlag(MpContentQueryBitFlags.UrlTitle)) {
                opts.Add(new Tuple<string,string>($"{(int)MpRootOptionType.Source},{(int)MpSourceOptionType.Website},{(int)MpWebsiteOptionType.Title},{(int)tot}",st));
            }
            if(sqf.HasFlag(MpContentQueryBitFlags.AppPath)) {
                opts.Add(new Tuple<string,string>($"{(int)MpRootOptionType.Source},{(int)MpSourceOptionType.App},{(int)MpAppOptionType.ProcessPath},{(int)tot}",st));
            }
            if(sqf.HasFlag(MpContentQueryBitFlags.AppName)) {
                opts.Add(new Tuple<string,string>($"{(int)MpRootOptionType.Source},{(int)MpSourceOptionType.App},{(int)MpAppOptionType.ApplicationName},{(int)tot}",st));
            }
            return opts;
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
                MpAvSearchCriteriaItemViewModel nscivm = await CreateCriteriaItemViewModelAsync(nsci);
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
                MpPlatform.Services.Query.NotifyQueryChanged(true);
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

                ttrvm.SelectTagCommand.Execute(ttrvm.RootGroupTagViewModel);
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
             },()=>IsAdvSearchActive, this, new[] { this });
        
        public ICommand CancelPendingOrCollapseSavedQueryCommand => new MpAsyncCommand(
             async() => {
                 // NOTE only hooked to adv header minus button
                 if(IsPendingQuery) {
                     await RejectPendingCriteriaItemsCommand.ExecuteAsync();
                 }
                 IsExpanded = false;
             },()=>IsAdvSearchActive, this, new[] { this });


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
                InitializeAsync(queryTagId, false).FireAndForgetSafeAsync(this);
            });


        #endregion
    }
}
