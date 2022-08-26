using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste.Common;
using Avalonia;
using Avalonia.Threading;
using SharpHook.Native;
using Avalonia.Input;
using SharpHook;
using System.Threading;
using MonkeyPaste.Common.Avalonia;
using Gtk;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutCollectionViewModel : 
        MpAvSelectorViewModelBase<object,MpAvShortcutViewModel>, 
        MpIAsyncSingletonViewModel<MpAvShortcutCollectionViewModel> {

        private const bool IS_GLOBAL_INPUT_ENABLED = false;
        private const double _MIN_GLOBAL_DRAG_DIST = 20;

        #region Private Variables

        private string _sim_keystr_to_this_app = string.Empty;
        private MpAvKeyGestureHelper2 _keyboardGestureHelper;

        private DateTime? _waitToExecuteShortcutStartDateTime;

        private const int _MAX_WAIT_TO_EXECUTE_SHORTCUT_MS = 500;

        private SimpleGlobalHook _hook;
        private EventSimulator _eventSimulator;

        #endregion

        #region Statics

        private static MpAvShortcutCollectionViewModel _instance;
        public static MpAvShortcutCollectionViewModel Instance => _instance ?? (_instance = new MpAvShortcutCollectionViewModel());

        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvShortcutViewModel> ApplicationShortcuts => Items.Where(x => x.RoutingType == MpRoutingType.Internal);

        public IEnumerable<MpAvShortcutViewModel> GlobalShortcuts => Items.Where(x => !ApplicationShortcuts.Contains(x));

        #endregion

        #region Input Hooks

        //public IKeyboardMouseEvents GlobalHook { get; set; }
        //public IKeyboardMouseEvents ApplicationHook { get; set; }

        #endregion

        #region State

        public bool GlobalIsCtrlDown { get; private set; } = false;

        public bool GlobalIsAltDown { get; private set; } = false;

        public bool GlobalIsShiftDown { get; private set; } = false;

        public bool IsMultiSelectKeyDown => GlobalIsCtrlDown || GlobalIsAltDown || GlobalIsShiftDown;

        public int SelectedShortcutIndex { get; set; }

        public MpPoint? GlobalMouseLocation { get; private set; } = null;

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
        public event EventHandler OnGlobalEscKeyPressed;

        public event EventHandler<MouseWheelHookEventArgs> OnGlobalMouseWheelScroll;

        public event EventHandler<MouseHookEventArgs> OnGlobalMouseMove;

        public event EventHandler<MouseHookEventArgs> OnGlobalMousePressed;
        public event EventHandler<MouseHookEventArgs> OnGlobalMouseReleased;

        public event EventHandler<MouseHookEventArgs> OnGlobalMouseClicked;

        public event EventHandler<MouseHookEventArgs> OnGlobalMouseDragged;

        #endregion

        #region Keyboard Events

        public event EventHandler<KeyboardHookEventArgs> OnGlobalKeyPressed;
        public event EventHandler<KeyboardHookEventArgs> OnGlobalKeyReleased;

        #endregion


        #endregion

        #region Public Methods
        public MpAvShortcutCollectionViewModel() : base(null) { }

        public async Task InitAsync() {
            _keyboardGestureHelper = new MpAvKeyGestureHelper2();
            await InitShortcuts();

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

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

        public void StartGlobalListener() {
            if (_hook == null) {
                _hook = new SimpleGlobalHook();

                _hook.MouseWheel += Hook_MouseWheel;

                _hook.MouseMoved += Hook_MouseMoved;

                _hook.MousePressed += Hook_MousePressed;
                _hook.MouseReleased += Hook_MouseReleased;

                _hook.MouseClicked += Hook_MouseClicked;

                _hook.MouseDragged += Hook_MouseDragged;

                _hook.KeyPressed += Hook_KeyPressed;
                _hook.KeyReleased += Hook_KeyReleased;
            }

            if (_eventSimulator == null) {
                _eventSimulator = new EventSimulator();
            }


            _hook.RunAsync();
        }

        public void StopGlobalListener() {
            if(_hook != null) {
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

            _eventSimulator = null;
        }

        public async Task<string> RegisterViewModelShortcutAsync(
            string title,
            ICommand command,
            MpShortcutType shortcutType,
            string commandParameter,
            string keys) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
            string shortcutKeyString = await MpAvAssignShortcutModalWindowViewModel.ShowAssignShortcutWindow(title, keys, command, commandParameter);

            MpAvShortcutViewModel scvm = null;
            if((int)shortcutType < MpShortcut.MIN_USER_SHORTCUT_TYPE) {
                // for non-custom shortcuts check shortcut type
                scvm = Items.FirstOrDefault(x => x.ShortcutType == shortcutType);
            } else {
                scvm = Items.FirstOrDefault(x => x.CommandParameter == commandParameter && x.ShortcutType == shortcutType);
            }
            //if ((int)shortcutType < (int)MpShortcutType.CustomMinimum) {
            //    scvm = Shortcuts.FirstOrDefault(x => x.Command == command && (int)x.CommandId == commandId);
            //} else {
            //    scvm = Shortcuts.FirstOrDefault(x => x.CommandId == (int)commandId && x.ShortcutType == shortcutType && );
            //}

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

                    if (scvm.IsCustom())  {
                        Items.Remove(scvm);
                    }
                } else {
                    //nothing to do since no shortcut created
                }
            } else if(scvm == null) {
                //if new shortcut
                MpRoutingType routingType = shortcutType == MpShortcutType.PasteCopyItem ? MpRoutingType.Direct : MpRoutingType.Internal;
                //copyitem direct, tag internal, analyzer internal
                var sc = await MpShortcut.CreateAsync(
                    shortcutLabel: title, 
                    keyString: shortcutKeyString,
                    routeType: routingType, 
                    shortcutType: shortcutType, 
                    commandParameter: commandParameter);
                scvm = await CreateShortcutViewModel(sc, command);
                Items.Add(scvm);
            } else {
                //if shorcut updated
                scvm.KeyString = shortcutKeyString;

                await scvm.InitializeAsync(scvm.Shortcut, scvm.Command);
            }
            MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
            return shortcutKeyString;
        }

        #endregion

        #region Protected Methods

        #region Db Overrides

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            MpAvShortcutViewModel scvmToRemove = null;
            if (e is MpCopyItem ci) {
                scvmToRemove = Items.FirstOrDefault(x => x.CommandParameter == ci.Id.ToString() && x.ShortcutType == MpShortcutType.PasteCopyItem);
            } else if (e is MpTag t) {
                scvmToRemove = Items.FirstOrDefault(x => x.CommandParameter == t.Id.ToString() && x.ShortcutType == MpShortcutType.SelectTag);
            } else if (e is MpPluginPreset aip) {
                scvmToRemove = Items.FirstOrDefault(x => x.CommandParameter == aip.Id.ToString() && x.ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset);
            }
            if (scvmToRemove != null) {
                Items.Remove(scvmToRemove);
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(ApplicationShortcuts));
                OnPropertyChanged(nameof(GlobalShortcuts));
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private async Task InitShortcuts() {
            await Dispatcher.UIThread.InvokeAsync(async () => {
                //using mainwindow, map all saved shortcuts to their commands
                var scl = await MpDb.GetItemsAsync<MpShortcut>();

                //IsCustomRoutingEnabled = scl.All(x => x.RoutingType == MpRoutingType.Internal || x.RoutingType == MpRoutingType.Direct);

                foreach (var sc in scl) {
                    ICommand shortcutCommand = null;
                    switch (sc.ShortcutType) {
                        case MpShortcutType.ShowMainWindow:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.ShowWindowCommand;
                            break;
                        case MpShortcutType.HideMainWindow:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.HideWindowCommand;
                            break;
                        case MpShortcutType.ToggleAppendMode:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ToggleAppendModeCommand;
                            break;
                        case MpShortcutType.ToggleAutoCopyMode:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ToggleAutoCopyModeCommand;
                            break;
                        case MpShortcutType.ToggleRightClickPasteMode:
                            //right click paste mode
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ToggleRightClickPasteCommand;
                            break;
                        case MpShortcutType.PasteSelectedItems:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.PasteSelectedClipsCommand;
                            break;
                        case MpShortcutType.PasteHere:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.PasteCurrentClipboardIntoSelectedTileCommand;
                            break;
                        case MpShortcutType.DeleteSelectedItems:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.DeleteSelectedClipsCommand;
                            break;
                        case MpShortcutType.SelectNextItem:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.SelectNextItemCommand;
                            break;
                        case MpShortcutType.SelectPreviousItem:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.SelectPreviousItemCommand;
                            break;
                        case MpShortcutType.SelectAll:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.SelectAllCommand;
                            break;
                        case MpShortcutType.InvertSelection:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.InvertSelectionCommand;
                            break;
                        case MpShortcutType.BringSelectedToFront:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.BringSelectedClipTilesToFrontCommand;
                            break;
                        case MpShortcutType.SendSelectedToBack:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.SendSelectedClipTilesToBackCommand;
                            break;
                        case MpShortcutType.AssignShortcut:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.AssignHotkeyCommand;
                            break;
                        case MpShortcutType.ChangeColor:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ChangeSelectedClipsColorCommand;
                            break;
                        case MpShortcutType.SpeakSelectedItem:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.SpeakSelectedClipsCommand;
                            break;
                        case MpShortcutType.MergeSelectedItems:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.MergeSelectedClipsCommand;
                            break;
                        case MpShortcutType.Undo:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.UndoCommand;
                            break;
                        case MpShortcutType.Redo:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.RedoCommand;
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
                        case MpShortcutType.SendToEmail:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.SendToEmailCommand;
                            break;
                        case MpShortcutType.CreateQrCode:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.CreateQrCodeFromSelectedClipsCommand;
                            break;
                        case MpShortcutType.ToggleAppendLineMode:
                            shortcutCommand = MpAvClipTrayViewModel.Instance.ToggleAppendLineModeCommand;
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
                            shortcutCommand = MpAvClipTrayViewModel.Instance.FindAndReplaceSelectedItem;
                            break;
                        case MpShortcutType.ToggleMainWindowLocked:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.ToggleMainWindowLockCommand;
                            break;
                        case MpShortcutType.ToggleFilterMenuVisible:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.ToggleFilterMenuVisibleCommand;
                            break;
                        default:
                            if (sc.ShortcutType == MpShortcutType.PasteCopyItem) {
                                shortcutCommand = MpAvClipTrayViewModel.Instance.PasteCopyItemByIdCommand;
                            } else if (sc.ShortcutType == MpShortcutType.SelectTag) {
                                shortcutCommand = MpAvTagTrayViewModel.Instance.SelectTagCommand;
                            } else if (sc.ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset) {
                                shortcutCommand = MpAvClipTrayViewModel.Instance.AnalyzeSelectedItemCommand;
                            }
                            break;
                    }
                    var scvm = await CreateShortcutViewModel(sc,shortcutCommand);
                    Items.Add(scvm);
                }
            });
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowLoadComplete: {
                        if (IS_GLOBAL_INPUT_ENABLED) {
                            StartGlobalListener();
                        }
                        break;
                    }
                case MpMessageType.MainWindowHid:
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
            }
        }


        private async Task<MpAvShortcutViewModel> CreateShortcutViewModel(MpShortcut sc, ICommand comamnd) {
            MpAvShortcutViewModel nscvm = new MpAvShortcutViewModel(this);
            await nscvm.InitializeAsync(sc, comamnd);
            return nscvm;
        }


        #region Global Handlers

        private MpPoint GetScaledMousePoint(MouseEventData med) {
            MpPoint unscaled_mp = new MpPoint(med.X, med.Y);
            double scale = MpPlatformWrapper.Services.ScreenInfoCollection.PixelScaling;
            return new MpPoint(Math.Max(0, (double)med.X / scale), Math.Max(0, (double)med.Y / scale));

        }
        #region Mouse Event Handlers

        private void Hook_MouseWheel(object? sender, MouseWheelHookEventArgs e) {
            if (!MpAvMainWindowViewModel.Instance.IsMainWindowOpen &&
                !MpAvMainWindowViewModel.Instance.IsMainWindowOpening &&
                MpBootstrapperViewModelBase.IsLoaded) {
                if (MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    if (GlobalMouseLocation != null && 
                        GlobalMouseLocation.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight) {
                        MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    }
                }
            }

            //MpConsole.WriteLine("Scroll wheel: " + e.Data.Rotation);
            OnGlobalMouseWheelScroll?.Invoke(typeof(MpAvShortcutCollectionViewModel).ToString(), e);
        }

        private void Hook_MouseMoved(object? sender, MouseHookEventArgs e) {
            GlobalMouseLocation = GetScaledMousePoint(e.Data);
            //MpConsole.WriteLine("WinForms: " + e.Location.X + "," + e.Location.Y);
            //MpConsole.WriteLine("Wpf: " + GlobalMouseLocation.X + "," + GlobalMouseLocation.Y);
            //MpConsole.WriteLine("");

            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                // NOTE!! this maybe bad only firing when window open 
                // but its for drag/drop and not doing could interfere w/ performance too much
                OnGlobalMouseMove?.Invoke(typeof(MpAvShortcutCollectionViewModel).ToString(), e);
            } else {
                bool isShowingMainWindow = false;
                if (MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdge &&
                    !MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    if (GlobalMouseLocation.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight) {
                        MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                        isShowingMainWindow = true;
                    }
                }

                if (!isShowingMainWindow &&
                    GlobalMouseLeftButtonDownLocation != null &&
                    GlobalMouseLocation.Distance(GlobalMouseLeftButtonDownLocation) >= _MIN_GLOBAL_DRAG_DIST &&
                    GlobalMouseLocation.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight &&
                    MpPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop) {
                    MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                }
            }
        }


        private void Hook_MousePressed(object sender, MouseHookEventArgs e) {
            if (e.Data.Button == SharpHook.Native.MouseButton.Button1) {
                GlobalIsMouseLeftButtonDown = true;
                GlobalMouseLeftButtonDownLocation = GlobalMouseLocation;
                LastLeftClickDateTime = DateTime.Now;
            } else if (e.Data.Button == SharpHook.Native.MouseButton.Button2) {
                GlobalIsMouseRightButtonDown = true;
                LastRightClickDateTime = DateTime.Now;
            } else {
                MpConsole.WriteTraceLine("Unknown mouse button pressed: " + e.Data.Button);
            }

            OnGlobalMousePressed?.Invoke(sender, e);
        }
        private void Hook_MouseReleased(object sender, MouseHookEventArgs e) {

            if (e.Data.Button == SharpHook.Native.MouseButton.Button1) {
                GlobalMouseLeftButtonDownLocation = null;
                GlobalIsMouseLeftButtonDown = false;

                if (MpAvDragDropManager.IsDragAndDrop) {
                    Dispatcher.UIThread.Post(async () => {
                        var handle = MpPlatformWrapper.Services.ProcessWatcher.ThisAppHandle;
                        WinApi.SetForegroundWindow(handle);
                        WinApi.SetActiveWindow(handle);

                        while (true) {
                            await Task.Delay(100);
                        }
                        //MessageBox.Show("Mouse up");
                    });
                }

                OnGlobalMouseReleased?.Invoke(sender, e);
            } else if (e.Data.Button == SharpHook.Native.MouseButton.Button2) {
                GlobalIsMouseRightButtonDown = false;

                OnGlobalMouseReleased?.Invoke(sender, e);
            }            

            Dispatcher.UIThread.Post(() => {
                if (MpAvMainWindow.Instance != null) {
                    if (!MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                        if (MpAvClipTrayViewModel.Instance.IsAutoCopyMode) {
                            if (e.Data.Button == SharpHook.Native.MouseButton.Button1 && !MpAvMainWindow.Instance.IsActive) {
                                SimulateKeyPress("control+c");
                            }
                        }
                        if (MpAvClipTrayViewModel.Instance.IsRightClickPasteMode) {
                            if (e.Data.Button == SharpHook.Native.MouseButton.Button2 && !MpAvMainWindow.Instance.IsActive) {
                                SimulateKeyPress("control+v");
                            }
                        }
                    } else if (!MpAvMainWindowViewModel.Instance.IsMainWindowClosing &&
                              !MpAvMainWindowViewModel.Instance.IsMainWindowLocked &&
                              //!MpExternalDropBehavior.Instance.IsPreExternalTemplateDrop &&
                              GlobalMouseLocation != null &&
                              GlobalMouseLocation.Y < MpAvMainWindowViewModel.Instance.MainWindowTop) {
                        MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
                    }
                }
            });
        }

        private void Hook_MouseClicked(object sender, MouseHookEventArgs e) {
            if(GlobalMouseLocation == null) {
                GlobalMouseLocation = GetScaledMousePoint(e.Data);
            }
            

            Dispatcher.UIThread.Post(() => {
                if (!MpAvMainWindow.Instance.IsActive &&
               !MpAvMainWindow.Instance.Bounds.Contains(GlobalMouseLocation.ToAvPoint()) &&
               MpAvMainWindowViewModel.Instance.IsMainWindowOpen &&
               !MpAvMainWindowViewModel.Instance.IsMainWindowClosing) {

                    MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
                }

                OnGlobalMouseClicked?.Invoke(sender, e);
            });
            
            
        }

        private void Hook_MouseDragged(object sender, MouseHookEventArgs e) {
            GlobalMouseLocation = GetScaledMousePoint(e.Data);
            OnGlobalMouseDragged?.Invoke(sender, e);
        }

        #endregion

        #region Keyboard EventHadlers
        private void Hook_KeyPressed(object sender, KeyboardHookEventArgs e) {
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftShift ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightShift) {
                GlobalIsShiftDown = true;
            }
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftAlt ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightAlt) {
                GlobalIsAltDown = true;
            }
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftControl ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightControl) {
                GlobalIsCtrlDown = true;
            }
            if (e.Data.KeyCode == KeyCode.VcEscape) {
                if (MpAvDragDropManager.IsDragAndDrop) {
                    //_keyboardGestureHelper.Reset();
                    //e.SuppressKeyPress = true;
                    OnGlobalEscKeyPressed?.Invoke(sender, KeyboardHookEventArgs.Empty);
                    return;
                }
            }

            OnGlobalKeyPressed?.Invoke(sender, e);

            if (IsShortcutsEnabled) {
                HandleGestureRouting_Down(ref e);
            }
        }


        private void Hook_KeyReleased(object sender, KeyboardHookEventArgs e) {
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftShift ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightShift) {
                GlobalIsShiftDown = false;
            }
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftAlt ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightAlt) {
                GlobalIsAltDown = false;
            }
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftControl ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightControl) {
                GlobalIsCtrlDown = false;
            }

            OnGlobalKeyReleased?.Invoke(sender, e);

            if (IsShortcutsEnabled) {
                HandleGestureRouting_Up(e);
            }
        }


        #endregion

        #endregion


        private void HandleGestureRouting_Down(ref KeyboardHookEventArgs e) {
           string keyLiteral = MpSharpHookKeyboardInputHelpers.GetKeyLiteral(e.Data.KeyCode);
            _keyboardGestureHelper.AddKeyDown(keyLiteral);

            //string curGesture = _keyboardGestureHelper.GetCurrentGesture();
            
            //if (!string.IsNullOrEmpty(_sim_keystr_to_this_app) && _sim_keystr_to_this_app.StartsWith(curGesture)) {
            //    // user MAY be pressing a defined shortcut

            //    if (_sim_keystr_to_this_app.Length > curGesture.Length) {
            //        _sim_keystr_to_this_app = _sim_keystr_to_this_app.Substring(curGesture.Length);
            //    } else {
            //        _sim_keystr_to_this_app = String.Empty;
            //    }

            //    //_keyboardGestureHelper.Reset();
            //    e.Reserved = EventReservedValueMask.SuppressEvent;
            //    return;
            //}

            //_waitToExecuteShortcutStartDateTime = null;

            //var possibleMatches = Items.Where(x => x.KeyString.StartsWith(curGesture));
            //if (possibleMatches.Count() == 0) {
            //    //_keyboardGestureHelper.Reset();
            //    return;
            //}

            //e.SuppressKeyPress = true;

            //possibleMatches.ForEach(x => MpConsole.WriteLine("Possible match DOWN: " + x));
        }

        private async void HandleGestureRouting_Up(KeyboardHookEventArgs e) {
            string keyLiteral = MpSharpHookKeyboardInputHelpers.GetKeyLiteral(e.Data.KeyCode);
            _keyboardGestureHelper.RemoveKeyDown(keyLiteral);

            string curGestureStr = _keyboardGestureHelper.GetCurrentGesture();

            MpConsole.WriteLine("Current Gesture: " + curGestureStr);

            var exactMatch = Items.FirstOrDefault(x => x.KeyString.ToLower() == curGestureStr.ToLower());
            if(exactMatch != default) {
                exactMatch.PerformShortcutCommand.Execute(null);
            }

            //IEnumerable<MpAvShortcutViewModel> exactMatches = null;
            //IEnumerable<MpAvShortcutViewModel> possibleMatches = null;

            //if(IsApplicationShortcutsEnabled) {
            //    exactMatches = Items.Where(x => x.KeyString == curGestureStr);
            //    possibleMatches = Items.Where(x => exactMatches.All(y => y != x) && x.KeyString.StartsWith(curGestureStr));
            //} else if(IsGlobalShortcutsEnabled) {
            //    exactMatches = GlobalShortcuts.Where(x => x.KeyString.ToLower() == curGestureStr.ToLower());
            //    possibleMatches = GlobalShortcuts.Where(x => exactMatches.All(y => y != x) && x.KeyString.ToLower().StartsWith(curGestureStr.ToLower()));
            //}

            //bool passInput = false;

            //if (exactMatches.Count() > 0) {
            //    if (exactMatches.Count() > 1) {
            //        // should only be 1
            //        Debugger.Break();
            //    }
            //    // when current gesture is exact match check if it maybe part of a longer sequence
            //    var matchedShortcut = exactMatches.ElementAt(0);

            //    if (possibleMatches.Count() == 0) {
            //        // this means user issued the exact match so no need to dump suppressed input                   

            //        passInput = !PerformMatchedShortcut(matchedShortcut, curGestureStr);
            //    } else {
            //        // when current gesture is a match but a longer is possible set wait delay
            //        _waitToExecuteShortcutStartDateTime = DateTime.Now;

            //        while (true) {
            //            if (!_waitToExecuteShortcutStartDateTime.HasValue) {
            //                // a new key down was issued so the exact is not the final gesture
            //                break;
            //            }
            //            if (DateTime.Now - _waitToExecuteShortcutStartDateTime.Value >
            //                TimeSpan.FromMilliseconds(_MAX_WAIT_TO_EXECUTE_SHORTCUT_MS)) {
            //                // since no new key down was issued in given delay execute shortcut and clear buffer
            //                passInput = !PerformMatchedShortcut(matchedShortcut,curGestureStr);
            //                break;
            //            }
            //            await Task.Delay(10);
            //        }
            //    }
            //} else if (possibleMatches.Count() == 0) {
            //    passInput = true;
            //}
            //if (passInput && !string.IsNullOrEmpty(curGestureStr)) {
            //    // (i don't think this can happen) when both exact and possible have no matches pass current buffer
            //    //System.Windows.Forms.SendKeys.SendWait(curGestureStr);
            //    SimulateKeyPress(curGestureStr);

            //    MpConsole.WriteLine("Emptied gesture buffer with sendkey string: " + curGestureStr);
            //    //_keyboardGestureHelper.Reset();
            //}
        }

        

        private bool PerformMatchedShortcut(MpAvShortcutViewModel matchedShortcut, string cur_keystr) {
            if (!matchedShortcut.PerformShortcutCommand.CanExecute(null)) {
                //when shortcut can't execute pass gesture and clear buffer
                if (matchedShortcut.RoutingType == MpRoutingType.Internal) {
                    _sim_keystr_to_this_app = cur_keystr;
                }
                return false;
            } else {
                Dispatcher.UIThread.Post(async () => {
                    switch (matchedShortcut.RoutingType) {
                        case MpRoutingType.Internal:
                        case MpRoutingType.Direct:
                            // direct or internal means no further action, gesture is suppressed
                            matchedShortcut.PerformShortcutCommand.Execute(null);
                            break;
                        case MpRoutingType.Bubble:
                            // pass gesture before invoking command

                            //System.Windows.Forms.SendKeys.SendWait(cur_keystr);
                            SimulateKeyPress(cur_keystr);

                            await Task.Delay(matchedShortcut.RoutingDelayMs);

                            matchedShortcut.PerformShortcutCommand.Execute(null);
                            break;
                        case MpRoutingType.Tunnel:
                            // pass gesture after invoking command                                

                            matchedShortcut.PerformShortcutCommand.Execute(null);

                            await Task.Delay(matchedShortcut.RoutingDelayMs);
                            //System.Windows.Forms.SendKeys.SendWait(cur_keystr);
                            SimulateKeyPress(cur_keystr);
                            break;
                    }
                });
            }
            //_keyboardGestureHelper.Reset();
            _waitToExecuteShortcutStartDateTime = null;
            return true;
        }

        private bool SimulateKeyPress(string keystr) {;
            List<List<KeyCode>> seq = MpSharpHookKeyboardInputHelpers.ConvertStringToKeySequence(keystr);
            foreach(var combo in seq) {
                foreach(var key in combo) {
                    UioHookResult result = _eventSimulator.SimulateKeyPress(key);
                    if(result != UioHookResult.Success) {
                        throw new Exception($"Error pressing key: '{key}' in seq: '{keystr}' error: '{result}'");
                        //return false;
                    }
                }

                foreach (var key in combo) {
                    UioHookResult result = _eventSimulator.SimulateKeyRelease(key);
                    if (result != UioHookResult.Success) {
                        throw new Exception($"Error releasing key: '{key}' in seq: '{keystr}' error: '{result}'");
                        //return false;
                    }
                }
            }
            return true;
        }

        #endregion

        #region Commands

        public ICommand ReassignSelectedShortcutCommand => new MpCommand(
            async () => {
                var scvm = Items[SelectedShortcutIndex];
                await RegisterViewModelShortcutAsync(
                    scvm.ShortcutDisplayName,
                    scvm.Command,
                    scvm.ShortcutType,
                    scvm.CommandParameter,
                    scvm.KeyString
                );
            });

        public ICommand ShowAssignShortcutDialogCommand => new MpCommand<MpIShortcutCommandViewModel>(
            async (sccvm) => {
                string shortcutKeyString = await MpDataModelProvider.GetShortcutKeystringAsync(sccvm.ShortcutType.ToString(), sccvm.ModelId.ToString());

                await RegisterViewModelShortcutAsync(
                    sccvm.ShortcutLabel,
                    sccvm.ShortcutCommand,
                    sccvm.ShortcutType, 
                    sccvm.ModelId.ToString(), 
                    shortcutKeyString);

                if(sccvm is MpViewModelBase vmb) {
                    vmb.OnPropertyChanged(nameof(vmb.SelfBindingRef));
                }
            });

        public ICommand DeleteShortcutCommand => new MpCommand<object>(
            async (args) => {
                MpConsole.WriteLine("Deleting shortcut row: " + SelectedShortcutIndex);
                var scvm = Items[SelectedShortcutIndex];
                //await RemoveAsync(scvm);
                await scvm.Shortcut.DeleteFromDatabaseAsync();
            },(args)=> args is MpAvShortcutViewModel svm && svm.CanDelete);


        public ICommand ResetShortcutCommand => new MpCommand<object>(
            async (args) => {
                MpConsole.WriteLine("Reset row: " + SelectedShortcutIndex);

                var scvm = Items[SelectedShortcutIndex];
                scvm.KeyString = scvm.Shortcut.DefaultKeyString;
                await scvm.InitializeAsync(scvm.Shortcut,scvm.Command);
                await scvm.Shortcut.WriteToDatabaseAsync();
            },(args) => args is MpAvShortcutViewModel svm && !string.IsNullOrEmpty(svm.DefaultKeyString));      

        #endregion
    }
    public static class MpDebuggerHelper {
        public static void Break() {
            MpAvShortcutCollectionViewModel.Instance.StopGlobalListener();
            Thread.Sleep(1000);
            Debugger.Break();
            MpAvShortcutCollectionViewModel.Instance.StartGlobalListener();
        }
    }
}
