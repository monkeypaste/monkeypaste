using System;
using System.Linq;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpShortcutCollectionViewModel : MpObservableCollectionViewModel<MpShortcutViewModel> {
        private static readonly Lazy<MpShortcutCollectionViewModel> _Lazy = new Lazy<MpShortcutCollectionViewModel>(() => new MpShortcutCollectionViewModel());
        public static MpShortcutCollectionViewModel Instance { get { return _Lazy.Value; } }

        #region View Models
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public void Init() {
            //empty call to init singleton
        }

        public MpShortcutCollectionViewModel() : base() {
            //using mainwindow, map all saved shortcuts to their commands
            foreach (var sc in MpShortcut.GetAllShortcuts()) {
                ICommand shortcutCommand = null;
                object commandParameter = null;
                switch(sc.ShortcutId) {
                    case 1:
                        shortcutCommand = MainWindowViewModel.ShowWindowCommand;
                        break;
                    case 2:
                        shortcutCommand = MainWindowViewModel.HideWindowCommand;
                        break;
                    case 3:
                        shortcutCommand = MainWindowViewModel.AppModeViewModel.ToggleAppendModeCommand;
                        commandParameter = true;
                        break;
                    case 4:
                        shortcutCommand = MainWindowViewModel.AppModeViewModel.ToggleAutoCopyModeCommand;
                        commandParameter = true;
                        break;
                    case 5:
                        //right click paste mode
                        shortcutCommand = MainWindowViewModel.AppModeViewModel.ToggleRightClickPasteCommand;
                        commandParameter = true;
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
                        //search
                        shortcutCommand = null;
                        break;
                    case 9:
                        //left
                        shortcutCommand = null;
                        break;
                    case 10:
                        //right
                        shortcutCommand = null;
                        break;
                    case 11:
                        //select all
                        shortcutCommand = null;
                        break;
                    case 12:
                        //invert selection
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.InvertSelectionCommand;
                        break;
                    case 13:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.BringSelectedClipTilesToFrontCommand;
                        break;
                    case 14:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.SendSelectedClipTilesToBackCommand;
                        break;
                    case 15:
                        //assign hotkey
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.AssignHotkeyCommand;
                        break;
                    case 16:
                        //change color
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.ChangeSelectedClipsColorCommand;
                        break;
                    case 17:
                        //share
                        shortcutCommand = null;
                        break;
                    case 18:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.SpeakSelectedClipsAsyncCommand;
                        break;
                    case 19:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.MergeSelectedClipsCommand;
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
                        } catch(Exception ex) {
                            Console.WriteLine("ShortcutCollection init error, unknown shortcut: " + sc.ToString());
                            Console.WriteLine("With exception: " + ex.ToString());
                        }
                        break;
                }
                var scvm = new MpShortcutViewModel(sc, shortcutCommand, commandParameter);
                scvm.Register();
                this.Add(scvm);
            }
        }

        public string RegisterViewModelShortcut(
            MpViewModelBase vm, 
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
            if (vm.GetType() == typeof(MpClipTileViewModel)) {
                var ctvm = (MpClipTileViewModel)vm;
                base.Add(
                    new MpShortcutViewModel(
                        new MpShortcut(
                            ctvm.CopyItem.CopyItemId,
                            0,
                            keys,
                            "Paste " + ctvm.CopyItemTitle),
                            command,commandParameter));
            } else if (vm.GetType() == typeof(MpTagTileViewModel)) {
                var ttvm = (MpTagTileViewModel)vm;
                base.Add(
                    new MpShortcutViewModel(
                        new MpShortcut(
                            0,
                            ttvm.TagId,
                            keys,
                            "Select " + ttvm.TagName),
                            command,commandParameter));
            } else if (vm.GetType() == typeof(MpShortcutViewModel)) {
                var scvm = (MpShortcutViewModel)vm;
                scvm.KeyString = keys;
                scvm.Command = command;
                scvm.CommandParameter = commandParameter;
                scvm.Shortcut.WriteToDatabase();

                //check by command if shortcut exists if it does swap it with scvm otherwise add and always register
                var curScvml = this.Where(x => x.Command == scvm.Command).ToList();
                if (curScvml != null && curScvml.Count > 0) {
                    foreach (var curscvm in curScvml) {
                        this[this.IndexOf(curscvm)] = scvm;
                    }
                } else {
                    this.Insert(this.Count, scvm);
                }

                scvm.Register();
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
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
