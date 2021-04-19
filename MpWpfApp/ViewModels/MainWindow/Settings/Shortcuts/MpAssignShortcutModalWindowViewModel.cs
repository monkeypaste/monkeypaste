using GalaSoft.MvvmLight.Command;
using Gma.System.MouseKeyHook;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        #region Controls
        public Canvas KeyCanvas { get; set; }
        #endregion

        private ObservableCollection<MpShortcutKeyViewModel> _keyItems = new ObservableCollection<MpShortcutKeyViewModel>();
        public ObservableCollection<MpShortcutKeyViewModel> KeyItems {
            get {
                return _keyItems;
            }
            set {
                if(_keyItems != value) {
                    _keyItems = value;
                    OnPropertyChanged(nameof(KeyItems));
                }
            }
        }

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

        public string KeyString {
            get {
                KeyItems.Clear();
                foreach(var kl in _keyList) {
                    foreach(var k in kl) {
                        if(kl.Count > 1 && kl.IndexOf(k) < kl.Count - 1) {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpHelpers.Instance.GetKeyLiteral(k),
                                            true,false));
                        } else if (kl.IndexOf(k) == kl.Count - 1 && _keyList.IndexOf(kl) < _keyList.Count -1) {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpHelpers.Instance.GetKeyLiteral(k),
                                            false, true));
                        } else {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpHelpers.Instance.GetKeyLiteral(k),
                                            false, false));
                        }

                    }
                }
                return MpHelpers.Instance.ConvertKeySequenceToString(_keyList);
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
                return ((MpAssignShortcutModalWindowViewModel)ascw.DataContext).KeyString;
            } else {
                return null;
            }
        }
        #endregion

        #region Public Methods
        public MpAssignShortcutModalWindowViewModel() : this(string.Empty,string.Empty,null) { }

        public void AssignHotkeyModalWindow_Loaded(object sender, RoutedEventArgs e) {
            IsOpen = true;

            _windowRef = (Window)sender;
            KeyCanvas = _windowRef.FindName("KeyCanvas") as Canvas;

            //the following hides close button
            var hwnd = new WindowInteropHelper(_windowRef).Handle;
            WinApi.SetWindowLong(hwnd, WinApi.GWL_STYLE, WinApi.GetWindowLong(hwnd, WinApi.GWL_STYLE) & ~WinApi.WS_SYSMENU);

            _windowRef.PreviewKeyDown += (s, e1) => {
                seqTimer.Stop();
                
                if (_isSeqComplete) {
                    ClearCommand.Execute(null);
                }
                int precount = KeyString.Length;
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftCtrl)) {
                    AddKey(Key.LeftCtrl, _isNewCombination && KeyString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightCtrl)) {
                    AddKey(Key.LeftCtrl, _isNewCombination && KeyString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftShift)) {
                    AddKey(Key.LeftShift, _isNewCombination && KeyString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightShift)) {
                    AddKey(Key.LeftShift, _isNewCombination && KeyString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftAlt)) {
                    AddKey(Key.LeftAlt, _isNewCombination && KeyString.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightAlt)) {
                    AddKey(Key.LeftAlt, _isNewCombination && KeyString.Length == precount);
                }
                //if (e1.KeyboardDevice.IsKeyDown(Key.LWin)) {
                //    AddKey(Key.LWin, _isNewCombination && KeysString.Length == precount);
                //}
                //if (e1.KeyboardDevice.IsKeyDown(Key.RWin)) {
                //    AddKey(Key.LWin, _isNewCombination && KeysString.Length == precount);
                //}
                if (e1.Key != Key.LeftCtrl &&
                   e1.Key != Key.RightCtrl &&
                   e1.Key != Key.LeftAlt &&
                   e1.Key != Key.RightAlt &&
                   e1.Key != Key.LeftShift &&
                   e1.Key != Key.RightShift /*&&
                   e1.Key != Key.LWin &&
                   e1.Key != Key.RWin*/) {
                    if (KeyString.Length != precount) {
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

        #region Private Methods

        private MpAssignShortcutModalWindowViewModel(string shortcutName, string keyString, ICommand command) : base() {
            _keyList = MpHelpers.Instance.ConvertStringToKeySequence(keyString);
            _assigningCommand = command;
            ShortcutDisplayName = shortcutName;
            //KeysString = keysList;
            _isSeqComplete = true;
        }

        private void AddKey(Key key, bool isNewCombination) {
            if (isNewCombination && KeyString.Length > 0) {
                //KeysString += ",";
                _keyList.Add(new List<Key>());
            }
            if (_keyList.Count == 0) {
                _keyList.Add(new List<Key>());
            }
            if (!_keyList[_keyList.Count - 1].Contains(key)) {
                _keyList[_keyList.Count - 1].Add(key);
            }

            for (int i = 0; i < _keyList.Count; i++) {
                var kl = _keyList[i];
                if (kl.Contains(Key.LeftShift)) {
                    kl.Remove(Key.LeftShift);
                    kl.Insert(0, Key.LeftShift);
                }
                if (kl.Contains(Key.LeftAlt)) {
                    kl.Remove(Key.LeftAlt);
                    kl.Insert(0, Key.LeftAlt);
                }
                if (kl.Contains(Key.LeftCtrl)) {
                    kl.Remove(Key.LeftCtrl);
                    kl.Insert(0, Key.LeftCtrl);
                }
                _keyList[i] = kl;
            }

            OnPropertyChanged(nameof(KeyString));
            Validate();
        }

        private bool Validate() {
            //when KeysString changes check full system for duplicates, ignoring order of combinations
            WarningString = string.Empty;
            DuplicatedShortcutViewModel = null;

            //iterate over ALL shortcuts
            foreach (var scvm in MpShortcutCollectionViewModel.Instance) {                
                if (scvm.Command == _assigningCommand ||
                    scvm.KeyList.Count != _keyList.Count || 
                    scvm.KeyList.Count == 0) {
                    //ignore same, empty or shortcut w/ different key counts
                    continue;
                }

                bool isDuplicate = true;
                int klIdx = 0;
                foreach(var kl in scvm.KeyList) {
                    if(kl.Count != _keyList[klIdx].Count) {
                        isDuplicate = false;
                        break;
                    }
                    foreach(var k in kl) {
                        if(!_keyList[klIdx].Contains(k)) {
                            isDuplicate = false;
                        }
                    }
                    klIdx++;
                }
                if (isDuplicate && KeyString != string.Empty) {
                    DuplicatedShortcutViewModel = scvm;
                    WarningString = "This combination conflicts with '" + scvm.ShortcutDisplayName + "' which will be cleared if saved";
                    return false;
                }
            }
            return true;
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
            _isSeqComplete = false;
            _isNewCombination = true;
            _keyList.Clear();

            Validate();
            OnPropertyChanged(nameof(KeyString));
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
                if(DuplicatedShortcutViewModel.IsCustom()) {
                    if(DuplicatedShortcutViewModel.CopyItemId > 0) {
                        //clear input gesture text
                        MainWindowViewModel.ClipTrayViewModel.GetClipTileByCopyItemId(DuplicatedShortcutViewModel.CopyItemId).ShortcutKeyString = string.Empty;
                    } else {
                        MainWindowViewModel.TagTrayViewModel.Where(x => x.Tag.TagId == DuplicatedShortcutViewModel.TagId).ToList()[0].ShortcutKeyString = string.Empty;
                    }
                }
                DuplicatedShortcutViewModel.KeyString = string.Empty;
                DuplicatedShortcutViewModel.Shortcut.WriteToDatabase();
                DuplicatedShortcutViewModel.Unregister();
            }
            _windowRef.DialogResult = true;
            _windowRef.Close();
        }
        #endregion
    }
}
