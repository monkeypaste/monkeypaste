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

namespace MonkeyPaste.Avalonia {
    public class MpShortcutCollectionViewModel : 
        MpSelectorViewModelBase<object,MpShortcutViewModel>, 
        MpIAsyncSingletonViewModel<MpShortcutCollectionViewModel>{
        #region Private Variables

        private string _sendKeysToThisApp = string.Empty;

        private MpKeyGestureHelper _keyboardGestureHelper;

        private DateTime? _waitToExecuteShortcutStartDateTime;

        private const int _MAX_WAIT_TO_EXECUTE_SHORTCUT_MS = 500;

        #endregion

        #region Properties


        #region View Models
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

        public MpPoint GlobalMouseLocation => MpAvGlobalInputHook.Instance.GlobalMouseLocation;
        public bool GlobalIsMouseLeftButtonDown { get; private set; } = false;

        public bool ForceDisableCustomRouting { get; set; } = true;

        private bool _isCustomRoutingEnabled;
        public bool IsCustomRoutingEnabled {
            get {
                if(ForceDisableCustomRouting) {
                    return false;
                }
                return _isCustomRoutingEnabled;
            }
            set {
                if(_isCustomRoutingEnabled != value) {
                    _isCustomRoutingEnabled = value;
                    OnPropertyChanged(nameof(IsCustomRoutingEnabled));
                }
            }
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler<Point> GlobalMouseMove;
        public event EventHandler GlobalMouseLeftButtonUp;
        public event EventHandler GlobalEscKeyPressed;

        #endregion

        #region Public Methods

        private static MpShortcutCollectionViewModel _instance;
        public static MpShortcutCollectionViewModel Instance => _instance ?? (_instance = new MpShortcutCollectionViewModel());

        public MpShortcutCollectionViewModel() : base(null) { }

        public async Task InitAsync() {
            var sw = new Stopwatch();
            sw.Start();
            InitGestureListener();
            await InitHotkeysAndMouseEvents();
            await InitShortcuts();
            sw.Stop();
            MpConsole.WriteLine($"Shortcut loading: {sw.ElapsedMilliseconds} ms");            
        }        

        public async Task<string> RegisterViewModelShortcutAsync(
            string title,
            ICommand command,
            MpShortcutType shortcutType,
            int commandId,
            string keys) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
            string shortcutKeyString = MpAssignShortcutModalWindowViewModel.ShowAssignShortcutWindow(title, keys, command, commandId);

            MpShortcutViewModel scvm = null;
            if(commandId == 0) {
                // for non-custom shortcuts check shortcut type
                scvm = Items.FirstOrDefault(x => x.ShortcutType == shortcutType);
            } else {
                scvm = Items.FirstOrDefault(x => x.CommandId == (int)commandId && x.ShortcutType == shortcutType);
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
                var sc = await MpShortcut.Create(title, shortcutKeyString, shortcutKeyString, routingType, shortcutType, commandId);
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
        //public void UpdateInputGestures(ItemsControl cm) {
        //    foreach (var item in cm.Items) {
        //        if (item is MenuItem mi) {
        //            int tagNum =  mi.Tag == null ? -1 : Convert.ToInt32(mi.Tag.ToString());
        //            if(tagNum == 8080) {
        //                foreach (var smi in ((MenuItem)mi).Items) {
        //                    if (smi == null || smi is Separator) {
        //                        continue;
        //                    }
        //                    string header = (smi as MpMenuItemViewModel).Header.ToString();
        //                    (smi as MpMenuItemViewModel).InputGestureText = MpAvTagTrayViewModel.Instance.Items.Where(x => x.TagName == header).FirstOrDefault().ShortcutKeyString;
        //                }
        //            } else {
        //                var scvm = Items.Where(x => x.ShortcutId == tagNum).FirstOrDefault();
        //                if (scvm != null) {
        //                    mi.InputGestureText = scvm.KeyString;
        //                }
        //                if (mi.HasItems) {
        //                    UpdateInputGestures(mi);
        //                }
        //            }                    
        //        }
        //    }
        //}

        public string GetShortcutKeyStringByCommand(ICommand command, int commandId = 0) {
            var scvm = Items.FirstOrDefault(x => x.Command == command && x.CommandId == commandId);
            if(scvm == null) {
                return string.Empty;
            }
            return scvm.KeyString;
        }

        public MpShortcutViewModel GetShortcutViewModelById(int shortcutId) {
            var scvml = Items.Where(x => x.ShortcutId == shortcutId).ToList();
            if (scvml.Count > 0) {
                return scvml[0];
            }
            return null;
        }

        #endregion

        #region Protected Methods

        #region Db Overrides

        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            await Task.Run(async () => {
                MpShortcutViewModel scvmToRemove = null;
                if (e is MpCopyItem ci) {
                    scvmToRemove = Items.FirstOrDefault(x => x.CommandId == ci.Id && x.ShortcutType == MpShortcutType.PasteCopyItem);
                } else if (e is MpTag t) {
                    scvmToRemove = Items.FirstOrDefault(x => x.CommandId == t.Id && x.ShortcutType == MpShortcutType.SelectTag);
                } else if (e is MpPluginPreset aip) {
                    scvmToRemove = Items.FirstOrDefault(x => x.CommandId == aip.Id && x.ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset);
                }
                if(scvmToRemove != null) {
                    await RemoveAsync(scvmToRemove);
                }
            });
        }

        #endregion

        #endregion

        #region Private Methods
        private void InitGestureListener() {
            _keyboardGestureHelper = new MpKeyGestureHelper();

        }

        private async Task InitHotkeysAndMouseEvents() {
            await MpHelpers.RunOnMainThreadAsync(() => {
                try {
                    GlobalHook = Hook.GlobalEvents();
                    ApplicationHook = Hook.AppEvents();


                    //MpAvMainWindowViewModel.Instance.OnMainWindowShow += Mwvm_OnMainWindowShow;
                    //MpAvMainWindowViewModel.Instance.OnMainWindowHidden += Mwvm_OnMainWindowHide;

                    ////trigger global hooks since mainwindow init's hidden
                    //Mwvm_OnMainWindowHide(this, new EventArgs());

                    #region Global

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
                    GlobalHook.MouseMove += GlobalHook_MouseMove;
                    GlobalHook.MouseDown += GlobalHook_MouseDown; ;
                    GlobalHook.MouseUp += GlobalHook_MouseUp;
                    GlobalHook.MouseWheel += GlobalHook_MouseWheel;

                    GlobalHook.KeyDown += GlobalHook_KeyDown;
                    GlobalHook.KeyUp += GlobalHook_KeyUp;
                    
                    #endregion

                    #region Application

                    

                    #endregion

                }
                catch (Exception ex) {
                    MpConsole.WriteLine("Error creating mainwindow hotkeys: " + ex.ToString());
                }
            });
        }


        //private void Mwvm_OnMainWindowHide(object sender, EventArgs e) {
        //    GlobalHook.MouseMove += GlobalHook_MouseMove;
        //    GlobalHook.MouseUp += GlobalHook_MouseUp;
        //    GlobalHook.MouseWheel += GlobalHook_MouseWheel;
        //}

        //private void Mwvm_OnMainWindowShow(object sender, EventArgs e) {
        //    GlobalHook.MouseMove -= GlobalHook_MouseMove;
        //    GlobalHook.MouseUp -= GlobalHook_MouseUp;
        //    GlobalHook.MouseWheel -= GlobalHook_MouseWheel;
        //}

        private async Task InitShortcuts() {
            await Dispatcher.UIThread.InvokeAsync(async () => {
                //using mainwindow, map all saved shortcuts to their commands
                var scl = await MpDb.GetItemsAsync<MpShortcut>();

                IsCustomRoutingEnabled = scl.All(x => x.RoutingType == MpRoutingType.Internal || x.RoutingType == MpRoutingType.Direct);

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
                        case MpShortcutType.IncreaseSize:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.IncreaseSizeCommand;
                            break;
                        case MpShortcutType.DecreaseSize:
                            shortcutCommand = MpAvMainWindowViewModel.Instance.DecreaseSizeCommand;
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

        private void AutoSearchOnKeyPress(char keyChar) {
            var sbvm = MpSearchBoxViewModel.Instance;

            if (MpAvMainWindowViewModel.Instance.IsAnyTextBoxFocused) {
                return;
            }
            if (sbvm != null && sbvm.IsTextBoxFocused) {
                return;
            }
            if (MpAvTagTrayViewModel.Instance != null && MpAvTagTrayViewModel.Instance.IsEditingTagName) {
                return;
            }
            if (MpAvClipTrayViewModel.Instance != null && MpAvClipTrayViewModel.Instance.IsAnyEditingClipTitle) {
                return;
            }
            if (MpAvMainWindowViewModel.Instance.IsShowingDialog) {
                return;
            }
            if (!char.IsControl(keyChar)) {
                foreach (var scvm in Items) {
                }
                if (!sbvm.IsTextBoxFocused) {
                    if (sbvm.HasText) {
                        sbvm.SearchText += keyChar.ToString();
                    } else {
                        sbvm.SearchText = keyChar.ToString();
                    }
                    sbvm.RequestSearchBoxFocus();
                }
            }
        }

        private async Task<MpShortcutViewModel> CreateShortcutViewModel(MpShortcut sc, ICommand comamnd) {
            MpShortcutViewModel nscvm = new MpShortcutViewModel(this);
            await nscvm.InitializeAsync(sc, comamnd);
            return nscvm;
        }

        private async Task RemoveAsync(MpShortcutViewModel scvm) {
            Items.Remove(scvm);
            scvm.Unregister();
            if (scvm.IsCustom()) {
                await scvm.Shortcut.DeleteFromDatabaseAsync();
            }
        }

        #region Global Handlers

        #region Global Mouse Handlers
        private Point? _globalMouseDownPosition = null;
        private double _MIN_GLOBAL_DRAG_DIST = 20;

        private void GlobalHook_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (!MpAvMainWindowViewModel.Instance.IsMainWindowOpen && 
                !MpAvMainWindowViewModel.Instance.IsMainWindowOpening && 
                MpBootstrapperViewModelBase.IsLoaded) {
                if (MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    if (e.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight) {
                        MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    }
                }
            }
        }

        private void GlobalHook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            GlobalMouseLocation = MpScreenInformation.ConvertWinFormsScreenPointToWpf(e.Location);

            //MpConsole.WriteLine("WinForms: " + e.Location.X + "," + e.Location.Y);
            //MpConsole.WriteLine("Wpf: " + GlobalMouseLocation.X + "," + GlobalMouseLocation.Y);
            //MpConsole.WriteLine("");

            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                // NOTE!! this maybe bad only firing when window open 
                // but its for drag/drop and not doing could interfere w/ performance too much
                GlobalMouseMove?.Invoke(this, GlobalMouseLocation);
            } else {
                bool isShowingMainWindow = false;
                if (MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdge &&
                    !MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    if (e.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight) {
                        MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                        isShowingMainWindow = true;
                    }
                }

                if(!isShowingMainWindow &&
                    _globalMouseDownPosition.HasValue && 
                    GlobalMouseLocation.Distance(_globalMouseDownPosition.Value) >= _MIN_GLOBAL_DRAG_DIST &&
                    GlobalMouseLocation.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight &&
                    MpPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop) {
                    MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                }
            }


        }

        private void GlobalHook_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if(e.Button == System.Windows.Forms.MouseButtons.Left) {
                GlobalIsMouseLeftButtonDown = true;
                _globalMouseDownPosition = GlobalMouseLocation;
            }
        }
        private void GlobalHook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {            

            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                _globalMouseDownPosition = null;
                GlobalIsMouseLeftButtonDown = false;

                if(MpDragDropManager.IsDragAndDrop) {
                    MpHelpers.RunOnMainThread(async () => {
                        var handle = MpProcessHelper.MpProcessManager.ThisAppHandle;
                        WinApi.SetForegroundWindow(handle);
                        WinApi.SetActiveWindow(handle);

                        while(true) {
                            await Task.Delay(100);
                        }
                        //MessageBox.Show("Mouse up");
                    });
                }
                
                GlobalMouseLeftButtonUp?.Invoke(this, null);
            }

            if (!MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                if (MpAvClipTrayViewModel.Instance.IsAutoCopyMode) {
                    if (e.Button == System.Windows.Forms.MouseButtons.Left && !MpHelpers.ApplicationIsActivated()) {
                        System.Windows.Forms.SendKeys.SendWait("^c");
                    }
                }
                if (MpAvClipTrayViewModel.Instance.IsRightClickPasteMode) {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right && !MpHelpers.ApplicationIsActivated()) {
                        System.Windows.Forms.SendKeys.SendWait("^v");
                    }
                }
            } else if (!MpAvMainWindowViewModel.Instance.IsMainWindowClosing &&
                      !MpAvMainWindowViewModel.Instance.IsMainWindowLocked &&
                      !MpExternalDropBehavior.Instance.IsPreExternalTemplateDrop &&
                      GlobalMouseLocation.Y < MpAvMainWindowViewModel.Instance.MainWindowTop) {
                MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
            }
        }

        #endregion

        #region Global Key Handlers

        private void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
             if (MpAssignHotkeyModalWindow.IsOpen && IsCustomRoutingEnabled) {
                return;
            }

            if (e.KeyCode == System.Windows.Forms.Keys.Alt || e.Alt ||
               e.KeyCode == System.Windows.Forms.Keys.LMenu ||
               e.KeyCode == System.Windows.Forms.Keys.RMenu) {
                GlobalIsAltDown = true;
            }
            if (e.KeyCode == System.Windows.Forms.Keys.LShiftKey || e.KeyCode == System.Windows.Forms.Keys.RShiftKey) {
                GlobalIsShiftDown = true;
            }
            if (e.KeyCode == System.Windows.Forms.Keys.LControlKey || e.KeyCode == System.Windows.Forms.Keys.RControlKey) {
                GlobalIsCtrlDown = true;
            }

            if (e.KeyCode == System.Windows.Forms.Keys.Escape) {
                if (MpDragDropManager.IsDragAndDrop) {
                    _keyboardGestureHelper.Reset();
                    //e.SuppressKeyPress = true;
                    GlobalEscKeyPressed?.Invoke(this, null);
                    return;
                }
            }

            if (IsCustomRoutingEnabled) {
                 HandleGestureRouting_Down(ref e);
            }            
        }

        private void GlobalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyCode == System.Windows.Forms.Keys.Alt || !e.Alt ||
               e.KeyCode == System.Windows.Forms.Keys.LMenu ||
               e.KeyCode == System.Windows.Forms.Keys.RMenu) {
                GlobalIsAltDown = false;
            }
            if (e.KeyCode == System.Windows.Forms.Keys.LShiftKey || e.KeyCode == System.Windows.Forms.Keys.RShiftKey) {
                GlobalIsShiftDown = false;
            }
            if (e.KeyCode == System.Windows.Forms.Keys.LControlKey || e.KeyCode == System.Windows.Forms.Keys.RControlKey) {
                GlobalIsCtrlDown = false;
            }

            if(IsCustomRoutingEnabled) {
                HandleGestureRouting_Up(e);
            }            
        }


        private void GlobalHook_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e) {
            //AutoSearchOnKeyPress(e.KeyChar);
            
        }

        #endregion

        #endregion

        #region Application Handlers

        #endregion

        private void HandleGestureRouting_Down(ref System.Windows.Forms.KeyEventArgs e) {
            var wpfKey = MpWpfKeyboardInputHelpers.WinformsToWPFKey(e.KeyCode);
            _keyboardGestureHelper.AddKeyDown(wpfKey);

            if (!string.IsNullOrEmpty(_sendKeysToThisApp) && _sendKeysToThisApp.StartsWith(_keyboardGestureHelper.CurrentGesture)) {
                if (_sendKeysToThisApp.Length > _keyboardGestureHelper.CurrentGesture.Length) {
                    _sendKeysToThisApp = _sendKeysToThisApp.Substring(_keyboardGestureHelper.CurrentGesture.Length);
                } else {
                    _sendKeysToThisApp = String.Empty;
                }
                if (Keyboard.PrimaryDevice != null) {
                    if (Keyboard.PrimaryDevice.ActiveSource != null) {
                        var ie = new KeyEventArgs(
                            Keyboard.PrimaryDevice,
                            Keyboard.PrimaryDevice.ActiveSource, 0, wpfKey) {
                            RoutedEvent = Keyboard.KeyDownEvent
                        };
                        InputManager.Current.ProcessInput(ie);

                        // Note: Based on your requirements you may also need to fire events for:
                        // RoutedEvent = Keyboard.PreviewKeyDownEvent
                        // RoutedEvent = Keyboard.KeyUpEvent
                        // RoutedEvent = Keyboard.PreviewKeyUpEvent
                    }
                }

                _keyboardGestureHelper.Reset();
                e.Handled = false;
                e.SuppressKeyPress = false;
                return;
            }

            _waitToExecuteShortcutStartDateTime = null;

            var possibleMatches = Items.Where(x => x.SendKeyStr.StartsWith(_keyboardGestureHelper.CurrentGesture));
            if (possibleMatches.Count() == 0) {
                _keyboardGestureHelper.Reset();
                return;
            }

            e.SuppressKeyPress = true;

            possibleMatches.ForEach(x => MpConsole.WriteLine("Possible match DOWN: " + x));
        }

        private async void HandleGestureRouting_Up(System.Windows.Forms.KeyEventArgs e) {
            string curGestureStr = _keyboardGestureHelper.CurrentGesture;

            if (string.IsNullOrEmpty(curGestureStr)) {
                // no possible or exact shorcuts were suppressed so ignore
                return;
            }

            var wpfKey = MpWpfKeyboardInputHelpers.WinformsToWPFKey(e.KeyCode);
            _keyboardGestureHelper.AddKeyUp(wpfKey);


            var exactMatches = Items.Where(x => x.SendKeyStr == curGestureStr);
            var possibleMatches = Items.Where(x => exactMatches.All(y => y != x) && x.SendKeyStr.StartsWith(curGestureStr));

            possibleMatches.ForEach(x => MpConsole.WriteLine("Possible match UP: " + x));
            exactMatches.ForEach(x => MpConsole.WriteLine("Exact match UP: " + x));

            bool passInput = false;

            if (exactMatches.Count() > 0) {
                if (exactMatches.Count() > 1) {
                    // should only be 1
                    Debugger.Break();
                }
                // when current gesture is exact match check if it maybe part of a longer sequence
                var matchedShortcut = exactMatches.ElementAt(0);

                if (possibleMatches.Count() == 0) {
                    // this means user issued the exact match so no need to dump suppressed input                   

                    passInput = !PerformMatchedShortcut(matchedShortcut);
                } else {
                    // when current gesture is a match but a longer is possible set wait delay
                    _waitToExecuteShortcutStartDateTime = DateTime.Now;

                    while (true) {
                        if (!_waitToExecuteShortcutStartDateTime.HasValue) {
                            // a new key down was issued so the exact is not the final gesture
                            break;
                        }
                        if (DateTime.Now - _waitToExecuteShortcutStartDateTime.Value >
                            TimeSpan.FromMilliseconds(_MAX_WAIT_TO_EXECUTE_SHORTCUT_MS)) {
                            // since no new key down was issued in given delay execute shortcut and clear buffer
                            passInput = !PerformMatchedShortcut(matchedShortcut);
                            break;
                        }
                        await Task.Delay(10);
                    }
                }
            } else if (possibleMatches.Count() == 0) {
                passInput = true;
            }
            if (passInput && !string.IsNullOrEmpty(curGestureStr)) {
                // (i don't think this can happen) when both exact and possible have no matches pass current buffer
                System.Windows.Forms.SendKeys.SendWait(curGestureStr);

                MpConsole.WriteLine("Emptied gesture buffer with sendkey string: " + curGestureStr);
                _keyboardGestureHelper.Reset();
            }
        }

        private bool PerformMatchedShortcut(MpShortcutViewModel matchedShortcut) {
            string sendKeyStr = _keyboardGestureHelper.CurrentGesture;
            if (!matchedShortcut.PerformShortcutCommand.CanExecute(null)) {
                //when shortcut can't execute pass gesture and clear buffer
                if(matchedShortcut.RoutingType == MpRoutingType.Internal) {
                    _sendKeysToThisApp = sendKeyStr;
                }
                return false;
            } else {
                MpHelpers.RunOnMainThread(async () => {
                    switch (matchedShortcut.RoutingType) {
                        case MpRoutingType.Internal:
                        case MpRoutingType.Direct:
                            // direct or internal means no further action, gesture is suppressed
                            matchedShortcut.PerformShortcutCommand.Execute(null);
                            break;
                        case MpRoutingType.Bubble:
                            // pass gesture before invoking command

                            System.Windows.Forms.SendKeys.SendWait(sendKeyStr);

                            await Task.Delay(matchedShortcut.RoutingDelayMs);

                            matchedShortcut.PerformShortcutCommand.Execute(null);
                            break;
                        case MpRoutingType.Tunnel:
                            // pass gesture after invoking command                                

                            matchedShortcut.PerformShortcutCommand.Execute(null);

                            await Task.Delay(matchedShortcut.RoutingDelayMs);
                            System.Windows.Forms.SendKeys.SendWait(sendKeyStr);
                            break;
                    }
                });
            }
            _keyboardGestureHelper.Reset();
            _waitToExecuteShortcutStartDateTime = null;
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
                    scvm.CommandId,
                    scvm.KeyString
                );
            });

        public ICommand ShowAssignShortcutDialogCommand => new MpCommand<MpIShortcutCommandViewModel>(
            async (sccvm) => {
                string shortcutKeyString = await MpDataModelProvider.GetShortcutKeystringAsync(sccvm.ShortcutType, sccvm.ModelId);

                await RegisterViewModelShortcutAsync(
                    sccvm.ShortcutLabel,
                    sccvm.ShortcutCommand,
                    sccvm.ShortcutType, 
                    sccvm.ModelId, 
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
            },(args)=> args is MpShortcutViewModel svm && svm.CanDelete);


        public ICommand ResetShortcutCommand => new MpCommand<object>(
            async (args) => {
                MpConsole.WriteLine("Reset row: " + SelectedShortcutIndex);

                var scvm = Items[SelectedShortcutIndex];
                scvm.KeyString = scvm.Shortcut.DefaultKeyString;
                await scvm.InitializeAsync(scvm.Shortcut,scvm.Command);
                await scvm.Shortcut.WriteToDatabaseAsync();
            },(args) => args is MpShortcutViewModel svm && !string.IsNullOrEmpty(svm.DefaultKeyString));      

        #endregion
    }

}
