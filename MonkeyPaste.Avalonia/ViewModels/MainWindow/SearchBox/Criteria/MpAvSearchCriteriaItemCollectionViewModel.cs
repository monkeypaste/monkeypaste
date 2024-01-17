using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchCriteriaItemCollectionViewModel :
        MpAvViewModelBase,
        MpIWantsTopmostWindowViewModel,
        MpICloseWindowViewModel,
        MpIExpandableViewModel {

        #region Private Variable
        //private Window _criteriaWindow;
        private Tuple<int, int>[] _lastSaveCriteriaItemIdAndSortLookup;

        private object _initLockObj = new object();
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
        bool MpICloseWindowViewModel.IsWindowOpen {
            get => IsCriteriaWindowOpen;
            set => IsCriteriaWindowOpen = value;
        }
        bool MpIWantsTopmostWindowViewModel.WantsTopmost =>
            true;
        #endregion



        #region MpIExpandableViewModel Implementation

        public bool IsExpanded { get; set; }

        #endregion

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvSearchCriteriaItemViewModel> Items { get; set; } = new ObservableCollection<MpAvSearchCriteriaItemViewModel>();

        public IEnumerable<MpAvSearchCriteriaItemViewModel> SortedItems =>
            Items.OrderBy(x => x.SortOrderIdx).ToList();

        public MpAvSearchCriteriaItemViewModel HeadItem =>
            SortedItems.FirstOrDefault();
        public MpAvSearchCriteriaItemViewModel SelectedItem { get; set; }

        public MpAvTagTileViewModel CurrentQueryTagViewModel =>
            MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == QueryTagId);

        #endregion

        #region State

        public string DisabledInputTooltip =>
            CanAlter ? string.Empty : UiStrings.SearchDisabeledCriteriaTooltip;
        public bool IsAnyDragging =>
            Items.Any(x => x.IsDragging);
        public bool IsCriteriaWindowOpen { get; set; }

        public bool HasAnyCriteriaModelChanged =>
            Items.Any(x => x.HasModelChanged);

        public bool HasSearchChanged {
            get {
                if (_lastSaveCriteriaItemIdAndSortLookup == null) {
                    // should only be null before init
                    return false;
                }
                var cur = GetCurrentSearchItemSaveCheckState();
                if (cur.Length != _lastSaveCriteriaItemIdAndSortLookup.Length) {
                    return true;
                }
                for (int i = 0; i < cur.Length; i++) {
                    var cur_i = cur[i];
                    var last_i = _lastSaveCriteriaItemIdAndSortLookup[i];
                    if (cur_i.Item1 != last_i.Item1 ||
                        cur_i.Item2 != last_i.Item2) {
                        return true;
                    }
                }
                return false;
            }
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

        public bool CanSave =>
            IsPendingQuery ||
            HasAnyCriteriaModelChanged ||
            HasSearchChanged;

        public bool CanAlter =>
            CurrentQueryTagViewModel == null ||
            !CurrentQueryTagViewModel.IsTagReadOnly;

        #endregion

        #region Appearance

        public string CurrentTagName =>
            CurrentQueryTagViewModel == null ? string.Empty : CurrentQueryTagViewModel.TagName;

        public string CurrentTagHexColor =>
            CurrentQueryTagViewModel == null ? string.Empty : CurrentQueryTagViewModel.TagHexColor;
        #endregion

        #region Layout

        public double CriteriaDropLineHeight =>
            5;
        public double DefaultCriteriaRowHeight =>
            60;
        public double HeaderHeight =>
            40;

        #endregion

        #region Model

        #region Global Enumerables

        public IEnumerable<MpUserDevice> UserDevices { get; private set; }
        #endregion

        public int QueryTagId { get; private set; }

        #endregion

        #endregion

        #region Constructors
        public MpAvSearchCriteriaItemCollectionViewModel() : base(null) {
            PropertyChanged += MpAvSearchCriteriaItemCollectionViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

        }
        #endregion

        #region Public Methods

        public async Task InitializeAsync(int tagId, bool isPending) {
            if (UserDevices == null) {
                _ = Task.Run(async () => {
                    UserDevices = await MpDataModelProvider.GetItemsAsync<MpUserDevice>();
                });
            }

            MpConsole.WriteLine($"adv search called tagId: {tagId} pending: {isPending}");
            await MpFifoAsyncQueue.WaitByConditionAsync(_initLockObj, () => { return IsBusy; });

            IsBusy = true;

            if (isPending && tagId > 0) {
                // shouldn't happen
                MpDebug.Break();
            }
            if (MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == tagId) is MpAvTagTileViewModel ttvm &&
                !ttvm.IsQueryTag) {
                // clear adv search
                tagId = 0;
            }
            QueryTagId = isPending ? 0 : tagId;

            Items.Clear();
            if (QueryTagId > 0) {
                var cil = await MpDataModelProvider.GetCriteriaItemsByTagIdAsync(QueryTagId);

                var simple_cil = cil.Where(x => x.QueryType == MpQueryType.Simple);
                await MpAvQueryViewModel.Instance.RestoreAdvSearchValuesAsync(simple_cil.FirstOrDefault());

                var adv_cil = cil.Where(x => x.QueryType == MpQueryType.Advanced);
                foreach (var adv_ci in adv_cil.OrderBy(x => x.SortOrderIdx)) {
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

            ResetLastStateToCurrent();
            IsBusy = false;

            if (IsSavedQuery) {
                Mp.Services.Query.NotifyQueryChanged(true);
            }
            RefreshProperties();
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
                Dispatcher.UIThread.Post(async () => {
                    await InitializeAsync(0, false);
                });
            }
        }

        #endregion

        #region Private Methods

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                //case MpMessageType.AdvancedSearchExpanded:
                //case MpMessageType.AdvancedSearchUnexpanded:
                //    AnimateAdvSearchMenuAsync(IsExpanded).FireAndForgetSafeAsync(this);
                //    break;
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
                case nameof(IgnoreHasModelChanged):
                    Items.ForEach(x => x.IgnoreHasModelChanged = IgnoreHasModelChanged);
                    Items.ForEach(x => x.OnPropertyChanged(nameof(HasModelChanged)));
                    break;
                case nameof(QueryTagId):
                    OnPropertyChanged(nameof(CurrentQueryTagViewModel));
                    OnPropertyChanged(nameof(CanSave));
                    OnPropertyChanged(nameof(CanAlter));
                    break;
                case nameof(CanAlter):
                    OnPropertyChanged(nameof(DisabledInputTooltip));
                    break;
                case nameof(IsCriteriaWindowOpen):
                    if (IsCriteriaWindowOpen) {
                        IsExpanded = false;
                    } else if (IsExpanded && this is MpICloseWindowViewModel cwvm) {
                        // when close button on search expander is clicked IsExpanded=true
                        // so this is only called when closed from the window x button
                        cwvm.IsWindowOpen = false;
                        cwvm.OnPropertyChanged(nameof(cwvm.IsWindowOpen));
                    }
                    break;
                case nameof(IsExpanded):
                    MpMessenger.SendGlobal(MpMessageType.AdvancedSearchExpandedChanged);
                    HandleExpandChangedAsync(IsExpanded).FireAndForgetSafeAsync(this);
                    break;
                case nameof(HasAnyCriteriaModelChanged):
                    OnPropertyChanged(nameof(CanSave));
                    break;
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            MpMessenger.SendGlobal(MpMessageType.SearchCriteriaItemsChanged);
            RefreshProperties();

            UpdateCriteriaSortOrderAsync().FireAndForgetSafeAsync(this);
        }

        private void RefreshProperties() {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SortedItems));
            OnPropertyChanged(nameof(HasCriteriaItems));
            OnPropertyChanged(nameof(HasSearchChanged));
            OnPropertyChanged(nameof(SortedItems));
            OnPropertyChanged(nameof(IsAdvSearchActive));
            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(IsCriteriaWindowOpen));
            OnPropertyChanged(nameof(IsPendingQuery));
        }

        private Tuple<int, int>[] GetCurrentSearchItemSaveCheckState() {
            return
                Items.Select(x => new Tuple<int, int>(x.SearchCriteriaItemId, x.SortOrderIdx)).ToArray();
        }
        private void ResetLastStateToCurrent() {
            _lastSaveCriteriaItemIdAndSortLookup = GetCurrentSearchItemSaveCheckState();

            OnPropertyChanged(nameof(CanSave));
        }

        private async Task HandleExpandChangedAsync(bool isExpanding) {
            if (isExpanding) {
                if (!IsAdvSearchActive) {
                    // plus on search box toggled to checked
                    await InitializeAsync(0, true);
                } else if (IsCriteriaWindowOpen) {
                    IsCriteriaWindowOpen = false;
                }
                double default_visible_row_count = 2d;
                double delta_open_height = DefaultCriteriaRowHeight * default_visible_row_count;

                //MpAvResizeExtension.ResizeByDelta(MpAvSearchCriteriaListBoxView.Instance, 0, delta_open_height, false);
                Items.ForEach(x => x.Items.ForEach(y => y.OnPropertyChanged(nameof(y.SelectedItemIdx))));
            } else {
                if (IsPendingQuery && IsAllCriteriaEmpty && !IsCriteriaWindowOpen) {
                    // discard pending if nothing changed
                    await InitializeAsync(0, false);
                }
                //MpAvResizeExtension.ResizeByDelta(MpAvMainView.Instance, 0, delta_close_height, false);                    

            }
            RefreshProperties();
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
                MpDebug.Break();
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
                if (IsSavedQuery) {
                    // manually write since was busy during init
                    await nscivm.SearchCriteriaItem.WriteToDatabaseAsync();
                }

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

        public ICommand DuplicateQueryCommand => new MpAsyncCommand(
            async () => {
                var ttvm_to_duplicate = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == QueryTagId);
                if (ttvm_to_duplicate == null) {
                    return;
                }
                await ttvm_to_duplicate.MoveOrCopyThisTagCommand.ExecuteAsync(new object[] { ttvm_to_duplicate.ParentTagId, -1, false, true });

                var dup_ttvm = ttvm_to_duplicate.ParentTreeItem.SortedItems.Last();
                await MpAvTagTrayViewModel.Instance.SelectTagAndBringIntoTreeViewCommand.ExecuteAsync(dup_ttvm);

                // NOTE select tag again to update adv search ui
                await Task.Delay(300);
                MpAvTagTrayViewModel.Instance.SelectTagCommand.Execute(dup_ttvm.TagId);
            }, () => {
                return IsSavedQuery;
            });

        public MpIAsyncCommand SaveQueryCommand => new MpAsyncCommand(
            async () => {
                bool was_pending = IsPendingQuery;
                if (was_pending) {
                    // creates query tag id, triggers rename tag
                    await SavePendingQueryCommand.ExecuteAsync();
                }
                Items.ForEach(x => x.IgnoreHasModelChanged = false);
                await Task.Delay(100);
                while (IsAnyBusy) {
                    await Task.Delay(100);
                }
                Items.ForEach(x => x.IgnoreHasModelChanged = true);
                if (was_pending) {
                    MpAvTagTrayViewModel.Instance.SelectTagCommand.Execute(QueryTagId);
                }
            });

        public MpIAsyncCommand SavePendingQueryCommand => new MpAsyncCommand(
            async () => {
                var ttrvm = MpAvTagTrayViewModel.Instance;
                await ttrvm.SelectTagAndBringIntoTreeViewCommand.ExecuteAsync(
                    MpTag.FiltersTagId);

                int new_query_tag_id = await ConvertPendingToQueryTagAsync();

                // clear pending flag 
                QueryTagId = new_query_tag_id;

                await ttrvm.FiltersTagViewModel.AddNewChildTagCommand.ExecuteAsync(new_query_tag_id);
            }, () => {
                return IsPendingQuery;
            });

        public ICommand RejectPendingCriteriaItemsCommand => new MpAsyncCommand(
            async () => {
                await InitializeAsync(0, false);
                IsExpanded = false;
            });

        public ICommand SelectAdvancedSearchCommand => new MpCommand<object>(
            (args) => {
                // NOTE only called from tag tray when query tag is selected
                // instead of query change ntf
                int queryTagId = 0;
                if (args is int tagId) {
                    queryTagId = tagId;
                }

                // NOTE since query takes have no linked content
                // but are the selected tag treat search as from
                // all until selected tag is changed
                InitializeAsync(queryTagId, false).FireAndForgetSafeAsync(this);
            }, (args) => {
                if (IsPendingQuery) {
                    return false;
                }
                if (args is int tagId) {
                    if (tagId == QueryTagId) {
                        return false;
                    }
                }
                return true;
            });

        public ICommand ExpandCriteriaFromDragEnterCommand => new MpCommand(
            () => {
                IsExpanded = true;
            }, () => {
                return IsAdvSearchActive && !IsExpanded && !IsCriteriaWindowOpen;
            });

        public ICommand OpenCriteriaWindowCommand => new MpCommand<object>(
            (args) => {
                if (!IsExpanded) {
                    IsExpanded = true;
                }
                if (Mp.Services.PlatformInfo.IsDesktop) {
                    var _criteriaWindow = new MpAvWindow() {
                        SizeToContent = SizeToContent.Manual,
                        Width = 1100,
                        Height = 300,
                        DataContext = this,
                        ShowInTaskbar = true,
                        Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("BinocularsImage", typeof(WindowIcon), null, null) as WindowIcon,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Content = new MpAvSearchCriteriaListBoxView(),
                        Topmost = true,
                        Padding = new Thickness(5)
                    };

                    _criteriaWindow.Bind(
                        Window.TitleProperty,
                        new Binding() {
                            Source = this,
                            Path = nameof(CurrentTagName),
                            StringFormat = "Search Criteria '{0}'",
                            TargetNullValue = "Search Criteria 'Untitled'",
                            FallbackValue = "Search Criteria 'Untitled'",
                            Converter = MpAvStringToWindowTitleConverter.Instance
                        });
                    _criteriaWindow.Show();
                } else {
                    // Some kinda view nav here
                    // see https://github.com/AvaloniaUI/Avalonia/discussions/9818

                }
                OnPropertyChanged(nameof(IsCriteriaWindowOpen));
            }, (args) => {
                return !IsCriteriaWindowOpen;
            });

        public ICommand RefreshSearchCommand => new MpCommand(
            () => {
                Mp.Services.Query.NotifyQueryChanged(true);
            });
        #endregion
    }
}
