using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

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

        public int SelectedShortcutIndex { get; set; }

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
            string title,
            ICommand command,
            MpShortcutType shortcutType,
            int commandId,
            string keys) {
            MpMainWindowViewModel.Instance.IsShowingDialog = true;
            string shortcutKeyString = MpAssignShortcutModalWindowViewModel.ShowAssignShortcutWindow(title, keys, command);

            MpShortcutViewModel scvm = null;
            if ((int)shortcutType < (int)MpShortcutType.CustomMinimum) {
                scvm = Shortcuts.FirstOrDefault(x => x.Command == command && (int)x.CommandId == commandId);
            } else {
                scvm = Shortcuts.FirstOrDefault(x => x.CommandId == (int)commandId && x.ShortcutType == shortcutType);
            }

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
                        Shortcuts.Remove(scvm);
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
                Shortcuts.Add(scvm);
            } else {
                //if shorcut updated
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
                MpShortcutViewModel scvmToRemove = null;
                if (e is MpCopyItem ci) {
                    scvmToRemove = Shortcuts.FirstOrDefault(x => x.CommandId == ci.Id && x.ShortcutType == MpShortcutType.PasteCopyItem);
                } else if (e is MpTag t) {
                    scvmToRemove = Shortcuts.FirstOrDefault(x => x.CommandId == t.Id && x.ShortcutType == MpShortcutType.SelectTag);
                } else if (e is MpAnalyticItemPreset aip) {
                    scvmToRemove = Shortcuts.FirstOrDefault(x => x.CommandId == aip.Id && x.ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset);
                }
                if(scvmToRemove != null) {
                    await RemoveAsync(scvmToRemove);
                }
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
                    //            MpConsole.WriteLine("Global Keyboard Paste watch exception getting text: "+ex);
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
                    MpConsole.WriteLine("Error creating mainwindow hotkeys: " + ex.ToString());
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
                var scl = await MpDb.Instance.GetItemsAsync<MpShortcut>();
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
                            break;
                        case 7:
                            shortcutCommand = MpClipTrayViewModel.Instance.DeleteSelectedClipsCommand;
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
                            break;
                        case 21:
                            shortcutCommand = MpClipTrayViewModel.Instance.EditSelectedTitleCommand;
                            break;
                        case 22:
                            shortcutCommand = MpClipTrayViewModel.Instance.DuplicateSelectedClipsCommand;
                            break;
                        case 23:
                            shortcutCommand = MpClipTrayViewModel.Instance.SendToEmailCommand;
                            break;
                        case 24:
                            shortcutCommand = MpClipTrayViewModel.Instance.CreateQrCodeFromSelectedClipsCommand;
                            break;
                        case 25:
                            shortcutCommand = MpAppModeViewModel.Instance.ToggleAppendLineModeCommand;
                            break;
                        case 26:
                            shortcutCommand = MpAppModeViewModel.Instance.ToggleIsAppPausedCommand;
                            break;
                        case 27:
                            //shortcutCommand = MpClipTrayViewModel.Instance.CopyCommand;
                            break;
                        case 28:
                            shortcutCommand = MpClipTrayViewModel.Instance.ScrollToHomeCommand;
                            break;
                        case 29:
                            shortcutCommand = MpClipTrayViewModel.Instance.ScrollToEndCommand;
                            break;
                        case 30:
                            shortcutCommand = MpClipTrayViewModel.Instance.ScrollUpCommand;
                            break;
                        case 31:
                            shortcutCommand = MpClipTrayViewModel.Instance.ScrollDownCommand;
                            break;
                        default:
                            if (sc.ShortcutType == MpShortcutType.PasteCopyItem) {
                                shortcutCommand = MpClipTrayViewModel.Instance.PasteCopyItemByIdCommand;
                                commandParameter = sc.CommandId;
                            } else if (sc.ShortcutType == MpShortcutType.SelectTag) {
                                shortcutCommand = MpTagTrayViewModel.Instance.SelectTagCommand;
                                commandParameter = sc.CommandId;
                            } else if (sc.ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset) {
                                shortcutCommand = MpClipTrayViewModel.Instance.AnalyzeSelectedItemCommand;
                                commandParameter = sc.CommandId;
                            }
                            break;
                    }
                    var scvm = await CreateShortcutViewModel(sc,shortcutCommand);
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
            if (MpTagTrayViewModel.Instance != null && MpTagTrayViewModel.Instance.IsEditingTagName) {
                return;
            }
            if (MpClipTrayViewModel.Instance != null && MpClipTrayViewModel.Instance.IsAnyEditingClipTitle) {
                return;
            }
            if (MpMainWindowViewModel.Instance.IsShowingDialog) {
                return;
            }
            if (!char.IsControl(keyChar)) {
                foreach (var scvm in Shortcuts) {
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
            Shortcuts.Remove(scvm);
            scvm.Unregister();
            if (scvm.IsCustom()) {
                await scvm.Shortcut.DeleteFromDatabaseAsync();
            }
        }

        #region Global Handlers

        #region Global Mouse Handlers

        private void GlobalHook_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (!MpMainWindowViewModel.Instance.IsMainWindowOpen && !MpMainWindowViewModel.Instance.IsMainWindowOpening) {
                if (Properties.Settings.Default.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    if (e.Y <= Properties.Settings.Default.ShowMainWindowMouseHitZoneHeight) {
                        MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    }
                }
            }
        }

        private void GlobalHook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (!MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                if (Properties.Settings.Default.DoShowMainWindowWithMouseEdge &&
               !Properties.Settings.Default.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    if (e.Y <= Properties.Settings.Default.ShowMainWindowMouseHitZoneHeight) {
                        MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    }
                }
            }
        }

        private void GlobalHook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (!MpMainWindowViewModel.Instance.IsMainWindowOpen) {
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

        public ICommand ReassignShortcutCommand => new RelayCommand(
            async () => {
                var scvm = Shortcuts[SelectedShortcutIndex];
                await RegisterViewModelShortcutAsync(
                    scvm.ShortcutDisplayName,
                    scvm.Command,
                    scvm.ShortcutType,
                    scvm.CommandId,
                    scvm.KeyString
                );
            });


        public ICommand DeleteShortcutCommand => new RelayCommand(
            async () => {
                MpConsole.WriteLine("Deleting shortcut row: " + SelectedShortcutIndex);
                var scvm = Shortcuts[SelectedShortcutIndex];
                //await RemoveAsync(scvm);
                await scvm.Shortcut.DeleteFromDatabaseAsync();
            });

        public ICommand ResetShortcutCommand => new RelayCommand(
            async () => {
                MpConsole.WriteLine("Reset row: " + SelectedShortcutIndex);
                var scvm = Shortcuts[SelectedShortcutIndex];
                scvm.KeyString = scvm.Shortcut.DefaultKeyString;
                await scvm.InitializeAsync(scvm.Shortcut,scvm.Command);
                await scvm.Shortcut.WriteToDatabaseAsync();
            });
        #endregion
    }

}
