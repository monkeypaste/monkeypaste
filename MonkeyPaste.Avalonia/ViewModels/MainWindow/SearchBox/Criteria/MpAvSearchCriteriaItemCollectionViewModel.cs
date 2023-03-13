using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using CefNet.CApi;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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
        MpIChildWindowViewModel,
        MpIExpandableViewModel {

        #region Private Variable
        //private Window _criteriaWindow;
        #endregion

        #region Constants
        #endregion

        #region Statics

        private static MpAvSearchCriteriaItemCollectionViewModel _instance;
        public static MpAvSearchCriteriaItemCollectionViewModel Instance => _instance ?? (_instance = new MpAvSearchCriteriaItemCollectionViewModel());

        #endregion

        #region Interfaces

        #region MpIWindowViewModel Implementation
        public MpWindowType WindowType =>
            MpWindowType.PopOut;
        bool MpIChildWindowViewModel.IsOpen {
            get => IsCriteriaWindowOpen;
            set => IsCriteriaWindowOpen = value;
        }
        #endregion



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

        public MpAvTagTileViewModel CurrentQueryTagViewModel =>
            MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == QueryTagId);

        #endregion

        #region State
        public bool IsAnyDragging =>
            Items.Any(x => x.IsDragging);
        public bool IsCriteriaWindowOpen { get; set; }
        //public bool IsCriteriaWindowOpen => _criteriaWindow != null;//{ get; set; }

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

        public double CriteriaDropLineHeight =>
            5;
        public double DefaultCriteriaRowHeight =>
            60;
        public double HeaderHeight =>
            40;
        public double BoundHeaderHeight { get; set; }

        public double BoundCriteriaListViewScreenHeight { get; set; }
        public double BoundCriteriaListBoxScreenHeight =>
            Math.Max(0, BoundCriteriaListViewScreenHeight - BoundHeaderHeight);

        public double MaxSearchCriteriaRowHeight =>
            IsCriteriaWindowOpen ? 0 : MaxSearchCriteriaViewHeight;

        public double MaxSearchCriteriaListBoxHeight =>
            Math.Max(0, MaxSearchCriteriaViewHeight - BoundHeaderHeight);
        public double MaxSearchCriteriaViewHeight {
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
                return Math.Max(0, h);
            }
        }

        #endregion

        #region Model

        #region Global Enumerables

        public IEnumerable<MpUserDevice> UserDevices { get; private set; }
        #endregion

        public int QueryTagId { get; private set; }

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
            if (MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == tagId) is MpAvTagTileViewModel ttvm &&
                ttvm.IsLinkTag) {
                // clear adv search
                tagId = 0;
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

            if (!HasCriteriaItems && (IsSavedQuery || isPending)) {
                // create empty criteria item
                var empty_civm = await CreateCriteriaItemViewModelAsync(null);
                Items.Add(empty_civm);
            }

            if (!HasCriteriaItems && IsCriteriaWindowOpen) {
                // active search no longer query tag, close criteria window
                IsCriteriaWindowOpen = false;
            }

            while (Items.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            IsBusy = false;

            Mp.Services.Query.NotifyQueryChanged(true);
        }

        public async Task<MpAvSearchCriteriaItemViewModel> CreateCriteriaItemViewModelAsync(MpSearchCriteriaItem sci) {
            MpAvSearchCriteriaItemViewModel nscivm = new MpAvSearchCriteriaItemViewModel(this);
            if (sci == null) {
                // create default empty item
                // NOTE when pending its not written until saved
                sci = await MpSearchCriteriaItem.CreateAsync(
                    tagId: QueryTagId,
                    sortOrderIdx: Items.Count,
                    queryType: MpQueryType.Advanced,
                    suppressWrite: !IsSavedQuery);
            }
            await nscivm.InitializeAsync(sci);
            return nscivm;
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
                case nameof(QueryTagId):
                    OnPropertyChanged(nameof(CurrentQueryTagViewModel));
                    break;
                case nameof(IsCriteriaWindowOpen):

                    OnPropertyChanged(nameof(MaxSearchCriteriaViewHeight));
                    OnPropertyChanged(nameof(MaxSearchCriteriaListBoxHeight));
                    if (IsCriteriaWindowOpen) {
                        IsExpanded = false;
                    } else {
                        // force remeasure 
                        SetIsExpandedAsync(false).FireAndForgetSafeAsync();
                    }
                    break;
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SearchCriteriaItemsChanged);
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SortedItems));
            OnPropertyChanged(nameof(HasCriteriaItems));
            OnPropertyChanged(nameof(MaxSearchCriteriaViewHeight));

            UpdateCriteriaSortOrderAsync().FireAndForgetSafeAsync(this);
        }

        private async Task AnimateAdvSearchMenuAsync(bool isExpanding) {
            await Dispatcher.UIThread.InvokeAsync(async () => {
                await Task.Delay(1);
                if (isExpanding) {
                    double default_visible_row_count = 2d;
                    double delta_open_height = DefaultCriteriaRowHeight * default_visible_row_count;

                    BoundHeaderHeight = HeaderHeight;
                    BoundCriteriaListViewScreenHeight = delta_open_height;
                    //MpAvResizeExtension.ResizeByDelta(MpAvSearchCriteriaListBoxView.Instance, 0, delta_open_height, false);
                    OnPropertyChanged(nameof(IsPendingQuery));
                    OnPropertyChanged(nameof(BoundCriteriaListBoxScreenHeight));
                    Items.ForEach(x => x.Items.ForEach(y => y.OnPropertyChanged(nameof(y.SelectedItemIdx))));
                } else {
                    double delta_close_height = -BoundCriteriaListViewScreenHeight;
                    BoundCriteriaListViewScreenHeight = 0;
                    //MpAvResizeExtension.ResizeByDelta(MpAvMainView.Instance, 0, delta_close_height, false);                    

                }
            });

        }

        private async Task SetIsExpandedAsync(bool newExpandedValue) {
            if (newExpandedValue) {
                if (!IsAdvSearchActive) {
                    // plus on search box toggled to checked
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

            var simple_ci = await MpSearchCriteriaItem.CreateAsync(
                queryType: MpQueryType.Simple,
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

                MpAvSearchCriteriaItemViewModel nscivm = await CreateCriteriaItemViewModelAsync(null);
                Items.Add(nscivm);
                if (args is MpAvSearchCriteriaItemViewModel scivm) {
                    var resorted_items = SortedItems.ToList();
                    resorted_items.Move(Items.Count - 1, scivm.SortOrderIdx + 1);
                    resorted_items.ForEach((x, idx) => x.SortOrderIdx = idx);
                }
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
                Mp.Services.Query.NotifyQueryChanged(true);
            },
            (args) => args is MpAvSearchCriteriaItemViewModel);

        public ICommand SavePendingQueryCommand => new MpAsyncCommand(
            async () => {
                if (IsCriteriaWindowOpen && !MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                    // when saving from float window show mw to rename/confirm new query tag
                    if (!MpAvMainWindowViewModel.Instance.ShowMainWindowCommand.CanExecute(null)) {
                        // why not?
                        MpDebug.Break();
                    } else {
                        MpAvMainWindowViewModel.Instance.ShowMainWindowCommand.Execute(null);
                        while (!MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                            await Task.Delay(100);
                        }
                    }
                }
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

                // clear pending flag 
                QueryTagId = new_query_tag_id;

                await ttrvm.RootGroupTagViewModel.AddNewChildTagCommand.ExecuteAsync(new_query_tag_id);
                // wait for tag to be added

                await Task.Delay(300);
                if (ttrvm.RootGroupTagViewModel.Items.FirstOrDefault(x => x.TagId == new_query_tag_id) is MpAvTagTileViewModel new_query_ttvm) {
                    // trigger rename
                    new_query_ttvm.RenameTagCommand.Execute(null);
                }

            }, () => {
                return IsPendingQuery;
            });

        public MpIAsyncCommand RejectPendingCriteriaItemsCommand => new MpAsyncCommand(
            async () => {
                await InitializeAsync(0, false);
                IsExpanded = false;
            }, () => IsPendingQuery, this, new[] { this });

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
            }, (args) => {
                if (args is int tagId &&
                    tagId == QueryTagId) {
                    return false;
                }
                return true;
            });

        public ICommand OpenCriteriaWindowCommand => new MpCommand<object>(
            (args) => {
                if (Mp.Services.PlatformInfo.IsDesktop) {
                    Items.ForEach(x => x.LogPropertyChangedEvents = true);
                    var _criteriaWindow = new MpAvWindow() {
                        Width = 1100,
                        Height = 300,
                        DataContext = this,
                        ShowInTaskbar = true,
                        Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("AppIcon", null, null, null) as WindowIcon,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Content = new Border() {
                            Padding = new Thickness(3),
                            Background = Brushes.Black,
                            Child = new MpAvSearchCriteriaListBoxView() {
                                Background = Brushes.Violet,
                            }
                        },
                        Topmost = true,
                        Padding = new Thickness(10)
                    };

                    //_criteriaWindow.Bind(
                    //    Window.DataContextProperty,
                    //    new Binding() {
                    //        Source = this,
                    //        Path = nameof(CurrentQueryTagViewModel)
                    //    });

                    _criteriaWindow.Bind(
                        Window.TitleProperty,
                        new Binding() {
                            Source = CurrentQueryTagViewModel,
                            Path = nameof(MpAvTagTileViewModel.TagName),
                            StringFormat = "Search Criteria - {0}",
                            TargetNullValue = "Search Criteria - Untitled",
                            FallbackValue = "Search Criteria - Untitled"
                        });

                    _criteriaWindow.Bind(
                        Window.BackgroundProperty,
                        new Binding() {
                            Source = CurrentQueryTagViewModel,
                            Path = nameof(MpAvTagTileViewModel.TagHexColor),
                            Mode = BindingMode.OneWay,
                            Converter = MpAvStringHexToBrushConverter.Instance,
                            TargetNullValue = MpSystemColors.darkviolet,
                            FallbackValue = MpSystemColors.darkviolet
                        });
                    //IsCriteriaWindowOpen = true;
                    _criteriaWindow.ShowChild();
                } else {
                    // Some kinda view nav here
                    // see https://github.com/AvaloniaUI/Avalonia/discussions/9818

                }
                OnPropertyChanged(nameof(IsCriteriaWindowOpen));
            }, (args) => {
                return !IsCriteriaWindowOpen;
            }, new[] { this });

        #endregion
    }
}
