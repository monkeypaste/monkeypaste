using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DynamicData;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Avalonia.Utils.Extensions;
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FocusManager = Avalonia.Input.FocusManager;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;


namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutCollectionViewModel :
        MpAvSelectorViewModelBase<object, MpAvShortcutViewModel>,
        MpIGlobalInputListener,
        MpIShortcutGestureLocator,
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

        private SimpleGlobalHook _hook;

        private CancellationTokenSource _simInputCts;

        #endregion

        #region Statics

        private static MpAvShortcutCollectionViewModel _instance;
        public static MpAvShortcutCollectionViewModel Instance => _instance ?? (_instance = new MpAvShortcutCollectionViewModel());

        #endregion

        #region Interfaces

        #region  Implementation

        string MpIShortcutGestureLocator.LocateByType(MpShortcutType sct) {
            if (Items.FirstOrDefault(x => x.ShortcutType == sct) is MpAvShortcutViewModel scvm) {
                return scvm.KeyString;
            }
            return null;
        }

        string MpIShortcutGestureLocator.LocateByCommand(MpIShortcutCommandViewModel scvm) =>

            GetViewModelCommandShortcutKeyString(scvm);
        #endregion


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
        public ObservableCollection<MpAvShortcutViewModel> FilteredItems =>
            new ObservableCollection<MpAvShortcutViewModel>(
                Items
                    .Where(x => (x as MpIFilterMatch)
                    .IsMatch(MpAvSettingsViewModel.Instance.FilterText)));

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
                            MpAvMainWindowViewModel.Instance.DecreaseFocusCommand
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
                            //right click paste mode
                            MpAvClipTrayViewModel.Instance.ToggleRightClickPasteCommand
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
                            MpAvClipTrayViewModel.Instance.DeleteSelectedClipFromShortcutCommand
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
                            MpShortcutType.ChangeColor,
                            MpAvClipTrayViewModel.Instance.ChangeSelectedClipsColorCommand
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
                            MpShortcutType.EditContent,
                            MpAvClipTrayViewModel.Instance.EditSelectedContentCommand
                        },
                        {
                            MpShortcutType.EditTitle,
                            MpAvClipTrayViewModel.Instance.EditSelectedTitleCommand
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
                            MpShortcutType.OpenContentInWindow,
                            MpAvClipTrayViewModel.Instance.OpenSelectedTileInWindowCommand
                        },
                        {
                            MpShortcutType.PasteCopyItem,
                            MpAvClipTrayViewModel.Instance.PasteCopyItemByIdCommand
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


        public int GlobalShortcutDelay =>
            MpPrefViewModel.Instance.GlobalShortcutDelay;
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
            Mp.Services.ShortcutGestureLocator = this;

            if (!Mp.Services.PlatformInfo.IsDesktop) {
                IS_GLOBAL_MOUSE_INPUT_ENABLED = false;
                IS_GLOBAL_KEYBOARD_INPUT_ENABLED = false;

                IS_PSEUDO_GLOBAL_INPUT_ENABLED = false;
            }
            Items.CollectionChanged += Items_CollectionChanged;
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
            MpIShortcutCommandViewModel iscvm,
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

            var result_tuple = await MpAvAssignShortcutViewModel.ShowAssignShortcutDialog(
                title,
                keys,
                scvm == null ? 0 : scvm.ShortcutId,
                shortcutType == MpShortcutType.None ?
                    MpShortcutAssignmentType.None :
                    shortcutType.CanBeGlobal() ?
                        MpShortcutAssignmentType.CanBeGlobalCommand :
                        MpShortcutAssignmentType.InternalCommand,
                iconResourceObj);

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
                    scvm.RoutingType = result_routing_type;
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
                    MpShortcutType.PasteCopyItem ? MpRoutingType.Bubble : result_routing_type; //MpRoutingType.Internal;
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
                scvm.RoutingType = result_routing_type;
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
                    AppCommandLookup.TryGetValue(sc.ShortcutType, out ICommand shortcutCommand);
                    var scvm = await CreateShortcutViewModel(sc, shortcutCommand);
                    Items.Add(scvm);
                }
            });
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsFilterTextChanged:
                    OnPropertyChanged(nameof(FilteredItems));
                    //OnPropertyChanged(nameof(CustomShortcuts));
                    //OnPropertyChanged(nameof(InternalApplicationShortcuts));
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
                case MpMessageType.AppWindowActivated:
                case MpMessageType.AppWindowDeactivated:
                    if (!string.IsNullOrEmpty(_gesture.PeekGestureString)) {
                        MpConsole.WriteLine($"App window activation changed w/ peek gesture '{_gesture.PeekGestureString}' does it need to be cleared?");
                    }
                    break;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(FilteredItems));
            //OnPropertyChanged(nameof(InternalApplicationShortcuts));
            //OnPropertyChanged(nameof(CustomShortcuts));
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

        private void HandleGlobalKeyEvents(string keyStr, bool isDown) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => HandleGlobalKeyEvents(keyStr, isDown));
                return;
            }
            if (string.IsNullOrEmpty(keyStr)) {
                // what key is this with no string val?
                Debugger.Break();
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

        private void HandleKeyDown(string keyStr, object down_e) {
            HandleGlobalKeyEvents(keyStr, true);
            if (!IsShortcutsEnabled) {
                return;
            }
            HandleGestureRouting_Down(keyStr, down_e);
        }

        private void HandleKeyUp(string keyStr, object up_e) {
            HandleGlobalKeyEvents(keyStr, false);
            if (!IsShortcutsEnabled) {
                return;
            }
            HandleGestureRouting_Up(keyStr, up_e).FireAndForgetSafeAsync(this);
        }


        #region Gesture Handling

        private async Task<bool> PerformMatchedShortcutAsync(MpAvShortcutViewModel svm) {
            if (svm == null) {
                return false;
            }
            await Task.Delay(1);

            bool can_perform = Dispatcher.UIThread.Invoke(() => {
                return svm.PerformShortcutCommand.CanExecute(null);
            });
            MpConsole.WriteLine($"Shorcut Gesture '{svm.KeyString}' matched for shortcut '{svm.ShortcutType}' {(can_perform ? "CAN" : "CANNOT")} execute");
            if (!can_perform) {
                return false;
            }

            //if (//svm.RoutingType == MpRoutingType.Internal ||
            //    svm.RoutingType == MpRoutingType.Bubble) {
            //    await Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequenceAsync(svm.KeyString);
            //}
            Dispatcher.UIThread.Post(() => svm.PerformShortcutCommand.Execute(null));
            //if (svm.RoutingType == MpRoutingType.Tunnel) {
            //    await Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequenceAsync(svm.KeyString);
            //}
            return true;
        }

        private async Task ClearKeyboardBufferAsync() {
            //when shortcut can't execute pass gesture and clear buffer
            foreach (var sup_key_str in _suppressedKeys) {
                await Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequenceAsync(sup_key_str);
            }
            _suppressedKeys.Clear();
        }


        private MpKeyGestureHelper2<KeyCode> _gesture = new MpKeyGestureHelper2<KeyCode>("Cur");
        private List<List<KeyCode>> _sim = new List<List<KeyCode>>();
        private IEnumerable<MpAvShortcutViewModel> _matches;
        //private MpKeyGestureHelper2<KeyCode> _suppress = new MpKeyGestureHelper2<KeyCode>("Supp");
        //private string _last_up_gesture_str;
        //private int _wait_count = 0;

        private void HandleGestureRouting_Down(string keyLiteral, object down_e) {
            var sharp_down = down_e as KeyboardHookEventArgs;
            if (sharp_down == null) {
                return;
            }
            KeyCode kc = sharp_down.Data.KeyCode;
            if (_sim.Any() && _sim.First().Any(x => x.IsSameKey(kc, true))) {
                // ignore sim down, remove in up
                MpConsole.WriteLine($"Sim Key '{keyLiteral}' down ignored");
                return;
            }

            bool new_down = _gesture.Down(kc);
            string down_gesture = _gesture.PeekGestureString;
            var peek_gesture = _gesture.PeekGesture;

            _matches =
                AvailableItems
                .Where(x => x.GlobalKeyList.StartsWith(peek_gesture, true));

            if (_matches.Any()) {
                _gesture.HasMatches = true;
            } else {
                if (_gesture.HasMatches) {
                    MpConsole.WriteLine($"Gesture '{_gesture.GestureString}' invalidated by '{keyLiteral}'. Simulating: {(_gesture.IsSuppressed)}");
                    if (_gesture.IsSuppressed) {
                        // when already suppressing, suppress invalidating key too
                        sharp_down.SuppressEvent = true;
                    }
                    _gesture.HasMatches = false;
                }
                return;
            }

            _gesture.IsSuppressed = _matches.Any(x => x.SuppressesKeys);

            if (_gesture.IsSuppressed) {
                //_suppress.Down(kc);
                sharp_down.SuppressEvent = true;
            }

            if (new_down) {
                MpConsole.WriteLine($"DOWN GESTURE: '{down_gesture}' DOWN COUNT: {_gesture.DownCount}");
            }
        }

        private async Task HandleGestureRouting_Up(string keyLiteral, object up_e) {
            var sharp_up = up_e as KeyboardHookEventArgs;
            if (sharp_up == null) {
                return;
            }
            KeyCode kc = sharp_up.Data.KeyCode;
            if (_sim.Any() && _sim.First().Any(x => x.IsSameKey(kc, true))) {
                // remove sim and ignore
                _sim.First().Remove(kc.GetUnifiedKey());
                MpConsole.WriteLine($"Sim Key '{keyLiteral}' removed. Up ignored");
                return;
            }

            _gesture.Up(kc);

            bool delay_reached = true;
            int wait_count = _gesture.DownCount;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= GlobalShortcutDelay) {
                if (_gesture.DownCount != wait_count) {
                    delay_reached = false;
                    break;
                }
                await Task.Delay(20);
            }
            if (!_gesture.HasMatches) {
                if (_gesture.IsSuppressed &&
                    _sim.Count == 0) {
                    SimGestureAsync(_gesture.GestureString).FireAndForgetSafeAsync(this); ;
                }
                if (_gesture.HasGesture()) {
                    _gesture.Reset();
                }
                return;
            }
            if (!delay_reached) {
                return;
            }

            // no new downs, execute exact if found
            var exact_match =
                _matches
                    .FirstOrDefault(x => x.GlobalKeyList.IsMatch(_gesture.Gesture, true));

            bool perform_gesture = exact_match != null;
            if (perform_gesture) {
                perform_gesture = Dispatcher.UIThread.Invoke(() => {
                    return exact_match.PerformShortcutCommand.CanExecute(null);
                });
            }
            if (!perform_gesture) {
                if (_gesture.IsSuppressed &&
                    _sim.Count == 0) {
                    SimGestureAsync(_gesture.GestureString).FireAndForgetSafeAsync(this);
                }
                if (_gesture.HasGesture()) {
                    _gesture.Reset();
                }
                return;
            }

            _gesture.Reset();
            if (exact_match.RoutingType == MpRoutingType.Bubble) {
                await SimGestureAsync(exact_match.KeyString);
            }
            Dispatcher.UIThread.Invoke(() => {
                exact_match.PerformShortcutCommand.Execute(null);
            });
            if (exact_match.RoutingType == MpRoutingType.Tunnel) {
                await SimGestureAsync(exact_match.KeyString);
            }
        }

        private async Task SimGestureAsync(string simGesture) {
            MpDebug.Assert(_sim.Count == 0, $"Sim gesture should only be set once. Attempting to again...");

            // NOTE need to seperate what goes to simulator or simulator gets collection modified as input is simulated
            var output_gesture =
                Mp.Services.KeyConverter
                .ConvertStringToKeySequence<KeyCode>(simGesture);
            _sim = output_gesture.CloneGesture();

            await Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequenceAsync(output_gesture);
        }

        //private void HandleGestureRouting_Up_old(string keyLiteral, object up_e) {
        //    var sharp_up = up_e as KeyboardHookEventArgs;
        //    if (sharp_up == null) {
        //        return;
        //    }
        //    KeyCode kc = sharp_up.Data.KeyCode;
        //    _last_up_gesture_str = _gesture.GestureString;

        //    MpConsole.WriteLine("Gesture: " + _last_up_gesture_str);
        //    _gesture.Up(kc);


        //    //var exact_match =
        //    //    Items
        //    //    .FirstOrDefault(x => x.KeyString == _last_up_gesture_str);


        //    //if (_gesture.DownCount == 0) {
        //    //    ResetGesture(false);
        //    //}
        //    //PerformMatchedShortcutAsync(exact_match).FireAndForgetSafeAsync(this);
        //    //return;

        //    var possible_matches =
        //        Items
        //        .Where(x => x.KeyString.StartsWith(_last_up_gesture_str));

        //    if (possible_matches.Any()) {
        //        _ = Task.Run(async () => {
        //            _wait_count++;
        //            while (true) {
        //                string wait_gesture = _last_up_gesture_str;//.Clone().ToString();
        //                // wait to see if new input
        //                await Task.Delay(_MAX_WAIT_TO_EXECUTE_SHORTCUT_MS);
        //                if (wait_gesture == _last_up_gesture_str) {
        //                    // no new input
        //                    var exact_match =
        //                        possible_matches
        //                            .FirstOrDefault(x => x.KeyString == _last_up_gesture_str);
        //                    if (exact_match == null) {
        //                        // if (_gesture.DownCount == 0) {
        //                        ResetGesture(true);
        //                        //    return;
        //                        //} else {
        //                        //    continue;
        //                        //}
        //                    } else {
        //                        bool success = await PerformMatchedShortcutAsync(exact_match);
        //                        if (success) {
        //                            _suppress.Reset();
        //                        } else {
        //                            Mp.Services.KeyStrokeSimulator
        //                                .SimulateKeyStrokeSequenceAsync(_suppress.GestureString).FireAndForgetSafeAsync(this);
        //                            _suppress.Reset();
        //                        }
        //                    }
        //                    return;
        //                } else {
        //                    var new_matches =
        //                        Items
        //                        .Where(x => x.KeyString.StartsWith(_last_up_gesture_str));
        //                    if (new_matches.Any()) {
        //                        continue;
        //                    }
        //                    ResetGesture(true);
        //                    return;
        //                }
        //            }
        //        });

        //    } else {
        //        ResetGesture(true);
        //    }
        //}

        //private void ResetGesture(bool simulate) {
        //    _last_up_gesture_str = string.Empty;
        //    _gesture.Reset();
        //    if (simulate) {
        //        Mp.Services.KeyStrokeSimulator
        //            .SimulateKeyStrokeSequenceAsync(_suppress.GestureString).FireAndForgetSafeAsync(this);
        //    }
        //    _suppress.Reset();
        //}
        #endregion

        #endregion

        #endregion

        #endregion

        #region Commands

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
                OnPropertyChanged(nameof(FilteredItems));
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
