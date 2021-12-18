
using Gma.System.MouseKeyHook;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {

    public class MpAssignShortcutModalWindowViewModel : MpViewModelBase<object> {
        #region Static Variables
        public static bool IsOpen = false;
        #endregion

        #region Private Variables
        private ListBox _shortCutListBox = null;
        private bool _isSeqComplete = false;
        private bool _isNewCombination = true;

        private bool _isReplacingShortcut = false;
        private bool _wasPreviouslyASequence = false;

        private Window _windowRef = null;

        private System.Timers.Timer seqTimer = null;
        private double seqTimerMaxMs = 1000;

        private ICommand _assigningCommand = null;
        private string _keys = string.Empty;

        private List<List<Key>> _keyList = new List<List<Key>>();
        #endregion

        #region Properties

        #region Controls
        //public Canvas KeyCanvas { get; set; }
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
                    int seqIdx = _keyList.IndexOf(kl);
                    foreach(var k in kl) {
                        if(kl.Count > 1 && kl.IndexOf(k) < kl.Count - 1) {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpHelpers.Instance.GetKeyLiteral(k),
                                            true,false,seqIdx));
                        } else if (kl.IndexOf(k) == kl.Count - 1 && _keyList.IndexOf(kl) < _keyList.Count -1) {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpHelpers.Instance.GetKeyLiteral(k),
                                            false, true, seqIdx));
                        } else {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpHelpers.Instance.GetKeyLiteral(k),
                                            false, false, seqIdx));
                        }

                    }
                }
                //if(KeyItems.Count > 0) {
                //    var ki = KeyItems[0];
                //    var view = (CollectionView)CollectionViewSource.GetDefaultView(_shortCutListBox);
                //    var groupDescription = new PropertyGroupDescription(nameof(ki.SeqIdx));
                //    view.GroupDescriptions.Add(groupDescription);
                //}
                
                return MpHelpers.Instance.ConvertKeySequenceToString(_keyList);
            }
        }        

        private string _warningString =string.Empty;
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

        public BitmapSource WarningBmp {
            get {
                if (_isReplacingShortcut) {
                    return (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath+@"/Images/warning.png"));
                }
                if (_wasPreviouslyASequence || _keyList.Count > 1) {
                    return (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/info.png"));
                }
                return new BitmapImage();
            }
        }

        public Visibility WarningVisibility {
            get {
                if (!string.IsNullOrEmpty(WarningString)) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Brush WarningBorderBrush {
            get {
                if (_isReplacingShortcut) {
                    return Brushes.IndianRed;
                }
                if(_wasPreviouslyASequence || _keyList.Count > 1) {
                    return Brushes.LightBlue;
                }
                return Brushes.Transparent;
            }
        }

        public Brush WarningTextBrush {
            get {
                if (_isReplacingShortcut) {
                    return Brushes.AntiqueWhite;
                }
                if (_wasPreviouslyASequence || _keyList.Count > 1) {
                    return Brushes.DimGray;
                }
                return Brushes.Transparent;
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
            _shortCutListBox = _windowRef.FindName("ShortcutListBox") as ListBox;

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

        private MpAssignShortcutModalWindowViewModel(string shortcutName, string keyString, ICommand command) : base(null) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(WarningString):
                        OnPropertyChanged(nameof(WarningBorderBrush));
                        OnPropertyChanged(nameof(WarningVisibility));
                        OnPropertyChanged(nameof(WarningBmp));
                        OnPropertyChanged(nameof(WarningTextBrush));
                        break;
                }
            };

            if(!string.IsNullOrEmpty(keyString) && keyString.Contains(@",")) {
                _wasPreviouslyASequence = true;
                WarningString = @"Sequence hot key's require a restart to be enabled";
            }
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
            _isReplacingShortcut = false;
            //iterate over ALL shortcuts
            foreach (var scvm in MpShortcutCollectionViewModel.Instance.Shortcuts) {                
                if (scvm.Command == _assigningCommand ||
                    scvm.KeyList.Count != _keyList.Count || 
                    scvm.KeyList.Count == 0) {
                    //ignore same, empty or shortcut w/ different key counts
                    continue;
                }

                _isReplacingShortcut = true;
                int klIdx = 0;
                foreach(var kl in scvm.KeyList) {
                    if(kl.Count != _keyList[klIdx].Count) {
                        _isReplacingShortcut = false;
                        break;
                    }
                    foreach(var k in kl) {
                        if(!_keyList[klIdx].Contains(k)) {
                            _isReplacingShortcut = false;
                        }
                    }
                    klIdx++;
                }
                if (_isReplacingShortcut && KeyString != string.Empty) {
                    DuplicatedShortcutViewModel = scvm;
                    WarningString = "This combination conflicts with '" + scvm.ShortcutDisplayName + "' which will be cleared if saved";
                    return false;
                }
            }
            if (_wasPreviouslyASequence || _keyList.Count > 1) {
                WarningString = @"Sequence hot key's require a restart to be enabled";
                return true;
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
            KeyItems.Clear();
            Validate();
            OnPropertyChanged(nameof(KeyString));
        }

        public ICommand OkCommand => new RelayCommand(
            async () => {
                if (DuplicatedShortcutViewModel != null) {
                    //if (DuplicatedShortcutViewModel.IsCustom()) {
                    //    if (DuplicatedShortcutViewModel.CopyItemId > 0) {
                    //        //clear input gesture text
                    //        MpClipTrayViewModel.Instance.GetContentItemViewModelById(DuplicatedShortcutViewModel.CopyItemId).ShortcutKeyString = string.Empty;
                    //    } else {
                    //        MpTagTrayViewModel.Instance.TagTileViewModels.Where(x => x.Tag.Id == DuplicatedShortcutViewModel.TagId).ToList()[0].ShortcutKeyString = string.Empty;
                    //    }
                    //}
                    DuplicatedShortcutViewModel.KeyString = string.Empty;
                    await DuplicatedShortcutViewModel.Shortcut.WriteToDatabaseAsync();
                    DuplicatedShortcutViewModel.Unregister();
                }
                _windowRef.DialogResult = true;
                _windowRef.Close();
            });
        #endregion
    }
}
