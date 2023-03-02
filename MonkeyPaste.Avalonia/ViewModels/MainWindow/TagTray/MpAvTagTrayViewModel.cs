using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvTagTrayViewModel :
        MpAvSelectorViewModelBase<object, MpAvTagTileViewModel>,
        MpIAsyncSingletonViewModel<MpAvTagTrayViewModel>,
        MpIHasDragOverProperty,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpISidebarItemViewModel,
        MpITagQueryTools {
        #region Private Variables


        #endregion

        #region Statics


        private static MpAvTagTrayViewModel _instance;
        public static MpAvTagTrayViewModel Instance => _instance ?? (_instance = new MpAvTagTrayViewModel());

        #endregion

        #region Interfaces
        #region MpIHasDragOverProperty Implementation

        bool MpIHasDragOverProperty.IsDragOver {
            get => IsPinTrayDragOver;
            set => IsPinTrayDragOver = value;
        }
        #endregion

        #region MpITagQueryTools Implementation

        IEnumerable<int> MpITagQueryTools.GetSelfAndAllAncestorTagIds(int tagId) {
            if (Items.FirstOrDefault(x => x.TagId == tagId) is MpAvTagTileViewModel ttvm) {
                return
                    ttvm
                    .SelfAndAllAncestors
                    .Cast<MpAvTagTileViewModel>()
                    .Select(x => x.TagId);
            }
            return null;
        }
        IEnumerable<int> MpITagQueryTools.GetSelfAndAllDescendantsTagIds(int tagId) {
            if (Items.FirstOrDefault(x => x.TagId == tagId) is MpAvTagTileViewModel ttvm) {
                return
                    ttvm
                    .SelfAndAllDescendants
                    .Cast<MpAvTagTileViewModel>()
                    .Select(x => x.TagId);
            }
            return null;
        }
        #endregion

        #region MpISidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = 0;// MpMeasurements.Instance.DefaultTagTreePanelWidth;
        public double SidebarHeight { get; set; }

        public double DefaultSidebarWidth {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return 300;// MpMeasurements.Instance.DefaultTagTreePanelWidth;
                } else {
                    return MpAvMainWindowViewModel.Instance.MainWindowWidth;
                }
            }
        }
        public double DefaultSidebarHeight {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return MpAvClipTrayViewModel.Instance.ObservedQueryTrayScreenHeight;
                } else {
                    return 300;
                }
            }
        }

        public string SidebarBgHexColor =>
            (MpPlatform.Services.PlatformResource.GetResource("TagTreeBgBrush") as IBrush).ToHex();


        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #endregion

        #region Properties

        #region View Models
        public override MpAvTagTileViewModel SelectedItem {
            get {
                return Items.FirstOrDefault(x => x.TagId == SelectedItemId);
            }
            set {
                if (value == null) {
                    // reject no selection
                    return;
                }
                _selectedItemId = value.TagId;
                Items.ForEach(x => x.IsSelected = x.TagId == _selectedItemId);
            }
        }

        public MpAvTagTileViewModel LastSelectedActiveItem =>
            Items
            .Where(x => x.IsNotGroupTag)
            .OrderByDescending(x => x.LastSelectedDateTime)
            .FirstOrDefault();

        public IEnumerable<MpAvTagTileViewModel> PinnedItems =>
            Items.Where(x => x.IsModelPinned).OrderBy(x => x.PinSortIdx);

        public MpAvTagTileViewModel SelectedPinnedItem {
            get => PinnedItems.FirstOrDefault(x => x.IsSelected);
            set {
                if (value != SelectedPinnedItem) {
                    PinnedItems.ForEach(x => x.IsSelected = x == value);
                }
            }
        }

        public IEnumerable<MpAvTagTileViewModel> RootItems =>
            Items.Where(x => x.ParentTagId == 0);

        public MpAvTagTileViewModel AllTagViewModel { get; set; }
        public MpAvTagTileViewModel RootGroupTagViewModel { get; set; }
        public MpAvTagTileViewModel HelpTagViewModel { get; set; }

        public IList<MpMenuItemViewModel> ContentMenuItemViewModels =>
            AllTagViewModel.ContextMenuViewModel.SubItems;

        #endregion

        #region State

        public bool IsAnyDragging =>
            Items.Any(x => x.IsDragging);
        public bool IsAnyPinTagDragging =>
            Items.Any(x => x.IsPinTagDragging);

        public bool IsPinTrayDragOver { get; set; }

        private int _selectedItemId;
        public int SelectedItemId {
            // NOTE only used w/ QueryInfo
            get {
                return _selectedItemId;
            }
            set {
                SelectTagCommand.Execute(value);
            }
        }

        public int LastActiveId =>
            LastSelectedActiveItem == null ? 0 : LastSelectedActiveItem.TagId;

        public bool IsSelecting { get; private set; } = false;
        //public bool IsNavButtonsVisible => MpAvMainWindowViewModel.Instance.IsHorizontalOrientation && 
        //                                    TagTrayTotalWidth > MaxTagTrayScreenWidth;
        public bool IsNavButtonsVisible { get; private set; } = false;

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy) || PinnedItems.Any(x => x.IsBusy);

        #endregion

        #region Layout        
        public double NavButtonSize => 25;
        public double MaxTagTrayScreenWidth {
            get {
                if (MpAvMainWindowViewModel.Instance.IsVerticalOrientation) {
                    // NOTE for now, tag tray is hidden vertically
                    return 0;
                }
                double mww = MpAvMainWindowViewModel.Instance.MainWindowWidth;

                //MpRect ppbw = MpAvClipTrayViewModel.Instance.PlayPauseButtonBounds;
                //MpRect ctsvw = MpAvClipTileSortViewModel.Instance.ClipTileSortViewBounds;
                //MpRect sbvw = MpAvSearchBoxViewModel.Instance.SearchBoxViewBounds;

                //double other_items_width = ppbw.Width + ctsvw.Width + sbvw.Width;
                //double total_filter_width = other_items_width + TagTrayTotalWidth;
                //if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ||
                //    total_filter_width <= mww) {
                //    return mww - other_items_width;
                //}
                //return double.NaN;
                var fmvm = MpAvFilterMenuViewModel.Instance;
                double ctsvw = fmvm.ObservedSortViewWidth;
                double sbvw = fmvm.ObservedSearchBoxWidth;

                double ttsw = mww - ctsvw - sbvw;

                //IsNavButtonsVisible = TagTrayTotalWidth > ttsw;
                //if(IsNavButtonsVisible) {
                //    MpConsole.WriteLine("nav button visible");
                //    ttsw -= (NavButtonSize * 2);
                //} else {
                //    MpConsole.WriteLine("nav button hidden");
                //}
                return ttsw;
            }
        }
        //public double TagTrayTotalWidth => PinnedItems.Sum(x => x.TagTileTrayWidth);
        public double TagTrayTotalWidth { get; set; }

        public MpRect TagTrayBounds { get; set; }
        #endregion

        #region Appearance

        #endregion

        #region Business Logic

        public int DefaultTagId {
            get {
                return MpTag.AllTagId;
            }
        }

        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors

        private static int createCount = 0;
        public MpAvTagTrayViewModel() : base(null) {
            createCount++;
            PropertyChanged += MpTagTrayViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitAsync() {
            IsBusy = true;

            MpPlatform.Services.TagQueryTools = this;

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            MpDb.SyncAdd += MpDbObject_SyncAdd;
            MpDb.SyncUpdate += MpDbObject_SyncUpdate;
            MpDb.SyncDelete += MpDbObject_SyncDelete;

            MpTag allTag = await MpDataModelProvider.GetItemAsync<MpTag>(MpTag.AllTagId);

            AllTagViewModel = new MpAvTagTileViewModel(this);
            await AllTagViewModel.InitializeAsync(allTag);

            MpTag helpTag = await MpDataModelProvider.GetItemAsync<MpTag>(MpTag.HelpTagId);

            HelpTagViewModel = new MpAvTagTileViewModel(this);
            await HelpTagViewModel.InitializeAsync(helpTag);

            MpTag rootGroupTag = await MpDataModelProvider.GetItemAsync<MpTag>(MpTag.RootGroupTagId);

            RootGroupTagViewModel = new MpAvTagTileViewModel(this);
            await RootGroupTagViewModel.InitializeAsync(rootGroupTag);

            while (AllTagViewModel.IsBusy || HelpTagViewModel.IsBusy || RootGroupTagViewModel.IsBusy) {
                await Task.Delay(100);
            }

            Items.CollectionChanged += TagTileViewModels_CollectionChanged;

            AllTagViewModel.IsExpanded = true;
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(PinnedItems));
            OnPropertyChanged(nameof(RootItems));

            IsBusy = false;
        }

        private void TagTileViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {

        }

        private void PinnedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(MaxTagTrayScreenWidth));
        }


        public void ClearTagEditing() {
            Items.ForEach(x => x.IsTagNameReadOnly = true);
            //PinnedItems.ForEach(x => x.IsTagNameReadOnly = true);
        }
        public void ClearTagSelection() {
            ClearTagEditing();
            Items.ForEach(x => x.IsSelected = false);
            //PinnedItems.ForEach(x => x.IsSelected = false);
        }

        public void ResetTagSelection() {
            if (SelectedItem == null) {
                return;
            }
            if (SelectedItem.TagId != DefaultTagId) {
                ClearTagSelection();
                Items.Where(x => x.TagId == DefaultTagId).FirstOrDefault().IsSelected = true;
            }
        }


        #endregion

        #region Protected Methods

        #region Db Events
        #endregion

        #endregion
        #region Private Methods

        private void MpTagTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(RootItems));
                    break;
                case nameof(SelectedItem):
                    if (SelectedItem == null) {
                        Debugger.Break();
                        SelectedItem = AllTagViewModel;
                    }
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsActiveTag)));
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowSizeChanged:
                case MpMessageType.MainWindowOrientationChangeEnd:
                    OnPropertyChanged(nameof(MaxTagTrayScreenWidth));
                    OnPropertyChanged(nameof(TagTrayTotalWidth));
                    break;
                //case MpMessageType.MainWindowOpened:

                //    OnPropertyChanged(nameof(PinnedItems));
                //    OnPropertyChanged(nameof(MaxTagTrayScreenWidth));
                //    break;

                case MpMessageType.TraySelectionChanged:
                    HandleClipTraySelectionChange();//.FireAndForgetSafeAsync(this);
                    break;

                case MpMessageType.QueryChanged:
                    if (SelectedItem == null || !SelectedItem.IsQueryTag) {
                        break;
                    }
                    SelectedItem.SortType = MpAvClipTileSortFieldViewModel.Instance.SelectedSortType;
                    SelectedItem.IsSortDescending = MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending;
                    break;
            }
        }

        private void HandleClipTraySelectionChange() {
            var ctrvm = MpAvClipTrayViewModel.Instance;

            if (ctrvm.SelectedItem == null) {
                Items.ForEach(x => x.IsLinkedToSelectedClipTile = false);
                return;
            }

            var tag_ids_for_selected_copy_item = MpDataModelProvider.GetTagIdsForCopyItem(ctrvm.SelectedItem.CopyItemId);
            Items.ForEach(x => x.UpdateLinkToSelectedClipTile(tag_ids_for_selected_copy_item));

            // Notify clip tray context menu changed if was selected w/ right click
            MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.ContextMenuViewModel));
        }
        #region Model Sync Events
        private void MpDbObject_SyncDelete(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            Dispatcher.UIThread.Post(() => {
                if (sender is MpTag t) {
                    var ttvmToRemove = Items.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
                    if (ttvmToRemove != null) {
                        ttvmToRemove.Tag.StartSync(e.SourceGuid);
                        ttvmToRemove.DeleteThisTagCommand.Execute(null);
                        ttvmToRemove.Tag.EndSync();
                    }
                }
            });
        }

        private void MpDbObject_SyncUpdate(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            //Dispatcher.UIThread.Post(() => {
            //});
        }

        private void MpDbObject_SyncAdd(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            // Dispatcher.UIThread.Post(async() => {
            //if (sender is MpTag t) {
            //    t.StartSync(e.SourceGuid);
            //    var dupCheck = Items.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
            //    if (dupCheck == null) {
            //        var ttvm = await CreateTagTileViewModel(t);
            //        Items.Add(ttvm);
            //    } else {
            //        MpConsole.WriteTraceLine(@"Warning, attempting to add existing tag: " + dupCheck.TagName + " ignoring and updating existing.");
            //        dupCheck.Tag = t;
            //    }
            //    t.EndSync();
            //}
            // });
        }

        #endregion

        #endregion

        #region Commands

        public ICommand ToggleTileIsPinnedCommand => new MpCommand<object>(
            (args) => {
                var ttvm = args as MpAvTagTileViewModel;
                if (ttvm == null) {
                    return;
                }
                if (ttvm.IsModelPinned) {
                    // unpin
                    ttvm.PinSortIdx = -1;
                } else {
                    // pin
                    ttvm.PinSortIdx = PinnedItems.Count() - 1;
                }
                OnPropertyChanged(nameof(ttvm.IsModelPinned));

                OnPropertyChanged(nameof(PinnedItems));
                OnPropertyChanged(nameof(IsNavButtonsVisible));
                OnPropertyChanged(nameof(MaxTagTrayScreenWidth));
            }, (args) => {
                return args is MpAvTagTileViewModel ttvm && ttvm.CanPin;
            });

        public ICommand SelectTagCommand => new MpCommand<object>(
            async (args) => {
                IsSelecting = true;

                int tagId;
                if (args == null) {
                    tagId = DefaultTagId;
                } else if (args is MpAvTagTileViewModel ttvm) {
                    tagId = ttvm.TagId;
                } else if (args is int) {
                    tagId = (int)args;
                } else if (args is string shorcutCommandParamTagIdStr &&
                    int.Parse(shorcutCommandParamTagIdStr) is int cmdTagId) {
                    // from shortcut
                    tagId = cmdTagId;
                } else {
                    Debugger.Break();
                    tagId = MpTag.AllTagId;
                }
                if (Items.FirstOrDefault(x => x.TagId == tagId) is MpAvTagTileViewModel sttvm &&
                    !sttvm.CanSelect) {
                    MpConsole.WriteLine($"Rejecting select tag '{sttvm}'.");
                    IsSelecting = false;
                    return;
                }

                ClearTagSelection();

                _selectedItemId = tagId;
                Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));

                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(SelectedPinnedItem));

                if (SelectedItem != null) {
                    SelectedItem.AllAncestors.ForEach(x => x.IsExpanded = true);
                }

                //if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                //    // last loaded tag is selected in ClipTray OnPostMainWindowLoaded 
                //    // which notifies query changed so don't notify
                //    IsSelecting = false;
                //    return;
                //}

                MpMessenger.SendGlobal(MpMessageType.TagSelectionChanged);

                if (!MpAvMainWindowViewModel.Instance.IsMainWindowLoading &&
                    SelectedItem != null && SelectedItem.IsNotGroupTag) {
                    //if (SelectedItem.IsQueryTag) {
                    //    MpAvSearchCriteriaItemCollectionViewModel.Instance
                    //        .SelectAdvancedSearchCommand.Execute(SelectedItem.TagId);
                    //} else {
                    //    MpPlatform.Services.Query.NotifyQueryChanged(true);
                    //}
                    MpPlatform.Services.Query.NotifyQueryChanged();
                    while (!MpPlatform.Services.Query.CanRequery) {
                        await Task.Delay(100);
                    }
                }
                IsSelecting = false;
            },
            (args) => {
                return args != null && !IsSelecting;
            });


        #endregion
    }
}
