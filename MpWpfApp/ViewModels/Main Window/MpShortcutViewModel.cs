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

        private string _defaultKeyList = string.Empty;
        public string DefaultKeyList {
            get {
                return _defaultKeyList;
            }
            set {
                if(_defaultKeyList != value) {
                    _defaultKeyList = value;
                    OnPropertyChanged(nameof(DefaultKeyList));
                }
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

        //private string _shortcutTypeName = string.Empty;
        public string ShortcutTypeName {
            get {
                //return _shortcutTypeName;
                if (IsCustom()) {
                    if (Shortcut.CopyItemId > 0) {
                        return "Clip";
                    } else {
                        return "Tag";
                    }
                } else {
                    return "Application";
                }
            }
            
            //set {
            //    if (_shortcutTypeName != value) {
            //        _shortcutTypeName = value;
            //        OnPropertyChanged(nameof(ShortcutTypeName));
            //    }
            //}
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

        public bool IsNew {
            get {
                return Shortcut.ShortcutId == 0;
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
                    foreach (MpShortcut sc in MpShortcut.GetAllShortcuts()) {
                        _shortcutViewModels.Add(new MpShortcutViewModel(sc, null));
                    }
                }
                return _shortcutViewModels;
            }
            set {
                _shortcutViewModels = value;
            }
        }
        
        #endregion

        #region Public Methods
        public MpShortcutViewModel(string shortcutName, MpRoutingType routingType, ICommand command, string keys, string defaultKeys, int copyItemId = -1, int tagId = -1, int shortcutId = -1) {
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
                        if(IsCustom()) {
                            var mwvm = (MpMainWindowViewModel)Application.Current.MainWindow.DataContext;
                            if (Shortcut.CopyItemId > 0) {
                                var ctvm = mwvm.ClipTrayViewModel.Where(x => x.CopyItem.CopyItemId == Shortcut.CopyItemId).Single();
                                ctvm.ShortcutKeyList = Shortcut.KeyList;
                            } else {
                                var ttvm = mwvm.TagTrayViewModel.Where(x => x.Tag.TagId == Shortcut.TagId).Single();
                                ttvm.ShortcutKeyList = Shortcut.KeyList;
                            }
                        }
                        break;
                    case nameof(ShortcutDisplayName):
                        Shortcut.ShortcutName = ShortcutDisplayName;
                        break;
                    case nameof(RoutingType):
                        Shortcut.RoutingType = RoutingType;
                        break;
                    case nameof(DefaultKeyList):
                        Shortcut.DefaultKeyList = DefaultKeyList;
                        break;
                }
            };
            Shortcut = new MpShortcut();
            ShortcutId = shortcutId;
            CopyItemId = copyItemId;
            TagId = tagId;
            ShortcutDisplayName = shortcutName.Replace("'", string.Empty);
            KeyList = keys;
            DefaultKeyList = defaultKeys;
            RoutingType = routingType;

            Command = command;
            if (IsCustom()) {
                ResetButtonVisibility = Visibility.Collapsed;
                DeleteButtonVisibility = Visibility.Visible;
            } else {
                ResetButtonVisibility = Visibility.Visible;
                DeleteButtonVisibility = Visibility.Collapsed;
            }
        }

        public MpShortcutViewModel(MpShortcut s, ICommand command) : this(s.ShortcutName, s.RoutingType, command, s.KeyList, s.DefaultKeyList, s.CopyItemId, s.TagId, s.ShortcutId) { }

        public void Register() {
            //unregister if already exists
            Unregister();

            //only register if non-empty keysstring
            if (string.IsNullOrEmpty(KeyList)) {
                Shortcut.WriteToDatabase();
                return;
            } else {
                try {
                    var mwvm = (MpMainWindowViewModel)System.Windows.Application.Current.MainWindow.DataContext;
                    var t = new MouseKeyHook.Rx.Trigger[] { MouseKeyHook.Rx.Trigger.FromString(KeyList) };
                    if (RoutingType == MpRoutingType.None) {
                        throw new Exception("ShortcutViewModel error, routing type cannot be none");
                    }
                    if (RoutingType == MpRoutingType.Internal) {
                        KeysObservable = mwvm.ApplicationHook.KeyDownObservable().Matching(t).Subscribe((trigger) => {
                            if (mwvm.ClipTrayViewModel.IsEditingClipTitle ||
                                mwvm.TagTrayViewModel.IsEditingTagName ||
                                MpAssignShortcutModalWindowViewModel.IsOpen || 
                                MpSettingsWindowViewModel.IsOpen) {
                                //ignore hotkey since attempting to reassign
                            } else {
                                Command?.Execute(null);
                            }
                        });
                    } else {
                        KeysObservable = mwvm.GlobalHook.KeyDownObservable().Matching(t).Subscribe((trigger) => {
                            if (MpMainWindowViewModel.IsOpen ||
                                MpAssignShortcutModalWindowViewModel.IsOpen || 
                                MpSettingsWindowViewModel.IsOpen) {
                                //ignore hotkey since attempting to reassign
                            } else {
                                if (RoutingType == MpRoutingType.Bubble) {
                                    PassKeysToForegroundWindow();
                                }

                                Command?.Execute(null);

                                if (RoutingType == MpRoutingType.Tunnel) {
                                    PassKeysToForegroundWindow();
                                }
                            }
                        });
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("Error creating shortcut: " + ex.ToString());
                    return;
                }
                Shortcut.WriteToDatabase();
                Console.WriteLine("Shortcut Successfully registered for '" + ShortcutDisplayName + "' with hotkeys: " + KeyList);
                return;
            }
        }

        public void Unregister() {
            if(KeysObservable != null) {
                KeysObservable.Dispose();
                Console.WriteLine("Unregistering shortcut " + Shortcut.ToString() + " was successful");
            }
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

        public object Clone() {
            return new MpShortcutViewModel(Shortcut, Command);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
