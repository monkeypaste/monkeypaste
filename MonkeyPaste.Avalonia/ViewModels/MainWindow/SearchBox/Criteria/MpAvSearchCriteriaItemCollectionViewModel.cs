using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchCriteriaItemCollectionViewModel : MpViewModelBase {

        #region Private Variable

        private MpIQueryInfo _simpleSearchRef;
        #endregion

        #region Constants
        #endregion

        #region Statics

        private static MpAvSearchCriteriaItemCollectionViewModel _instance;
        public static MpAvSearchCriteriaItemCollectionViewModel Instance => _instance ?? (_instance = new MpAvSearchCriteriaItemCollectionViewModel());
        
        #endregion

        #region Interfaces
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

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsAnyBusy);
        public bool IsExpanded { get; set; }
        public bool HasCriteriaItems => Items.Count > 0;

        public bool IsSavedQuery => CurrentQueryTagId > 0;

        public bool IsPendingQuery => PendingQueryTagId > 0;
        #endregion

        #region Layout

        public double BoundCriteriaListBoxScreenHeight { get; set; }

        public double LastSearchCriteriaListBoxHeight { get; private set; }

        public double MaxSearchCriteriaListBoxHeight =>
            Items.Sum(x => x.SearchCriteriaListBoxItemHeight) +
            Items.Sum(x=>x.CriteriaItemBorder.Top + x.CriteriaItemBorder.Bottom);

        #endregion

        #region Model

        #region Global Enumerables

        public IEnumerable<MpUserDevice> UserDevices { get; private set; }
        #endregion

        public int PendingQueryTagId { get; private set; }
        public int CurrentQueryTagId { get; private set;}

        public int QueryTagId {
            get {
                if(PendingQueryTagId > 0) {
                    if(CurrentQueryTagId > 0) {
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
        #endregion

        #region Public Methods
        public MpAvSearchCriteriaItemCollectionViewModel() : base() {
            PropertyChanged += MpAvSearchCriteriaItemCollectionViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            if(UserDevices == null) {
                Task.Run(async () => {
                    UserDevices = await MpDataModelProvider.GetItemsAsync<MpUserDevice>();
                });
            }
        }


        public async Task InitializeAsync(int tagId, bool isPending) {
            IsBusy = true;

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
                    MpPlatform.Services.QueryInfo is not MpAvSearchCriteriaItemViewModel) {
                    _simpleSearchRef = MpPlatform.Services.QueryInfo;
                }
                MpPlatform.Services.QueryInfo = HeadItem;
            } else {
                if (_simpleSearchRef != null) {
                    MpPlatform.Services.QueryInfo = _simpleSearchRef;
                    _simpleSearchRef = null;
                }
            }            

            IsBusy = false;

            while (MpAvClipTrayViewModel.Instance.IsAnyBusy) {
                // wait for tile layouts to update or something that prevents requery
                await Task.Delay(100);
            }
            MpPlatform.Services.QueryInfo.NotifyQueryChanged(true);
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private async Task<MpAvSearchCriteriaItemViewModel> CreateCriteriaItemViewModel(MpSearchCriteriaItem sci) {
            MpAvSearchCriteriaItemViewModel nscivm = new MpAvSearchCriteriaItemViewModel(this);
            await nscivm.InitializeAsync(sci);
            return nscivm;
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.AdvancedSearchOpened:
                    double default_visible_row_count = 1.5d;
                    double row_height = Items.Count == 0 ? 0 : Items.First().SearchCriteriaListBoxItemHeight;
                    double delta_open_height = Math.Min((double)Items.Count, default_visible_row_count) * row_height;

                    MpAvResizeExtension.ResizeByDelta(MpAvMainWindow.Instance, 0, delta_open_height,false);
                    BoundCriteriaListBoxScreenHeight = delta_open_height;


                    break;
                case MpMessageType.AdvancedSearchClosed:
                    double delta_close_height = -BoundCriteriaListBoxScreenHeight;
                    BoundCriteriaListBoxScreenHeight = 0;
                    MpAvResizeExtension.ResizeByDelta(MpAvMainWindow.Instance, 0, delta_close_height, false);
                    
                    break;

            }
        }
        private void MpAvSearchCriteriaItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedItem):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    break;
                case nameof(IsExpanded):
                    if(IsExpanded) {
                        MpMessenger.SendGlobal(MpMessageType.AdvancedSearchOpened);
                    } else {
                        MpMessenger.SendGlobal(MpMessageType.AdvancedSearchClosed);
                    }
                    break;
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SearchCriteriaItemsChanged);
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SortedItems));
            OnPropertyChanged(nameof(HasCriteriaItems));
            OnPropertyChanged(nameof(MaxSearchCriteriaListBoxHeight));

            UpdateCriteriaSortOrder().FireAndForgetSafeAsync(this);
            LastSearchCriteriaListBoxHeight = BoundCriteriaListBoxScreenHeight;
        }

        private async Task UpdateCriteriaSortOrder(bool fromModel = false) {
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

            MpContentQueryBitFlags[] typeFilters = new[] { MpContentQueryBitFlags.TextType, MpContentQueryBitFlags.ImageType, MpContentQueryBitFlags.FileType };
            if (typeFilters.All(x => true_filters.Contains(x))) {
                // TODO should probably account for non content type in query as any
                // when all types selected not needed in advanced
                //true_filters.RemoveAll(x=>typeFilters.Contains(x));
            }

            List<MpSearchCriteriaItem> items = new List<MpSearchCriteriaItem>();
            foreach (var ctf in true_filters) {
                string opt_path =
                    GetSimpleSearchOptionString(
                        ctf,
                        all_filters,
                        st);

                var sci = await MpSearchCriteriaItem.CreateAsync(
                    tagId: pending_tag.Id,
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
            // parent/non match opt:
            // <Enum Module>|<Enum Name>|<MpContentFilterFlag Name>
            // match opt:
            // <Enum Module>|<Enum Name>|<base64 search text>|<is case sensitive>|<MpContentFilterFlag Name>

            searchText = searchText == null ? string.Empty : searchText;
            List<object> opts = new List<object>();
            switch (current_flag) {
                // Root.Content
                case MpContentQueryBitFlags.Title:
                    opts.Add(MpRootOptionType.Content);
                    opts.Add(MpContentOptionType.Title);
                    opts.Add(MpTextOptionType.Contains);
                    opts.Add(new object[] { MpContentQueryBitFlags.MatchValue, searchText, all_flags.HasFlag(MpContentQueryBitFlags.CaseSensitive) });
                    break;
                case MpContentQueryBitFlags.Content:
                    opts.Add(MpRootOptionType.Content);
                    opts.Add(MpContentOptionType.AnyText);
                    opts.Add(MpTextOptionType.Contains);
                    opts.Add(new object[] { MpContentQueryBitFlags.MatchValue, searchText, all_flags.HasFlag(MpContentQueryBitFlags.CaseSensitive) });
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

            List<string> parts = new List<string>();
            foreach (var opt in opts) {
                string opt_str = string.Empty;
                if (opt is Enum enumOpt) {
                    opt_str = $"{enumOpt.GetType()}|{enumOpt}";
                } else if (opt is object[] matchParts) {
                    // special case for case sensitive
                    if (matchParts[0] is Enum matchEnum) {
                        opt_str += $"{matchEnum.GetType()}|{matchEnum}";
                    }
                    if (matchParts[1] is string match_text) {
                        opt_str += $"|{match_text.ToBase64String()}";
                    }
                    if (matchParts[2] is bool case_val) {
                        opt_str += $"|{case_val}";
                    }
                }
                opt_str += $"|{current_flag}";
                parts.Add(opt_str);
            }
            string result = string.Join(",", parts);
            return result;
        }
        
        #endregion

        #region Commands


        public ICommand AddSearchCriteriaItemCommand => new MpAsyncCommand<object>(
            async (args) => {
                int add_idx = Items.Count;
                if (args is MpAvSearchCriteriaItemViewModel scivm) {
                    add_idx = scivm.SortOrderIdx + 1;
                }
                MpSearchCriteriaItem nsci = new MpSearchCriteriaItem() {
                    SortOrderIdx = Items.Count
                };
                MpAvSearchCriteriaItemViewModel nscivm = await CreateCriteriaItemViewModel(nsci);
                Items.Insert(add_idx, nscivm);
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(HasCriteriaItems));
            });

        public ICommand RemoveSearchCriteriaItemCommand => new MpCommand<object>(
            async (args) => {
                var scivm = args as MpAvSearchCriteriaItemViewModel;
                int scivmIdx = Items.IndexOf(scivm);
                Items.RemoveAt(scivmIdx);
                if (scivm.SearchCriteriaItem.Id > 0) {
                    await scivm.SearchCriteriaItem.DeleteFromDatabaseAsync();
                }
                await UpdateCriteriaSortOrder();
                HeadItem.NotifyQueryChanged(true);
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
            }, () => PendingQueryTagId > 0);

        public ICommand RejectPendingCriteriaItemsCommand => new MpAsyncCommand(
            async() => {
                var t = await MpDataModelProvider.GetItemAsync<MpTag>(PendingQueryTagId);
                t.DeleteFromDatabaseAsync().FireAndForgetSafeAsync();
                PendingQueryTagId = 0;
                await InitializeAsync(0,false);
            },()=> IsPendingQuery);

        public MpIAsyncCommand ConvertCurrentSearchToAdvancedCommand => new MpAsyncCommand(
            async () => {
                bool is_pending = true;
                int converted_query_tag_id = 0;
                if(QueryTagId > 0) {
                    // NOTE since this should only be the case from tag add, its assumed 
                    // save follows this cmd in add tag

                    is_pending = false;

                    var cur_query_tag = await MpDataModelProvider.GetItemAsync<MpTag>(QueryTagId);
                    bool is_cur_pending = PendingQueryTagId == QueryTagId;

                    // clone current adv search
                    var clone_of_cur_query_tag = await cur_query_tag.CloneDbModelAsync();

                    converted_query_tag_id = clone_of_cur_query_tag.Id;
                    if(is_cur_pending) {
                        // if current is pending delete it
                        cur_query_tag.DeleteFromDatabaseAsync().FireAndForgetSafeAsync(this);
                    }
                } else {
                    // clone current simple search
                    converted_query_tag_id = await ConvertCurrentSimpleSearchAsync();
                }
                
                // convert result to criteria rows
                await InitializeAsync(converted_query_tag_id, is_pending);

            });

        public ICommand ToggleIsExpandedCommand => new MpAsyncCommand(
            async () => {
                if(IsExpanded) {
                    if(IsPendingQuery) {
                        RejectPendingCriteriaItemsCommand.Execute(null);
                        await Task.Delay(50);
                        while (IsAnyBusy) {
                            await Task.Delay(100);
                        }
                    } 
                    
                    IsExpanded = false;
                } else {
                    if(!IsPendingQuery && !IsSavedQuery) {
                        if(HasCriteriaItems) {
                            // what's going on here?
                            Debugger.Break();
                        }
                        await ConvertCurrentSearchToAdvancedCommand.ExecuteAsync();
                    } 
                    IsExpanded = true;
                }
            });

        public ICommand SelectAdvancedSearchCommand => new MpCommand<object>(
            (args) => {
                int queryTagId = 0;
                if(args is int tagId) {
                    queryTagId = tagId;
                }
                IsExpanded = false;
                InitializeAsync(queryTagId, false).FireAndForgetSafeAsync(this);
            });

        #endregion
    }
}
