using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common;
using System.Diagnostics;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    public class MpAvTagTrayViewModel : 
        MpAvSelectorViewModelBase<object,MpAvTagTileViewModel>, 
        MpIAsyncSingletonViewModel<MpAvTagTrayViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpIOrientedSidebarItemViewModel,
        MpIQueryInfoProvider {
        #region Private Variables
        
        private bool _isSelecting = false;

        #endregion

        #region Statics


        private static MpAvTagTrayViewModel _instance;
        public static MpAvTagTrayViewModel Instance => _instance ?? (_instance = new MpAvTagTrayViewModel());

        #endregion

        #region Properties

        #region View Models

        //public ObservableCollection<MpAvTagTileViewModel> PinnedItems { get; set; } = new ObservableCollection<MpAvTagTileViewModel>();
        public IEnumerable<MpAvTagTileViewModel> PinnedItems => Items.Where(x => x.IsModelPinned).OrderBy(x=>x.TagTraySortIdx);

        public MpAvTagTileViewModel SelectedPinnedItem {
            get => PinnedItems.FirstOrDefault(x => x.IsSelected);
            set {
                if (value != SelectedPinnedItem) {
                    //Items.ForEach(x => x.IsSelected = false);
                    //if (value != null) {
                    //    value.IsSelected = true;
                    //}
                    PinnedItems.ForEach(x => x.IsSelected = x == value);
                }
}
        }

        public IEnumerable<MpAvTagTileViewModel> RootItems => Items.Where(x => x.ParentTagId == 0);

        public MpAvTagTileViewModel AllTagViewModel { get; set; }
        public MpAvTagTileViewModel HelpTagViewModel { get; set; }

        public IList<MpMenuItemViewModel> ContentMenuItemViewModels => AllTagViewModel.ContextMenuViewModel.SubItems;

        #endregion

        #region MpIQueryInfoProvider Implementation
        public void RestoreQueryInfo() {
            SelectTagCommand.Execute(MpAvQueryInfoViewModel.Current.TagId);
        }

        public void SetQueryInfo() {
            if(SelectedItem == null) {
                // default to all 
                MpAvQueryInfoViewModel.Current.TagId = AllTagViewModel.TagId;
            } else {
                MpAvQueryInfoViewModel.Current.TagId = SelectedItem.TagId;
            }
            
        }

        #endregion

        #region MpISidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = 0;// MpMeasurements.Instance.DefaultTagTreePanelWidth;
        public double SidebarHeight { get; set; }
        
        public double DefaultSidebarWidth {
            get {
                if(MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return 300;// MpMeasurements.Instance.DefaultTagTreePanelWidth;
                } else {
                    return MpAvMainWindowViewModel.Instance.MainWindowWidth;
                }
            }
        }
        public double DefaultSidebarHeight {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return MpAvClipTrayViewModel.Instance.ClipTrayScreenHeight;
                } else {
                    return 300;
                }
            }
        }
        public bool IsSidebarVisible { get; set; }

        public MpISidebarItemViewModel NextSidebarItem { get; }
        public MpISidebarItemViewModel PreviousSidebarItem { get; }
        

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region State

        //public bool IsNavButtonsVisible => MpAvMainWindowViewModel.Instance.IsHorizontalOrientation && 
        //                                    TagTrayTotalWidth > TagTrayScreenWidth;
        public bool IsNavButtonsVisible { get; private set; } = false;

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy) || PinnedItems.Any(x=>x.IsBusy);

        #endregion

        #region Layout        
        public double NavButtonSize => 25;
        public double TagTrayScreenWidth {
            get {
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
                double ppbw = MpAvClipTrayViewModel.Instance.PlayPauseButtonWidth;
                double ctsvw = MpAvClipTileSortViewModel.Instance.ClipTileSortViewWidth;
                double sbvw = MpAvSearchBoxViewModel.Instance.SearchBoxViewWidth;

                double ttsw = mww - ppbw - ctsvw - sbvw;

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

        public string TagTreeBgOctColor => MpSystemColors.cyan1.AdjustAlpha(MpPrefViewModel.Instance.MainWindowOpacity);
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


        public MpAvTagTrayViewModel() : base(null) {
            PropertyChanged += MpTagTrayViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitAsync() {
            IsBusy = true;

            MpAvQueryInfoViewModel.Current.RegisterProvider(this);

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            MpDb.SyncAdd += MpDbObject_SyncAdd;
            MpDb.SyncUpdate += MpDbObject_SyncUpdate;
            MpDb.SyncDelete += MpDbObject_SyncDelete;

            MpTag allTag = await MpDb.GetItemAsync<MpTag>(MpTag.AllTagId);

            AllTagViewModel = new MpAvTagTileViewModel(this);
            await AllTagViewModel.InitializeAsync(allTag);

            MpTag helpTag = await MpDb.GetItemAsync<MpTag>(MpTag.HelpTagId);

            HelpTagViewModel = new MpAvTagTileViewModel(this);
            await HelpTagViewModel.InitializeAsync(helpTag);

            while (AllTagViewModel.IsBusy || HelpTagViewModel.IsBusy) {
                await Task.Delay(100);
            }

            //foreach (var ttvm in Items.Where(x => x.Tag.IsPinned).OrderBy(x => x.TagTraySortIdx)) {
            //    var pttvm = new MpAvTagTileViewModel(this);
            //    await pttvm.InitializeAsync(ttvm.Tag);
            //    pttvm.ParentTreeItem = ttvm.ParentTreeItem;
            //    PinnedItems.Add(pttvm);
            //}

            Items.CollectionChanged += TagTileViewModels_CollectionChanged;
            //PinnedItems.CollectionChanged += PinnedItems_CollectionChanged;
            //UpdateSortOrder(true);

            Items.FirstOrDefault(x => x.TagId == DefaultTagId).IsSelected = true;

            AllTagViewModel.IsExpanded = true;

            SelectTagCommand.Execute(DefaultTagId);


            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(PinnedItems));
            OnPropertyChanged(nameof(RootItems));

            IsBusy = false;
        }
        private void TagTileViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateTreeSortOrder();
            //OnPropertyChanged(nameof(RootItems));
        }

        private void PinnedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateTraySortOrder();
            OnPropertyChanged(nameof(TagTrayScreenWidth));
        }

        public void UpdateTreeSortOrder(bool fromModel = false) {
            if (fromModel) {
                //Items.Sort(x => x.TagSortIdx);
            } else {
                foreach (var ttvm in Items) {
                    ttvm.TagSortIdx = Items.IndexOf(ttvm);
                }
            }
        }
        public void UpdateTraySortOrder(bool fromModel = false) {
            if (fromModel) {
                //Items.Sort(x => x.TagSortIdx);
            } else {
                foreach (var ttvm in PinnedItems) {
                    ttvm.TagTraySortIdx = PinnedItems.IndexOf(ttvm);
                }
            }
        }


        public void ClearTagEditing() {
            Items.ForEach(x => x.IsTagNameReadOnly = true);
            PinnedItems.ForEach(x => x.IsTagNameReadOnly = true);
        }
        public void ClearTagSelection() {
            ClearTagEditing();
            Items.ForEach(x => x.IsSelected = false);
            PinnedItems.ForEach(x => x.IsSelected = false);
        }

        public void ResetTagSelection() {
            if(SelectedItem == null) {
                return;
            }
            if(SelectedItem.TagId != DefaultTagId) {
                ClearTagSelection();
                Items.Where(x => x.TagId == DefaultTagId).FirstOrDefault().IsSelected = true;
            }            
        }


        #endregion

        #region Private Methods

        private void MpTagTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSidebarVisible):
                    if(IsSidebarVisible) {
                        MpAnalyticItemCollectionViewModel.Instance.IsSidebarVisible = false;
                        MpActionCollectionViewModel.Instance.IsSidebarVisible = false;
                        MpAvClipboardHandlerCollectionViewModel.Instance.IsSidebarVisible = false;
                    }
                    MpAvMainWindowViewModel.Instance.OnPropertyChanged(nameof(MpAvMainWindowViewModel.Instance.SelectedSidebarItemViewModel));
                    break;
                case nameof(IsSelected):
                    if(IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(RootItems));
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowSizeChanged:
                case MpMessageType.MainWindowOrientationChangeEnd:
                    OnPropertyChanged(nameof(TagTrayScreenWidth));
                    OnPropertyChanged(nameof(TagTrayTotalWidth));
                    break;
                //case MpMessageType.MainWindowOpened:
                    
                //    OnPropertyChanged(nameof(PinnedItems));
                //    OnPropertyChanged(nameof(TagTrayScreenWidth));
                //    break;

                case MpMessageType.TraySelectionChanged:
                    HandleClipTraySelectionChange();//.FireAndForgetSafeAsync(this);
                    break;
            }
        }

        private void HandleClipTraySelectionChange() {
            var ctrvm = MpAvClipTrayViewModel.Instance;

            if(ctrvm.SelectedItem == null) {
                Items.ForEach(x => x.IsLinkedToSelectedClipTile = false);
                return;
            }

            var tag_ids_for_selected_copy_item = MpDataModelProvider.GetTagIdsForCopyItem(ctrvm.SelectedItem.CopyItemId);
            Items.ForEach(x => x.UpdateLinkToSelectedClipTile(tag_ids_for_selected_copy_item));

            // Notify clip tray context menu changed if was selected w/ right click
            MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.ContextMenuViewModel));
        }


        #region Db Events

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                
            }
        }

        #endregion

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
            async(args) => {
                var ttvm = args as MpAvTagTileViewModel;
                if(ttvm == null) {
                    return;
                }
                ttvm.IsModelPinned = !ttvm.IsModelPinned;
                if(ttvm.IsModelPinned) {
                    ttvm.TagTraySortIdx = PinnedItems.Count() - 1;
                } else {
                    ttvm.TagTraySortIdx = -1;
                }
                //if(wasPinned) {
                //    var ttvm_toRemove = PinnedItems.FirstOrDefault(x => x.TagId == ttvm.TagId);
                //    PinnedItems.Remove(ttvm_toRemove);
                //    var tree_ttvm = Items.FirstOrDefault(x => x.TagId == ttvm.TagId);
                //    if(tree_ttvm == null) {
                //        Debugger.Break();
                //    }
                //    tree_ttvm.IsModelPinned = false;
                //} else {
                //    var pttvm = new MpAvTagTileViewModel(this);
                //    await pttvm.InitializeAsync(ttvm.Tag, false);
                //    pttvm.ParentTreeItem = ttvm.ParentTreeItem;
                //    pttvm.TagClipCount = ttvm.TagClipCount;
                //    pttvm.CopyItemIdsNeedingView = ttvm.CopyItemIdsNeedingView;
                //    PinnedItems.Add(pttvm);
                //    pttvm.IsModelPinned = true;
                //}

                OnPropertyChanged(nameof(PinnedItems));
                OnPropertyChanged(nameof(IsNavButtonsVisible));
                OnPropertyChanged(nameof(TagTrayScreenWidth));
            });

        public ICommand SelectTagCommand => new MpCommand<object>(
            (args) => {
                _isSelecting = true;

                int tagId;
                if(args == null) {
                    tagId = DefaultTagId;
                } else if(args is MpAvTagTileViewModel ttvm) {
                    tagId = ttvm.TagId;
                } else if(args is int){
                    tagId = (int)args;
                } else {
                    Debugger.Break();
                    tagId = MpTag.AllTagId;
                }
                ClearTagSelection();

                Items.ForEach(x => x.IsSelected = x.TagId == tagId);
                PinnedItems.ForEach(x => x.IsSelected = x.TagId == tagId);

                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(SelectedPinnedItem));

                if(MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    // last loaded tag is selected in ClipTray OnPostMainWindowLoaded 
                    // which notifies query changed so don't notify
                    _isSelecting = false;
                    return;
                }
                SetQueryInfo();
                _isSelecting = false;

                MpAvQueryInfoViewModel.Current.NotifyQueryChanged();
            },
            (args)=>args != null && !_isSelecting);


        #endregion
    }
}
