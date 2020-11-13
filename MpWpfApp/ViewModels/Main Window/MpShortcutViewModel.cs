using System;
using System.Collections.Generic;
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

        public List<string> SendKeysKeyStringList {
            get {
                var outStrList = new List<string>();
                var combos = KeyList.Split(',').ToList();
                foreach(var combo in combos) {
                    var outStr = string.Empty;
                    var keys = combo.Split('+').ToList();
                    foreach(var key in keys) {
                        switch (key) {
                            case "Control":
                                outStr += "^";
                                break;
                            case "Shift":
                                outStr += "+";
                                break;
                            case "Alt":
                                outStr += "%";
                                break;
                            case "Enter":
                            case "Tab":
                                outStr += "{" + key.ToUpper() + "}";
                                break;
                            default:
                                outStr += key.ToUpper();
                                break;
                        }
                    }
                    outStrList.Add(outStr);
                }
                return outStrList;
            }
        }

        public bool IsRoutable {
            get {
                return RoutingType != MpRoutingType.None && RoutingType != MpRoutingType.Internal;
            }
        }

        public string DefaultKeyList {
            get {
                if(Shortcut != null) {
                    return Shortcut.DefaultKeyList;
                }
                return string.Empty;
            }
        }

        private int _copyItemId = 0;
        public int CopyItemId {
            get {
                return _copyItemId;
            }
            set {
                if (_copyItemId != value) {
                    _copyItemId = value;
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        private int _tagId = 0;
        public int TagId {
            get {
                return _tagId;
            }
            set {
                if (_tagId != value) {
                    _tagId = value;
                    OnPropertyChanged(nameof(TagId));
                }
            }
        }

        private int _shortcutId = 0;
        public int ShortcutId {
            get {
                return _shortcutId;
            }
            set {
                if (_shortcutId != value) {
                    _shortcutId = value;
                    OnPropertyChanged(nameof(ShortcutId));
                }
            }
        }

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

        private MpRoutingType _routingType = MpRoutingType.None;
        public MpRoutingType RoutingType {
            get {
                return _routingType;
            }
            set {
                if (_routingType != value) {
                    _routingType = value;
                    OnPropertyChanged(nameof(RoutingType));
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
        private static ObservableCollection<MpShortcutViewModel> _shortcutViewModels = null;
        public static ObservableCollection<MpShortcutViewModel> ShortcutViewModels {
            get {
                if(_shortcutViewModels == null) {
                    _shortcutViewModels = new ObservableCollection<MpShortcutViewModel>();
                    foreach(MpShortcut sc in MpShortcut.GetAllShortcuts()) {
                        _shortcutViewModels.Add(new MpShortcutViewModel(sc, null));
                    }
                }
                return _shortcutViewModels;
            }
            set {
                _shortcutViewModels = value;
            }
        }
        
        public static MpShortcutViewModel RegisterShortcutViewModel(string shortcutName, MpRoutingType routingType, ICommand command, string keys, int copyItemId = 0, int tagId = 0, int shortcutId = 0) {            
            //lookup shortcut by its command to see if it already exists
            var temp = ShortcutViewModels.Where(x => (x.ShortcutDisplayName == shortcutName && x.Shortcut.CopyItemId == copyItemId && x.Shortcut.TagId == tagId) || x.Shortcut.ShortcutId == shortcutId).ToList();
            int scvmIdx = -1;
            //if shortcut already exists update the keys or create a new one
            if(temp != null && temp.Count > 0) {
                scvmIdx = ShortcutViewModels.IndexOf(temp[0]);
                ShortcutViewModels[scvmIdx].KeyList = keys;
                ShortcutViewModels[scvmIdx].Command = command;
                ShortcutViewModels[scvmIdx].ShortcutId = shortcutId;
                ShortcutViewModels[scvmIdx].CopyItemId = copyItemId;
                ShortcutViewModels[scvmIdx].TagId = tagId;
                ShortcutViewModels[scvmIdx].RoutingType = routingType;
            } else {
                ShortcutViewModels.Add(new MpShortcutViewModel(shortcutName, routingType, command, keys, copyItemId, tagId));
                scvmIdx = ShortcutViewModels.Count - 1;
            }

            //unregister if already exists
            ShortcutViewModels[scvmIdx].Unregister();

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
                    if(ShortcutViewModels[scvmIdx].RoutingType == MpRoutingType.None) {
                        throw new Exception("ShortcutViewModel error, routing type cannot be none");
                    }
                    if (ShortcutViewModels[scvmIdx].RoutingType == MpRoutingType.Internal) {
                        ShortcutViewModels[scvmIdx].KeysObservable = mwvm.ApplicationHook.KeyDownObservable().Matching(t).Subscribe((trigger) => {
                            if (MpAssignShortcutModalWindowViewModel.IsOpen || MpSettingsWindowViewModel.IsOpen) {
                                //ignore hotkey since attempting to reassign
                            } else {
                                ShortcutViewModels[scvmIdx].Command?.Execute(null);
                            }
                        });
                    } else {
                        ShortcutViewModels[scvmIdx].KeysObservable = mwvm.GlobalHook.KeyDownObservable().Matching(t).Subscribe((trigger) => {
                            if (MpAssignShortcutModalWindowViewModel.IsOpen || MpSettingsWindowViewModel.IsOpen) {
                                //ignore hotkey since attempting to reassign
                            } else {
                                if (ShortcutViewModels[scvmIdx].RoutingType == MpRoutingType.Bubble) {
                                    ShortcutViewModels[scvmIdx].PassKeysToForegroundWindow();
                                }

                                ShortcutViewModels[scvmIdx].Command?.Execute(null);

                                if (ShortcutViewModels[scvmIdx].RoutingType == MpRoutingType.Tunnel) {
                                    ShortcutViewModels[scvmIdx].PassKeysToForegroundWindow();
                                }
                            }
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
        public MpShortcutViewModel(string shortcutName, MpRoutingType routingType, ICommand command, string keys, int copyItemId = -1, int tagId = -1, int shortcutId = -1) {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(CopyItemId):
                        Shortcut.CopyItemId = CopyItemId;                        
                        break;
                    case nameof(TagId):
                        Shortcut.TagId = TagId;
                        break;
                    case nameof(ShortcutId):
                        Shortcut.ShortcutId = ShortcutId;
                        break;
                    case nameof(KeyList):
                        Shortcut.KeyList = KeyList;
                        break;
                    case nameof(ShortcutDisplayName):
                        Shortcut.ShortcutName = ShortcutDisplayName;
                        break;
                    case nameof(RoutingType):
                        Shortcut.RoutingType = RoutingType;
                        break;
                }
            };
            Shortcut = new MpShortcut();
            Shortcut.ShortcutId = shortcutId;
            Shortcut.CopyItemId = copyItemId;
            Shortcut.TagId = tagId;
            KeyList = keys;
            ShortcutDisplayName = shortcutName;
            RoutingType = routingType;
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

        public MpShortcutViewModel(MpShortcut s, ICommand command) : this(s.ShortcutName, s.RoutingType, command, s.KeyList, s.CopyItemId, s.TagId, s.ShortcutId) {}

        public void Unregister() {
            KeysObservable?.Dispose();
            Console.WriteLine("Unregistering shortcut " + Shortcut.ToString() + " was successful");
        }

        public void PassKeysToForegroundWindow() {
            var mwvm = (MpMainWindowViewModel)System.Windows.Application.Current.MainWindow.DataContext;
            WinApi.SetForegroundWindow(mwvm.ClipTrayViewModel.ClipboardMonitor.LastWindowWatcher.LastHandle);
            foreach (var str in SendKeysKeyStringList) {
                System.Windows.Forms.SendKeys.SendWait(str);
            }
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
