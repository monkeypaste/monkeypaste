using GalaSoft.MvvmLight.Command;
using Gma.System.MouseKeyHook;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MpWpfApp {
    public class MpAssignShortcutModalWindowViewModel : MpViewModelBase {
        #region Static Variables
        public static bool IsOpen = false;
        #endregion

        #region Private Variables
        private bool _isSeqComplete = false;
        private bool _isNewCombination = true;

        private Window _windowRef = null;

        private System.Timers.Timer seqTimer = null;
        private double seqTimerMaxMs = 1000;

        private ICommand _assigningCommand = null;
        private string _keys = string.Empty;

        private List<List<Key>> _keyList = new List<List<Key>>();
        #endregion

        #region Properties
        private MpShortcutViewModel _duplicatedShortcutViewModel;
        public MpShortcutViewModel DuplicatedShortcutViewModel {
            get {
                return _duplicatedShortcutViewModel;
            }
            set {
                if (_duplicatedShortcutViewModel != value) {
                    _duplicatedShortcutViewModel = value;
                    OnPropertyChanged(nameof(DuplicatedShortcutViewModel));
                }
            }
        }

        //private MpShortcut _shortcut;
        //public MpShortcut Shortcut {
        //    get {
        //        return _shortcut;
        //    }
        //    set {
        //        if (_shortcut != value) {
        //            _shortcut = value;
        //            OnPropertyChanged(nameof(Shortcut));
        //        }
        //    }
        //}

        private string _shortcutDisplayName = string.Empty;
        public string ShortcutDisplayName {
            get {
                return _shortcutDisplayName;
            }
            set {
                if(_shortcutDisplayName != value) {
                    _shortcutDisplayName = value;
                    OnPropertyChanged(nameof(ShortcutDisplayName));
                }
            }
        }

        private string _keysString = string.Empty;
        public string KeysString {
            get {
                return _keysString;
            }
            set {
                if(_keysString != value) {
                    _keysString = value;
                    OnPropertyChanged(nameof(KeysString));
                }
            }
        }

        private string _warningString = string.Empty;
        public string WarningString {
            get {
                return _warningString;
            }
            set {
                if (_warningString != value) {
                    _warningString = value;
                    OnPropertyChanged(nameof(WarningString));
                }
            }
        }
        #endregion

        #region Static Methods
        public static string ShowAssignShortcutWindow(string shortcutName,string keys,ICommand command) {
            //var ascwvm = new MpAssignShortcutModalWindowViewModel(shortcutName);
            var ascw = new MpAssignHotkeyModalWindow();
            ascw.DataContext = new MpAssignShortcutModalWindowViewModel(shortcutName,keys,command);
            var assignResult = ascw.ShowDialog();
            if (assignResult == true) {
                return ((MpAssignShortcutModalWindowViewModel)ascw.DataContext).KeysString;
            } else {
                return null;
            }
        }
        #endregion

        #region Private Methods

        private MpAssignShortcutModalWindowViewModel(string shortcutName, string keysList, ICommand command) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(KeysString):
                        //when KeysString changes check full system for duplicates, ignoring order of combinations
                        WarningString = string.Empty;
                        DuplicatedShortcutViewModel = null;
                        //split hotkey into sequences combinations
                        var combos = KeysString.Split(',').ToList<string>();
                        //iterate over ALL shortcuts
                        foreach (var scvm in MpShortcutCollectionViewModel.Instance) {
                            //ignore same shortcut comparision
                            if (scvm.Command == _assigningCommand) {
                                continue;
                            }
                            var scCombos = scvm.KeyList.Split(',').ToList<string>();
                            if (combos.Count != scCombos.Count) {
                                continue;
                            }
                            int curComboIdx = 0;
                            bool isDuplicate = false;
                            foreach (string scCombo in scCombos) {
                                var scKeys = scCombo.Split('+').ToList<string>();
                                var keys = combos[curComboIdx].Split('+').ToList<string>();
                                if (keys.Count != scKeys.Count) {
                                    isDuplicate = false;
                                    break;
                                }
                                foreach (string k in keys) {
                                    if (scKeys.Contains(k)) {
                                        isDuplicate = true;
                                    } else {
                                        isDuplicate = false;
                                    }
                                }
                                curComboIdx++;
                            }
                            if (isDuplicate && KeysString != string.Empty) {
                                DuplicatedShortcutViewModel = scvm;
                                WarningString = "This combination conflicts with '" + scvm.ShortcutDisplayName + "' which will be cleared if saved";
                                break;
                            }
                        }
                        break;
                }
            };
            _assigningCommand = command;
            ShortcutDisplayName = shortcutName;
            KeysString = keysList;
            _isSeqComplete = true;
        }

        public void AddKey(Key key, bool isNewCombination) {
            if(isNewCombination && KeysString.Length > 0) {
                KeysString += ",";
                _keyList.Add(new List<Key>());
            }
            if(_keyList.Count == 0) {
                _keyList.Add(new List<Key>());
            }
            if (!_keyList[_keyList.Count - 1].Contains(key)) {
                _keyList[_keyList.Count - 1].Add(key);
                switch (key) {
                    case Key.LeftCtrl:
                        KeysString += "+Control";
                        break;
                    case Key.LeftShift:
                        KeysString += "+Shift";
                        break;
                    case Key.LeftAlt:
                        KeysString += "+Alt";
                        break;
                    case Key.LWin:
                        KeysString += "+LWin";
                        break;
                    default:
                        KeysString += "+" + key.ToString();
                        break;
                }
            }
            if (KeysString.StartsWith("+")) {
                KeysString = KeysString.Remove(0, 1);
            }
            KeysString = KeysString.Replace(",+", ",");

            //sort keysstring by with precedence ctrl, win, alt, shift, text
            //int sortIdx = 0;
            //var ksl = KeysString.Split('+').ToList();
            //while(sortIdx < ksl.Count) {

            //}
        }
        #endregion

        #region Public Methods
        public MpAssignShortcutModalWindowViewModel() : this(string.Empty,string.Empty,null) { }

        public void AssignHotkeyModalWindow_Loaded(object sender, RoutedEventArgs e) {
            IsOpen = true;

            _windowRef = (Window)sender;

            //the following hides close button
            var hwnd = new WindowInteropHelper(_windowRef).Handle;
            WinApi.SetWindowLong(hwnd, WinApi.GWL_STYLE, WinApi.GetWindowLong(hwnd, WinApi.GWL_STYLE) & ~WinApi.WS_SYSMENU);

            _windowRef.PreviewKeyDown += (s, e1) => {
                seqTimer.Stop();

                if (_isSeqComplete) {
                    KeysString = string.Empty;
                    _isSeqComplete = false;
                    _isNewCombination = true;
                }
                int precount = KeysString.Length;
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftCtrl)) {
                    AddKey(Key.LeftCtrl, _isNewCombination && KeysString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightCtrl)) {
                    AddKey(Key.LeftCtrl, _isNewCombination && KeysString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftShift)) {
                    AddKey(Key.LeftShift, _isNewCombination && KeysString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightShift)) {
                    AddKey(Key.LeftShift, _isNewCombination && KeysString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftAlt)) {
                    AddKey(Key.LeftAlt, _isNewCombination && KeysString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightAlt)) {
                    AddKey(Key.LeftAlt, _isNewCombination && KeysString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LWin)) {
                    AddKey(Key.LWin, _isNewCombination && KeysString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RWin)) {
                    AddKey(Key.LWin, _isNewCombination && KeysString.Length == precount);
                }
                if (e1.Key != Key.LeftCtrl &&
                   e1.Key != Key.RightCtrl &&
                   e1.Key != Key.LeftAlt &&
                   e1.Key != Key.RightAlt &&
                   e1.Key != Key.LeftShift &&
                   e1.Key != Key.RightShift &&
                   e1.Key != Key.LWin &&
                   e1.Key != Key.RWin) {
                    if (KeysString.Length != precount) {
                        _isNewCombination = false;
                    }
                    AddKey(e1.Key, _isNewCombination);
                } else {
                    _isNewCombination = false;
                }
            };

            _windowRef.PreviewKeyUp += (s, e1) => {
                _isNewCombination = true;
                //KeysString = KeyList;
                seqTimer.Start();
            };

            _windowRef.Closed += (s, e1) => {
                IsOpen = false;
            };

            seqTimer = new System.Timers.Timer(seqTimerMaxMs);
            seqTimer.AutoReset = true;
            seqTimer.Elapsed += (s, e2) => {
                _isSeqComplete = true;
            };

            _windowRef.Focus();
        }
        #endregion

        #region Commands
        private RelayCommand _cancelCommand;
        public ICommand CancelCommand {
            get {
                if(_cancelCommand == null) {
                    _cancelCommand = new RelayCommand(Cancel);
                }
                return _cancelCommand;
            }
        }
        private void Cancel() {
            KeysString = string.Empty;
            _windowRef.DialogResult = false;
            _windowRef.Close();
        }

        private RelayCommand _clearCommand;
        public ICommand ClearCommand {
            get {
                if (_clearCommand == null) {
                    _clearCommand = new RelayCommand(Clear);
                }
                return _clearCommand;
            }
        }
        private void Clear() {
            KeysString = string.Empty;
        }

        private RelayCommand _okCommand;
        public ICommand OkCommand {
            get {
                if (_okCommand == null) {
                    _okCommand = new RelayCommand(Ok);
                }
                return _okCommand;
            }
        }
        private void Ok() {
            if(DuplicatedShortcutViewModel != null) {
                if(DuplicatedShortcutViewModel.CopyItemId > 0 || DuplicatedShortcutViewModel.TagId > 0) {
                    if(DuplicatedShortcutViewModel.CopyItemId > 0) {
                        //clear input gesture text
                        MainWindowViewModel.ClipTrayViewModel.Where(x => x.CopyItem.CopyItemId == DuplicatedShortcutViewModel.CopyItemId).ToList()[0].ShortcutKeyList = string.Empty;
                        // TODO Unregister hotkey here
                    } else {
                        MainWindowViewModel.TagTrayViewModel.Where(x => x.Tag.TagId == DuplicatedShortcutViewModel.TagId).ToList()[0].ShortcutKeyList = string.Empty;
                        // TODO Unregister hotkey here
                    }
                    //DuplicatedShortcutViewModel.Shortcut.DeleteFromDatabase();
                }
                DuplicatedShortcutViewModel.KeyList = string.Empty;
                DuplicatedShortcutViewModel.Shortcut.WriteToDatabase();
                DuplicatedShortcutViewModel.Unregister();
            }
            _windowRef.DialogResult = true;
            _windowRef.Close();
        }
        #endregion
    }
}
