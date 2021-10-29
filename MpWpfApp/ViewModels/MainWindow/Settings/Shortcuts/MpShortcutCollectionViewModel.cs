using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MonkeyPaste;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpShortcutCollectionViewModel : MpViewModelBase<object> {
        #region Singleton Definition
        private static readonly Lazy<MpShortcutCollectionViewModel> _Lazy = new Lazy<MpShortcutCollectionViewModel>(() => new MpShortcutCollectionViewModel());
        public static MpShortcutCollectionViewModel Instance { get { return _Lazy.Value; } }
        #endregion

        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpShortcutViewModel> Shortcuts { get; set; } = new ObservableCollection<MpShortcutViewModel>();
        #endregion

        #region Input Hooks
        public IKeyboardMouseEvents GlobalHook { get; set; }
        public IKeyboardMouseEvents ApplicationHook { get; set; }
        #endregion

        #region State

        public bool IsCtrlDown { get; private set; } = false;

        public bool IsAltDown { get; private set; } = false;

        public bool IsShiftDown { get; private set; } = false;

        public bool IsMultiSelectKeyDown => IsCtrlDown || IsAltDown || IsShiftDown;

        #endregion

        #endregion

        #region Public Methods
        public MpShortcutCollectionViewModel() : base(null) { }

        public async Task Init() {
            var sw = new Stopwatch();
            sw.Start();
            await InitHotkeysAndMouseEvents();
            await InitShortcuts();
            sw.Stop();
            MpConsole.WriteLine($"Shortcut loading: {sw.ElapsedMilliseconds} ms");            
        }        

        public async Task<string> RegisterViewModelShortcutAsync(
            object vm, 
            string title, 
            string keys, 
            ICommand command,
            object commandParameter) {
            MpMainWindowViewModel.Instance.IsShowingDialog = true;
            var shortcutKeyString = MpAssignShortcutModalWindowViewModel.ShowAssignShortcutWindow(title, keys, command);

            if (shortcutKeyString == null) {
                //if assignment was canceled ignore but reset skl
                shortcutKeyString = string.Empty;
            } else if (shortcutKeyString == string.Empty) {
                //if an empty assignment was ok'd check if exists 
                List<MpShortcutViewModel> scvml = null;
                if (vm.GetType() == typeof(MpShortcutViewModel)) {
                    scvml = Shortcuts.Where(x => x.Command == command && x.CommandParameter == commandParameter).ToList();
                } else if(vm is MpContentItemViewModel) {
                    scvml = Shortcuts.Where(x => x.CopyItemId == (int)commandParameter).ToList();
                } else {
                    scvml = Shortcuts.Where(x => x.TagId == (int)commandParameter).ToList();
                }
                //var scvml = this.Where(x => x.Command == command && x.CommandParameter == commandParameter).ToList();
                //if it does clear, save and unregister
                if (scvml != null && Shortcuts.Count > 0) {
                    foreach (var scvm in scvml) {
                        scvm.ClearShortcutKeyString();
                        await scvm.Shortcut.WriteToDatabaseAsync();
                        scvm.Unregister();
                        if (vm is MpContentItemViewModel) {
                            (vm as MpContentItemViewModel).ShortcutKeyString = string.Empty;
                        } else if (vm is MpTagTileViewModel) {
                            (vm as MpTagTileViewModel).ShortcutKeyString = string.Empty;
                        }

                        if (vm is MpShortcutViewModel) {

                        } else {
                            Shortcuts.Remove(scvm);
                        }
                    }
                } else {
                    //nothing to do since no shortcut created
                }
            } else {
                await AddAsync(vm, shortcutKeyString, command, commandParameter);                
            }
            MpMainWindowViewModel.Instance.IsShowingDialog = false;
            return shortcutKeyString;
        }
        
        public void UpdateInputGestures(ItemsControl cm) {
            foreach (var item in cm.Items) {
                if (item is MenuItem mi) {
                    int tagNum =  mi.Tag == null ? -1 : Convert.ToInt32(mi.Tag.ToString());
                    if(tagNum == 8080) {
                        foreach (var smi in ((MenuItem)mi).Items) {
                            if (smi == null || smi is Separator) {
                                continue;
                            }
                            string header = (smi as MpContextMenuItemViewModel).Header.ToString();
                            (smi as MpContextMenuItemViewModel).InputGestureText = MpTagTrayViewModel.Instance.TagTileViewModels.Where(x => x.TagName == header).FirstOrDefault().ShortcutKeyString;
                        }
                    } else {
                        var scvm = Shortcuts.Where(x => x.ShortcutId == tagNum).FirstOrDefault();
                        if (scvm != null) {
                            mi.InputGestureText = scvm.KeyString;
                        }
                        if (mi.HasItems) {
                            UpdateInputGestures(mi);
                        }
                    }                    
                }
            }
        }

        public MpShortcutViewModel GetShortcutViewModelById(int shortcutId) {
            var scvml = Shortcuts.Where(x => x.ShortcutId == shortcutId).ToList();
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
                var scvmToRemoveTasks = new List<Task>();
                if (e is MpCopyItem ci) {
                    foreach (var scvmToRemove in Shortcuts.Where(x => x.CopyItemId == ci.Id).ToList()) {
                        scvmToRemoveTasks.Add(RemoveAsync(scvmToRemove));
                    }

                } else if (e is MpTag t) {
                    foreach (var scvmToRemove in Shortcuts.Where(x => x.TagId == t.Id).ToList()) {
                        scvmToRemoveTasks.Add(RemoveAsync(scvmToRemove));
                    }
                }

                await Task.WhenAll(scvmToRemoveTasks.ToArray());
            });
        }

        #endregion

        #endregion

        #region Private Methods

        private async Task InitHotkeysAndMouseEvents() {
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                try {
                    GlobalHook = Hook.GlobalEvents();
                    ApplicationHook = Hook.AppEvents();


                    MpMainWindowViewModel.Instance.OnMainWindowShow += Mwvm_OnMainWindowShow;
                    MpMainWindowViewModel.Instance.OnMainWindowHide += Mwvm_OnMainWindowHide;

                    //trigger global hooks since mainwindow init's hidden
                    Mwvm_OnMainWindowHide(this, new EventArgs());

                    #region Global


                    //GlobalHook.OnCombination(new Dictionary<Combination, Action> {
                    //{
                    //    Combination.FromString("Control+V"), () => {
                    //        try {
                    //            string cbText = Clipboard.GetText();
                    //            if(!string.IsNullOrEmpty(cbText)) {
                    //                Application.Current.Dispatcher.BeginInvoke((Action)(()=>{
                    //                    foreach(var ctvm in MpClipTrayViewModel.Instance.Items) {
                    //                        foreach(var rtbvm in ctvm.ItemViewModels) {
                    //                            if(rtbvm.CopyItem.ItemData.ToPlainText() == cbText) {
                    //                                rtbvm.CopyItem.PasteCount++;
                    //                            }
                    //                        }
                    //                    }
                    //                }),System.Windows.Threading.DispatcherPriority.Background);
                    //            }
                    //        } catch(Exception ex) {
                    //            MonkeyPaste.MpConsole.WriteLine("Global Keyboard Paste watch exception getting text: "+ex);
                    //        }
                    //    }
                    //}
                    //});
                    #endregion

                    #region Local

                    ApplicationHook.KeyPress += ApplicationHook_KeyPress;

                    ApplicationHook.KeyDown += ApplicationHook_KeyDown;

                    ApplicationHook.KeyUp += ApplicationHook_KeyUp;

                    #endregion

                    #endregion
                }
                catch (Exception ex) {
                    MonkeyPaste.MpConsole.WriteLine("Error creating mainwindow hotkeys: " + ex.ToString());
                }
            });
        }

        private void Mwvm_OnMainWindowHide(object sender, EventArgs e) {
            GlobalHook.MouseMove += GlobalHook_MouseMove;
            GlobalHook.MouseUp += GlobalHook_MouseUp;
            GlobalHook.MouseWheel += GlobalHook_MouseWheel;
        }

        private void Mwvm_OnMainWindowShow(object sender, EventArgs e) {
            GlobalHook.MouseMove -= GlobalHook_MouseMove;
            GlobalHook.MouseUp -= GlobalHook_MouseUp;
            GlobalHook.MouseWheel -= GlobalHook_MouseWheel;
        }

        private async Task InitShortcuts() {
            await MpHelpers.Instance.RunOnMainThreadAsync(async () => {
                //using mainwindow, map all saved shortcuts to their commands
                var scl = await MpDataModelProvider.Instance.GetAllShortcuts();
                foreach (var sc in scl) {
                    ICommand shortcutCommand = null;
                    object commandParameter = null;
                    switch (sc.ShortcutId) {
                        case 1:
                            shortcutCommand = MpMainWindowViewModel.Instance.ShowWindowCommand;
                            break;
                        case 2:
                            shortcutCommand = MpMainWindowViewModel.Instance.HideWindowCommand;
                            break;
                        case 3:
                            shortcutCommand = MpAppModeViewModel.Instance.ToggleAppendModeCommand;
                            break;
                        case 4:
                            shortcutCommand = MpAppModeViewModel.Instance.ToggleAutoCopyModeCommand;
                            break;
                        case 5:
                            //right click paste mode
                            shortcutCommand = MpAppModeViewModel.Instance.ToggleRightClickPasteCommand;
                            break;
                        case 6:
                            shortcutCommand = MpClipTrayViewModel.Instance.PasteSelectedClipsCommand;
                            commandParameter = true;
                            break;
                        case 7:
                            shortcutCommand = MpClipTrayViewModel.Instance.DeleteSelectedClipsCommand;
                            commandParameter = true;
                            break;
                        case 8:
                            shortcutCommand = MpClipTrayViewModel.Instance.SelectNextItemCommand;
                            break;
                        case 9:
                            shortcutCommand = MpClipTrayViewModel.Instance.SelectPreviousItemCommand;
                            break;
                        case 10:
                            shortcutCommand = MpClipTrayViewModel.Instance.SelectAllCommand;
                            break;
                        case 11:
                            shortcutCommand = MpClipTrayViewModel.Instance.InvertSelectionCommand;
                            break;
                        case 12:
                            shortcutCommand = MpClipTrayViewModel.Instance.BringSelectedClipTilesToFrontCommand;
                            break;
                        case 13:
                            shortcutCommand = MpClipTrayViewModel.Instance.SendSelectedClipTilesToBackCommand;
                            break;
                        case 14:
                            shortcutCommand = MpClipTrayViewModel.Instance.AssignHotkeyCommand;
                            break;
                        case 15:
                            shortcutCommand = MpClipTrayViewModel.Instance.ChangeSelectedClipsColorCommand;
                            break;
                        case 16:
                            shortcutCommand = MpClipTrayViewModel.Instance.SpeakSelectedClipsCommand;
                            break;
                        case 17:
                            shortcutCommand = MpClipTrayViewModel.Instance.MergeSelectedClipsCommand;
                            break;
                        case 18:
                            shortcutCommand = MpMainWindowViewModel.Instance.UndoCommand;
                            break;
                        case 19:
                            shortcutCommand = MpMainWindowViewModel.Instance.RedoCommand;
                            break;
                        case 20:
                            shortcutCommand = MpClipTrayViewModel.Instance.EditSelectedContentCommand;
                            commandParameter = "edit";
                            break;
                        case 21:
                            shortcutCommand = MpClipTrayViewModel.Instance.EditSelectedTitleCommand;
                            break;
                        case 22:
                            shortcutCommand = MpClipTrayViewModel.Instance.DuplicateSelectedClipsCommand;
                            break;
                        case 23:
                            shortcutCommand = MpClipTrayViewModel.Instance.SendSelectedClipsToEmailCommand;
                            break;
                        case 24:
                            shortcutCommand = MpClipTrayViewModel.Instance.CreateQrCodeFromSelectedClipsCommand;
                            break;
                        case 25:
                            shortcutCommand = MpAppModeViewModel.Instance.ToggleAutoAnalysisModeCommand;
                            break;
                        case 26:
                            shortcutCommand = MpAppModeViewModel.Instance.ToggleIsAppPausedCommand;
                            break;
                        case 27:
                            shortcutCommand = MpClipTrayViewModel.Instance.CopyCommand;
                            break;
                        default:
                            try {
                                if (sc.CopyItemId > 0) {
                                    var ctvm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(sc.CopyItemId);
                                    if (ctvm == null) {
                                        var ci = await MpDb.Instance.GetItemAsync<MpCopyItem>(sc.CopyItemId);
                                        if (ci == null) {
                                            MonkeyPaste.MpConsole.WriteLine("SHortcut init error cannot find copy item w/ id: " + sc.CopyItemId);
                                            break;
                                        }
                                        //ctvm = MpClipTrayViewModel.Instance.GetCopyItemViewModelById(ci.CompositeParentCopyItemId);
                                        if (ctvm == null) {
                                            MonkeyPaste.MpConsole.WriteLine("SHortcut init error cannot find hostclip w/ id: " + ci.CompositeParentCopyItemId);
                                            break;
                                        }
                                        //var rtbvm = ctvm.GetContentItemByCopyItemId(ci.Id);
                                        //rtbvm.ShortcutKeyString = sc.KeyString;
                                        // shortcutCommand = MpClipTrayViewModel.Instance.HotkeyPasteCommand;
                                        // commandParameter = rtbvm.CopyItem.Id;
                                    } else {
                                        ctvm.ShortcutKeyString = sc.KeyString;
                                        shortcutCommand = ctvm.PasteSubItemCommand;
                                        shortcutCommand = MpClipTrayViewModel.Instance.PerformHotkeyPasteCommand;
                                        commandParameter = ctvm.CopyItem.Id;
                                    }
                                } else if (sc.TagId > 0) {
                                    var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.Where(x => x.Tag.Id == sc.TagId).Single();
                                    ttvm.ShortcutKeyString = sc.KeyString;
                                    shortcutCommand = ttvm.Parent.SelectTagCommand;
                                    commandParameter = ttvm.TagId;
                                }
                            }
                            catch (Exception ex) {
                                MonkeyPaste.MpConsole.WriteLine("ShortcutCollection init error, unknown shortcut: " + sc.ToString());
                                MonkeyPaste.MpConsole.WriteLine("With exception: " + ex.ToString());
                            }
                            break;
                    }
                    var scvm = new MpShortcutViewModel(this, sc, shortcutCommand, commandParameter);
                    await scvm.RegisterAsync();
                    Shortcuts.Add(scvm);
                }
            });
        }

        private void AutoSearchOnKeyPress(char keyChar) {
            var sbvm = MpSearchBoxViewModel.Instance;

            if (MpClipTrayViewModel.Instance != null && MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                return;
            }
            if (sbvm != null && sbvm.IsTextBoxFocused) {
                return;
            }
            if (MpMainWindowViewModel.Instance.TagTrayViewModel != null && MpMainWindowViewModel.Instance.TagTrayViewModel.IsEditingTagName) {
                return;
            }
            if (MpClipTrayViewModel.Instance != null && MpClipTrayViewModel.Instance.IsAnyEditingClipTitle) {
                return;
            }
            if (MpSettingsWindowViewModel.IsOpen || MpAssignShortcutModalWindowViewModel.IsOpen) {
                return;
            }
            if (!char.IsControl(keyChar)) {
                foreach (var scvm in MpShortcutCollectionViewModel.Instance.Shortcuts) {
                }
                if (!sbvm.IsTextBoxFocused) {
                    if (sbvm.HasText) {
                        sbvm.Text += keyChar.ToString();
                    } else {
                        sbvm.Text = keyChar.ToString();
                    }
                    sbvm.RequestSearchBoxFocus();
                }
            }
        }

        private async Task AddAsync(object vm, string keys, ICommand command, object commandParameter) {
            MpShortcutViewModel nscvm = null;
            if (vm.GetType() == typeof(MpContentItemViewModel)) {
                var ctvm = (MpContentItemViewModel)vm;
                nscvm = new MpShortcutViewModel(
                            this,
                            new MpShortcut() {
                                CopyItemId = ctvm.CopyItem.Id,
                                TagId = 0,
                                KeyString = keys,
                                ShortcutName = "Paste " + ctvm.CopyItem.Title
                            },
                            command, commandParameter);
            } else if (vm.GetType() == typeof(MpTagTileViewModel)) {
                var ttvm = (MpTagTileViewModel)vm;
                nscvm = new MpShortcutViewModel(
                            this,
                            new MpShortcut() {
                                CopyItemId = 0,
                                TagId = ttvm.Tag.Id,
                                KeyString = keys,
                                ShortcutName = "Select " + ttvm.TagName
                            },
                            command, commandParameter);
            } else if (vm.GetType() == typeof(MpShortcutViewModel)) {
                nscvm = (MpShortcutViewModel)vm;
                nscvm.KeyString = keys;
                nscvm.Command = command;
                nscvm.CommandParameter = commandParameter;
            }

            if (nscvm != null) {
                //check by command if shortcut exists if it does swap it with scvm otherwise add and always register
                List<MpShortcutViewModel> scvml = null;
                if (vm.GetType() == typeof(MpShortcutViewModel)) {
                    scvml = Shortcuts.Where(x => x.Command == nscvm.Command && x.CommandParameter == commandParameter).ToList();
                } else {
                    scvml = Shortcuts.Where(x => x.CopyItemId == nscvm.CopyItemId && x.TagId == nscvm.TagId && x.KeyString != nscvm.KeyString).ToList();
                }
                if (scvml != null && scvml.Count > 0) {
                    var scvm = scvml[0];
                    int scvmIdx = Shortcuts.IndexOf(scvm);
                    Shortcuts[scvmIdx].Unregister();
                    Shortcuts[scvmIdx].KeyString = nscvm.KeyString;
                    nscvm = Shortcuts[scvmIdx];
                } else {
                    Shortcuts.Insert(Shortcuts.Count, nscvm);
                }

                await nscvm.RegisterAsync();

                await nscvm.Shortcut.WriteToDatabaseAsync();
            }
        }

        private async Task RemoveAsync(MpShortcutViewModel scvm) {
            Shortcuts.Remove(scvm);
            scvm.Unregister();
            if (scvm.IsCustom()) {
                await scvm.Shortcut.DeleteFromDatabaseAsync();
                if (scvm.Shortcut.CopyItemId > 0) {
                    var ctvm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(scvm.Shortcut.CopyItemId);
                    if (ctvm != null) {
                        ctvm.ShortcutKeyString = string.Empty;
                    } else {
                        foreach (var ctvm1 in MpClipTrayViewModel.Instance.Items) {
                            foreach (var rtbvm in ctvm1.ItemViewModels) {
                                if (rtbvm.CopyItem.Id == scvm.CopyItemId) {
                                    rtbvm.ShortcutKeyString = string.Empty;
                                }
                            }
                        }
                    }
                } else {
                    foreach (var ttvm in MpTagTrayViewModel.Instance.TagTileViewModels.Where(x => x.Tag.Id == scvm.Shortcut.TagId).ToList()) {
                        ttvm.ShortcutKeyString = string.Empty;
                    }
                }
            }
        }

        #region Global Handlers

        #region Global Mouse Handlers

        private void GlobalHook_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (!MpMainWindowViewModel.IsMainWindowOpen && !MpMainWindowViewModel.IsMainWindowOpening) {
                if (Properties.Settings.Default.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    if (e.Y <= Properties.Settings.Default.ShowMainWindowMouseHitZoneHeight) {
                        MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    }
                }
            }
        }

        private void GlobalHook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (!MpMainWindowViewModel.IsMainWindowOpen) {
                if (Properties.Settings.Default.DoShowMainWindowWithMouseEdge &&
               !Properties.Settings.Default.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    if (e.Y <= Properties.Settings.Default.ShowMainWindowMouseHitZoneHeight) {
                        MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    }
                }
            }
        }

        private void GlobalHook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (!MpMainWindowViewModel.IsMainWindowOpen) {
                if (MpAppModeViewModel.Instance.IsAutoCopyMode) {
                    if (e.Button == System.Windows.Forms.MouseButtons.Left && !MpHelpers.Instance.ApplicationIsActivated()) {
                        System.Windows.Forms.SendKeys.SendWait(" ^ c");
                    }
                }
                if (MpAppModeViewModel.Instance.IsRightClickPasteMode) {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right && !MpHelpers.Instance.ApplicationIsActivated()) {
                        System.Windows.Forms.SendKeys.SendWait("^v");
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Application Handlers


        private void ApplicationHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {
            IsAltDown = e.Alt;
            IsShiftDown = e.Shift;
            IsCtrlDown = e.Control;
        }

        private void ApplicationHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            IsAltDown = e.Alt;
            IsShiftDown = e.Shift;
            IsCtrlDown = e.Control;
        }

        private void ApplicationHook_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e) {
            //AutoSearchOnKeyPress(e.KeyChar);
        }

        #endregion

        #region Commands
        #endregion
    }

}
