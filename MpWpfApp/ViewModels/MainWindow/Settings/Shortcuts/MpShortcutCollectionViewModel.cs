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
    public class MpShortcutCollectionViewModel : MpViewModelBase, MpISingletonViewModel<MpShortcutCollectionViewModel> {
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

        private static MpShortcutCollectionViewModel _instance;
        public static MpShortcutCollectionViewModel Instance => _instance ?? (_instance = new MpShortcutCollectionViewModel());


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

            MpShortcutViewModel scvm = Shortcuts.FirstOrDefault(x => x.CommandId == (int)commandId && x.ShortcutType == shortcutType && x.Command == command);
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
                            string header = (smi as MpMenuItemViewModel).Header.ToString();
                            (smi as MpMenuItemViewModel).InputGestureText = MpTagTrayViewModel.Instance.Items.Where(x => x.TagName == header).FirstOrDefault().ShortcutKeyString;
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

        public string GetShortcutKeyStringByCommand(ICommand command, int commandId = 0) {
            var scvm = Shortcuts.FirstOrDefault(x => x.Command == command && x.CommandId == commandId);
            if(scvm == null) {
                return string.Empty;
            }
            return scvm.KeyString;
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
            await MpHelpers.RunOnMainThreadAsync(() => {
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
            await MpHelpers.RunOnMainThreadAsync(async () => {
                //using mainwindow, map all saved shortcuts to their commands
                var scl = await MpDb.GetItemsAsync<MpShortcut>();
                foreach (var sc in scl) {
                    ICommand shortcutCommand = null;
                    switch ((MpShortcutType)sc.ShortcutId) {
                        case MpShortcutType.ShowMainWindow:
                            shortcutCommand = MpMainWindowViewModel.Instance.ShowWindowCommand;
                            break;
                        case MpShortcutType.HideMainWindow:
                            shortcutCommand = MpMainWindowViewModel.Instance.HideWindowCommand;
                            break;
                        case MpShortcutType.ToggleAppendMode:
                            shortcutCommand = MpSidebarViewModel.Instance.ToggleAppendModeCommand;
                            break;
                        case MpShortcutType.ToggleAutoCopyMode:
                            shortcutCommand = MpSidebarViewModel.Instance.ToggleAutoCopyModeCommand;
                            break;
                        case MpShortcutType.ToggleRightClickPasteMode:
                            //right click paste mode
                            shortcutCommand = MpSidebarViewModel.Instance.ToggleRightClickPasteCommand;
                            break;
                        case MpShortcutType.PasteSelectedItems:
                            shortcutCommand = MpClipTrayViewModel.Instance.PasteSelectedClipsCommand;
                            break;
                        case MpShortcutType.DeleteSelectedItems:
                            shortcutCommand = MpClipTrayViewModel.Instance.DeleteSelectedClipsCommand;
                            break;
                        case MpShortcutType.SelectNextItem:
                            shortcutCommand = MpClipTrayViewModel.Instance.SelectNextItemCommand;
                            break;
                        case MpShortcutType.SelectPreviousItem:
                            shortcutCommand = MpClipTrayViewModel.Instance.SelectPreviousItemCommand;
                            break;
                        case MpShortcutType.SelectAll:
                            shortcutCommand = MpClipTrayViewModel.Instance.SelectAllCommand;
                            break;
                        case MpShortcutType.InvertSelection:
                            shortcutCommand = MpClipTrayViewModel.Instance.InvertSelectionCommand;
                            break;
                        case MpShortcutType.BringSelectedToFront:
                            shortcutCommand = MpClipTrayViewModel.Instance.BringSelectedClipTilesToFrontCommand;
                            break;
                        case MpShortcutType.SendSelectedToBack:
                            shortcutCommand = MpClipTrayViewModel.Instance.SendSelectedClipTilesToBackCommand;
                            break;
                        case MpShortcutType.AssignShortcut:
                            shortcutCommand = MpClipTrayViewModel.Instance.AssignHotkeyCommand;
                            break;
                        case MpShortcutType.ChangeColor:
                            shortcutCommand = MpClipTrayViewModel.Instance.ChangeSelectedClipsColorCommand;
                            break;
                        case MpShortcutType.SpeakSelectedItem:
                            shortcutCommand = MpClipTrayViewModel.Instance.SpeakSelectedClipsCommand;
                            break;
                        case MpShortcutType.MergeSelectedItems:
                            shortcutCommand = MpClipTrayViewModel.Instance.MergeSelectedClipsCommand;
                            break;
                        case MpShortcutType.Undo:
                            shortcutCommand = MpMainWindowViewModel.Instance.UndoCommand;
                            break;
                        case MpShortcutType.Redo:
                            shortcutCommand = MpMainWindowViewModel.Instance.RedoCommand;
                            break;
                        case MpShortcutType.EditContent:
                            shortcutCommand = MpClipTrayViewModel.Instance.EditSelectedContentCommand;
                            break;
                        case MpShortcutType.EditTitle:
                            shortcutCommand = MpClipTrayViewModel.Instance.EditSelectedTitleCommand;
                            break;
                        case MpShortcutType.Duplicate:
                            shortcutCommand = MpClipTrayViewModel.Instance.DuplicateSelectedClipsCommand;
                            break;
                        case MpShortcutType.SendToEmail:
                            shortcutCommand = MpClipTrayViewModel.Instance.SendToEmailCommand;
                            break;
                        case MpShortcutType.CreateQrCode:
                            shortcutCommand = MpClipTrayViewModel.Instance.CreateQrCodeFromSelectedClipsCommand;
                            break;
                        case MpShortcutType.ToggleAppendLineMode:
                            shortcutCommand = MpSidebarViewModel.Instance.ToggleAppendLineModeCommand;
                            break;
                        case MpShortcutType.ToggleListenToClipboard:
                            shortcutCommand = MpSidebarViewModel.Instance.ToggleIsAppPausedCommand;
                            break;
                        case MpShortcutType.CopySelectedItems:
                            shortcutCommand = MpClipTrayViewModel.Instance.CopySelectedClipsCommand;
                            break;
                        case MpShortcutType.ScrollToHome:
                            shortcutCommand = MpClipTrayViewModel.Instance.ScrollToHomeCommand;
                            break;
                        case MpShortcutType.ScrollToEnd:
                            shortcutCommand = MpClipTrayViewModel.Instance.ScrollToEndCommand;
                            break;
                        case MpShortcutType.ScrollUp:
                            shortcutCommand = MpClipTrayViewModel.Instance.ScrollUpCommand;
                            break;
                        case MpShortcutType.ScrollDown:
                            shortcutCommand = MpClipTrayViewModel.Instance.ScrollDownCommand;
                            break;
                        default:
                            if (sc.ShortcutType == MpShortcutType.PasteCopyItem) {
                                shortcutCommand = MpClipTrayViewModel.Instance.PasteCopyItemByIdCommand;
                            } else if (sc.ShortcutType == MpShortcutType.SelectTag) {
                                shortcutCommand = MpTagTrayViewModel.Instance.SelectTagCommand;
                            } else if (sc.ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset) {
                                shortcutCommand = MpClipTrayViewModel.Instance.AnalyzeSelectedItemCommand;
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

            if (MpIsFocusedExtension.IsAnyTextBoxFocused) {
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
            if (!MpMainWindowViewModel.Instance.IsMainWindowOpen && !MpMainWindowViewModel.Instance.IsMainWindowOpening && MpBootstrapperViewModelBase.IsLoaded) {
                if (MpPreferences.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    if (e.Y <= MpPreferences.ShowMainWindowMouseHitZoneHeight) {
                        MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    }
                }
            }
        }

        private void GlobalHook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (!MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                if (MpPreferences.DoShowMainWindowWithMouseEdge &&
               !MpPreferences.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    if (e.Y <= MpPreferences.ShowMainWindowMouseHitZoneHeight) {
                        MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    }
                }
            }
        }

        private void GlobalHook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (!MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                if (MpSidebarViewModel.Instance.IsAutoCopyMode) {
                    if (e.Button == System.Windows.Forms.MouseButtons.Left && !MpHelpers.ApplicationIsActivated()) {
                        System.Windows.Forms.SendKeys.SendWait(" ^ c");
                    }
                }
                if (MpSidebarViewModel.Instance.IsRightClickPasteMode) {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right && !MpHelpers.ApplicationIsActivated()) {
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
