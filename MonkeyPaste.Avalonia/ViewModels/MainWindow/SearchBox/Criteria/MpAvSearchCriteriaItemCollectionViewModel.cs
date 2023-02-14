using Avalonia.Threading;

using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchCriteriaItemCollectionViewModel :
        MpViewModelBase,
        //MpIQueryResultProvider, 
        MpIExpandableViewModel {

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
                if (IsExpanded != value) {
                    SetIsExpandedAsync(value).FireAndForgetSafeAsync(this);
                }
            }
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

        #endregion

        #region State

        public bool HasAnyCriteriaChanged {
            get => Items.Any(x => x.HasCriteriaChanged);
            set => Items.ForEach(x => x.HasCriteriaChanged = value);
        }

        public bool IsAllCriteriaEmpty =>
            Items.All(x => x.IsEmptyCriteria);

        public bool IsAnyBusy =>
            IsBusy || Items.Any(x => x.IsAnyBusy);
        public bool HasCriteriaItems =>
            Items.Count > 0;

        public bool IsAdvSearchActive =>
            IsSavedQuery || IsPendingQuery;

        public bool IsSavedQuery =>
            QueryTagId > 0;

        public bool IsPendingQuery =>
            !IsSavedQuery && HasCriteriaItems;

        #endregion

        #region Layout

        public double BoundHeaderHeight { get; set; }

        public double BoundCriteriaListBoxScreenHeight { get; set; }

        public double MaxSearchCriteriaListBoxHeight {
            get {
                if (!IsAdvSearchActive) {
                    return 0;
                }
                double h = 0;
                if (true) {
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

        //public int PendingQueryTagId { get; private set; }
        public int QueryTagId { get; private set; }


        //public int QueryTagId {
        //    get {
        //        if(IsPendingQuery) {
        //            if(IsSavedQuery) {
        //                // should only be 1
        //                Debugger.Break();
        //            }
        //            return 0;
        //        }
        //        return QueryTagId;
        //    }
        //}

        #endregion

        #endregion

        #region Constructors
        //public MpAvSearchCriteriaItemCollectionViewModel(int startupQueryTagId) : this() {
        //    // NOTE only called BEFORE bootstrap in MpAvQueryInfoViewModel.Parse when json is an int (QueryTagId)
        //    _instance = this;

        //    if (startupQueryTagId == 0) {
        //        return;
        //    }
        //    if(startupQueryTagId < 0) {
        //        // shutdown was a pending query
        //        PendingQueryTagId = -startupQueryTagId;
        //    } else {
        //        // shutdown was a saved query
        //        CurrentQueryTagId = startupQueryTagId;
        //    }
        //}

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

            if (isPending && tagId > 0) {
                // shouldn't happen
                Debugger.Break();
            }
            QueryTagId = isPending ? 0 : tagId;

            Items.Clear();
            if (QueryTagId > 0) {
                var cil = await MpDataModelProvider.GetCriteriaItemsByTagId(QueryTagId);

                var simple_cil = cil.Where(x => x.QueryType == MpQueryType.Simple);
                await MpAvQueryViewModel.Instance.RestoreAdvSearchValuesAsync(simple_cil.FirstOrDefault());

                var adv_cil = cil.Where(x => x.QueryType == MpQueryType.Advanced);
                foreach (var adv_ci in adv_cil) {
                    var civm = await CreateCriteriaItemViewModelAsync(adv_ci);
                    Items.Add(civm);
                }
            }

            if (!HasCriteriaItems && isPending && !IsSavedQuery) {
                // create empty criteria item
                var empty_civm = await CreateCriteriaItemViewModelAsync(null);
                Items.Add(empty_civm);
            }

            while (Items.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            IsBusy = false;

            MpPlatform.Services.Query.NotifyQueryChanged(true);
        }



        #endregion

        #region Protected Methods

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTag t && t.Id == QueryTagId) {
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
            switch (msg) {
                case MpMessageType.AdvancedSearchExpanded:
                    AnimateAdvSearchMenuAsync(true).FireAndForgetSafeAsync(this);
                    break;
                case MpMessageType.AdvancedSearchUnexpanded:
                    AnimateAdvSearchMenuAsync(false).FireAndForgetSafeAsync(this);
                    break;
                case MpMessageType.TagSelectionChanged:
                    OnPropertyChanged(nameof(IsSavedQuery));
                    break;

            }
        }
        private void MpAvSearchCriteriaItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
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

        private async Task AnimateAdvSearchMenuAsync(bool isExpanding) {
            await Dispatcher.UIThread.InvokeAsync(async () => {
                await Task.Delay(1);
                if (isExpanding) {
                    double default_visible_row_count = 2d;
                    double delta_open_height = MpAvSearchCriteriaItemViewModel.DEFAULT_HEIGHT * default_visible_row_count;

                    BoundHeaderHeight = MpAvSearchCriteriaItemViewModel.DEFAULT_HEIGHT;
                    BoundCriteriaListBoxScreenHeight = delta_open_height;
                    //MpAvResizeExtension.ResizeByDelta(MpAvSearchCriteriaListBoxView.Instance, 0, delta_open_height, false);
                    OnPropertyChanged(nameof(IsPendingQuery));
                    Items.ForEach(x => x.Items.ForEach(y => y.OnPropertyChanged(nameof(y.SelectedItemIdx))));
                } else {
                    double delta_close_height = -BoundCriteriaListBoxScreenHeight;
                    BoundCriteriaListBoxScreenHeight = 0;
                    //MpAvResizeExtension.ResizeByDelta(MpAvMainWindow.Instance, 0, delta_close_height, false);                    

                }
            });

        }

        private async Task SetIsExpandedAsync(bool newExpandedValue) {
            if (newExpandedValue) {
                if (!IsAdvSearchActive) {
                    // plus on search box toggled to checked
                    //await ConvertCurrentSearchToAdvSearchAsync(true);
                    await InitializeAsync(0, true);
                }
                _isExpanded = true;
            } else {
                if (IsPendingQuery && IsAllCriteriaEmpty) {
                    // discard pending if nothing changed
                    await InitializeAsync(0, false);
                }
                _isExpanded = false;
            }
            OnPropertyChanged(nameof(IsExpanded));
        }

        private async Task UpdateCriteriaSortOrderAsync(bool fromModel = false) {
            if (fromModel) {
                Items.Sort(x => x.SortOrderIdx);
            } else {
                Items.ToList().ForEach((x, idx) => x.SortOrderIdx = idx);
                while (Items.ToList().Any(x => x.IsBusy)) {
                    await Task.Delay(100);
                }
            }
        }

        private async Task<int> ConvertPendingToQueryTagAsync() {
            if (QueryTagId > 0) {
                // not a simple search, check call stack
                Debugger.Break();
                return 0;
            }
            var pending_tag = await MpTag.CreateAsync(
                            tagType: MpTagType.Query,
                            sortType: MpAvClipTileSortFieldViewModel.Instance.SelectedSortType,
                            isSortDescending: MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending);

            // NOTE this is called at end of provider create so sortIdx is seed for these

            var simple_ci = await MpSearchCriteriaItem.CreateAsync(
                tagId: pending_tag.Id,
                joinType: MpLogicalQueryType.And,
                sortOrderIdx: 0,
                options: ((long)MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel.FilterType).ToString(),
                matchValue: MpAvSearchBoxViewModel.Instance.SearchText);

            Items.ForEach(x => x.QueryTagId = pending_tag.Id);
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.HasModelChanged)));
            await Task.Delay(50);
            while (IsAnyBusy) {
                await Task.Delay(100);
            }
            return pending_tag.Id;
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
                while (Items.Any(x => x.IsAnyBusy)) {
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
            async () => {
                // NOTE this should only occur for new searches, onced created saving is by HasModelChanged
                var ttrvm = MpAvTagTrayViewModel.Instance;
                int waitTimeMs = MpAvSidebarItemCollectionViewModel.Instance.SelectedItem == ttrvm ? 0 : 500;
                MpAvSidebarItemCollectionViewModel.Instance.SelectSidebarItemCommand.Execute(ttrvm);
                // wait for panel open
                await Task.Delay(waitTimeMs);

                if (ttrvm.SelectedItem.IsNotGroupTag) {
                    // NOTE when non-group tag selected 
                    // select root group automatically
                    // this shouldn't affect the current query cause its a group tag

                    ttrvm.SelectTagCommand.Execute(ttrvm.RootGroupTagViewModel);
                    while (ttrvm.IsSelecting) {
                        await Task.Delay(100);
                    }
                }

                int new_query_tag_id = await ConvertPendingToQueryTagAsync();

                ttrvm.RootGroupTagViewModel.AddNewChildTagCommand.Execute(new_query_tag_id);

                // clear pending flag 
                QueryTagId = new_query_tag_id;
            }, () => {
                if (!IsPendingQuery ||
                    MpAvTagTrayViewModel.Instance.SelectedItem.IsNotGroupTag) {
                    return false;
                }
                return true;
            });

        public MpIAsyncCommand RejectPendingCriteriaItemsCommand => new MpAsyncCommand(
            async () => {
                await InitializeAsync(0, false);
                IsExpanded = false;
            }, () => IsPendingQuery, this, new[] { this });

        public ICommand SaveQueryCommand => new MpCommand(
             () => {
                 if (IsPendingQuery) {
                     SavePendingQueryCommand.Execute(null);
                 } else {
                     IgnoreHasModelChanged = false;
                 }
             }, () => IsAdvSearchActive, this, new[] { this });



        public ICommand SelectAdvancedSearchCommand => new MpCommand<object>(
            (args) => {
                // NOTE only called from tag tray when query tag is selected
                // instead of query change ntf
                int queryTagId = 0;
                if (args is int tagId) {
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
