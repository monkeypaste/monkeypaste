using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpShortcutCollectionViewModel : MpObservableCollectionViewModel<MpShortcutViewModel> {        
        #region View Models
        public MpMainWindowViewModel MainWindowViewModel { get; set; }
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public MpShortcutCollectionViewModel(MpMainWindowViewModel mwvm) {
            MainWindowViewModel = mwvm;

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
                                var ctvm = MainWindowViewModel.ClipTrayViewModel.Where(x => x.CopyItem.CopyItemId == sc.CopyItemId).Single();
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

        public new void Add(MpShortcutViewModel scvm) {
            if (scvm.IsNew) {
                scvm.Shortcut.WriteToDatabase();                
            }
            this.Insert(this.Count, scvm);

            scvm.Register();
        }

        public new void Remove(MpShortcutViewModel scvm) {
            base.Remove(scvm);
            scvm.Unregister();
            if(scvm.IsCustom()) {
                scvm.Shortcut.DeleteFromDatabase();
                if (scvm.Shortcut.CopyItemId > 0) {
                    var ctvm = MainWindowViewModel.ClipTrayViewModel.Where(x => x.CopyItem.CopyItemId == scvm.Shortcut.CopyItemId).Single();
                    ctvm.ShortcutKeyList = string.Empty;
                } else {
                    var ttvm = MainWindowViewModel.TagTrayViewModel.Where(x => x.Tag.TagId == scvm.Shortcut.TagId).Single();
                    ttvm.ShortcutKeyList = string.Empty;
                }
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
