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
                switch(sc.ShortcutId) {
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
                        shortcutCommand = null;
                        break;
                    case 6:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand;
                        break;
                    case 7:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.DeleteSelectedClipsCommand;
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
                        shortcutCommand = null;
                        break;
                    case 13:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.BringSelectedClipTilesToFrontCommand;
                        break;
                    case 14:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.SendSelectedClipTilesToBackCommand;
                        break;
                    case 15:
                        //assign hotkey
                        shortcutCommand = null;
                        break;
                    case 16:
                        //change color
                        shortcutCommand = null;
                        break;
                    case 17:
                        //share
                        shortcutCommand = null;
                        break;
                    case 18:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.SpeakSelectedClipsCommand;
                        break;
                    case 19:
                        shortcutCommand = MainWindowViewModel.ClipTrayViewModel.MergeSelectedClipsCommand;
                        break;
                    default:
                        try {
                            if (sc.CopyItemId > 0) {
                                var ctvm = MainWindowViewModel.ClipTrayViewModel.ClipTileViewModels.Where(x => x.CopyItem.CopyItemId == sc.CopyItemId).Single();
                                ctvm.ShortcutKeyList = sc.KeyList;
                                shortcutCommand = ctvm.PasteClipCommand;
                            } else if (sc.TagId > 0) {
                                var ttvm = MainWindowViewModel.TagTrayViewModel.Where(x => x.Tag.TagId == sc.TagId).Single();
                                ttvm.ShortcutKeyList = sc.KeyList;
                                shortcutCommand = ttvm.SelectTagCommand;
                            }
                        } catch(Exception ex) {
                            Console.WriteLine("ShortcutCollection init error, unknown shortcut: " + sc.ToString());
                        }
                        break;
                }
                this.Add(new MpShortcutViewModel(sc, shortcutCommand));
            }
        }

        public string RegisterViewModelShortcut(
            MpViewModelBase vm, 
            string title, 
            string keys, 
            ICommand command) {
            MainWindowViewModel.IsShowingDialog = true;
            var shortcutKeyList = MpAssignShortcutModalWindowViewModel.ShowAssignShortcutWindow(title, keys, command);

            if (shortcutKeyList == null) {
                //if assignment was canceled ignore but reset skl
                shortcutKeyList = string.Empty;
            } else if (shortcutKeyList == string.Empty) {
                //if an empty assignment was ok'd check if exists 
                var scvml = this.Where(x => x.Command == command).ToList();
                //if it does clear, save and unregister
                if (scvml != null && this.Count > 0) {
                    foreach (var scvm in scvml) {
                        scvm.ClearKeyList();
                        scvm.Shortcut.WriteToDatabase();
                        scvm.Unregister();
                    }
                } else {
                    //nothing to do since no shortcut created
                }
            } else {
                this.Add(vm, shortcutKeyList, command);                
            }
            MainWindowViewModel.IsShowingDialog = false;
            return shortcutKeyList;
        }
        public void Add(MpViewModelBase vm, string keys, ICommand command) {
            if (vm.GetType() == typeof(MpClipTileViewModel)) {
                this.Add((MpClipTileViewModel)vm, keys, command);
            } else if (vm.GetType() == typeof(MpTagTileViewModel)) {
                this.Add((MpTagTileViewModel)vm, keys, command);
            } else if (vm.GetType() == typeof(MpShortcutViewModel)) {
                this.Add((MpShortcutViewModel)vm, keys, command);
            }
        }
        public new void Remove(MpShortcutViewModel scvm) {
            base.Remove(scvm);
            scvm.Unregister();
            if(scvm.IsCustom()) {
                scvm.Shortcut.DeleteFromDatabase();
                if (scvm.Shortcut.CopyItemId > 0) {
                    foreach(var ctvm in MainWindowViewModel.ClipTrayViewModel.ClipTileViewModels.Where(x => x.CopyItem.CopyItemId == scvm.Shortcut.CopyItemId).ToList()) {
                        ctvm.ShortcutKeyList = string.Empty;
                    }
                } else {
                    foreach(var ttvm in MainWindowViewModel.TagTrayViewModel.Where(x => x.Tag.TagId == scvm.Shortcut.TagId).ToList()) {
                        ttvm.ShortcutKeyList = string.Empty;
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        private new void Add(MpShortcutViewModel scvm) {
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

        private void Add(MpClipTileViewModel ctvm, string keys, ICommand command) {
            //add new shortcut to collection
            this.Add(
                new MpShortcutViewModel(
                    new MpShortcut(
                        ctvm.CopyItem.CopyItemId,
                        0,
                        keys,
                        "Paste " + ctvm.CopyItemTitle),
                    command));
        }

        private void Add(MpTagTileViewModel ttvm, string keys, ICommand command) {
            //add new shortcut to collection
            this.Add(
                new MpShortcutViewModel(
                    new MpShortcut(
                        0,
                        ttvm.TagId,
                        keys,
                        "Select " + ttvm.TagName),
                    command));
        }
        #endregion

        #region Commands
        #endregion
    }
}
