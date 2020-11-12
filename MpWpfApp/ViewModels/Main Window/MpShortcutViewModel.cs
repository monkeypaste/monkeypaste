using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using MouseKeyHook.Rx;

namespace MpWpfApp {
    public class MpShortcutViewModel : MpViewModelBase {
        #region Properties
        public IDisposable KeysObservable { get; set; }

        private ICommand _command = null;
        public ICommand Command {
            get {
                return _command;
            }
            set {
                if (_command != value) {
                    _command = value;
                    OnPropertyChanged(nameof(Command));
                }
            }
        }

        private string _keyList = string.Empty;
        public string KeyList {
            get {
                return _keyList;
            }
            set {
                if(_keyList != value) {
                    _keyList = value;
                    OnPropertyChanged(nameof(KeyList));
                }
            }
        }

        private string _shortcutDisplayName = string.Empty;
        public string ShortcutDisplayName {
            get {
                return _shortcutDisplayName;
            }
            set {
                if (_shortcutDisplayName != value) {
                    _shortcutDisplayName = value;
                    OnPropertyChanged(nameof(ShortcutDisplayName));
                }
            }
        }

        private bool _isGlobal = false;
        public bool IsGlobal {
            get {
                return _isGlobal;
            }
            set {
                if (_isGlobal != value) {
                    _isGlobal = value;
                    OnPropertyChanged(nameof(IsGlobal));
                }
            }
        }

        private string _shortcutTypeName = string.Empty;
        public string ShortcutTypeName {
            get {
                return _shortcutTypeName;
            }
            set {
                if (_shortcutTypeName != value) {
                    _shortcutTypeName = value;
                    OnPropertyChanged(nameof(ShortcutTypeName));
                }
            }
        }

        private Visibility _deleteButtonVisibility;
        public Visibility DeleteButtonVisibility {
            get {
                return _deleteButtonVisibility;
            }
            set {
                if (_deleteButtonVisibility != value) {
                    _deleteButtonVisibility = value;
                    OnPropertyChanged(nameof(DeleteButtonVisibility));
                }
            }
        }

        private Visibility _resetButtonVisibility;
        public Visibility ResetButtonVisibility {
            get {
                return _resetButtonVisibility;
            }
            set {
                if (_resetButtonVisibility != value) {
                    _resetButtonVisibility = value;
                    OnPropertyChanged(nameof(ResetButtonVisibility));
                }
            }
        }

        public MpShortcut Shortcut { get; set; }
        #endregion

        #region Statics
        public static ObservableCollection<MpShortcutViewModel> ShortcutViewModels { get; set; } = new ObservableCollection<MpShortcutViewModel>();
        
        public static MpShortcutViewModel RegisterShortcutViewModel(string shortcutName, bool isGlobal, ICommand command, string keys, int copyItemId = -1, int tagId = -1) {            
            //lookup shortcut by its command to see if it already exists
            var temp = ShortcutViewModels.Where(x => x.Command == command).ToList();
            int scvmIdx = -1;
            //if shortcut already exists update the keys or create a new one
            if(temp != null && temp.Count > 0) {
                scvmIdx = ShortcutViewModels.IndexOf(temp[0]);
                ShortcutViewModels[scvmIdx].KeyList = keys;
            } else {
                ShortcutViewModels.Add(new MpShortcutViewModel(shortcutName, isGlobal, command, keys, copyItemId, tagId));
                scvmIdx = ShortcutViewModels.Count - 1;
            }

            //unregister if already exists
            if(ShortcutViewModels[scvmIdx].KeysObservable != null) {
                ShortcutViewModels[scvmIdx].KeysObservable.Dispose();
            }
            //only register if non-empty keysstring
            if (string.IsNullOrEmpty(ShortcutViewModels[scvmIdx].KeyList)) {
                if(ShortcutViewModels[scvmIdx].IsCustom()) {
                    ShortcutViewModels[scvmIdx].Shortcut.DeleteFromDatabase();
                    return null;
                } else {
                    ShortcutViewModels[scvmIdx].Shortcut.WriteToDatabase();
                    return ShortcutViewModels[scvmIdx];
                }
            } else { 
                try {
                    var mwvm = (MpMainWindowViewModel)System.Windows.Application.Current.MainWindow.DataContext;
                    var t = new MouseKeyHook.Rx.Trigger[] { MouseKeyHook.Rx.Trigger.FromString(ShortcutViewModels[scvmIdx].KeyList) };
                    if (ShortcutViewModels[scvmIdx].IsGlobal) {
                        ShortcutViewModels[scvmIdx].KeysObservable = mwvm.GlobalHook.KeyDownObservable().Matching(t).Subscribe((trigger) => {
                            ShortcutViewModels[scvmIdx].Command?.Execute(null);
                        });
                    } else {
                        ShortcutViewModels[scvmIdx].KeysObservable = mwvm.ApplicationHook.KeyDownObservable().Matching(t).Subscribe((trigger) => {
                            ShortcutViewModels[scvmIdx].Command?.Execute(null);
                        });
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("Error creating shortcut: " + ex.ToString());
                    return null;
                }
                ShortcutViewModels[scvmIdx].Shortcut.WriteToDatabase();
                Console.WriteLine("Shortcut Successfully registered for '" + ShortcutViewModels[scvmIdx].ShortcutDisplayName + "' with hotkeys: " + ShortcutViewModels[scvmIdx].KeyList);
                return ShortcutViewModels[scvmIdx];
            }
        }
        #endregion

        #region Public Methods
        public MpShortcutViewModel(string shortcutName, bool isGlobal, ICommand command, string keys, int copyItemId = -1, int tagId = -1) {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(KeyList):
                        Shortcut.KeyList = KeyList;
                        break;
                    case nameof(ShortcutDisplayName):
                        Shortcut.ShortcutName = ShortcutDisplayName;
                        break;
                    case nameof(IsGlobal):
                        Shortcut.IsGlobal = IsGlobal;
                        break;
                }
            };
            Shortcut = new MpShortcut();
            Shortcut.CopyItemId = copyItemId;
            Shortcut.TagId = tagId;
            KeyList = keys;
            ShortcutDisplayName = shortcutName;
            IsGlobal = isGlobal;
            Command = command;
            if (IsCustom()) {
                if (Shortcut.CopyItemId > 0) {
                    ShortcutTypeName = "Clip";
                } else {
                    ShortcutTypeName = "Tag";
                }
                ResetButtonVisibility = Visibility.Collapsed;
                DeleteButtonVisibility = Visibility.Visible;
            } else {
                ShortcutTypeName = "Application"; 
                ResetButtonVisibility = Visibility.Visible;
                DeleteButtonVisibility = Visibility.Collapsed;
            }
        }

        public MpShortcutViewModel(MpShortcut s, ICommand command) : this(s.ShortcutName, s.IsGlobal, command, s.KeyList, s.CopyItemId, s.TagId) {
            //Shortcut = s;
        }

        public bool IsSequence() {
            return KeyList.Contains(",");
        }
        public bool IsCustom() {
            return Shortcut.CopyItemId > 0 || Shortcut.TagId > 0;
        }
        public void ClearKeyList() {
            KeyList = string.Empty;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
