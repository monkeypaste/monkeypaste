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
using MonoMac.AppKit;

namespace MonkeyPaste.Avalonia {
    public class MpAvTagTrayViewModel : 
        MpAvSelectorViewModelBase<object,MpAvTagTileViewModel>, 
        MpIAsyncSingletonViewModel<MpAvTagTrayViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpISidebarItemViewModel,
        MpIQueryInfoValueProvider {
        #region Private Variables
        

        #endregion

        #region Statics


        private static MpAvTagTrayViewModel _instance;
        public static MpAvTagTrayViewModel Instance => _instance ?? (_instance = new MpAvTagTrayViewModel());

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

        public IEnumerable<MpAvTagTileViewModel> PinnedItems => Items.Where(x => x.IsModelPinned).OrderBy(x=>x.PinSortIdx);

        public MpAvTagTileViewModel SelectedPinnedItem {
            get => PinnedItems.FirstOrDefault(x => x.IsSelected);
            set {
                if (value != SelectedPinnedItem) {
                    PinnedItems.ForEach(x => x.IsSelected = x == value);
                }
}
        }

        public IEnumerable<MpAvTagTileViewModel> RootItems => Items.Where(x => x.ParentTagId == 0);

        public MpAvTagTileViewModel AllTagViewModel { get; set; }
        public MpAvTagTileViewModel RootGroupTagViewModel { get; set; }
        public MpAvTagTileViewModel HelpTagViewModel { get; set; }

        public IList<MpMenuItemViewModel> ContentMenuItemViewModels => AllTagViewModel.ContextMenuViewModel.SubItems;

        #endregion

        #region MpIQueryInfoProvider Implementation
        object MpIQueryInfoValueProvider.Source => this;
        string MpIQueryInfoValueProvider.SourcePropertyName => nameof(SelectedItemId);

        string MpIQueryInfoValueProvider.QueryValueName => nameof(MpPlatform.Services.QueryInfo.TagId);

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


        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region State

        private int _selectedItemId;
        public int SelectedItemId {
            // NOTE only used w/ QueryInfo
            get {
                return _selectedItemId;
            }
            set {
                //if(SelectedItemId != value) {
                //    if(value <= 0) {
                //        SelectedItem = null;
                //    }
                //}
                SelectTagCommand.Execute(value);
            }
        }

        public bool IsSelectionEnabled {
            get {
                if(MpAvSearchCriteriaItemCollectionViewModel.Instance.HasCriteriaItems) {
                    return false;
                }
                return true;
            }
        }
        public bool IsSelecting { get; private set; } = false;
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
                var fmvm = MpAvFilterMenuViewModel.Instance;
                double ppbw = fmvm.PlayPauseButtonWidth;
                double ctsvw = fmvm.ClipTileSortViewWidth;
                double sbvw = fmvm.SearchBoxObservedWidth;

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

            MpPlatform.Services.QueryInfo.RegisterProvider(this);

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
                    ttvm.TreeSortIdx = Items.IndexOf(ttvm);
                }
            }
        }
        public void UpdateTraySortOrder(bool fromModel = false) {
            if (fromModel) {
                //Items.Sort(x => x.TagSortIdx);
            } else {
                foreach (var ttvm in PinnedItems) {
                    ttvm.PinSortIdx = PinnedItems.IndexOf(ttvm);
                }
            }
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
                //case nameof(IsSidebarVisible):
                //    if(IsSidebarVisible) {
                //        MpAvAnalyticItemCollectionViewModel.Instance.IsSidebarVisible = false;
                //        MpAvTriggerCollectionViewModel.Instance.IsSidebarVisible = false;
                //        MpAvClipboardHandlerCollectionViewModel.Instance.IsSidebarVisible = false;
                //    }
                //    MpAvMainWindowViewModel.Instance.OnPropertyChanged(nameof(MpAvMainWindowViewModel.Instance.SelectedSidebarItemViewModel));
                //    break;
                case nameof(IsSelected):
                    if(IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(RootItems));
                    break;
                case nameof(SelectedItem):
                    if(SelectedItem == null) {
                        //Debugger.Break();
                        SelectedItem = AllTagViewModel;
                    }
                    break;
                case nameof(IsSelectionEnabled):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.CanSelect)));
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
                case MpMessageType.SearchCriteriaItemsChanged:
                    OnPropertyChanged(nameof(IsSelectionEnabled));
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
            (args) => {
                var ttvm = args as MpAvTagTileViewModel;
                if(ttvm == null) {
                    return;
                }
                if(ttvm.IsModelPinned) {
                    // unpin
                    ttvm.PinSortIdx = -1;
                } else {
                    // pin
                    ttvm.PinSortIdx = PinnedItems.Count() - 1;
                }
                OnPropertyChanged(nameof(ttvm.IsModelPinned));

                OnPropertyChanged(nameof(PinnedItems));
                OnPropertyChanged(nameof(IsNavButtonsVisible));
                OnPropertyChanged(nameof(TagTrayScreenWidth));
            });

        public ICommand SelectTagCommand => new MpCommand<object>(
            (args) => {
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
                }
                else {
                    Debugger.Break();
                    tagId = MpTag.AllTagId;
                }
                ClearTagSelection();

                _selectedItemId = tagId;
                Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));

                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(SelectedPinnedItem));

                if(SelectedItem != null) {
                    SelectedItem.AllAncestors.ForEach(x => x.IsExpanded = true);
                }

                if(MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    // last loaded tag is selected in ClipTray OnPostMainWindowLoaded 
                    // which notifies query changed so don't notify
                    IsSelecting = false;
                    return;
                }
                IsSelecting = false;

                MpPlatform.Services.QueryInfo.NotifyQueryChanged(true);
            },
            (args)=>args != null && !IsSelecting && IsSelectionEnabled);


        #endregion
    }
}
