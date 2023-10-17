using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Avalonia.Utils.Extensions;
using MonkeyPaste.Common.Plugin;
using SharpHook;
using SharpHook.Logging;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutCollectionViewModel :
        MpAvSelectorViewModelBase<object, MpAvShortcutViewModel>,
        MpIGlobalInputListener,
        MpIDownKeyHelper,
        MpIShortcutGestureLocator,
        MpIDndUserCancelNotifier {

        #region Statics
        static bool IS_GLOBAL_INPUT_LOGGING_ENABLED { get; set; } = false;
        static bool IS_GLOBAL_MOUSE_INPUT_ENABLED { get; set; } = true;
        static bool IS_GLOBAL_KEYBOARD_INPUT_ENABLED { get; set; } = true;
        static bool IS_GLOBAL_INPUT_ENABLED =>
            IS_GLOBAL_KEYBOARD_INPUT_ENABLED || IS_GLOBAL_MOUSE_INPUT_ENABLED;
        public const double MIN_GLOBAL_DRAG_DIST = 20;

        #endregion

        #region Private Variables

        private SimpleGlobalHook _hook;

        private MpAvKeyGestureHelper<KeyCode> _keyboardGestureHelper = new MpAvKeyGestureHelper<KeyCode>();
        private MpAvShortcutViewModel _exact_match;

        #endregion

        #region Statics

        private static MpAvShortcutCollectionViewModel _instance;
        public static MpAvShortcutCollectionViewModel Instance => _instance ?? (_instance = new MpAvShortcutCollectionViewModel());

        #endregion

        #region Interfaces

        #region MpIDownKeyHelper Implementation

        IReadOnlyList<object> MpIDownKeyHelper.Downs =>
            _keyboardGestureHelper.Downs.Cast<object>().ToList();

        #endregion

        #region MpIShortcutGestureLocator Implementation

        string MpIShortcutGestureLocator.LocateByType(MpShortcutType sct) {
            if (Items.FirstOrDefault(x => x.ShortcutType == sct) is MpAvShortcutViewModel scvm) {
                return scvm.KeyString;
            }
            return null;
        }

        string MpIShortcutGestureLocator.LocateByCommand(MpIShortcutCommandViewModel scvm) =>

            GetViewModelCommandShortcutKeyString(scvm);

        object MpIShortcutGestureLocator.LocateSourceByType(MpShortcutType sct) {
            if (Items.FirstOrDefault(x => x.ShortcutType == sct) is MpAvShortcutViewModel scvm) {
                return scvm;
            }
            return null;
        }
        #endregion

        #region MpIGlobalInputListener
        public void StartInputListener() {
            if (IS_GLOBAL_INPUT_ENABLED) {
                CreateGlobalInputHooks();
            }
        }
        public void StopInputListener() {
            if (IS_GLOBAL_INPUT_ENABLED) {
                DisposeGlobalInputHooks();
            }
        }

        #endregion

        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvShortcutViewModel> FilteredItems { get; private set; } = new ObservableCollection<MpAvShortcutViewModel>();

        public IEnumerable<MpAvShortcutViewModel> FilteredAppItems =>
            FilteredItems
            .Where(x => !x.CanBeGlobalShortcut && !x.IsCustom);
        public IEnumerable<MpAvShortcutViewModel> FilteredGlobalItems =>
            FilteredItems
            .Where(x => x.CanBeGlobalShortcut && !x.IsCustom);

        public IEnumerable<MpAvShortcutViewModel> FilteredCustomItems =>
            FilteredItems
            .Where(x => x.IsCustom);

        public IEnumerable<MpAvShortcutViewModel> AvailableItems =>
            MpAvMainWindowViewModel.Instance.IsAnyAppWindowActive ?
                Items :
                Items.Where(x => x.IsGlobal);

        private Dictionary<MpShortcutType, ICommand> _appCommandLookup;
        public Dictionary<MpShortcutType, ICommand> AppCommandLookup {
            get {
                if (_appCommandLookup == null) {
                    _appCommandLookup = new Dictionary<MpShortcutType, ICommand>() {
                        {
                            MpShortcutType.ToggleMainWindow,
                            MpAvMainWindowViewModel.Instance.ToggleShowMainWindowCommand
                        },
                        {
                            MpShortcutType.HideMainWindow,
                            MpAvApplicationCommand.Instance.DecreaseFocusCommand
                        },
                        {
                            MpShortcutType.ShowSettings,
                            MpAvSettingsViewModel.Instance.ShowSettingsWindowCommand
                        },
                        {
                            MpShortcutType.ExitApplication,
                            MpAvSystemTrayViewModel.Instance.ExitApplicationCommand
                        },
                        {
                            MpShortcutType.ToggleAppendInsertMode,
                            MpAvClipTrayViewModel.Instance.ToggleAppendInsertModeCommand
                        },
                        {
                            MpShortcutType.ToggleAppendLineMode,
                            MpAvClipTrayViewModel.Instance.ToggleAppendLineModeCommand
                        },
                        {
                            MpShortcutType.ToggleAppendPreMode,
                            MpAvClipTrayViewModel.Instance.ToggleAppendPreModeCommand
                        },
                        {
                            MpShortcutType.ToggleAppendManualMode,
                            MpAvClipTrayViewModel.Instance.ToggleAppendManualModeCommand
                        },
                        {
                            MpShortcutType.ToggleAppendPaused,
                            MpAvClipTrayViewModel.Instance.ToggleAppendPausedCommand
                        },
                        {
                            MpShortcutType.ToggleAutoCopyMode,
                            MpAvClipTrayViewModel.Instance.ToggleAutoCopyModeCommand
                        },
                        {
                            MpShortcutType.ToggleRightClickPasteMode,
                            MpAvClipTrayViewModel.Instance.ToggleRightClickPasteCommand
                        },
                        {
                            MpShortcutType.ToggleDropWidgetEnabled,
                            MpAvExternalDropWindowViewModel.Instance.ToggleIsDropWidgetEnabledCommand
                        },
                        {
                            MpShortcutType.PasteSelectedItems,
                            MpAvClipTrayViewModel.Instance.PasteSelectedClipTileFromShortcutCommand
                        },
                        {
                            MpShortcutType.PasteHere,
                            MpAvClipTrayViewModel.Instance.PasteCurrentClipboardIntoSelectedTileCommand
                        },
                        {
                            MpShortcutType.DeleteSelectedItems,
                            MpAvClipTrayViewModel.Instance.TrashOrDeleteSelectedClipFromShortcutCommand
                        },
                        {
                            MpShortcutType.SelectNextColumnItem,
                            MpAvClipTrayViewModel.Instance.SelectNextColumnItemCommand
                        },
                        {
                            MpShortcutType.SelectPreviousColumnItem,
                            MpAvClipTrayViewModel.Instance.SelectPreviousColumnItemCommand
                        },
                        {
                            MpShortcutType.SelectNextRowItem,
                            MpAvClipTrayViewModel.Instance.SelectNextRowItemCommand
                        },
                        {
                            MpShortcutType.SelectPreviousRowItem,
                            MpAvClipTrayViewModel.Instance.SelectPreviousRowItemCommand
                        },
                        {
                            MpShortcutType.AssignShortcut,
                            MpAvClipTrayViewModel.Instance.AssignShortcutToSelectedItemCommand
                        },
                        {
                            MpShortcutType.Undo,
                            MpAvUndoManagerViewModel.Instance.UndoCommand
                        },
                        {
                            MpShortcutType.Redo,
                            MpAvUndoManagerViewModel.Instance.RedoCommand
                        },
                        {
                            MpShortcutType.ToggleContentReadOnly,
                            MpAvClipTrayViewModel.Instance.ToggleIsSelectedContentReadOnlyCommand
                        },
                        {
                            MpShortcutType.Rename,
                            //MpAvClipTrayViewModel.Instance.EditSelectedTitleCommand
                            MpAvApplicationCommand.Instance.RenameCommand
                        },
                        {
                            MpShortcutType.Duplicate,
                            MpAvClipTrayViewModel.Instance.DuplicateSelectedClipsCommand
                        },
                        {
                            MpShortcutType.ToggleListenToClipboard,
                            MpAvClipTrayViewModel.Instance.ToggleIsAppPausedCommand
                        },
                        {
                            MpShortcutType.CopySelection,
                            MpAvClipTrayViewModel.Instance.CopySelectedClipFromShortcutCommand
                        },
                        {
                            MpShortcutType.ScrollToHome,
                            MpAvClipTrayViewModel.Instance.ScrollToHomeCommand
                        },
                        {
                            MpShortcutType.ScrollToEnd,
                            MpAvClipTrayViewModel.Instance.ScrollToEndCommand
                        },
                        {
                            MpShortcutType.WindowSizeUp,
                            MpAvMainWindowViewModel.Instance.WindowSizeUpCommand
                        },
                        {
                            MpShortcutType.WindowSizeDown,
                            MpAvMainWindowViewModel.Instance.WindowSizeDownCommand
                        },
                        {
                            MpShortcutType.WindowSizeLeft,
                            MpAvMainWindowViewModel.Instance.WindowSizeLeftCommand
                        },
                        {
                            MpShortcutType.WindowSizeRight,
                            MpAvMainWindowViewModel.Instance.WindowSizeRightCommand
                        },
                        {
                            MpShortcutType.PreviousPage,
                            MpAvClipTrayViewModel.Instance.ScrollToPreviousPageCommand
                        },
                        {
                            MpShortcutType.NextPage,
                            MpAvClipTrayViewModel.Instance.ScrollToNextPageCommand
                        },
                        {
                            MpShortcutType.FindAndReplaceSelectedItem,
                            MpAvClipTrayViewModel.Instance.EnableFindAndReplaceForSelectedItem
                        },
                        {
                            MpShortcutType.ToggleMainWindowLocked,
                            MpAvMainWindowViewModel.Instance.ToggleMainWindowLockCommand
                        },
                        {
                            MpShortcutType.ToggleFilterMenuVisible,
                            MpAvMainWindowViewModel.Instance.ToggleFilterMenuVisibleCommand
                        },
                        {
                            MpShortcutType.TogglePinned,
                            MpAvClipTrayViewModel.Instance.ToggleSelectedTileIsPinnedCommand
                        },
                        {
                            MpShortcutType.OpenInWindow,
                            MpAvApplicationCommand.Instance.OpenPopoutCommand
                        },
                        {
                            MpShortcutType.PasteCopyItem,
                            MpAvClipTrayViewModel.Instance.PasteCopyItemByIdFromShortcutCommand
                        },
                        {
                            MpShortcutType.SelectTag,
                            MpAvTagTrayViewModel.Instance.SelectTagCommand
                        },
                        {
                            MpShortcutType.AnalyzeCopyItemWithPreset,
                            MpAvClipTrayViewModel.Instance.AnalyzeSelectedItemCommand
                        },
                        {
                            MpShortcutType.InvokeTrigger,
                            MpAvTriggerCollectionViewModel.Instance.InvokeActionCommand
                        },
                        {
                            MpShortcutType.PermanentlyDelete,
                            MpAvClipTrayViewModel.Instance.PermanentlyDeleteSelectedClipFromShortcutCommand
                        },
                        {
                            MpShortcutType.ForceMinimizeMainWindow,
                            MpAvMainWindowViewModel.Instance.ForceMinimizeMainWindowCommand
                        }
                    };
                }
                return _appCommandLookup;
            }
        }

        //public IEnumerable<MpAvShortcutViewModel> CustomShortcuts =>
        //    FilteredItems.Where(x => x.IsCustom);

        //public IEnumerable<MpAvShortcutViewModel> InternalApplicationShortcuts =>
        //    FilteredItems.Where(x => !x.IsCustom && !x.CanBeGlobalShortcut);

        //public IEnumerable<MpAvShortcutViewModel> GlobalApplicationShortcuts =>
        //    FilteredItems.Where(x => !x.IsCustom && x.CanBeGlobalShortcut);

        #endregion

        #region State

        public bool IsGlobalHooksPaused { get; private set; }

        // NOTE this needs to be updated when shortcuts changed
        // to avoid re-serializing it for every tile
        private string _editorShortcutsMsgBase64;
        public string EditorShortcutsMsgBase64 =>
            _editorShortcutsMsgBase64;

        public bool IsAnyBusy =>
            IsBusy || Items.Any(x => x.IsBusy);
        public MpKeyModifierFlags GlobalKeyModifierFlags {
            get {
                MpKeyModifierFlags kmf = MpKeyModifierFlags.None;
                if (GlobalIsCtrlDown) {
                    kmf |= MpKeyModifierFlags.Control;
                }
                if (GlobalIsAltDown) {
                    kmf |= MpKeyModifierFlags.Alt;
                }
                if (GlobalIsShiftDown) {
                    kmf |= MpKeyModifierFlags.Shift;
                }
                if (GlobalIsMetaDown) {
                    kmf |= MpKeyModifierFlags.Meta;
                }
                return kmf;
            }
        }

        public bool GlobalIsCtrlDown { get; private set; } = false;

        public bool GlobalIsAltDown { get; private set; } = false;

        public bool GlobalIsShiftDown { get; private set; } = false;
        public bool GlobalIsMetaDown { get; private set; } = false;
        public bool GlobalIsEscapeDown { get; private set; } = false;
        public bool GlobalIsPointerDragging { get; private set; } = false;

        public bool IsMultiSelectKeyDown => GlobalIsCtrlDown || GlobalIsAltDown || GlobalIsShiftDown;

        public int SelectedShortcutIndex { get; set; }

        public MpPoint GlobalMouseLocation { get; set; } = MpPoint.Zero;

        public MpPoint? GlobalMouseLeftButtonDownLocation { get; private set; } = null;
        public bool GlobalIsMouseLeftButtonDown { get; private set; } = false;
        public bool GlobalIsMouseRightButtonDown { get; private set; } = false;

        public bool IsApplicationShortcutsEnabled { get; set; } = true;
        public bool IsGlobalShortcutsEnabled { get; set; } = true;

        public bool IsShortcutsEnabled {
            get => IsApplicationShortcutsEnabled || IsGlobalShortcutsEnabled;
            set {
                IsApplicationShortcutsEnabled = value;
                IsGlobalShortcutsEnabled = value;
                OnPropertyChanged(nameof(IsShortcutsEnabled));
            }
        }

        public DateTime LastRightClickDateTime { get; set; }
        public DateTime LastLeftClickDateTime { get; set; }

        public int RoutingProfileTypeIdx {
            get => (int)RoutingProfileType;
            set {
                if (value < 0 || value >= typeof(MpShortcutRoutingProfileType).Length()) {
                    // bad ui value set
                    return;
                }
                if (value != (int)RoutingProfileType) {
                    RoutingProfileType = (MpShortcutRoutingProfileType)value;
                    OnPropertyChanged(nameof(RoutingProfileTypeIdx));
                }
            }
        }


        public MpShortcutRoutingProfileType RoutingProfileType { get; private set; }

        #endregion

        #region Appearance
        public string HookPauseLabel =>
            $"Toggle Global Hooks {(IsGlobalHooksPaused ? "ON" : "OFF")}";
        #endregion

        #endregion

        #region Events

        #region Mouse Events

        public event EventHandler OnGlobalDragBegin;
        public event EventHandler<MpPoint> OnGlobalDrag;
        public event EventHandler OnGlobalDragEnd;

        public event EventHandler<MpPoint> OnGlobalMouseWheelScroll;

        public event EventHandler<MpPoint> OnGlobalMouseMove;

        public event EventHandler<bool> OnGlobalMousePressed;
        public event EventHandler<bool> OnGlobalMouseReleased;

        public event EventHandler<bool> OnGlobalMouseClicked;

        #endregion

        #region Keyboard Events

        public event EventHandler<string> OnGlobalKeyPressed;
        public event EventHandler<string> OnGlobalKeyReleased;

        public event EventHandler OnGlobalEscKeyPressed;
        public event EventHandler OnGlobalEscapeReleased;

        #endregion

        #region Clipboard Events

        public event EventHandler OnGlobalPasteShortcutPerformed;

        #endregion

        #endregion

        #region Constructors
        public MpAvShortcutCollectionViewModel() : base(null) {
            Mp.Services.GlobalInputListener = this;
            Mp.Services.ShortcutGestureLocator = this;
            Mp.Services.KeyDownHelper = this;

            if (!Mp.Services.PlatformInfo.IsDesktop) {
                IS_GLOBAL_MOUSE_INPUT_ENABLED = false;
                IS_GLOBAL_KEYBOARD_INPUT_ENABLED = false;
            }
            Items.CollectionChanged += Items_CollectionChanged;
            PropertyChanged += MpAvShortcutCollectionViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }


        #endregion

        #region Public Methods

        public async Task InitAsync() {
            IsBusy = true;

            await InitShortcutsAsync();
            InitExternalPasteTracking();

            UpdateRoutingProfileCommand.Execute(null);

            IsBusy = false;
        }

        public async Task<string> CreateOrUpdateViewModelShortcutAsync(MpIShortcutCommandViewModel iscvm) {

            string keys = null;
            ICommand command = iscvm.ShortcutCommand;
            string commandParameter = iscvm.ShortcutCommandParameter.ToStringOrDefault();
            MpShortcutType shortcutType = iscvm.ShortcutType;
            MpAvShortcutViewModel scvm = null;
            if (GetViewModelCommandShortcutId(iscvm) is int sid && Items.FirstOrDefault(x => x.ShortcutId == sid) is MpAvShortcutViewModel svm) {
                scvm = svm;
                keys = svm.KeyString;
            }
            string title = await iscvm.ShortcutType.GetShortcutTitleAsync(iscvm);
            object icon_obj = iscvm is MpIIconResource ? (iscvm as MpIIconResource).IconResourceObj : null;
            var result_tuple = await MpAvAssignShortcutViewModel.ShowAssignShortcutDialog(
                shortcutName: title,
                keys: keys,
                curShortcutId: scvm == null ? 0 : scvm.ShortcutId,
                assignmentType: shortcutType == MpShortcutType.None ?
                    MpShortcutAssignmentType.None :
                    shortcutType.CanBeGlobal() ?
                        MpShortcutAssignmentType.CanBeGlobalCommand :
                        MpShortcutAssignmentType.InternalCommand,
                iconResourceObj: icon_obj,
                owner: MpAvWindowManager.ActiveWindow);

            string shortcutKeyString = result_tuple == null ? null : result_tuple.Item1;
            MpRoutingType result_routing_type = result_tuple == null ? MpRoutingType.None : result_tuple.Item2;

            if (shortcutType == MpShortcutType.None) {
                // presume this is just a key recorer request
                return shortcutKeyString;
            }

            if (shortcutKeyString == null) {
                //if assignment was canceled ignore but reset skl
                shortcutKeyString = string.Empty;
            } else if (shortcutKeyString == string.Empty) {
                //if an empty assignment was ok'd check if exists 

                //if it does clear, save and unregister
                if (scvm != null) {

                    if (scvm.IsCustom) {
                        // clearing custom implies delete
                        Items.Remove(scvm);
                        await scvm.Shortcut.DeleteFromDatabaseAsync();
                    } else {
                        scvm.RoutingType = result_routing_type;
                        scvm.ClearShortcutKeyString();
                        await scvm.Shortcut.WriteToDatabaseAsync();
                    }
                } else {
                    //nothing to do since no shortcut created
                }
            } else if (scvm == null) {
                //if new shortcut

                //copyitem direct, tag internal, analyzer internal
                var sc = await MpShortcut.CreateAsync(
                    keyString: shortcutKeyString,
                    routeType: result_routing_type,
                    shortcutType: shortcutType,
                    commandParameter: commandParameter);
                scvm = await CreateShortcutViewModel(sc, command);
                Items.Add(scvm);
            } else {
                //if shorcut updated
                scvm.KeyString = shortcutKeyString;
                scvm.RoutingType = result_routing_type;
                await Task.Delay(15);
                while (scvm.IsBusy) {
                    await Task.Delay(100);
                }
                await scvm.InitializeAsync(scvm.Shortcut, scvm.ShortcutCommand);
            }

            return shortcutKeyString;
        }


        public int GetViewModelCommandShortcutId(MpIShortcutCommandViewModel scvm) {
            if (GetViewModelCommandShortcut(scvm) is MpAvShortcutViewModel svm) {
                return svm.ShortcutId;
            }
            return 0;
        }
        public string GetViewModelCommandShortcutKeyString(MpIShortcutCommandViewModel scvm) {
            if (GetViewModelCommandShortcut(scvm) is MpAvShortcutViewModel svm) {
                return svm.KeyString;
            }
            return string.Empty;
        }

        public MpAvShortcutViewModel GetViewModelCommandShortcut(MpIShortcutCommandViewModel scvm) {
            if (scvm is MpAvShortcutViewModel svm) {
                return svm;
            }

            var matches = Items
                .Where(x => x.Shortcut.IsShortcutCommand(scvm));

            if (matches.FirstOrDefault() is MpAvShortcutViewModel match) {
                if (matches.Count() > 1) {
                    MpDebug.Break("Shortcut error, duplicate equality detected");
                }
                return match;
            }
            return null;
        }

        public void RefreshFilters() {
            OnPropertyChanged(nameof(FilteredAppItems));
            OnPropertyChanged(nameof(FilteredGlobalItems));
            OnPropertyChanged(nameof(FilteredCustomItems));
            FilteredItems.ForEach(x => x.OnPropertyChanged(nameof(x.SelectedRoutingTypeIdx)));
        }
        #endregion

        #region Protected Methods

        #region Db Overrides

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                Dispatcher.UIThread.Post(() => {
                    UpdateRoutingProfileCommand.Execute(null);
                });
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc && sc.ShortcutType.IsEditorShortcut()) {
                UpdateEditorShortcutsMessageStr();
                Dispatcher.UIThread.Post(() => {
                    UpdateRoutingProfileCommand.Execute(null);

                    MpAvClipTrayViewModel.Instance
                        .AllActiveItems
                        .Where(x => !x.IsAnyPlaceholder)
                        .Select(x => x.GetContentView())
                        .OfType<MpAvContentWebView>()
                        .Where(x => x != null)
                        .ForEach(x => x.SendMessage($"updateShortcuts_ext('{EditorShortcutsMsgBase64}')"));
                });
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            MpAvShortcutViewModel scvmToRemove = null;
            if (e is MpShortcut sc) {
                UpdateRoutingProfileCommand.Execute(null);
                scvmToRemove = Items.FirstOrDefault(x => x.ShortcutId == sc.Id);
            } else if (e is MpCopyItem ci) {
                scvmToRemove = Items.FirstOrDefault(x => x.CommandParameter == ci.Id.ToString() && x.ShortcutType == MpShortcutType.PasteCopyItem);
            } else if (e is MpTag t) {
                scvmToRemove = Items.FirstOrDefault(x => x.CommandParameter == t.Id.ToString() && x.ShortcutType == MpShortcutType.SelectTag);
            } else if (e is MpPluginPreset aip) {
                scvmToRemove = Items.FirstOrDefault(x => x.CommandParameter == aip.Id.ToString() && x.ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset);
            } else if (e is MpAction a) {
                scvmToRemove = Items.FirstOrDefault(x => x.CommandParameter == a.Id.ToString() && x.ShortcutType == MpShortcutType.InvokeTrigger);
            }
            if (scvmToRemove != null) {
                Dispatcher.UIThread.Post(() => {
                    Items.Remove(scvmToRemove);
                });
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private void MpAvShortcutCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(RoutingProfileType):
                    OnPropertyChanged(nameof(RoutingProfileTypeIdx));
                    MpMessenger.SendGlobal(MpMessageType.ShortcutRoutingProfileChanged);
                    break;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            UpdateFilteredItems();
        }

        private async Task InitShortcutsAsync() {
            Dispatcher.UIThread.VerifyAccess();
            //using mainwindow, map all saved shortcuts to their commands
            var scl = await MpDataModelProvider.GetItemsAsync<MpShortcut>();

            //IsCustomRoutingEnabled = scl.All(x => x.RoutingType == MpRoutingType.Internal || x.RoutingType == MpRoutingType.Direct);

            async Task AddShortcutViewModelAsync(MpShortcut sc) {
                AppCommandLookup.TryGetValue(sc.ShortcutType, out ICommand shortcutCommand);
                var scvm = await CreateShortcutViewModel(sc, shortcutCommand);
                Items.Add(scvm);
            }
            await Task.WhenAll(scl.Select(x => AddShortcutViewModelAsync(x)));
            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            // signal keystring change for sys tray shortcuts
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.KeyString)));
            UpdateEditorShortcutsMessageStr();
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsFilterTextChanged:
                    UpdateFilteredItems();
                    break;
                case MpMessageType.MainWindowLoadComplete:
                    StartInputListener();
                    break;
                case MpMessageType.MainWindowClosed:
                    IsApplicationShortcutsEnabled = false;
                    break;
                case MpMessageType.MainWindowOpened:
                    IsApplicationShortcutsEnabled = true;
                    break;
                case MpMessageType.ShortcutAssignmentActivated:
                    IsShortcutsEnabled = false;
                    _keyboardGestureHelper.ClearCurrentGesture();
                    break;
                case MpMessageType.ShortcutAssignmentDeactivated:
                    IsShortcutsEnabled = true;
                    break;
                case MpMessageType.SettingsWindowOpened:
                    Dispatcher.UIThread.Post(() => {
                        Task.WhenAll(Items.Where(x => x.IsCustom).Select(x => x.SetShortcutNameAsync())).FireAndForgetSafeAsync(this);
                    });
                    break;
            }
        }
        private async Task<MpAvShortcutViewModel> CreateShortcutViewModel(MpShortcut sc, ICommand comamnd) {
            MpAvShortcutViewModel nscvm = new MpAvShortcutViewModel(this);
            await nscvm.InitializeAsync(sc, comamnd);
            return nscvm;
        }

        private void UpdateEditorShortcutsMessageStr() {
            _editorShortcutsMsgBase64 =
                new MpQuillEditorShortcutKeystringMessage() {
                    shortcuts =
                            Items
                            .Where(x => x.IsEditorShortcut)
                            .Select(x => new MpQuillEditorShortcutKeystringItemFragment() {
                                shortcutType = x.ShortcutType.ToString(),
                                keys = x.KeyString
                            }).ToList()
                }.SerializeJsonObjectToBase64();
        }

        private void UpdateFilteredItems() {
            int selected_scid = SelectedItem == null ? 0 : SelectedItem.ShortcutId;
            FilteredItems.Clear();
            foreach (var scvm in Items.OrderBy(x => x.ShortcutDisplayName)) {
                if (scvm.IsFilterMatch(MpAvSettingsViewModel.Instance.FilterText)) {
                    FilteredItems.Add(scvm);
                }
            }
            if (selected_scid > 0 &&
                FilteredItems.All(x => x.ShortcutId != selected_scid)) {
                SelectedItem = null;
            }
            RefreshFilters();
        }

        #region Routing Profile
        private MpShortcutRoutingProfileType DetermineShortcutRoutingProfileType() {
            var globalable_scvml = Items.Where(x => x.CanBeGlobalShortcut);
            if (globalable_scvml.All(x => x.RoutingType == MpRoutingType.Internal)) {
                return MpShortcutRoutingProfileType.Internal;
            }
            var toggle_mw_override = globalable_scvml.FirstOrDefault(x => x.ShortcutType == MpShortcutType.ToggleMainWindow && x.RoutingType == MpRoutingType.Override);
            if (toggle_mw_override == null ||
                globalable_scvml.Where(x => x != toggle_mw_override).Any(x => x.RoutingType != MpRoutingType.Passive)) {
                return MpShortcutRoutingProfileType.Custom;
            }

            return MpShortcutRoutingProfileType.Global;
        }

        private async Task SetShortcutRoutingToProfileTypeAsync(MpShortcutRoutingProfileType new_profile_type) {
            var globalable_scvml = Items.Where(x => x.CanBeGlobalShortcut);
            foreach (var globalable_scvm in globalable_scvml) {
                globalable_scvm.RoutingType = new_profile_type.GetProfileBasedRoutingType(globalable_scvm.ShortcutType);
            }
            await Task.Delay(100);
            while (globalable_scvml.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            RoutingProfileType = DetermineShortcutRoutingProfileType();
            MpDebug.Assert(RoutingProfileType == new_profile_type, $"Routing profile mismatch. Set to '{new_profile_type}' but Determined is '{RoutingProfileType}'");
        }

        #endregion

        #region Paste Tracking

        private string _activePasteKeystring = null;
        private string _activeCopyKeystring = null;

        private MpPortableProcessInfo _activeProcessInfo = null;
        public void InitExternalPasteTracking() {
            if (MpAvPrefViewModel.Instance.TrackExternalPasteHistory) {
                EnableExternalPasteTracking();
            } else {
                DisableExternalPasteTracking();
            }
        }
        private void EnableExternalPasteTracking() {
            //_activePasteKeystring = Mp.Services.PlatformShorcuts.PasteKeys;
            Mp.Services.ProcessWatcher.OnAppActivated += ProcessWatcher_OnAppActivated;
            OnGlobalPasteShortcutPerformed += MpAvShortcutCollectionViewModel_OnGlobalPasteShortcutPerformed;
        }


        private void DisableExternalPasteTracking() {
            //_activePasteKeystring = null;
            Mp.Services.ProcessWatcher.OnAppActivated -= ProcessWatcher_OnAppActivated;
            OnGlobalPasteShortcutPerformed -= MpAvShortcutCollectionViewModel_OnGlobalPasteShortcutPerformed;
        }

        private void ProcessWatcher_OnAppActivated(object sender, MpPortableProcessInfo e) {
            _activeProcessInfo = e;
            if (MpAvAppCollectionViewModel.Instance.GetAppByProcessInfo(e) is MpAvAppViewModel avm) {
                if (avm.PasteShortcutViewModel != null &&
                    avm.PasteShortcutViewModel.HasShortcut) {
                    _activePasteKeystring = avm.PasteShortcutViewModel.ShortcutCmdKeyString;
                }
                if (avm.CopyShortcutViewModel != null &&
                    avm.CopyShortcutViewModel.HasShortcut) {
                    _activeCopyKeystring = avm.CopyShortcutViewModel.ShortcutCmdKeyString;
                }
            } else {
                _activePasteKeystring = Mp.Services.PlatformShorcuts.PasteKeys;
                _activeCopyKeystring = Mp.Services.PlatformShorcuts.CopyKeys;
            }
        }

        private void MpAvShortcutCollectionViewModel_OnGlobalPasteShortcutPerformed(object sender, EventArgs e) {
            // called from global key up when paste key string was performed...

            Task.Run(async () => {
                try {
                    // grab current cb text
                    string cbText = await TopLevel.GetTopLevel(MpAvMainView.Instance).Clipboard.GetTextAsync();
                    if (!string.IsNullOrEmpty(cbText)) {
                        // find matching data object items w/ current text

                        var match_doil = await MpDataModelProvider.GetDataObjectItemsForFormatByDataAsync(MpPortableDataFormats.Text, cbText);
                        var dobj_idl =
                            match_doil
                                .Select(x => x.DataObjectId)
                                .Distinct();
                        if (!dobj_idl.Any()) {
                            return;
                        }

                        // get ALL copyitems for dataobjects (maybe multiple if duplicates are not ignored)
                        var match_cil = await MpDataModelProvider.GetCopyItemsByDataObjectIdListAsync(dobj_idl.ToList());
                        if (!match_cil.Any()) {
                            return;
                        }

                        // NOTE pasteCount is used for relevanceScore cause active app recog and/or ignores may
                        // make trans history wishy washy also still tentatively using transactions so just as a backup i guess
                        match_cil
                        .ForEach(x => x.PasteCount = x.PasteCount + 1);
                        Task.WhenAll(match_cil.Select(x => x.WriteToDatabaseAsync())).FireAndForgetSafeAsync(this);
                        match_cil.ForEach(x => MpConsole.WriteLine($"New paste count: {x.PasteCount} for '{x}'"));
                        if (_activeProcessInfo != null) {
                            string pasted_app_url = await Mp.Services.SourceRefTools.FetchOrCreateAppRefUrlAsync(_activeProcessInfo);
                            if (string.IsNullOrEmpty(pasted_app_url)) {
                                // f'd
                                MpDebug.Break();
                                return;
                            }

                            Task.WhenAll(match_cil.Select(x =>
                                Mp.Services.TransactionBuilder.ReportTransactionAsync(
                                    copyItemId: x.Id,
                                    reqType: MpJsonMessageFormatType.DataObject,
                                    req: null,//mpdo.SerializeData(),
                                    respType: MpJsonMessageFormatType.None,
                                    resp: null,
                                    ref_uris: new[] { pasted_app_url },
                                    transType: MpTransactionType.Pasted))).FireAndForgetSafeAsync(this);
                        }
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteLine("Global Keyboard Paste watch exception getting text: " + ex);
                }
            });
        }
        #endregion

        #region Global Input

        private void CreateGlobalInputHooks() {
            if (_hook == null) {

                //_keyboardGestureHelper.StartChecker();
                _hook = new SimpleGlobalHook();

                if (IS_GLOBAL_KEYBOARD_INPUT_ENABLED) {
                    _hook.KeyPressed += Hook_KeyPressed;
                    _hook.KeyReleased += Hook_KeyReleased;
                    //_hook.KeyTyped += Hook_KeyTyped;
                }

                if (IS_GLOBAL_MOUSE_INPUT_ENABLED) {
                    _hook.MouseWheel += Hook_MouseWheel;
                    _hook.MouseMoved += Hook_MouseMoved;
                    _hook.MousePressed += Hook_MousePressed;
                    _hook.MouseReleased += Hook_MouseReleased;
                    _hook.MouseClicked += Hook_MouseClicked;
                    _hook.MouseDragged += Hook_MouseDragged;
                }
                if (IS_GLOBAL_INPUT_LOGGING_ENABLED) {
                    var logSource = LogSource.RegisterOrGet(minLevel: LogLevel.Debug);
                    logSource.MessageLogged += OnMessageLogged;

                    void OnMessageLogged(object? sender, LogEventArgs e) =>
                        MpConsole.WriteLine($" [HOOK LOG] {e.LogEntry.FullText}");
                }

            } else if (_hook.IsRunning) {
                return;
            }

            _hook.RunAsync();
        }


        private void DisposeGlobalInputHooks() {
            if (_hook != null) {
                _hook.MouseWheel -= Hook_MouseWheel;

                _hook.MouseMoved -= Hook_MouseMoved;

                _hook.MousePressed -= Hook_MousePressed;
                _hook.MouseReleased -= Hook_MouseReleased;

                _hook.MouseClicked -= Hook_MouseClicked;

                _hook.MouseDragged -= Hook_MouseDragged;

                _hook.KeyPressed -= Hook_KeyPressed;
                _hook.KeyReleased -= Hook_KeyReleased;

                _hook.Dispose();
                _hook = null;
            }
        }

        #region Mouse Event Handlers

        private void Hook_MouseWheel(object? sender, MouseWheelHookEventArgs e) {
            HandlePointerWheel(new MpPoint((double)e.Data.X, (double)e.Data.Y));
        }

        private void Hook_MouseMoved(object? sender, MouseHookEventArgs e) {
            var gmp = e.Data.GetScaledScreenPoint();
            //MpConsole.WriteLine("Unscaled: " + UnscaledGlobalMouseLocation + " Scaled: " + gmp + " PixelDensity: " + MpPlatformWrapper.Services.ScreenInfoCollection.PixelScaling);
            HandlePointerMove(gmp);
        }


        private void Hook_MousePressed(object sender, MouseHookEventArgs e) {
            if (!Mp.Services.StartupState.IsPlatformLoaded) {
                return;
            }
            MpPortablePointerButtonType button = e.Data.Button.ToPortableButton();
            if (button == MpPortablePointerButtonType.Left) {
                HandlePointerPress(true);
                return;
            }

            if (button == MpPortablePointerButtonType.Right) {
                HandlePointerPress(false);

                if (MpAvClipTrayViewModel.Instance.IsRightClickPasteMode) {
                    e.SuppressEvent = true;
                    Dispatcher.UIThread.Post(() => {
                        Mp.Services.KeyStrokeSimulator
                            .SimulateKeyStrokeSequence(
                                string.IsNullOrEmpty(_activePasteKeystring) ?
                                    Mp.Services.PlatformShorcuts.PasteKeys :
                                    _activePasteKeystring);
                    });
                }
                return;
            }

            MpConsole.WriteTraceLine("Unknown mouse button pressed: SharpButton: " + e.Data.Button + " PortableButton: " + button);
        }
        private void Hook_MouseReleased(object sender, MouseHookEventArgs e) {
            // NOTE: SharpHook Release event shows Button released

            MpPortablePointerButtonType button = e.Data.Button.ToPortableButton();
            if (button == MpPortablePointerButtonType.Left) {
                HandlePointerReleased(true);

                if (MpAvClipTrayViewModel.Instance.IsAutoCopyMode) {
                    Dispatcher.UIThread.Post(() => {
                        Mp.Services.KeyStrokeSimulator
                            .SimulateKeyStrokeSequence(
                                string.IsNullOrEmpty(_activeCopyKeystring) ?
                                    Mp.Services.PlatformShorcuts.CopyKeys :
                                    _activeCopyKeystring);
                    });
                }
            } else if (button == MpPortablePointerButtonType.Right) {
                HandlePointerReleased(false);
                if (MpAvClipTrayViewModel.Instance.IsRightClickPasteMode) {
                    e.SuppressEvent = true;
                }
            } else {
                MpConsole.WriteTraceLine("Unknown mouse button released: SharpButton: " + e.Data.Button + " PortableButton: " + button);
            }
        }

        private void Hook_MouseClicked(object sender, MouseHookEventArgs e) {
            MpPortablePointerButtonType button = e.Data.Button.ToPortableButton();
            if (button == MpPortablePointerButtonType.Left) {
                HandlePointerClick(true);
            } else if (button == MpPortablePointerButtonType.Right) {
                HandlePointerClick(false);
                if (MpAvClipTrayViewModel.Instance.IsRightClickPasteMode) {
                    e.SuppressEvent = true;
                }
            } else {
                MpConsole.WriteTraceLine("Unknown mouse button clicked: " + e.Data.Button);
            }
        }

        private void Hook_MouseDragged(object sender, MouseHookEventArgs e) {
            // NOTE: SharpHook Drag event doesn't receive button info

            if (GlobalIsMouseLeftButtonDown) {
                // NOTE only flag drag when left button is down, any other is poop
                if (!GlobalIsPointerDragging) {
                    Dispatcher.UIThread.Post(() => {

                        GlobalIsPointerDragging = true;
                        OnGlobalDragBegin?.Invoke(this, null);
                    });
                }
            }

            var gmp = e.Data.GetScaledScreenPoint();
            HandlePointerMove(gmp);
        }

        #endregion

        #region Keyboard EventHandlers


        private void Hook_KeyPressed(object sender, KeyboardHookEventArgs e) {
            if (IgnoreDebug(true)) {
                return;
            }
            _keyboardGestureHelper.AddKeyDown(e.Data.KeyCode);
            string keyStr = Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { e.Data.KeyCode } });
            //MpConsole.WriteLine($"Key[{e.Data.KeyCode}] '{keyStr}' PRESSED");
            Dispatcher.UIThread.Post(() => HandleGlobalKeyEvents(keyStr, true));
            if (!IsShortcutsEnabled) {
                return;
            }

            string down_gesture = _keyboardGestureHelper.GetCurrentGesture();
            _exact_match =
                AvailableItems
                .FirstOrDefault(x => x.KeyString == down_gesture);

            if (MpAvPrefViewModel.Instance.IsAutoSearchEnabled &&
                MpAvWindowManager.MainWindow != null &&
                MpAvWindowManager.MainWindow.IsActive &&
                Mp.Services.StartupState.IsReady) {
                bool was_auto_search_attempt = Dispatcher.UIThread.Invoke(() => {
                    /*
                        In global key DOWN, if auto search pref enabled and no other key 
                        is down and is not exact match and mw is active/open and 
                        focus is not textbox/ autocomplete/ editable wv (will only be valid on first typed key). 
                        Then  set searchbox to focus (should auto trigger expand) so input passes to searchbox. 
                        If not work would need to use keytyped event or keyup and figure out the char..
                    */

                    bool can_auto_search =
                        MpAvWindowManager.MainWindow.IsActive &&
                        MpAvWindowManager.MainWindow.IsVisible &&
                        !MpAvFocusManager.Instance.IsTextInputControlFocused &&
                        _exact_match == null &&
                        Mp.Services.KeyDownHelper.Downs.Count == 1 &&
                        e.Data.KeyCode.IsAlphaNumeric();

                    if (can_auto_search) {
                        string text_to_pass = ((KeyCode)Mp.Services.KeyDownHelper.Downs[0]).GetKeyLiteral().ToLower();
                        MpAvSearchBoxViewModel.Instance.BeginAutoSearchCommand.Execute(text_to_pass);
                    }
                    return can_auto_search;
                });
                if (was_auto_search_attempt) {
                    // BUG this is an attempt to get rid of orphan downs that randomly popup
                    // maybe related to focus or something 
                    _exact_match = null;
                    _keyboardGestureHelper.RemoveKeyDown(e.Data.KeyCode);
                    MpConsole.WriteLine($"removing down cause auto search was attempted. down key: '{e.Data.KeyCode}'");
                    return;
                }
            }

            if (_exact_match == null) {
                return;
            }
            // only suppress input key
            e.SuppressEvent = _exact_match.SuppressesKeys && !e.Data.KeyCode.IsModKey();
            MpConsole.WriteLine($"Recognized GESTURE: '{down_gesture}' SHORTCUT: {_exact_match.ShortcutType}");
        }


        private void Hook_KeyReleased(object sender, KeyboardHookEventArgs e) {
            HandleReleaseOrTyped(e, true);
        }

        private void Hook_KeyTyped(object sender, KeyboardHookEventArgs e) {
            //HandleReleaseOrTyped(e, false);≡
            string keyStr = Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { e.Data.KeyCode } });
            //MpConsole.WriteLine($"Key[{e.Data.KeyCode}] '{keyStr}' TYPED");
        }

        private void HandleReleaseOrTyped(KeyboardHookEventArgs e, bool isRelease) {
            if (IgnoreDebug(false)) {
                return;
            }
            bool was_down = _keyboardGestureHelper.RemoveKeyDown(e.Data.KeyCode);
            if (!was_down) {
                // NOTE typed receives up before release for input keys
                return;
            }
            string keyStr = Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { e.Data.KeyCode } });
            //MpConsole.WriteLine($"Key[{e.Data.KeyCode}] '{keyStr}' RELEASED");
            Dispatcher.UIThread.Post(() => HandleGlobalKeyEvents(keyStr, false));
            if (!IsShortcutsEnabled) {
                return;
            }
            if (MpAvPrefViewModel.Instance.TrackExternalPasteHistory &&
                _activePasteKeystring != null &&
                !e.Data.KeyCode.IsModKey() &&
                _keyboardGestureHelper.GetCurrentGesture() == _activePasteKeystring &&
                !MpAvClipTrayViewModel.Instance.IsPasting) {
                OnGlobalPasteShortcutPerformed?.Invoke(this, null);
            }

            if (_exact_match == null) {
                return;
            }
            if (e.Data.KeyCode.IsModKey()) {
                // only invoke if up is input key
                _exact_match = null;
                return;
            }

            // store local ref to match in case its reset during sim
            MpAvShortcutViewModel match_to_execute = _exact_match;

            if (match_to_execute.RoutingType == MpRoutingType.Bubble) {
                Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence(new[] { new[] { e.Data.KeyCode }.ToList() }.ToList());
            }
            Dispatcher.UIThread.Invoke(() => {
                match_to_execute.PerformShortcutCommand.Execute(null);
            });
            if (match_to_execute.RoutingType == MpRoutingType.Tunnel) {
                Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence(new[] { new[] { e.Data.KeyCode }.ToList() }.ToList());
            }
        }
        private bool IgnoreDebug(bool isDown) {
#if DEBUG
            //if (Mp.Services.ProcessWatcher.LastProcessInfo == null ||
            //    !Mp.Services.ProcessWatcher.LastProcessInfo.ProcessPath.Contains("devenv.exe")) {
            //    return false;
            //}
            //return true;
            return false;
#else
            return false;
#endif

        }
        #endregion

        #endregion

        #region Input Wrappers

        #region Pointer

        private void HandlePointerPress(bool isLeftButton) {
            Dispatcher.UIThread.Post(() => {
                if (isLeftButton) {
                    GlobalIsMouseLeftButtonDown = true;
                    GlobalMouseLeftButtonDownLocation = GlobalMouseLocation;
                    LastLeftClickDateTime = DateTime.Now;
                } else {
                    GlobalIsMouseRightButtonDown = true;
                    LastRightClickDateTime = DateTime.Now;
                    if (GlobalIsPointerDragging) {
                        // NOTE no matter what when right mouse is pressed don't treat as dragging (drag handler still processes mouse move)
                        GlobalIsPointerDragging = false;
                        OnGlobalDragEnd?.Invoke(this, null);
                    }
                }

                OnGlobalMousePressed?.Invoke(this, isLeftButton);
            });

        }

        private void HandlePointerReleased(bool isLeftButton) {
            Dispatcher.UIThread.Post(() => {
                if (isLeftButton) {
                    GlobalMouseLeftButtonDownLocation = null;
                    GlobalIsMouseLeftButtonDown = false;
                    if (GlobalIsPointerDragging) {
                        // this signals end of left pointer drag
                        GlobalIsPointerDragging = false;
                        OnGlobalDragEnd?.Invoke(this, null);
                    }

                } else {
                    GlobalIsMouseRightButtonDown = false;
                }

                OnGlobalMouseReleased?.Invoke(this, isLeftButton);
            });

        }
        private void HandlePointerMove(MpPoint gmp) {
            Dispatcher.UIThread.Post(() => {
                GlobalMouseLocation = gmp;
                OnGlobalMouseMove?.Invoke(typeof(MpAvShortcutCollectionViewModel).ToString(), GlobalMouseLocation);

                if (GlobalIsPointerDragging) {
                    OnGlobalDrag?.Invoke(typeof(MpAvShortcutCollectionViewModel).ToString(), GlobalMouseLocation);
                }

            });
        }


        private void HandlePointerClick(bool isLeftButton) {
            Dispatcher.UIThread.Post(() => {
                OnGlobalMouseClicked?.Invoke(this, isLeftButton);

            });
        }

        private void HandlePointerWheel(MpPoint delta) {
            Dispatcher.UIThread.Post(() => {

                OnGlobalMouseWheelScroll?.Invoke(typeof(MpAvShortcutCollectionViewModel).ToString(), delta);
            });
        }

        #endregion

        #region Keyboard

        private void HandleGlobalKeyEvents(string keyStr, bool isDown) {
            if (string.IsNullOrEmpty(keyStr)) {
                // what key is this with no string val?
                MpDebug.Break();
                keyStr = "???";
            }

            if (keyStr.IsShift()) {
                GlobalIsShiftDown = isDown;
            }
            if (keyStr.IsAlt()) {
                GlobalIsAltDown = isDown;
            }
            if (keyStr.IsCtrl()) {
                GlobalIsCtrlDown = isDown;
                //MpConsole.WriteLine("Global ctrl key: DOWN");
            }
            if (keyStr.IsMeta()) {
                GlobalIsMetaDown = isDown;
            }
            if (keyStr.IsEscape()) {
                GlobalIsEscapeDown = isDown;
                if (isDown) {
                    OnGlobalEscKeyPressed?.Invoke(this, EventArgs.Empty);
                } else {
                    OnGlobalEscapeReleased?.Invoke(this, EventArgs.Empty);
                }
            }
            if (isDown) {
                OnGlobalKeyPressed?.Invoke(this, keyStr);
            } else {
                OnGlobalKeyReleased?.Invoke(this, keyStr);
            }

        }



        #region Gesture Handling



        #endregion

        #endregion

        #endregion

        #endregion

        #region Commands
        public ICommand ToggleGlobalHooksCommand => new MpCommand<object>(
            (args) => {
                if (args is bool boolArg &&
                    boolArg == IsGlobalHooksPaused) {
                    // already in debug state

                    MpConsole.WriteLine($"Global hook change rejected. State: {IsGlobalHooksPaused} args: {args}");
                    return;
                }
                if (IsGlobalHooksPaused) {
                    //resume
                    CreateGlobalInputHooks();
                    IS_GLOBAL_KEYBOARD_INPUT_ENABLED = true;
                    IS_GLOBAL_MOUSE_INPUT_ENABLED = true;
                    IsGlobalHooksPaused = false;
                    MpConsole.WriteLine($"Global hooks resumed. args: {args}");
                } else {
                    // pause
                    DisposeGlobalInputHooks();
                    _keyboardGestureHelper.ClearCurrentGesture();
                    IS_GLOBAL_KEYBOARD_INPUT_ENABLED = false;
                    IS_GLOBAL_MOUSE_INPUT_ENABLED = false;
                    IsGlobalHooksPaused = true;
                    MpConsole.WriteLine($"Global hooks paused. args: {args}");
                }
            });

        public ICommand ShowAssignShortcutDialogCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is MpIShortcutCommandViewModel scvm) {
                    string result = await CreateOrUpdateViewModelShortcutAsync(scvm);
                    if (args is MpAvShortcutRecorderParameterViewModel scpvm &&
                        scpvm.Parent is MpAvKeySimulatorActionViewModel ksavm) {
                        // handle special case for input and hard-assign
                        // param's keystring once sure parent is key simulator
                        scpvm.KeyString = result;
                    }
                }
                UpdateFilteredItems();
                MpAvDataGridRefreshExtension.RefreshDataGrid(this);
            });

        public ICommand DeleteShortcutCommand => new MpCommand<object>(
            async (args) => {
                //MpConsole.WriteLine("Deleting shortcut row: " + SelectedShortcutIndex);
                var scvm = args as MpAvShortcutViewModel;
                //await RemoveAsync(scvm);
                await scvm.Shortcut.DeleteFromDatabaseAsync();
            }, (args) => args is MpAvShortcutViewModel svm && svm.CanDelete);


        public ICommand ResetShortcutCommand => new MpCommand<object>(
            async (args) => {
                //MpConsole.WriteLine("Reset row: " + SelectedShortcutIndex);

                var scvm = args as MpAvShortcutViewModel;
                scvm.KeyString = scvm.Shortcut.DefaultKeyString;
                await scvm.InitializeAsync(scvm.Shortcut, scvm.ShortcutCommand);
                await scvm.Shortcut.WriteToDatabaseAsync();
            }, (args) => args is MpAvShortcutViewModel svm && !string.IsNullOrEmpty(svm.DefaultKeyString));

        public ICommand UpdateRoutingProfileCommand => new MpAsyncCommand<object>(
            async (args) => {
                Dispatcher.UIThread.VerifyAccess();
                if (args is not MpShortcutRoutingProfileType srpt) {
                    // data changed, assess state
                    RoutingProfileType = DetermineShortcutRoutingProfileType();
                    return;
                }

                bool confirm = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                    title: UiStrings.CommonNtfConfirmTitle,
                    message: string.Format(UiStrings.ShortcutsRoutingProfileChangeNtfMsg, srpt),
                    iconResourceObj: "WarningImage");
                if (!confirm) {
                    // user canceled, send change msg to update pref ui
                    MpMessenger.SendGlobal(MpMessageType.ShortcutRoutingProfileChanged);
                    return;
                }
                await SetShortcutRoutingToProfileTypeAsync(srpt);
            });

        public ICommand ResetAllShortcutsCommand => new MpAsyncCommand(async () => {
            var drpt = MpAvPrefViewModel.Instance.DefaultRoutingProfileType;
            bool result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                title: UiStrings.CommonNtfConfirmTitle,
                message: UiStrings.ShortcutsResetNtfMsg,
                iconResourceObj: "WarningImage");
            if (!result) {
                return;
            }
            Items.Clear();
            await Mp.Services.DefaultDataCreator.ResetShortcutsAsync();
            await InitShortcutsAsync();
        });

        #endregion
    }
}
