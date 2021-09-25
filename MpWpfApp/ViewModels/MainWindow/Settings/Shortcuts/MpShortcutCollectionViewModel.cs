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
        #endregion

        #endregion

        #region Public Methods
        public MpShortcutCollectionViewModel() : base(null) { }

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
                        scvm.Shortcut.WriteToDatabase();
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
                this.Add(vm, shortcutKeyString, command, commandParameter);                
            }
            MainWindowViewModel.IsShowingDialog = false;
            return shortcutKeyString;
        }

        public void Add(object vm, string keys, ICommand command, object commandParameter) {
            MpShortcutViewModel nscvm = null;
            if (vm.GetType() == typeof(MpContentItemViewModel)) {
                var ctvm = (MpContentItemViewModel)vm;
                nscvm = new MpShortcutViewModel(
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

                nscvm.Register();

                nscvm.Shortcut.WriteToDatabase();
            }
        }

        public void Remove(MpShortcutViewModel scvm) {
            Shortcuts.Remove(scvm);
            scvm.Unregister();
            if(scvm.IsCustom()) {
                scvm.Shortcut.DeleteFromDatabase();
                if (scvm.Shortcut.CopyItemId > 0) {
                    var ctvm = MpClipTrayViewModel.Instance.GetCopyItemViewModelById(scvm.Shortcut.CopyItemId);
                    if(ctvm != null) {
                        ctvm.ShortcutKeyString = string.Empty;
                    } else {
                        foreach(var ctvm1 in MpClipTrayViewModel.Instance.ClipTileViewModels) {
                            foreach(var rtbvm in ctvm1.ItemViewModels) {
                                if(rtbvm.CopyItem.Id == scvm.CopyItemId) {
                                    rtbvm.ShortcutKeyString = string.Empty;
                                }
                            }
                        }
                    }
                } else {
                    foreach(var ttvm in MpTagTrayViewModel.Instance.TagTileViewModels.Where(x => x.Tag.Id == scvm.Shortcut.TagId).ToList()) {
                        ttvm.ShortcutKeyString = string.Empty;
                    }
                }
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            var scvmToRemoveList = new List<MpShortcutViewModel>();
            if (e is MpCopyItem ci) {
                foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Shortcuts.Where(x => x.CopyItemId == ci.Id).ToList()) {
                    scvmToRemoveList.Add(scvmToRemove);
                }
                
            } else if(e is MpTag t) {
                foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Shortcuts.Where(x => x.TagId == t.Id).ToList()) {
                    scvmToRemoveList.Add(scvmToRemove);
                }
            }

            foreach (var scvmToRemove in scvmToRemoveList) {
                MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
            }
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
        #endregion

        #region Private Methods
        private bool InitHotkeys() {
            try {
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
                };

                GlobalHook.MouseWheel += (s, e) => {
                    if (Properties.Settings.Default.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                        if (e.Y <= Properties.Settings.Default.ShowMainWindowMouseHitZoneHeight) {
                            MainWindowViewModel.ShowWindowCommand.Execute(null);
                        }
                    }
                };

                ApplicationHook.MouseWheel += (s, e) => {
                    if (!MpMainWindowViewModel.IsMainWindowLoading &&
                        MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                        var rtbvm = MpClipTrayViewModel.Instance.PrimaryItem.ItemViewModels[0];//SelectedItems[0].RtbItemCollectionViewModel;
                        rtbvm.RequestScrollWheelChange(e.Delta);

                        //var sv = (ScrollViewer)rtbvm.HostClipTileViewModel.ClipBorder.FindName("ClipTileRichTextBoxListBoxScrollViewer");//RtbLbAdornerLayer.GetVisualAncestor<ScrollViewer>();
                        //sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta);
                    }
                };
                #endregion

                #region Keyboard
                GlobalHook.OnCombination(new Dictionary<Combination, Action> {{
                        Combination.FromString("Control+V"), () => {
                            try {
                                string cbText = Clipboard.GetText();
                                if(!string.IsNullOrEmpty(cbText)) {
                                    Application.Current.Dispatcher.BeginInvoke((Action)(()=>{
                                        foreach(var ctvm in MpClipTrayViewModel.Instance.ClipTileViewModels) {
                                            foreach(var rtbvm in ctvm.ItemViewModels) {
                                                if(rtbvm.CopyItem.ItemData.ToPlainText() == cbText) {
                                                    rtbvm.CopyItem.PasteCount++;
                                                }
                                            }
                                        }
                                    }),System.Windows.Threading.DispatcherPriority.Background);
                                }
                            } catch(Exception ex) {
                                MonkeyPaste.MpConsole.WriteLine("Global Keyboard Paste watch exception getting text: "+ex);
                            }

                        }
                }});

                ApplicationHook.KeyPress += (s, e) => {
                    if (MpClipTrayViewModel.Instance != null && MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                        return;
                    }
                    if (MainWindowViewModel.SearchBoxViewModel != null && MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused) {
                        return;
                    }
                    if (MainWindowViewModel.TagTrayViewModel != null && MainWindowViewModel.TagTrayViewModel.IsEditingTagName) {
                        return;
                    }
                    if (MpClipTrayViewModel.Instance != null && MpClipTrayViewModel.Instance.IsEditingClipTitle) {
                        return;
                    }
                    if (MpSettingsWindowViewModel.IsOpen || MpAssignShortcutModalWindowViewModel.IsOpen) {
                        return;
                    }
                    if (!char.IsControl(e.KeyChar)) {
                        foreach (var scvm in MpShortcutCollectionViewModel.Instance.Shortcuts) {
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
                MonkeyPaste.MpConsole.WriteLine("Error creating mainwindow hotkeys: " + ex.ToString());
                return false;
            }
            return true;
        }

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
                        shortcutCommand = MainWindowViewModel.UndoCommand;
                        break;
                    case 19:
                        shortcutCommand = MainWindowViewModel.RedoCommand;
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
                                var ctvm = MpClipTrayViewModel.Instance.GetCopyItemViewModelById(sc.CopyItemId);
                                if(ctvm == null) {
                                    var ci = MpCopyItem.GetCopyItemById(sc.CopyItemId);
                                    if(ci == null) {
                                        MonkeyPaste.MpConsole.WriteLine("SHortcut init error cannot find copy item w/ id: " + sc.CopyItemId);
                                        break;
                                    }
                                    //ctvm = MpClipTrayViewModel.Instance.GetCopyItemViewModelById(ci.CompositeParentCopyItemId);
                                    if(ctvm == null) {
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
                                shortcutCommand = ttvm.SelectTagCommand;
                            }
                        }
                        catch (Exception ex) {
                            MonkeyPaste.MpConsole.WriteLine("ShortcutCollection init error, unknown shortcut: " + sc.ToString());
                            MonkeyPaste.MpConsole.WriteLine("With exception: " + ex.ToString());
                        }
                        break;
                }
                var scvm = new MpShortcutViewModel(sc, shortcutCommand, commandParameter);
                scvm.Register();
                Shortcuts.Add(scvm);
            }
            OnViewModelLoaded();
        }

        public MpShortcutViewModel GetShortcutViewModelById(int shortcutId) {
            var scvml = Shortcuts.Where(x => x.ShortcutId == shortcutId).ToList();
            if(scvml.Count > 0) {
                return scvml[0];
            }
            return null;
        }
        #endregion

        #region Commands
        #endregion
    }
}
