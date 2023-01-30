using Avalonia.Layout;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvClipTrayCollectionViewModel : 
        MpViewModelBase,
        MpIBootstrappedItem,
        MpIContentQueryTools {
        #region Private Variable

        private List<MpCopyItem> _newModels = new List<MpCopyItem>();

        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIContentQueryTools Implementation

        IEnumerable<int> MpIContentQueryTools.GetOmittedContentIds() =>
            PinCollection.Items.Select(x => x.CopyItemId);

        #endregion

        #region MpIBootstrappedItem Implementation

        string MpIBootstrappedItem.Label => "Content Tray";
        #endregion

        #endregion

        #region Properties

        #region View Models

        private MpAvCopyItemCollectionViewModel _pinCollection;
        public MpAvCopyItemCollectionViewModel PinCollection {
            get {
                if(_pinCollection == null) {
                    _pinCollection = new MpAvCopyItemCollectionViewModel(this);
                }
                return _pinCollection;
            }
        }

        private MpAvCopyItemCollectionViewModel _queryCollection;
        public MpAvCopyItemCollectionViewModel QueryCollection {
            get {
                if (_queryCollection == null) {
                    _queryCollection = new MpAvCopyItemCollectionViewModel(this);
                }
                return _queryCollection;
            }
        }

        private IEnumerable<MpAvCopyItemCollectionViewModel> _collections;
        public IEnumerable<MpAvCopyItemCollectionViewModel> Collections {
            get {
                if(_collections == null) {
                    _collections = new List<MpAvCopyItemCollectionViewModel>() {
                        PinCollection,
                        QueryCollection
                    };
                }
                return _collections;
            }
        }

        public IEnumerable<MpAvClipTileViewModel> AllItems =>
            PinCollection.Items.Union(QueryCollection.Items);

        public MpAvClipTileViewModel SelectedItem {
            get {
                return SelectedCollection == null ? null : SelectedCollection.SelectedItem;
            }
            set {
                if(SelectedItem != value) {
                    if(value == null) {
                        Collections.ForEach(x => x.SelectedItem = null);
                        OnPropertyChanged(nameof(SelectedItem));
                        return;
                    }
                    if(!SelectedCollection.Items.Contains(value)) {
                        SelectedCollection = Collections.FirstOrDefault(x => x.Items.Contains(value));
                    }
                    if(SelectedCollection == null) {
                        // whats going on? tile IsSelected funkiness?
                        Debugger.Break();
                        return;
                    }
                    SelectedCollection.SelectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        private MpAvCopyItemCollectionViewModel _selectedCollection;
        public MpAvCopyItemCollectionViewModel SelectedCollection {
            get {
                if(_selectedCollection == null) {
                    return PinCollection;
                }
                return _selectedCollection;
            }
            set {
                if(_selectedCollection != value) {
                    _selectedCollection = value;
                    OnPropertyChanged(nameof(SelectedCollection));
                }
            }
        }

        public MpAvClipTileViewModel ModalClipTileViewModel { get; private set; }

        public MpAvClipTileViewModel AppendClipTileViewModel => AllItems.FirstOrDefault(x => x.IsAppendNotifier);


        #endregion

        #region Layout

        public double MinClipOrPinTrayScreenWidth => 50;
        public double MinClipOrPinTrayScreenHeight => 50;
        public double ObservedContainerScreenWidth { get; set; }
        public double ObservedContainerScreenHeight { get; set; }
        public double ObservedPinTrayScreenWidth { get; set; }
        public double ObservedPinTrayScreenHeight { get; set; }
        public double DefaultPinTrayWidth => DefaultItemWidth * 1.4;

        public double DesiredPinTrayWidth { get; set; }
        public double DesiredPinTrayHeight { get; set; }

        public double MinPinTrayScreenWidth =>
            MinClipOrPinTrayScreenWidth;
        public double MinPinTrayScreenHeight =>
            MinClipOrPinTrayScreenHeight;

        public double MaxPinTrayScreenWidth {
            get {
                if (ListOrientation == Orientation.Horizontal) {

                    return ObservedContainerScreenWidth - MinClipTrayScreenWidth;
                }
                return double.PositiveInfinity;
            }
        }
        public double MaxPinTrayScreenHeight {
            get {
                if (ListOrientation == Orientation.Horizontal) {

                    return double.PositiveInfinity;
                }
                return ObservedContainerScreenHeight - MinClipTrayScreenHeight;
            }
        }
        #endregion

        #region State

        public bool IsArrowSelecting { get; set; } = false;
        public bool IsAnyBusy => 
            IsBusy || 
            ModalClipTileViewModel.IsAnyBusy || 
            Collections.Any(x => x.IsAnyBusy);

        public bool IsInitialQuery { get; set; }
        public bool IsAddingClipboardItem { get; private set; } = false;

        public bool IsAppPaused { get; set; } = false;

        public bool IsAnyTilePinned =>
            PinCollection.ActiveItems.Count() > 0;

        public bool IsQueryTrayEmpty =>
            QueryCollection.IsTrayEmpty;

        private bool _isPasting = false;
        public bool IsPasting {
            get {
                if (_isPasting) {
                    return true;
                }
                if (AllItems.Any(x => x.IsPasting)) {
                    // NOTE since copy items can be pasted from hot key and aren't in tray
                    // IsPasting cannot be auto-property
                    _isPasting = true;
                }
                return _isPasting;
            }
            set {
                if (IsPasting != value) {
                    _isPasting = value;
                    OnPropertyChanged(nameof(IsPasting));
                }
            }
        }

        public bool IsQueryAllPinned {
            get {
                if (!IsQueryTrayEmpty) {
                    return false;
                }
                if (MpAvTagTrayViewModel.Instance.SelectedItem == null) {
                    return false;
                }
                if (MpAvSearchCriteriaItemCollectionViewModel.Instance.HasCriteriaItems) {
                    if (MpAvSearchCriteriaItemCollectionViewModel.Instance.PendingQueryTagId > 0) {
                        return false;
                    }

                }
                return
                    MpAvTagTrayViewModel.Instance.SelectedItem
                            .LinkedCopyItemIds.Count() > 0;
            }
        }

        // this is to help keep new items added pin tray visible when created
        // but avoid overriding user splitter changes DURING one of their workflows
        // and presuming that unless the window hides its still a workflow
        public bool HasUserAlteredPinTrayWidthSinceWindowShow { get; set; } = false;


        public int PersistantSelectedItemId {
            get {
                if (SelectedItem == null) {
                    if (MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Count == 0) {
                        return -1;
                    }
                    return MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels[0].Id;
                }
                return SelectedItem.CopyItemId;
            }
        }
        #region Append
        public List<MpCopyItem> PendingNewModels => _newModels;
        public MpClipboardModeFlags ClipboardModeFlags {
            get {
                MpClipboardModeFlags cmf = MpClipboardModeFlags.None;
                if (!IsAppPaused) {
                    cmf |= MpClipboardModeFlags.ListeningForChanges;
                }
                if (IsAppendLineMode) {
                    cmf |= MpClipboardModeFlags.AppendBlock;
                }
                if (IsAppendMode) {
                    cmf |= MpClipboardModeFlags.AppendInline;
                }
                if (IsRightClickPasteMode) {
                    cmf |= MpClipboardModeFlags.RightClickPaste;
                }
                if (IsAutoCopyMode) {
                    cmf |= MpClipboardModeFlags.AutoCopy;
                }

                return cmf;
            }
        }

        public string AppendData { get; private set; } = null;

        private MpAppendModeFlags _appendModeFlags = MpAppendModeFlags.None;
        public MpAppendModeFlags AppendModeStateFlags {
            get => _appendModeFlags;
            set {
                UpdateAppendModeStateFlags(value, "property");
            }
        }


        public bool IsAppendMode { get; set; }

        public bool IsAppendLineMode { get; set; }

        public bool IsAppendManualMode { get; set; }

        public bool IsAnyAppendMode => IsAppendMode || IsAppendLineMode;

        #region Helpers
        private bool IsCopyItemAppendable(MpCopyItem ci) {
            if (ci == null || ci.Id < 1 || ci.ItemType == MpCopyItemType.Image) {
                return false;
            }
            if (ModalClipTileViewModel.IsPlaceholder) {
                return true;
            }
            return ModalClipTileViewModel.ItemType == ci.ItemType;
        }
        private async Task AssignAppendClipTileAsync() {
            // use cases
            // 1. app is hidden and user hits hot key or enables in system tray
            //  1.1 no new clipboard items have been created 
            //      -wait till appendable item is created to assign
            //  1.2 there's N new items 
            //      -only assign if most recent item is appendable
            // 2. app is open and user enables append
            //  2.1 no items are selected
            //      -wait till appendable item is created to assign
            //  2.2 selected item is not appendable
            //      -wait till appendable item is created to assign
            //  2.3 selected item is appendable
            //      -assign selected item

            Dispatcher.UIThread.VerifyAccess();
            if (!ModalClipTileViewModel.IsPlaceholder) {
                return;
            }
            MpAvClipTileViewModel append_ctvm = null;
            int append_ciid = 0;
            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                if (SelectedItem != null && IsCopyItemAppendable(SelectedItem.CopyItem)) {
                    append_ctvm = SelectedItem;
                    append_ciid = SelectedItem.CopyItemId;
                }
            } else if (PendingNewModels.Count > 0) {
                var most_recent_ci = PendingNewModels[PendingNewModels.Count - 1];
                if (IsCopyItemAppendable(most_recent_ci)) {
                    append_ciid = most_recent_ci.Id;
                }
            } else {
                // activate w/o item and wait (show AppMode change msg)
            }

            if (append_ciid > 0) {
                var append_ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(append_ciid);
                await ModalClipTileViewModel.InitializeAsync(append_ci);
                while (IsBusy) {
                    await Task.Delay(100);
                }
                if (append_ctvm != null) {
                    // pin (or move to front) if exists 
                    //PinTileCommand.Execute(append_ctvm);
                    append_ctvm.OnPropertyChanged(nameof(append_ctvm.IsAppendTrayItem));
                }
            }

            //OnPropertyChanged(nameof(ModalClipTileViewModel));
        }

        private void UpdateAppendModeStateFlags(MpAppendModeFlags flags, string source) {
            IsAppendLineMode = flags.HasFlag(MpAppendModeFlags.AppendLine);
            IsAppendMode = flags.HasFlag(MpAppendModeFlags.Append);
            IsAppendManualMode = flags.HasFlag(MpAppendModeFlags.Manual);
            _appendModeFlags = flags;
            OnPropertyChanged(nameof(AppendModeStateFlags));
        }

        private async Task ActivateAppendModeAsync(bool isAppendLine, bool isManualMode) {
            Dispatcher.UIThread.VerifyAccess();
            while (IsAddingClipboardItem) {
                // if new item is being added, its important to wait for it
                await Task.Delay(100);
            }

            if (ModalClipTileViewModel.IsPlaceholder) {
                // append mode was just toggled ON (param was null)
                await AssignAppendClipTileAsync();
            }
            bool was_append_already_enabled = IsAnyAppendMode && !ModalClipTileViewModel.IsPlaceholder;

            MpAppendModeFlags amf = MpAppendModeFlags.None;
            if (isAppendLine) {
                amf |= MpAppendModeFlags.AppendLine;
            } else {
                amf |= MpAppendModeFlags.Append;
            }
            if (isManualMode) {
                amf |= MpAppendModeFlags.Manual;
            }
            UpdateAppendModeStateFlags(amf, "command");

            MpAppendNotificationViewModel.Instance.OnPropertyChanged(nameof(MpAppendNotificationViewModel.Instance.Title));

            if (was_append_already_enabled) {
                // don't trigger if already activated, the AppendDataChanged() timesout because IsContentLoaded doesn't goto false
                return;
            }

            if (ModalClipTileViewModel.IsPlaceholder) {
                // no item assigned yet so just show enable message
                MpNotificationBuilder.ShowMessageAsync(
                       title: $"MODE CHANGED",
                       body: $"Append{(IsAppendLineMode ? "-Line" : "")} Mode Activated",
                       msgType: MpNotificationType.AppModeChange,
                       iconSourceObj: "NoEntryImage").FireAndForgetSafeAsync();
            } else {
                MpNotificationBuilder.ShowNotificationAsync(MpNotificationType.AppendChanged).FireAndForgetSafeAsync();
            }
        }
        public async Task DeactivateAppendModeAsync(bool isSilent = false) {
            Dispatcher.UIThread.VerifyAccess();

            bool wasAppendLineMode = IsAppendLineMode;

            //var append_tile = AllItems.FirstOrDefault(x => x.IsAppendTrayItem);
            UpdateAppendModeStateFlags(MpAppendModeFlags.None, "command");

            //if (append_tile != null) {
            //    append_tile.OnPropertyChanged(nameof(append_tile.IsAppendTrayItem));
            //}
            if (!isSilent) {

                OnPropertyChanged(nameof(AppendModeStateFlags));
            }


            await ModalClipTileViewModel.InitializeAsync(null);

            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen ||
                isSilent) {
                return;
            }

            MpAvNotificationWindowManager.Instance.HideNotification(MpAppendNotificationViewModel.Instance);

            await MpNotificationBuilder.ShowMessageAsync(
                       title: $"MODE CHANGED",
                       body: $"Append{(wasAppendLineMode ? "-Line" : "")} Mode Deactivated",
                       msgType: MpNotificationType.AppModeChange,
                       iconSourceObj: "NoEntryImage");
        }

        private async Task<bool> UpdateAppendModeAsync(MpCopyItem aci, bool isNew = true) {
            Dispatcher.UIThread.VerifyAccess();
            // NOTE only called in AdddItemFromClipboard when IsAnyAppendMode == true

            if (!IsAnyAppendMode) {
                return false;
            }
            if (ModalClipTileViewModel.IsPlaceholder) {
                await AssignAppendClipTileAsync();
                if (ModalClipTileViewModel.IsPlaceholder) {
                    return false;
                }
            }
            if (!IsCopyItemAppendable(aci)) {
                return false;
            }

            string append_data = aci.ItemData;

            if (ModalClipTileViewModel.ItemType == MpCopyItemType.FileList) {
                append_data = await MpAvFileItemCollectionViewModel.CreateFileListEditorFragment(aci);
            }
            if (isNew &&
                MpPrefViewModel.Instance.IgnoreAppendedItems &&
                ModalClipTileViewModel.CopyItemId != aci.Id) {
                aci.DeleteFromDatabaseAsync().FireAndForgetSafeAsync();
            }
            //Task.Run(async () => {
            //    // no need to wait for source updates
            //    if (AppendNotifierViewModel.CopyItemId == aci.Id) {
            //        // ignore self ref source info
            //        return;
            //    }
            //    // clone items sources into append item
            //    var aci_sources = await MpDataModelProvider.GetCopyItemSources(aci.Id);
            //    foreach (var aci_source in aci_sources) {
            //        await MpTransactionSource.CreateAsync(
            //               copyItemId: AppendNotifierViewModel.CopyItemId,
            //               sourceObjId: aci_source.SourceObjId,
            //               sourceType: aci_source.CopyItemSourceType);
            //    }
            //    if (aci.WasDupOnCreate) {
            //        // also ref if exisiting item
            //        await MpTransactionSource.CreateAsync(
            //                copyItemId: AppendNotifierViewModel.CopyItemId,
            //                sourceObjId: aci.Id,
            //                sourceType: MpCopyItemSourceType.CopyItem);
            //    } else {
            //        // delete redundant new item
            //        await aci.DeleteFromDatabaseAsync();
            //    }
            //}).FireAndForgetSafeAsync();

            //while(AppendNotifierViewModel.AppendData != null) {
            //    // probably won't happen but clipboard could change quickly so wait here
            //    // i guess for last item to process
            //    await Task.Delay(100);
            //}
            SetAppendDataCommand.Execute(append_data);
            return true;
        }
        public ICommand ToggleAppendModeCommand => new MpCommand(
            () => {
                if (IsAppendMode) {
                    DeactivateAppendModeAsync().FireAndForgetSafeAsync();
                } else {
                    ActivateAppendModeAsync(false, IsAppendManualMode).FireAndForgetSafeAsync();
                }

            });

        public ICommand ToggleAppendLineModeCommand => new MpCommand(
            () => {
                if (IsAppendLineMode) {
                    DeactivateAppendModeAsync().FireAndForgetSafeAsync();
                } else {
                    ActivateAppendModeAsync(true, IsAppendManualMode).FireAndForgetSafeAsync();
                }
            });
        public ICommand ToggleAppendManualModeCommand => new MpCommand(
            () => {
                bool new_manual_state = !IsAppendManualMode;
                bool append_state = IsAppendLineMode;
                if (!IsAnyAppendMode && new_manual_state) {
                    // append line by default
                    append_state = true;
                }

                ActivateAppendModeAsync(append_state, new_manual_state).FireAndForgetSafeAsync();
            });

        public ICommand SetAppendDataCommand => new MpCommand<object>(
            (dataArg) => {
                var append_data_str = dataArg as string;
                if (string.IsNullOrEmpty(append_data_str)) {
                    return;
                }
                AppendData = append_data_str;
            });

        #endregion

        #endregion

        #region Mouse Modes

        public bool IsAnyMouseModeEnabled => IsAutoCopyMode || IsRightClickPasteMode;


        public bool IsAutoCopyMode { get; set; }

        public bool IsRightClickPasteMode { get; set; }

        public string MouseModeImageSourcePath {
            get {
                if (IsRightClickPasteMode && IsAutoCopyMode) {
                    return MpPlatform.Services.PlatformResource.GetResource("BothClickImage") as string;
                }
                if (IsRightClickPasteMode) {
                    return MpPlatform.Services.PlatformResource.GetResource("RightClickImage") as string;
                }
                if (IsAutoCopyMode) {
                    return MpPlatform.Services.PlatformResource.GetResource("LeftClickImage") as string;
                }
                return MpPlatform.Services.PlatformResource.GetResource("NoneClickImage") as string;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Events

        public event EventHandler<MpCopyItem> OnCopyItemAdd;

        #endregion

        #region Constructors
        private MpAvClipTrayCollectionViewModel() : base() {
            PropertyChanged += MpAvClipTrayCollectionViewModel_PropertyChanged;
            PinCollection.Items.CollectionChanged += PinItems_CollectionChanged1;
            QueryCollection.Items.CollectionChanged += QueryItems_CollectionChanged;
        }


        #endregion

        #region Public Methods
        public async Task InitializeAsync() {
            IsBusy = true;

            MpPlatform.Services.ContentQueryTools = this;
            MpPlatform.Services.ClipboardMonitor.OnClipboardChanged += ClipboardChanged;

            MpDb.SyncAdd += MpDbObject_SyncAdd;
            MpDb.SyncUpdate += MpDbObject_SyncUpdate;
            MpDb.SyncDelete += MpDbObject_SyncDelete;

            ModalClipTileViewModel = new MpAvClipTileViewModel(null);
            await ModalClipTileViewModel.InitializeAsync(null);

            IsBusy = false;
        }

        #region View Invokers

        public void ScrollIntoView(object obj) {
            MpAvClipTileViewModel ctvm = null;
            if (obj is MpAvClipTileViewModel) {
                ctvm = obj as MpAvClipTileViewModel;
            } else if (obj is int ciid) {
                ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                if (ctvm == null) {
                    int ciid_query_idx = MpPlatform.Services.Query.PageTools.GetItemOffsetIdx(ciid);
                    if (ciid_query_idx < 0) {
                        if (ciid < 0) {
                            // means nothing is selected
                            ScrollIntoView(null);
                            return;
                        }
                        // ciid is neither pinned nor in query (maybe should reset query here to id but prolly not right place)
                        Debugger.Break();
                        return;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        QueryCollection.QueryCommand.Execute(ciid_query_idx);
                        while (IsAnyBusy) { await Task.Delay(100); }
                        ctvm = QueryCollection.Items.FirstOrDefault(x => x.CopyItemId == ciid);
                        if (ctvm == null) {
                            // data model provider should have come up w/ nothing here
                            Debugger.Break();
                            return;
                        }
                        ScrollIntoView(ctvm);
                    });
                    return;
                }
            } else if (obj == null) {
                // occurs when nothing is selected
                if (QueryCollection.IsTrayEmpty) {
                    if (PinCollection.IsTrayEmpty) {
                        return;
                    }
                    ScrollIntoView(0);
                    return;
                } else {
                    ctvm = PinCollection.SortOrderedItems.ElementAt(0);
                }
            }

            if (Collections.Any(x=>x.IsScrollingIntoView) || IsAnyBusy) {
                return;
            }
            if (ctvm.IsPinned) {
                PinCollection.ScrollIntoView(ctvm.ScreenRect);
            } else {
                QueryCollection.ScrollIntoView(ctvm.ScreenRect);
            }

        }


        #endregion    
        #endregion

        #region Protected Methods

        #region Db Events

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            //if (e is MpCopyItem ci) {
            //_allTiles.Add(CreateClipTileViewModel(ci));
            // }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                if (ci.Id == ModalClipTileViewModel.CopyItemId &&
                    IsAnyAppendMode) {

                    DeactivateAppendModeAsync().FireAndForgetSafeAsync();
                }
                MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Remove(ci);
                MpAvPersistentClipTilePropertiesHelper.RemovePersistentSize_ById(ci.Id);

                MpPlatform.Services.Query.PageTools.RemoveItemId(ci.Id);
            } else if (e is MpCopyItemTag cit) {
                var sttvm = MpAvTagTrayViewModel.Instance.SelectedItem;
                // check if unlink is part of current query
                bool is_part_of_query =
                    sttvm
                    .SelfAndAllDescendants
                    .Cast<MpAvTagTileViewModel>()
                    .Select(x => x.TagId)
                    .Any(x => x == cit.TagId);

                if (is_part_of_query && !sttvm.IsAllTag) {
                    // when unlinked item is part of current query remove its offset and do a reset query

                    if (MpPlatform.Services.Query.PageTools.RemoveItemId(cit.CopyItemId)) {
                        MpPlatform.Services.Query.NotifyQueryChanged();
                    } else {
                        // where/when was item removed from query?
                        Debugger.Break();
                    }
                }
            }
        }


        #endregion

        #region Sync Events

        private void MpDbObject_SyncDelete(object sender, MpDbSyncEventArgs e) {
            Dispatcher.UIThread.Post((Action)(() => {
                if (sender is MpCopyItem ci) {
                    var ctvmToRemove = AllItems.FirstOrDefault(x => x.CopyItemId == ci.Id);
                    if (ctvmToRemove != null) {
                        ctvmToRemove.CopyItem.StartSync(e.SourceGuid);
                        //ctvmToRemove.CopyItem.Color.StartSync(e.SourceGuid);
                        //Items.Remove(ctvmToRemove);
                        ctvmToRemove.CopyItem.EndSync();
                        //ctvmToRemove.CopyItem.Color.EndSync();
                    }
                }
            }));
        }

        private void MpDbObject_SyncUpdate(object sender, MpDbSyncEventArgs e) {
            //Dispatcher.UIThread.Post((Action)(() => {
            //}));
        }

        private void MpDbObject_SyncAdd(object sender, MpDbSyncEventArgs e) {
            //Dispatcher.UIThread.Post(async () => {
            //    if (sender is MpCopyItem ci) {
            //ci.StartSync(e.SourceGuid);

            //var svm = MpAvSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == ci.SourceId);

            //var app = svm.AppViewModel.App;
            //app.StartSync(e.SourceGuid);
            ////ci.Source.App.Icon.StartSync(e.SourceGuid);
            ////ci.Source.App.Icon.IconImage.StartSync(e.SourceGuid);

            //var dupCheck = this.GetClipTileViewModelById((int)ci.Id);
            //if (dupCheck == null) {
            //    if (ci.Id == 0) {
            //        await ci.WriteToDatabaseAsync();
            //    }
            //    _newModels.Add(ci);
            //    //AddNewTiles();
            //} else {
            //    MpConsole.WriteTraceLine(@"Warning, attempting to add existing copy item: " + dupCheck.CopyItem.ItemData + " ignoring and updating existing.");
            //    //dupCheck.CopyItem = ci;
            //}
            //app.EndSync();
            ////ci.Source.App.Icon.EndSync();
            ////ci.Source.App.Icon.IconImage.EndSync();
            //ci.EndSync();

            //ResetClipSelection();
            //    }
            //});
        }

        #endregion

        #endregion

        #region Private Methods

        private void MpAvClipTrayCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ModalClipTileViewModel):
                    if (ModalClipTileViewModel == null) {
                        return;
                    }
                    ModalClipTileViewModel.OnPropertyChanged(nameof(ModalClipTileViewModel.CopyItemId));

                    break;

                case nameof(IsAnyTilePinned):
                    MpMessenger.SendGlobal(MpMessageType.PinTrayEmptyOrHasTile);
                    break;
            }
        }

        private void PinItems_CollectionChanged1(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(IsAnyTilePinned));
        }

        private void QueryItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                // QUERY

                case MpMessageType.RequeryCompleted:
                    if (IsInitialQuery) {
                        IsInitialQuery = false;
                        Dispatcher.UIThread.Post(async () => {
                            while (IsAnyBusy) {
                                await Task.Delay(100);
                            }
                            // BUG this works around initial tile size being tiny and triggering resize fits
                            // them right
                            MpMessenger.SendGlobal(MpMessageType.MainWindowSizeChangeEnd);
                        });
                    }

                    break;
                case MpMessageType.QueryChanged:
                    QueryCollection.QueryCommand.Execute(null);
                    break;
                case MpMessageType.SubQueryChanged:
                    QueryCollection.QueryCommand.Execute(QueryCollection.ScrollOffset);
                    break;
                case MpMessageType.TotalQueryCountChanged:
                    QueryCollection.OnPropertyChanged(nameof(QueryCollection.TotalTilesInQuery));
                    break;

                    // DND
                    //case MpMessageType.ItemDragBegin:
                    //    OnPropertyChanged(nameof(IsAnyTileDragging));
                    //    if(DragItem == null) {
                    //        // shant be true
                    //        Debugger.Break();
                    //        return;
                    //    }
                    //    MpAvPersistentClipTilePropertiesHelper.AddPersistentIsTileDraggingTile_ById(DragItem.CopyItemId);
                    //    break;
                    //case MpMessageType.ItemDragEnd:
                    //    OnPropertyChanged(nameof(IsAnyTileDragging));

                    //    MpAvPersistentClipTilePropertiesHelper.ClearPersistentIsTileDragging();
                    //    break;
            }
        }
        private void ClipboardChanged(object sender, MpPortableDataObject mpdo) {
            bool is_change_ignored = MpAvMainWindowViewModel.Instance.IsMainWindowLoading ||
                                        IsAppPaused ||
                                        (MpPrefViewModel.Instance.IgnoreInternalClipboardChanges && MpPlatform.Services.ProcessWatcher.IsThisAppActive);
            if (is_change_ignored) {
                MpConsole.WriteLine("Clipboard Change Ignored by tray");
                MpConsole.WriteLine($"IsMainWindowLoading: {MpAvMainWindowViewModel.Instance.IsMainWindowLoading}");
                MpConsole.WriteLine($"IsAppPaused: {IsAppPaused}");
                MpConsole.WriteLine($"IgnoreInternalClipboardChanges: {MpPrefViewModel.Instance.IgnoreInternalClipboardChanges} IsThisAppActive: {MpPlatform.Services.ProcessWatcher.IsThisAppActive}");
                return;
            }

            Dispatcher.UIThread.Post(async () => {
                await AddItemFromClipboard(mpdo);
            });
        }

        private async Task AddItemFromClipboard(MpPortableDataObject cd) {
            if (IsAddingClipboardItem) {
                MpConsole.WriteLine("Warning! New Clipboard item detected while already adding one (seems to only occur internally). Ignoring this one.");
                return;
            }

            IsAddingClipboardItem = true;

            var newCopyItem = await MpPlatform.Services.CopyItemBuilder.BuildAsync(cd);

            if (newCopyItem == null || newCopyItem.Id < 1) {
                //this occurs if the copy item is not a known format or app init
                MpConsole.WriteTraceLine("Unable to create copy item from clipboard!");
                IsAddingClipboardItem = false;
                return;
            }

            if (MpPrefViewModel.Instance.NotificationDoCopySound) {
                //MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
            }
            if (MpPrefViewModel.Instance.IsTrialExpired) {
                MpNotificationBuilder.ShowMessageAsync(
                    title: "Trial Expired",
                    body: "Please update your membership to use Monkey Paste",
                    msgType: MpNotificationType.TrialExpired,
                    iconSourceObj: MpPrefViewModel.Instance.AbsoluteResourcesPath + @"/Images/monkey (2).png")
                    .FireAndForgetSafeAsync(this);
            }


            await AddUpdateOrAppendCopyItemAsync(newCopyItem);

            IsAddingClipboardItem = false;
        }
        private async Task AddUpdateOrAppendCopyItemAsync(MpCopyItem ci, int force_pin_idx = 0) {
            if (ci.WasDupOnCreate) {
                //item is a duplicate
                MpConsole.WriteLine("Duplicate item detected, incrementing copy count and updating copydatetime");
                ci.CopyCount++;
                // reseting CopyDateTime will move item to top of recent list
                ci.CopyDateTime = DateTime.Now;
                await ci.WriteToDatabaseAsync();
            }

            if (ModalClipTileViewModel.IsPlaceholder) {
                _newModels.Add(ci);
            }
            bool wasAppended = false;
            if (IsAnyAppendMode) {
                wasAppended = await UpdateAppendModeAsync(ci);
            }

            if (!wasAppended) {
                //if (!MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                //    MpAvTagTrayViewModel.Instance.AllTagViewModel.LinkCopyItemCommand.Execute(ci.Id);
                //}
                SelectedCollection.AddNewItemsCommand.Execute(null);
                OnCopyItemAdd?.Invoke(this, ci);
            }
        }
        private async Task PasteClipTileAsync(MpAvClipTileViewModel ctvm) {
            ctvm.IsPasting = true;

            var ds = ctvm.GetDragSource();
            if (ds == null) {
                Debugger.Break();
                return;
            }
            MpAvDataObject mpdo = await ds.GetDataObjectAsync(true);
            if (mpdo == null) {
                // is none selected?
                Debugger.Break();
            } else {
                var pi = MpPlatform.Services.ProcessWatcher.LastProcessInfo;
                await MpPlatform.Services.ExternalPasteHandler.PasteDataObject(mpdo, pi);
            }

            CleanupAfterPaste(ctvm);
        }
        private void CleanupAfterPaste(MpAvClipTileViewModel sctvm) {
            IsPasting = false;
            //clean up pasted items state after paste
            sctvm.PasteCount++;
            sctvm.IsPasting = false;
        }

        #region Tile State Wrappers

        public void ClearClipEditing() {
            QueryCollection.ClearClipEditing();
        }

        public void ClearPinnedEditing() {
            PinCollection.ClearClipEditing();
        }

        public void ClearAllEditing() {
            ClearClipEditing();
            ClearPinnedEditing();
        }

        public void ClearClipSelection(bool clearEditing = true) {
            QueryCollection.ClearClipSelection(clearEditing);
        }
        public void ClearAllSelection(bool clearEditing = true) {
            Collections.ForEach(x => x.ClearAllSelection(clearEditing));
        }
        public void ResetClipSelection(bool clearEditing = true) {
            QueryCollection.ResetClipSelection(clearEditing);
        }

        #endregion

        #region Keyboard Navigation
        private bool CanTileNavigate() {
            bool canNavigate = !IsAnyBusy && !IsArrowSelecting &&

                  !Collections.All(x=>x.HasScrollVelocity) &&
                  !Collections.All(x=>x.IsScrollingIntoView);

            if (canNavigate) {
                if (SelectedItem != null &&
                    SelectedItem.IsSubSelectionEnabled ||
                    (SelectedItem != null && !SelectedItem.IsTitleReadOnly && SelectedItem.IsTitleFocused)) {
                    canNavigate = false;
                }
            }
            return canNavigate;
        }

        private async Task SelectNeighborHelperAsync(int row_offset, int col_offset) {
            if (row_offset != 0 && col_offset != 0) {
                // NO! should only be one or the other
                Debugger.Break();
                return;
            }
            if (row_offset == 0 && col_offset == 0) {
                return;
            }

            IsArrowSelecting = true;
            ScrollIntoView(PersistantSelectedItemId);
            await Task.Delay(100);
            while (IsAnyBusy) { await Task.Delay(100); }
            if (SelectedItem == null) {
                if (PinCollection.IsTrayEmpty) {
                    if (QueryCollection.IsTrayEmpty) {
                        IsArrowSelecting = false;
                        return;
                    }
                    SelectedItem = QueryCollection.HeadItem;
                    IsArrowSelecting = false;
                    return;
                }
                SelectedItem = PinCollection.SortOrderedItems.ElementAt(0);
                IsArrowSelecting = false;
                return;
            }
            MpAvClipTileViewModel target_ctvm = null;
            if (row_offset != 0) {
                target_ctvm = await SelectedItem.GetNeighborByRowOffsetAsync(row_offset);
            } else {
                target_ctvm = await SelectedItem.GetNeighborByColumnOffsetAsync(col_offset);
            }

            if (target_ctvm != null) {
                SelectedItem = target_ctvm;
            }
            IsArrowSelecting = false;
        }
        #endregion

        #endregion

        #region Commands

        public ICommand ToggleIsAppPausedCommand => new MpCommand(
            () => {
                IsAppPaused = !IsAppPaused;
            });

        public ICommand ToggleRightClickPasteCommand => new MpCommand(
            () => {
                IsRightClickPasteMode = !IsRightClickPasteMode;
                MpNotificationBuilder.ShowMessageAsync("MODE CHANGED", string.Format("RIGHT CLICK PASTE MODE: {0}", IsRightClickPasteMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, () => !IsAppPaused);

        public ICommand ToggleAutoCopyModeCommand => new MpCommand(
            () => {
                IsAutoCopyMode = !IsAutoCopyMode;
                MpNotificationBuilder.ShowMessageAsync("MODE CHANGED", string.Format("AUTO-COPY SELECTION MODE: {0}", IsAutoCopyMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, () => !IsAppPaused);

        public ICommand ScrollToHomeCommand => new MpCommand(
            () => {
                SelectedCollection.QueryCommand.Execute(0);
            },
            () => {
                return CanTileNavigate();
            });

        public ICommand ScrollToEndCommand => new MpCommand(
            () => {
                SelectedCollection.QueryCommand.Execute(SelectedCollection.MaxClipTrayQueryIdx);
            }, () => {
                return CanTileNavigate();
            });

        public ICommand ScrollToNextPageCommand => new MpCommand(
             () => {
                 MpPoint scroll_delta = MpPoint.Zero;
                 if (SelectedCollection.DefaultScrollOrientation == Orientation.Horizontal) {
                     scroll_delta.X = SelectedCollection.QueryTrayScreenWidth;
                 } else {
                     scroll_delta.Y = SelectedCollection.QueryTrayScreenHeight;
                 }
                 var nextPageOffset = (SelectedCollection.ScrollOffset + scroll_delta);
                 SelectedCollection.QueryCommand.Execute(nextPageOffset);
             },
            () => {
                return CanTileNavigate();
            });

        public ICommand ScrollToPreviousPageCommand => new MpCommand(
            () => {
                MpPoint scroll_delta = MpPoint.Zero;
                if (SelectedCollection.DefaultScrollOrientation == Orientation.Horizontal) {
                    scroll_delta.X = SelectedCollection.QueryTrayScreenWidth;
                } else {
                    scroll_delta.Y = SelectedCollection.QueryTrayScreenHeight;
                }
                var prevPageOffset = (SelectedCollection.ScrollOffset - scroll_delta);
                SelectedCollection.QueryCommand.Execute(prevPageOffset);
            },
            () => {
                return CanTileNavigate();
            });

        public ICommand SelectNextRowItemCommand => new MpAsyncCommand(
            async () => {
                await SelectNeighborHelperAsync(1, 0);
            }, () => {
                return CanTileNavigate();
            });

        public ICommand SelectPreviousRowItemCommand => new MpAsyncCommand(
            async () => {
                await SelectNeighborHelperAsync(-1, 0);
            }, () => {
                return CanTileNavigate();
            });

        public ICommand SelectNextColumnItemCommand => new MpAsyncCommand(
            async () => {
                await SelectNeighborHelperAsync(0, 1);
            },
            () => {
                return CanTileNavigate();
            });

        public ICommand SelectPreviousColumnItemCommand => new MpCommand(
            async () => {
                await SelectNeighborHelperAsync(0, -1);
            },
            () => {
                return CanTileNavigate();
            });

        public ICommand PinTileCommand => new MpAsyncCommand<object>(
            async (args) => {
                int pin_idx = 0;
                MpAvClipTileViewModel ctvm_to_pin = null;
                if (args is MpAvClipTileViewModel) {
                    // pinning new or query tray tile from overlay button
                    ctvm_to_pin = args as MpAvClipTileViewModel;
                } else if (args is object[] argParts) {
                    // dnd pin tray drop 
                    ctvm_to_pin = argParts[0] as MpAvClipTileViewModel;
                    pin_idx = (int)argParts[1];
                }

                if (ctvm_to_pin == null || ctvm_to_pin.IsPlaceholder) {
                    MpConsole.WriteTraceLine("PinTile error, tile is either already pinned or placeholder");
                    Debugger.Break();
                    return;
                }

                if (MpPlatform.Services.Query.PageTools.RemoveItemId(ctvm_to_pin.CopyItemId)) {
                    // tile was part of query tray
                    if (QueryCollection.Items.Contains(ctvm_to_pin)) {
                        int ctvm_to_pin_qidx = ctvm_to_pin.QueryOffsetIdx;

                        // trigger PublicHandle change to unload view
                        ctvm_to_pin.QueryOffsetIdx = -1;
                        QueryCollection.Items.Remove(ctvm_to_pin);
                        QueryCollection.Items.Where(x => x.QueryOffsetIdx > ctvm_to_pin_qidx).ForEach(x => x.QueryOffsetIdx = x.QueryOffsetIdx - 1);
                    }
                }

                if (ctvm_to_pin.IsPinned) {
                    // for drop from pin tray or new duplicate was in pin tray
                    int cur_pin_idx = PinCollection.Items.IndexOf(ctvm_to_pin);
                    PinCollection.Items.Move(cur_pin_idx, pin_idx);
                } else if (pin_idx == PinCollection.Items.Count) {
                    // new item or user pinned query item
                    PinCollection.Items.Add(ctvm_to_pin);
                } else {
                    // for drop from external or query tray
                    PinCollection.Items.Insert(pin_idx, ctvm_to_pin);
                }

                OnPropertyChanged(nameof(IsAnyTilePinned));
                ctvm_to_pin.OnPropertyChanged(nameof(ctvm_to_pin.IsPinned));
                ctvm_to_pin.OnPropertyChanged(nameof(ctvm_to_pin.IsPlaceholder));

                await Task.Delay(100);
                while (IsAnyBusy) {
                    await Task.Delay(100);
                }
                RefreshQueryTrayLayout();
                await Task.Delay(200);
                SelectedItem = ctvm_to_pin;


                OnPropertyChanged(nameof(QueryCollection.Items));
                OnPropertyChanged(nameof(PinCollection.Items));
                OnPropertyChanged(nameof(PinCollection.MinClipOrPinTrayScreenHeight));
                OnPropertyChanged(nameof(PinCollection.Max));
                OnPropertyChanged(nameof(QueryTrayScreenWidth));
                OnPropertyChanged(nameof(QueryTrayScreenHeight));
                UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync(this);
            },
           (args) =>
           args != null);

        public ICommand UnpinTileCommand => new MpAsyncCommand<object>(
             async (args) => {
                 var upctvm = args as MpAvClipTileViewModel;
                 int unpinnedId = upctvm.CopyItemId;
                 int unpinned_ctvm_idx = PinCollection.Items.IndexOf(upctvm);
                 PinCollection.Items.Remove(upctvm);
                 OnPropertyChanged(nameof(IsAnyTilePinned));

                 if (!IsAnyTilePinned) {
                     ObservedPinTrayScreenWidth = 0;
                 }

                 OnPropertyChanged(nameof(PinCollection.Items));
                 OnPropertyChanged(nameof(IsAnyTilePinned));
                 OnPropertyChanged(nameof(MinPinTrayScreenWidth));
                 OnPropertyChanged(nameof(MaxPinTrayScreenWidth));
                 OnPropertyChanged(nameof(QueryTrayScreenWidth));
                 OnPropertyChanged(nameof(QueryTrayScreenHeight));

                 ClearClipSelection(false);
                 // perform inplace requery to potentially put unpinned tile back
                 QueryCommand.Execute(string.Empty);
                 while (IsAnyBusy) {
                     await Task.Delay(100);
                 }
                 await Task.Delay(300);
                 var unpinned_ctvm = Items.FirstOrDefault(x => x.CopyItemId == unpinnedId);

                 if (unpinned_ctvm != null) {
                     // if unpinned tile is in current page select it
                     SelectedItem = unpinned_ctvm;
                 } else if (IsPinTrayEmpty) {
                     // select left most visible tile if pin tray empty
                     SelectedItem = VisibleItems.AggregateOrDefault((a, b) => a.QueryOffsetIdx < b.QueryOffsetIdx ? a : b);
                 } else {
                     // prefer select neighbor pin tile 
                     int sel_idx = Math.Min(PinCollection.Items.Count - 1, Math.Max(0, unpinned_ctvm_idx));
                     SelectedItem = PinCollection.Items[sel_idx];
                 }

                 UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync(this);
             },
            (args) => args != null && args is MpAvClipTileViewModel ctvm && ctvm.IsPinned);

        public ICommand ToggleTileIsPinnedCommand => new MpCommand<object>(
            (args) => {
                MpAvClipTileViewModel pctvm = null;
                if (args is MpAvClipTileViewModel) {
                    pctvm = args as MpAvClipTileViewModel;
                } else if (args is object[] argParts) {
                    pctvm = argParts[0] as MpAvClipTileViewModel;
                }

                if (pctvm.IsPinned) {
                    UnpinTileCommand.Execute(args);
                } else {
                    PinTileCommand.Execute(args);
                }
            },
            (args) => args != null);
        public ICommand UnpinAllCommand => new MpCommand(() => {
            int pin_count = PinCollection.Items.Count;
            while (pin_count > 0) {
                UnpinTileCommand.Execute(PinCollection.Items[--pin_count]);
            }
        });
        #endregion
    }
}
