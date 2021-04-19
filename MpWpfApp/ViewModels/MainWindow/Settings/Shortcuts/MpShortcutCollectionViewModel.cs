using Gma.System.MouseKeyHook;
using System;
using System.Linq;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpShortcutCollectionViewModel : MpObservableCollectionViewModel<MpShortcutViewModel> {
        #region Singleton Definition
        private static readonly Lazy<MpShortcutCollectionViewModel> _Lazy = new Lazy<MpShortcutCollectionViewModel>(() => new MpShortcutCollectionViewModel());
        public static MpShortcutCollectionViewModel Instance { get { return _Lazy.Value; } }
        #endregion

        #region Private Variables
        private Timer _shortcutValidationTimer = null;
        private bool _isBlockingInput = false;
        private MpShortcutInputMonitor _shortcutInputMonitor = null;
        #endregion

        #region Properties

        #region View Models
        #endregion

        #region Input Hooks
        public IKeyboardMouseEvents GlobalHook { get; set; }
        public IKeyboardMouseEvents ApplicationHook { get; set; }
        #endregion

        #region State
        public bool IsAnyPerformingGlobalShortcut {
            get {
                return this.Any(x => x.WasShortcutTriggered && x.IsRoutable);
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpShortcutCollectionViewModel() : base() { }

        public void Init() {
            InitHotkeys();
            InitShortcuts();
            OnViewModelLoaded();
        }        

        public string RegisterViewModelShortcut(
            object vm, 
            string title, 
            string keys, 
            ICommand command,
            object commandParameter) {
            MainWindowViewModel.IsShowingDialog = true;
            var shortcutKeyString = MpAssignShortcutModalWindowViewModel.ShowAssignShortcutWindow(title, keys, command);

            if (shortcutKeyString == null) {
                //if assignment was canceled ignore but reset skl
                shortcutKeyString = string.Empty;
            } else if (shortcutKeyString == string.Empty) {
                //if an empty assignment was ok'd check if exists 
                var scvml = this.Where(x => x.Command == command).ToList();
                //if it does clear, save and unregister
                if (scvml != null && this.Count > 0) {
                    foreach (var scvm in scvml) {
                        scvm.ClearShortcutKeyString();
                        scvm.Shortcut.WriteToDatabase();
                        scvm.Unregister();
                    }
                } else {
                    //nothing to do since no shortcut created
                }
            } else {
                this.Add(vm, shortcutKeyString, command, commandParameter);                
            }
            MainWindowViewModel.IsShowingDialog = false;
            return shortcutKeyString;
        }

        public void Add(object vm, string keys, ICommand command, object commandParameter) {
            MpShortcutViewModel nscvm = null;
            if (vm.GetType() == typeof(MpClipTileViewModel)) {
                var ctvm = (MpClipTileViewModel)vm;
                nscvm = new MpShortcutViewModel(
                        new MpShortcut(
                            ctvm.CopyItem.CopyItemId,
                            0,
                            keys,
                            "Paste " + ctvm.CopyItemTitle),
                            command, commandParameter);
            } else if (vm.GetType() == typeof(MpTagTileViewModel)) {
                var ttvm = (MpTagTileViewModel)vm;
                nscvm = new MpShortcutViewModel(
                        new MpShortcut(
                            0,
                            ttvm.TagId,
                            keys,
                            "Select " + ttvm.TagName),
                            command, commandParameter);
            } else if (vm.GetType() == typeof(MpShortcutViewModel)) {
                nscvm = (MpShortcutViewModel)vm;
                nscvm.KeyString = keys;
                nscvm.Command = command;
                nscvm.CommandParameter = commandParameter;
            }
            if (vm.GetType() == typeof(MpRtbListBoxItemRichTextBoxViewModel)) {
                var rtbvm = (MpRtbListBoxItemRichTextBoxViewModel)vm;
                nscvm = new MpShortcutViewModel(
                        new MpShortcut(
                            rtbvm.CopyItem.CopyItemId,
                            0,
                            keys,
                            "Paste " + rtbvm.CopyItemTitle),
                            command, commandParameter);
            }

            if (nscvm != null) {
                //check by command if shortcut exists if it does swap it with scvm otherwise add and always register
                var curScvml = this.Where(x => x.Command == nscvm.Command).ToList();
                if (curScvml != null && curScvml.Count > 0) {
                    foreach (var curscvm in curScvml) {
                        this[this.IndexOf(curscvm)].Unregister();
                        this[this.IndexOf(curscvm)].KeyString = nscvm.KeyString;
                        nscvm = this[this.IndexOf(curscvm)];
                        break;
                    }
                } else {
                    this.Insert(this.Count, nscvm);
                }

                nscvm.Register();

                nscvm.Shortcut.WriteToDatabase();
            }
        }

        public new void Remove(MpShortcutViewModel scvm) {
            base.Remove(scvm);
            scvm.Unregister();
            if(scvm.IsCustom()) {
                scvm.Shortcut.DeleteFromDatabase();
                if (scvm.Shortcut.CopyItemId > 0) {
                    var ctvm = MainWindowViewModel.ClipTrayViewModel.GetClipTileByCopyItemId(scvm.Shortcut.CopyItemId);
                    if(ctvm != null) {
                        ctvm.ShortcutKeyString = string.Empty;
                    }
                } else {
                    foreach(var ttvm in MainWindowViewModel.TagTrayViewModel.Where(x => x.Tag.TagId == scvm.Shortcut.TagId).ToList()) {
                        ttvm.ShortcutKeyString = string.Empty;
                    }
                }
            }
        }

        public void UpdateInputGestures(ItemsControl cm) {
            foreach (var item in cm.Items) {
                if (item is MenuItem mi && mi.Tag != null) {
                    var scvm = this.Where(x => x.ShortcutId == Convert.ToInt32(mi.Tag.ToString())).First();
                    if (scvm != null) {
                        mi.InputGestureText = scvm.KeyString;
                    }                    
                    if(mi.HasItems) {
                        UpdateInputGestures(mi);
                    }
                }
            }
        }
        #endregion

        #region Private Methods

        private void InitShortcuts() {
            //using mainwindow, map all saved shortcuts to their commands
            foreach (var sc in MpShortcut.GetAllShortcuts()) {
                ICommand shortcutCommand = null;
                object commandParameter = null;
                switch (sc.ShortcutId) {
                    case 1:
                        shortcutCommand = MainWindowViewModel.ShowWindowCommand;
                        break;
                    case 2:
                        shortcutCommand = MainWindowViewModel.HideWindowCommand;
                        break;
                    case 3:
                        shortcutCommand = MainWindowViewModel.AppModeViewModel.ToggleAppendModeCommand;
                        break;
                    case 4:
                        shortcutCommand = MainWindowViewModel.AppModeViewModel.ToggleAutoCopyModeCommand;
                        break;
                    case 5:
                        //right click paste mode
                        shortcutCommand = MainWindowViewModel.AppModeViewModel.ToggleRightClickPasteCommand;
                        break;
                    case 6:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand;
                        commandParameter = true;
                        break;
                    case 7:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.DeleteSelectedClipsCommand;
                        commandParameter = true;
                        break;
                    case 8:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.SelectNextItemCommand;
                        break;
                    case 9:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.SelectPreviousItemCommand;
                        break;
                    case 10:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.SelectAllCommand;
                        break;
                    case 11:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.InvertSelectionCommand;
                        break;
                    case 12:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.BringSelectedClipTilesToFrontCommand;
                        break;
                    case 13:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.SendSelectedClipTilesToBackCommand;
                        break;
                    case 14:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.AssignHotkeyCommand;
                        break;
                    case 15:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.ChangeSelectedClipsColorCommand;
                        break;
                    case 16:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.SpeakSelectedClipsAsyncCommand;
                        break;
                    case 17:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.MergeSelectedClipsCommand;
                        break;
                    case 18:
                        shortcutCommand = MainWindowViewModel.UndoCommand;
                        break;
                    case 19:
                        shortcutCommand = MainWindowViewModel.RedoCommand;
                        break;
                    case 20:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.EditSelectedContentCommand;
                        break;
                    case 21:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.EditSelectedTitleCommand;
                        break;
                    case 22:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.DuplicateSelectedClipsCommand;
                        break;
                    case 23:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.SendSelectedClipsToEmailCommand;
                        break;
                    case 24:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.CreateQrCodeFromSelectedClipsCommand;
                        break;
                    default:
                        try {
                            if (sc.CopyItemId > 0) {
                                var ctvm = MainWindowViewModel.ClipTrayViewModel.GetClipTileByCopyItemId(sc.CopyItemId);
                                ctvm.ShortcutKeyString = sc.KeyString;
                                shortcutCommand = ctvm.PasteClipCommand;
                            } else if (sc.TagId > 0) {
                                var ttvm = MainWindowViewModel.TagTrayViewModel.Where(x => x.Tag.TagId == sc.TagId).Single();
                                ttvm.ShortcutKeyString = sc.KeyString;
                                shortcutCommand = ttvm.SelectTagCommand;
                            }
                        }
                        catch (Exception ex) {
                            Console.WriteLine("ShortcutCollection init error, unknown shortcut: " + sc.ToString());
                            Console.WriteLine("With exception: " + ex.ToString());
                        }
                        break;
                }
                var scvm = new MpShortcutViewModel(sc, shortcutCommand, commandParameter);
                scvm.Register();
                this.Add(scvm);
            }
            OnViewModelLoaded();
        }

        private bool InitHotkeys() {
            try {
                _shortcutInputMonitor = new MpShortcutInputMonitor();
                GlobalHook = Hook.GlobalEvents();
                ApplicationHook = Hook.AppEvents();

                #region Mouse
                GlobalHook.MouseMove += (s, e) => {
                    if (Properties.Settings.Default.DoShowMainWindowWithMouseEdge &&
                       !Properties.Settings.Default.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                        if (e.Y <= Properties.Settings.Default.ShowMainWindowMouseHitZoneHeight) {
                            MainWindowViewModel.ShowWindowCommand.Execute(null);
                        }
                    }
                };

                GlobalHook.MouseUp += (s, e) => {
                    if (MainWindowViewModel.AppModeViewModel.IsAutoCopyMode) {
                        if (e.Button == System.Windows.Forms.MouseButtons.Left && !MpHelpers.Instance.ApplicationIsActivated()) {
                            System.Windows.Forms.SendKeys.SendWait("^c");
                        }
                    }
                    if (MainWindowViewModel.AppModeViewModel.IsRightClickPasteMode) {
                        if (e.Button == System.Windows.Forms.MouseButtons.Right && !MpHelpers.Instance.ApplicationIsActivated()) {
                            System.Windows.Forms.SendKeys.SendWait("^v");
                        }
                    }
                };

                GlobalHook.MouseWheel += (s, e) => {
                    if (Properties.Settings.Default.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                        if (e.Y <= Properties.Settings.Default.ShowMainWindowMouseHitZoneHeight) {
                            MainWindowViewModel.ShowWindowCommand.Execute(null);
                        }
                    }
                };

                ApplicationHook.MouseWheel += (s, e) => {
                    if (!MainWindowViewModel.IsLoading &&
                        MainWindowViewModel.ClipTrayViewModel.IsAnyTileExpanded) {
                        var rtbvm = MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles[0].RichTextBoxViewModelCollection;
                        var sv = (ScrollViewer)rtbvm.HostClipTileViewModel.ClipBorder.FindName("ClipTileRichTextBoxListBoxScrollViewer");//RtbLbAdornerLayer.GetVisualAncestor<ScrollViewer>();
                        sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta);
                    }
                };
                #endregion

                #region Keyboard

                /*
                Hotkey Workflow Cases/Examples

                1. Hotkey sequence of 'A,A,A,A' pastes some tile
                    -when user types 'A' KeyDown will block input using scvm.StartsWith and gather in InputMonitor
                    -if user types 'A,A,A,B' input is unblocked and passed to active window
                    -when user has typed 'A,A,A,A' the scvm.IsPerformingShortcut gets flagged and 100ms timer is started here
                     and after elapsed if inputmonitor keylist is different then the shortcut cancel performing otherwise call
                     scvm.Command from here not in registered hook

                2. Hotkey sequence of 'A,A,A,A' pastes some tile and sequence 'A,A,A,A,G,C' pastes a different one
                    -user types 'A,A,A,A' then that scvm.WasTriggered flag is set and validation timer starts
                 
                */

                GlobalHook.KeyPress += (s, e) => {
                    if(MpHelpers.Instance.ApplicationIsActivated()) {
                        return;
                    }
                    if (IsAnyPerformingGlobalShortcut &&
                        this.Any(x => x.WasShortcutTriggered && x.RoutingType == MpRoutingType.Direct)) {
                        e.Handled = true;
                        return;
                    }
                    e.Handled = false;
                };
                GlobalHook.KeyUp += (s, e) => {
                    if (MpHelpers.Instance.ApplicationIsActivated()) {
                        return;
                    }
                    _shortcutInputMonitor.InputMonitor_KeyUp(null, e);

                    if (IsAnyPerformingGlobalShortcut &&
                       this.Any(x => x.WasShortcutTriggered && x.RoutingType == MpRoutingType.Direct)) {
                        e.Handled = true;
                        return;
                    }
                    e.Handled = false;
                };
                GlobalHook.KeyDown += (s, e) => {
                    if (MpHelpers.Instance.ApplicationIsActivated()) {
                        return;
                    }
                    _shortcutInputMonitor.InputMonitor_KeyDown(null, e);

                    bool wasBlockingInput = _isBlockingInput;
                    _isBlockingInput = false;
                    foreach (var gscvm in this.Where(x => x.IsRoutable).ToList()) {
                        if (gscvm.StartsWith(_shortcutInputMonitor.KeyList) && 
                            gscvm.KeyList != _shortcutInputMonitor.KeyList) {
                            _isBlockingInput = true;
                            e.Handled = true;
                        }
                    }
                    if (wasBlockingInput && 
                        !_isBlockingInput &&
                        !MpShortcutCollectionViewModel.Instance.IsAnyPerformingGlobalShortcut) {
                        MpHelpers.Instance.PassKeysListToWindow(
                            MpClipboardManager.Instance.LastWindowWatcher.LastHandle,
                            _shortcutInputMonitor.KeyList);
                    } else if(MpShortcutCollectionViewModel.Instance.IsAnyPerformingGlobalShortcut) {
                        _shortcutValidationTimer = new Timer();
                        _shortcutValidationTimer.Interval = 100;
                        _shortcutValidationTimer.AutoReset = false;
                        _shortcutValidationTimer.Elapsed += (s1, e1) => {
                            var scvml = this.Where(x => x.WasShortcutTriggered).ToList();
                            if (scvml != null && scvml.Count > 0) {
                                var scvm = scvml[0];
                                if (_shortcutInputMonitor.KeyList == scvm.KeyList) {
                                    scvm.PerformShortcutCommand.Execute(null);
                                    scvm.WasShortcutTriggered = false;
                                } else {
                                    scvm.WasShortcutTriggered = false;
                                }
                            }
                        };
                        _shortcutValidationTimer.Start();
                    }
                };

                ApplicationHook.KeyPress += (s, e) => {
                    if (MainWindowViewModel.ClipTrayViewModel != null && MainWindowViewModel.ClipTrayViewModel.IsAnyTileExpanded) {
                        return;
                    }
                    if (MainWindowViewModel.SearchBoxViewModel != null && MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused) {
                        return;
                    }
                    if (MainWindowViewModel.TagTrayViewModel != null && MainWindowViewModel.TagTrayViewModel.IsEditingTagName) {
                        return;
                    }
                    if (MainWindowViewModel.ClipTrayViewModel != null && MainWindowViewModel.ClipTrayViewModel.IsEditingClipTitle) {
                        return;
                    }
                    if (MpSettingsWindowViewModel.IsOpen || MpAssignShortcutModalWindowViewModel.IsOpen) {
                        return;
                    }
                    if (!char.IsControl(e.KeyChar)) {
                        foreach (var scvm in MpShortcutCollectionViewModel.Instance) {
                        }
                        if (!MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused) {
                            MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused = true;
                            if (MainWindowViewModel.SearchBoxViewModel.HasText) {
                                MainWindowViewModel.SearchBoxViewModel.SearchTextBox.Text += e.KeyChar.ToString();
                            } else {
                                MainWindowViewModel.SearchBoxViewModel.SearchTextBox.Text = e.KeyChar.ToString();
                            }
                            MainWindowViewModel.SearchBoxViewModel.SearchTextBox.Select(MainWindowViewModel.SearchBoxViewModel.SearchTextBox.Text.Length, 0);
                        }
                    }
                };
                #endregion
            }
            catch (Exception ex) {
                Console.WriteLine("Error creating mainwindow hotkeys: " + ex.ToString());
                return false;
            }
            return true;
        }
        #endregion

        #region Commands
        #endregion
    }
}
