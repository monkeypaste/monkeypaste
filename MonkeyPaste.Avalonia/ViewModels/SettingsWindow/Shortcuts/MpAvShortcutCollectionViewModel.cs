using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Avalonia.Utils.Extensions;
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;


namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutCollectionViewModel :
        MpAvSelectorViewModelBase<object, MpAvShortcutViewModel>,
        MpIGlobalInputListener,
        MpIDndUserCancelNotifier {

        #region Statics
        public static bool IS_GLOBAL_MOUSE_INPUT_ENABLED { get; set; } = true;
        public static bool IS_GLOBAL_KEYBOARD_INPUT_ENABLED { get; set; } = true;
        public static bool IS_GLOBAL_INPUT_ENABLED => IS_GLOBAL_KEYBOARD_INPUT_ENABLED || IS_GLOBAL_MOUSE_INPUT_ENABLED;
        public static bool IS_PSEUDO_GLOBAL_INPUT_ENABLED { get; set; } = false;
        public const double MIN_GLOBAL_DRAG_DIST = 20;

        #endregion

        #region Private Variables

        private MpKeyGestureHelper _keyboardGestureHelper;

        private List<string> _suppressedKeys = new List<string>();

        private const int _MAX_WAIT_TO_EXECUTE_SHORTCUT_MS = 500;

        private SimpleGlobalHook _hook;

        private CancellationTokenSource _simInputCts;

        #endregion

        #region Statics

        private static MpAvShortcutCollectionViewModel _instance;
        public static MpAvShortcutCollectionViewModel Instance => _instance ?? (_instance = new MpAvShortcutCollectionViewModel());

        #endregion

        #region Interfaces

        #region MpIGlobalInputListener
        public void StartInputListener() {
            if (IS_PSEUDO_GLOBAL_INPUT_ENABLED) {
                Dispatcher.UIThread.Post(() => {
                    CreatePseudoGlobalInputHooks(MpAvMainView.Instance);
                });
            }
            if (IS_GLOBAL_INPUT_ENABLED) {
                CreateGlobalInputHooks();
            }
        }
        public void StopInputListener() {
            if (IS_PSEUDO_GLOBAL_INPUT_ENABLED) {
                Dispatcher.UIThread.Post(() => {
                    DisposePseudoGlobalInputHooks(MpAvMainView.Instance);
                });
            }
            if (IS_GLOBAL_INPUT_ENABLED) {
                DisposeGlobalInputHooks();
            }
        }

        #endregion

        #endregion

        #region Properties

        #region View Models
        public IEnumerable<MpAvShortcutViewModel> FilteredItems =>
            Items
            .Where(x => (x as MpIFilterMatch).IsMatch(MpAvSettingsViewModel.Instance.FilterText));

        public IEnumerable<MpAvShortcutViewModel> CustomShortcuts =>
            FilteredItems.Where(x => x.IsCustom);

        public IEnumerable<MpAvShortcutViewModel> ApplicationShortcuts =>
            FilteredItems.Where(x => !x.IsCustom);

        #endregion

        #region State

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


        #endregion

        #region Constructors
        public MpAvShortcutCollectionViewModel() : base(null) {
            Mp.Services.GlobalInputListener = this;
            if (!Mp.Services.PlatformInfo.IsDesktop) {
                IS_GLOBAL_MOUSE_INPUT_ENABLED = false;
                IS_GLOBAL_KEYBOARD_INPUT_ENABLED = false;

                IS_PSEUDO_GLOBAL_INPUT_ENABLED = false;
            }
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(FilteredItems));
            OnPropertyChanged(nameof(ApplicationShortcuts));
            OnPropertyChanged(nameof(CustomShortcuts));
        }

        #endregion

        #region Public Methods

        public async Task InitAsync() {
            _keyboardGestureHelper = new MpKeyGestureHelper();
            _simInputCts = new CancellationTokenSource();

            await InitShortcutsAsync();

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            if (MpPrefViewModel.Instance.TrackExternalPasteHistory) {
                EnableExternalPasteTracking();
            }
        }


        public async Task<string> CreateOrUpdateViewModelShortcutAsync(
            MpAvIShortcutCommandViewModel iscvm,
            object iconResourceObj = null) {

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

            string shortcutKeyString = await MpAvAssignShortcutViewModel.ShowAssignShortcutDialog(
                title,
                keys,
                scvm == null ? 0 : scvm.ShortcutId,
                MpShortcutAssignmentType.InternalOrGlobalCommand,
                iconResourceObj);

            if (shortcutKeyString == null) {
                //if assignment was canceled ignore but reset skl
                shortcutKeyString = string.Empty;
            } else if (shortcutKeyString == string.Empty) {
                //if an empty assignment was ok'd check if exists 

                //if it does clear, save and unregister
                if (scvm != null) {
                    scvm.ClearShortcutKeyString();
                    await scvm.Shortcut.WriteToDatabaseAsync();
                    scvm.Unregister();

                    if (scvm.IsCustom) {
                        Items.Remove(scvm);
                    }
                } else {
                    //nothing to do since no shortcut created
                }
            } else if (scvm == null) {
                //if new shortcut
                MpRoutingType routingType = shortcutType ==
                    MpShortcutType.PasteCopyItem ? MpRoutingType.Bubble : MpRoutingType.Internal;
                //copyitem direct, tag internal, analyzer internal
                var sc = await MpShortcut.CreateAsync(
                    keyString: shortcutKeyString,
                    routeType: routingType,
                    shortcutType: shortcutType,
                    commandParameter: commandParameter);
                scvm = await CreateShortcutViewModel(sc, command);
                Items.Add(scvm);
            } else {
                //if shorcut updated
                scvm.KeyString = shortcutKeyString;

                await scvm.InitializeAsync(scvm.Shortcut, scvm.ShortcutCommand);
            }

            return shortcutKeyString;
        }

        public int GetViewModelCommandShortcutId(MpAvIShortcutCommandViewModel scvm) {
            if (GetViewModelCommandShortcut(scvm) is MpAvShortcutViewModel svm) {
                return svm.ShortcutId;
            }
            return 0;
        }
        public string GetViewModelCommandShortcutKeyString(MpAvIShortcutCommandViewModel scvm) {
            if (GetViewModelCommandShortcut(scvm) is MpAvShortcutViewModel svm) {
                return svm.KeyString;
            }
            return string.Empty;
        }

        public MpAvShortcutViewModel GetViewModelCommandShortcut(MpAvIShortcutCommandViewModel scvm) {
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
        #endregion

        #region Protected Methods

        #region Db Overrides

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            MpAvShortcutViewModel scvmToRemove = null;
            if (e is MpShortcut sc) {
                scvmToRemove = Items.FirstOrDefault(x => x.ShortcutId == sc.Id);
            } else if (e is MpCopyItem ci) {
                scvmToRemove = Items.FirstOrDefault(x => x.CommandParameter == ci.Id.ToString() && x.ShortcutType == MpShortcutType.PasteCopyItem);
            } else if (e is MpTag t) {
                scvmToRemove = Items.FirstOrDefault(x => x.CommandParameter == t.Id.ToString() && x.ShortcutType == MpShortcutType.SelectTag);
            } else if (e is MpPluginPreset aip) {
                scvmToRemove = Items.FirstOrDefault(x => x.CommandParameter == aip.Id.ToString() && x.ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset);
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

        private async Task InitShortcutsAsync() {
            await Dispatcher.UIThread.InvokeAsync(async () => {
                //using mainwindow, map all saved shortcuts to their commands
                var scl = await MpDataModelProvider.GetItemsAsync<MpShortcut>();

                //IsCustomRoutingEnabled = scl.All(x => x.RoutingType == MpRoutingType.Internal || x.RoutingType == MpRoutingType.Direct);

                foreach (var sc in scl) {
                    ICommand shortcutCommand = null;
                    switch (sc.ShortcutType) {
                        case MpShortcutType.ToggleMainWindow:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.ToggleShowMainWindowCommand;
                            break;
                        case MpShortcutType.HideMainWindow:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.DecreaseFocusCommand;
                            break;
                        case MpShortcutType.ShowSettings:
                            shortcutCommand = MpAvSettingsViewModel.Instance.ShowSettingsWindowCommand;
                            break;
                        case MpShortcutType.ExitApplication:
                            shortcutCommand = MpAvSystemTrayViewModel.Instance.ExitApplicationCommand;
                            break;
                        case MpShortcutType.ToggleAppendMode:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ToggleAppendModeCommand;
                            break;
                        case MpShortcutType.ToggleAppendLineMode:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ToggleAppendLineModeCommand;
                            break;
                        case MpShortcutType.ToggleAutoCopyMode:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ToggleAutoCopyModeCommand;
                            break;
                        case MpShortcutType.ToggleRightClickPasteMode:
                            //right click paste mode
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ToggleRightClickPasteCommand;
                            break;
                        case MpShortcutType.PasteSelectedItems:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.PasteSelectedClipTileFromShortcutCommand;
                            break;
                        case MpShortcutType.PasteHere:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.PasteCurrentClipboardIntoSelectedTileCommand;
                            break;
                        case MpShortcutType.DeleteSelectedItems:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.DeleteSelectedClipsCommand;
                            break;
                        case MpShortcutType.SelectNextColumnItem:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.SelectNextColumnItemCommand;
                            break;
                        case MpShortcutType.SelectPreviousColumnItem:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.SelectPreviousColumnItemCommand;
                            break;
                        case MpShortcutType.SelectNextRowItem:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.SelectNextRowItemCommand;
                            break;
                        case MpShortcutType.SelectPreviousRowItem:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.SelectPreviousRowItemCommand;
                            break;
                        case MpShortcutType.AssignShortcut:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.AssignShortcutToSelectedItemCommand;
                            break;
                        case MpShortcutType.ChangeColor:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ChangeSelectedClipsColorCommand;
                            break;
                        case MpShortcutType.Undo:
                            shortcutCommand = MpAvUndoManagerViewModel.Instance.UndoCommand;
                            break;
                        case MpShortcutType.Redo:
                            shortcutCommand = MpAvUndoManagerViewModel.Instance.RedoCommand;
                            break;
                        case MpShortcutType.EditContent:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.EditSelectedContentCommand;
                            break;
                        case MpShortcutType.EditTitle:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.EditSelectedTitleCommand;
                            break;
                        case MpShortcutType.Duplicate:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.DuplicateSelectedClipsCommand;
                            break;
                        case MpShortcutType.ToggleListenToClipboard:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ToggleIsAppPausedCommand;
                            break;
                        case MpShortcutType.CopySelection:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.CopySelectedClipsCommand;
                            break;
                        case MpShortcutType.ScrollToHome:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ScrollToHomeCommand;
                            break;
                        case MpShortcutType.ScrollToEnd:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ScrollToEndCommand;
                            break;
                        case MpShortcutType.WindowSizeUp:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.WindowSizeUpCommand;
                            break;
                        case MpShortcutType.WindowSizeDown:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.WindowSizeDownCommand;
                            break;
                        case MpShortcutType.WindowSizeLeft:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.WindowSizeLeftCommand;
                            break;
                        case MpShortcutType.WindowSizeRight:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.WindowSizeRightCommand;
                            break;
                        case MpShortcutType.PreviousPage:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ScrollToPreviousPageCommand;
                            break;
                        case MpShortcutType.NextPage:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ScrollToNextPageCommand;
                            break;
                        case MpShortcutType.FindAndReplaceSelectedItem:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.EnableFindAndReplaceForSelectedItem;
                            break;
                        case MpShortcutType.ToggleMainWindowLocked:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.ToggleMainWindowLockCommand;
                            break;
                        case MpShortcutType.ToggleFilterMenuVisible:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.ToggleFilterMenuVisibleCommand;
                            break;
                        case MpShortcutType.TogglePinned:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ToggleSelectedTileIsPinnedCommand;
                            break;
                        case MpShortcutType.OpenContentInWindow:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.OpenSelectedTileInWindowCommand;
                            break;
                        default:
                            if (sc.ShortcutType == MpShortcutType.PasteCopyItem) {
                                shortcutCommand = MpAvClipTrayViewModel.Instance.PasteCopyItemByIdCommand;
                            } else if (sc.ShortcutType == MpShortcutType.SelectTag) {
                                shortcutCommand = MpAvTagTrayViewModel.Instance.SelectTagCommand;
                            } else if (sc.ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset) {
                                shortcutCommand = MpAvClipTrayViewModel.Instance.AnalyzeSelectedItemCommand;
                            } else if (sc.ShortcutType == MpShortcutType.InvokeAction) {
                                shortcutCommand = MpAvTriggerCollectionViewModel.Instance.InvokeActionCommand;
                            }
                            break;
                    }
                    var scvm = await CreateShortcutViewModel(sc, shortcutCommand);
                    Items.Add(scvm);
                }
            });
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsFilterTextChanged:
                    OnPropertyChanged(nameof(FilteredItems));
                    OnPropertyChanged(nameof(CustomShortcuts));
                    OnPropertyChanged(nameof(ApplicationShortcuts));
                    break;
                case MpMessageType.MainWindowLoadComplete: {
                        StartInputListener();
                        break;
                    }
                case MpMessageType.MainWindowClosed:
                    IsApplicationShortcutsEnabled = false;
                    break;
                case MpMessageType.MainWindowOpened:
                    IsApplicationShortcutsEnabled = true;
                    break;
                case MpMessageType.ShortcutAssignmentStarted:
                    IsShortcutsEnabled = false;
                    break;
                case MpMessageType.ShortcutAssignmentEnded:
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

        #region Paste Tracking (unused)
        private void EnableExternalPasteTracking() {
            Mp.Services.ProcessWatcher.OnAppActivated += ProcessWatcher_OnAppActivated;
        }
        private void DisableExternalPasteTracking() {
            Mp.Services.ProcessWatcher.OnAppActivated -= ProcessWatcher_OnAppActivated;
        }

        private void ProcessWatcher_OnAppActivated(object sender, MpPortableProcessInfo e) {

            //GlobalHook.OnCombination(new Dictionary<Combination, Action> {
            //{
            //    Combination.FromString("Control+V"), () => {
            //        try {
            //            string cbText = Clipboard.GetText();
            //            if(!string.IsNullOrEmpty(cbText)) {
            //                Application.Current.Dispatcher.BeginInvoke((Action)(()=>{
            //                    foreach(var ctvm in MpAvClipTrayViewModel.Instance.Items) {
            //                        foreach(var rtbvm in ctvm.Items) {
            //                            if(rtbvm.CopyItem.ItemData.ToPlainText() == cbText) {
            //                                rtbvm.CopyItem.PasteCount++;
            //                            }
            //                        }
            //                    }
            //                }),System.Windows.Threading.DispatcherPriority.Background);
            //            }
            //        } catch(Exception ex) {
            //            MpConsole.WriteLine("Global Keyboard Paste watch exception getting text: "+ex);
            //        }
            //    }
            //}
            //});
        }

        #endregion

        private async Task PerformMatchedShortcutAsync(MpAvShortcutViewModel svm) {
            bool can_perform = await Dispatcher.UIThread.InvokeAsync<bool>(() => {
                return svm.PerformShortcutCommand.CanExecute(null);
            });

            MpConsole.WriteLine($"Shorcut Gesture '{svm.KeyString}' matched for shortcut '{svm.ShortcutType}' {(can_perform ? "CAN" : "CANNOT")} execute");
            if (!can_perform) {
                ClearKeyboardBufferAsync().FireAndForgetSafeAsync(this);
                return;
            }

            if (//svm.RoutingType == MpRoutingType.Internal ||
                svm.RoutingType == MpRoutingType.Bubble) {
                await Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequenceAsync(svm.KeyString);
            }
            Dispatcher.UIThread.Post(() => svm.PerformShortcutCommand.Execute(null));
            if (svm.RoutingType == MpRoutingType.Tunnel) {
                await Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequenceAsync(svm.KeyString);
            }
        }

        #region Gesture Handling
        private async Task ClearKeyboardBufferAsync() {
            //when shortcut can't execute pass gesture and clear buffer
            foreach (var sup_key_str in _suppressedKeys) {
                await Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequenceAsync(sup_key_str);
            }
            _suppressedKeys.Clear();
        }


        private int _downCount = 0;
        private void HandleGestureRouting_Down(string keyLiteral, object down_e) {
            _downCount++;
            _keyboardGestureHelper.AddKeyDown(keyLiteral);
            ValidateGesture();

            if (down_e is KeyboardHookEventArgs sharp_down) {
                string curGestureStr = _keyboardGestureHelper.GetCurrentGesture().ToLower();
                var exactMatch =
                    Items
                    .Where(x => !string.IsNullOrEmpty(x.KeyString))
                    .FirstOrDefault(x => x.KeyString.ToLower() == curGestureStr);

                //bool is_mod = MpInputConstants.MOD_LITERALS.Any(x => x.ToLower() == keyLiteral.ToLower());
                if (exactMatch == null) {
                    var startsWith_matches =
                        Items
                        .Where(x =>
                            x.RoutingType == MpRoutingType.Override &&
                            x.IncludesKeyLiteral(keyLiteral));

                    if (startsWith_matches.Any()) {
                        _suppressedKeys.Add(keyLiteral);
                    }
                } else if (exactMatch.RoutingType == MpRoutingType.Override) {
                    // NOTE SupressEvent will block key up from occuring so 
                    // override cmd's occur in down event
                    sharp_down.SuppressEvent = true;
                }

            }

        }

        private void HandleGestureRouting_Up(string keyLiteral, object up_e) {
            _downCount--;
            string curGestureStr = _keyboardGestureHelper.GetCurrentGesture().ToLower();
            _keyboardGestureHelper.ClearCurrentGesture();
            ValidateGesture();

            var exactMatch =
                Items
                .Where(x => !string.IsNullOrEmpty(x.KeyString))
                .FirstOrDefault(x => x.KeyString.ToLower() == curGestureStr);

            if (exactMatch == null) {
                ClearKeyboardBufferAsync().FireAndForgetSafeAsync(this);
            } else {
                PerformMatchedShortcutAsync(exactMatch).FireAndForgetSafeAsync(this);
            }
        }

        private void ValidateGesture() {
            if (_downCount < 0) {
                _downCount = 0;
                // should never be below zero
                //Debugger.Break();
            }
            string cur_gesture = _keyboardGestureHelper.GetCurrentGesture();
            if (_downCount == 0 && !string.IsNullOrWhiteSpace(cur_gesture)) {
                // gesture should be cleared
                //Debugger.Break();
                _keyboardGestureHelper.ClearCurrentGesture();
                MpConsole.WriteLine($"KeyDown Count 0, gesture cleared: '{cur_gesture}'");
            }
        }

        #endregion


        #region Global Input

        private void CreateGlobalInputHooks() {
            if (_hook == null) {
                _hook = new SimpleGlobalHook();

                if (IS_GLOBAL_MOUSE_INPUT_ENABLED) {
                    _hook.MouseWheel += Hook_MouseWheel;

                    _hook.MouseMoved += Hook_MouseMoved;

                    _hook.MousePressed += Hook_MousePressed;
                    _hook.MouseReleased += Hook_MouseReleased;

                    _hook.MouseClicked += Hook_MouseClicked;

                    _hook.MouseDragged += Hook_MouseDragged;
                }

                if (IS_GLOBAL_KEYBOARD_INPUT_ENABLED) {
                    _hook.KeyPressed += Hook_KeyPressed;
                    _hook.KeyReleased += Hook_KeyReleased;
                }

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
            MpPortablePointerButtonType button = e.Data.Button.ToPortableButton();
            if (button == MpPortablePointerButtonType.Left) {
                HandlePointerPress(true);
            } else if (button == MpPortablePointerButtonType.Right) {
                HandlePointerPress(false);
            } else {
                MpConsole.WriteTraceLine("Unknown mouse button pressed: SharpButton: " + e.Data.Button + " PortableButton: " + button);
            }

        }
        private void Hook_MouseReleased(object sender, MouseHookEventArgs e) {
            // NOTE: SharpHook Release event shows Button released

            MpPortablePointerButtonType button = e.Data.Button.ToPortableButton();
            if (button == MpPortablePointerButtonType.Left) {
                HandlePointerReleased(true);
            } else if (button == MpPortablePointerButtonType.Right) {
                HandlePointerReleased(false);
            } else {
                MpConsole.WriteTraceLine("Unknown mouse button released: SharpButton: " + e.Data.Button + " PortableButton: " + button);
            }

            //if (MpAvClipTrayViewModel.Instance.IsPinTrayDropPopOutVisible) {
            //    MpAvClipTrayViewModel.Instance.IsPinTrayDropPopOutVisible = false;
            //}
        }

        private void Hook_MouseClicked(object sender, MouseHookEventArgs e) {
            MpPortablePointerButtonType button = e.Data.Button.ToPortableButton();
            if (button == MpPortablePointerButtonType.Left) {
                HandlePointerClick(true);
            } else if (button == MpPortablePointerButtonType.Right) {
                HandlePointerClick(false);
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
            //string keyStr = MpGlobalKeyConverter.GetKeyLiteral(e.Data.KeyCode);
            string keyStr = Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { e.Data.KeyCode } });
            HandleKeyDown(keyStr, e);
        }


        private void Hook_KeyReleased(object sender, KeyboardHookEventArgs e) {
            string keyStr = Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { e.Data.KeyCode } });
            HandleKeyUp(keyStr, e);
        }


        #endregion

        #endregion

        #region Fake-Global Input

        private void CreatePseudoGlobalInputHooks(Control control) {
            // mouse
            control.PointerMoved += PseudoGlobalControl_PointerMoved;
            control.AddHandler(Window.PointerPressedEvent, PseudoGlobalControl_PointerPressed, RoutingStrategies.Tunnel);
            control.AddHandler(Window.PointerReleasedEvent, PseudoGlobalControl_PointerReleased, RoutingStrategies.Tunnel);
            control.PointerWheelChanged += PseudoGlobalControl_PointerWheelChanged;

            // keyboard
            control.KeyDown += PseudoGlobalControl_KeyDown;
            control.KeyUp += PseudoGlobalControl_KeyUp;
        }
        private void DisposePseudoGlobalInputHooks(Control control) {
            // mouse
            control.PointerMoved -= PseudoGlobalControl_PointerMoved;
            control.RemoveHandler(Window.PointerPressedEvent, PseudoGlobalControl_PointerPressed);
            control.RemoveHandler(Window.PointerReleasedEvent, PseudoGlobalControl_PointerReleased);
            control.PointerWheelChanged -= PseudoGlobalControl_PointerWheelChanged;

            // keyboard
            control.KeyDown -= PseudoGlobalControl_KeyDown;
            control.KeyUp -= PseudoGlobalControl_KeyUp;
        }

        #region Mouse

        private void PseudoGlobalControl_PointerPressed(object sender, PointerPressedEventArgs e) {
            e.Handled = false;
            HandlePointerPress(e.IsLeftDown(MpAvMainView.Instance));
        }
        private void PseudoGlobalControl_PointerMoved(object sender, PointerEventArgs e) {
            // NOTE only called when global input is disabled
            var mw_mp = e.GetClientMousePoint(MpAvMainView.Instance);
            var gmp = VisualExtensions.PointToScreen(MpAvMainView.Instance, mw_mp.ToAvPoint()).ToPortablePoint(MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling);
            HandlePointerMove(gmp);
        }

        private void PseudoGlobalControl_PointerReleased(object sender, PointerReleasedEventArgs e) {
            e.Handled = false;
            HandlePointerReleased(GlobalIsMouseLeftButtonDown != e.IsLeftDown(MpAvMainView.Instance));
        }


        private void PseudoGlobalControl_PointerWheelChanged(object sender, PointerWheelEventArgs e) {
            HandlePointerWheel(e.Delta.ToPortablePoint());
        }

        #endregion

        #region Keyboard

        private void PseudoGlobalControl_KeyDown(object sender, KeyEventArgs e) {
            //string keyLiteral = MpAvInternalKeyConverter.GetKeyLiteral(e.Key);
            string keyStr = Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { e.Key } });
            HandleKeyDown(keyStr, e);
        }

        private void PseudoGlobalControl_KeyUp(object sender, KeyEventArgs e) {
            //string keyStr = MpAvInternalKeyConverter.GetKeyLiteral(e.Key);
            string keyStr = Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { e.Key } });
            HandleKeyUp(keyStr, e);
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
        private void HandleKeyDown(string keyStr, object down_e) {

            if (Mp.Services.KeyStrokeSimulator.IsSimulating) {
                return;
            }
            Dispatcher.UIThread.Post(() => {

                if (string.IsNullOrEmpty(keyStr)) {
                    // what key is this with no string val?
                    Debugger.Break();
                    keyStr = "???";
                }

                if (keyStr.IsShift()) {
                    GlobalIsShiftDown = true;
                }
                if (keyStr.IsAlt()) {
                    GlobalIsAltDown = true;
                }
                if (keyStr.IsCtrl()) {
                    GlobalIsCtrlDown = true;
                    //MpConsole.WriteLine("Global ctrl key: DOWN");
                }
                if (keyStr.IsMeta()) {
                    GlobalIsMetaDown = true;
                }
                if (keyStr.IsEscape()) {
                    GlobalIsEscapeDown = true;
                    OnGlobalEscKeyPressed?.Invoke(this, EventArgs.Empty);
                }

                OnGlobalKeyPressed?.Invoke(this, keyStr);


            });
            if (IsShortcutsEnabled) {
                HandleGestureRouting_Down(keyStr, down_e);
            }
        }

        private void HandleKeyUp(string keyStr, object up_e) {

            if (Mp.Services.KeyStrokeSimulator.IsSimulating) {
                return;
            }
            Dispatcher.UIThread.Post(() => {
                if (string.IsNullOrEmpty(keyStr)) {
                    // what key is this with no string val?
                    Debugger.Break();
                    keyStr = "???";
                }

                if (keyStr.IsShift()) {
                    GlobalIsShiftDown = false;
                }
                if (keyStr.IsAlt()) {
                    GlobalIsAltDown = false;
                }
                if (keyStr.IsCtrl()) {
                    GlobalIsCtrlDown = false;
                    //MpConsole.WriteLine("Global ctrl key: UP");
                }
                if (keyStr.IsMeta()) {
                    GlobalIsMetaDown = false;
                }
                if (keyStr.IsEscape()) {
                    GlobalIsEscapeDown = false;
                    OnGlobalEscapeReleased?.Invoke(this, null);
                }

                OnGlobalKeyReleased?.Invoke(this, keyStr);


            });
            if (IsShortcutsEnabled) {
                HandleGestureRouting_Up(keyStr, up_e);
            }
        }


        #endregion

        #endregion

        #endregion

        #region Commands

        public ICommand ShowAssignShortcutDialogCommand => new MpCommand<object>(
            async (args) => {
                if (args is MpAvIShortcutCommandViewModel scvm) {
                    await CreateOrUpdateViewModelShortcutAsync(scvm);
                }
                //if (args is MpICustomShortcutCommandViewModel cscvm) {
                //    string param = MpShortcut.IsUserDefinedShortcut(cscvm.ShortcutType) ? cscvm.ModelId.ToString() : null;
                //    string shortcutKeyString = await MpDataModelProvider.GetShortcutKeystringAsync(cscvm.ShortcutType.ToString(), param);

                //    await CreateOrUpdateViewModelShortcutAsync(
                //        cscvm.ShortcutLabel,
                //        cscvm.ShortcutCommand,
                //        cscvm.ShortcutType,
                //        cscvm.ModelId.ToString(),
                //        shortcutKeyString);
                //} else if (args is MpAvShortcutViewModel scvm) {
                //    await CreateOrUpdateViewModelShortcutAsync(
                //        scvm.ShortcutDisplayName,
                //        scvm.Command,
                //        scvm.ShortcutType,
                //        scvm.CommandParameter,
                //        scvm.KeyString);
                //}


                //if (args is MpViewModelBase vmb) {
                //    vmb.OnPropertyChanged(nameof(vmb.SelfBindingRef));
                //}
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

        public ICommand ResetAllShortcuts => new MpAsyncCommand(async () => {
            bool result = await Mp.Services.NativeMessageBox.ShowOkCancelMessageBoxAsync(
                title: "Confirm",
                message: "Are you sure you want to reset all shortcuts? All custom shortcuts will be removed.",
                iconResourceObj: "WarningImage");
            if (!result) {
                return;
            }
            Items.Clear();
            await MpDb.ResetShortcutsAsync();
            InitShortcutsAsync().FireAndForgetSafeAsync(this);
        });

        #endregion
    }
}
