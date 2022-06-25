
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
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {

    public class MpAssignShortcutModalWindowViewModel : MpViewModelBase {
        #region Static Variables
        #endregion

        #region Private Variables

        private bool _isReplacingShortcut = false;
        private bool _wasPreviouslyASequence = false;


        private ICommand _assigningCommand = null;
        private int _commandId;
        #endregion

        #region Properties

        public List<List<Key>> KeyList { get; set; } = new List<List<Key>>();

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

        public bool IsEmpty => string.IsNullOrEmpty(KeyString);

        public string KeyString {
            get {
                KeyItems.Clear();
                foreach(var kl in KeyList) {
                    int seqIdx = KeyList.IndexOf(kl);
                    foreach(var k in kl) {
                        if(kl.Count > 1 && kl.IndexOf(k) < kl.Count - 1) {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpWpfKeyboardInputHelpers.GetKeyLiteral(k),
                                            true,false,seqIdx));
                        } else if (kl.IndexOf(k) == kl.Count - 1 && KeyList.IndexOf(kl) < KeyList.Count -1) {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpWpfKeyboardInputHelpers.GetKeyLiteral(k),
                                            false, true, seqIdx));
                        } else {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpWpfKeyboardInputHelpers.GetKeyLiteral(k),
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
                
                return MpWpfKeyboardInputHelpers.ConvertKeySequenceToString(KeyList);
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
                    return (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath+@"/Images/warning.png"));
                }
                if (_wasPreviouslyASequence || KeyList.Count > 1) {
                    return (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/info.png"));
                }
                return new BitmapImage();
            }
        }

        public bool HasWarning => !string.IsNullOrEmpty(WarningString);

        public Brush WarningBorderBrush {
            get {
                if (_isReplacingShortcut) {
                    return Brushes.IndianRed;
                }
                if(_wasPreviouslyASequence || KeyList.Count > 1) {
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
                if (_wasPreviouslyASequence || KeyList.Count > 1) {
                    return Brushes.DimGray;
                }
                return Brushes.Transparent;
            }
        }
        #endregion

        #region Events
        
        public event EventHandler OnClear;

        #endregion

        #region Static Methods
        public static string ShowAssignShortcutWindow(string shortcutName,string keys,ICommand command, int commandId) {
            MpMainWindowViewModel.Instance.IsShowingDialog = true;
            var ascw = new MpAssignHotkeyModalWindow();
            var ascwvm = new MpAssignShortcutModalWindowViewModel(shortcutName,keys,command, commandId);
            ascw.DataContext = ascwvm;
            ascw.Loaded += ascwvm.Ascw_Loaded;
            ascw.Unloaded += ascwvm.Ascw_Unloaded;
            var assignResult = ascw.ShowDialog();
            MpMainWindowViewModel.Instance.IsShowingDialog = false;
            ascw.KeyGestureBehavior.StopListening();
            ascw.Loaded -= ascwvm.Ascw_Loaded;
            ascw.Unloaded -= ascwvm.Ascw_Unloaded;
            if (assignResult == true) {
                return ascwvm.KeyString;
            } else {
                return null;
            }
        }

        public void Ascw_Unloaded(object sender, RoutedEventArgs e) {
            (sender as MpAssignHotkeyModalWindow).KeyGestureBehavior.StopListening();
        }

        public void Ascw_Loaded(object sender, RoutedEventArgs e) {
            (sender as MpAssignHotkeyModalWindow).KeyGestureBehavior.StartListening(this);
        }
        #endregion

        #region Constructors
        public MpAssignShortcutModalWindowViewModel() : this(string.Empty,string.Empty,null,0) { }

        private MpAssignShortcutModalWindowViewModel(string shortcutName, string keyString, ICommand command,int commandId) : base(null) {
            PropertyChanged += MpAssignShortcutModalWindowViewModel_PropertyChanged;

            if (!string.IsNullOrEmpty(keyString) && keyString.Contains(@",")) {
                _wasPreviouslyASequence = true;
                WarningString = @"Sequence hot key's require a restart to be enabled";
            }
            KeyList = MpWpfKeyboardInputHelpers.ConvertStringToKeySequence(keyString);
            _assigningCommand = command;
            _commandId = commandId;
            ShortcutDisplayName = shortcutName;

            OnPropertyChanged(nameof(KeyString));
            //KeysString = keysList;
        }

        private void MpAssignShortcutModalWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(WarningString):
                    OnPropertyChanged(nameof(WarningBorderBrush));
                    OnPropertyChanged(nameof(HasWarning));
                    OnPropertyChanged(nameof(WarningBmp));
                    OnPropertyChanged(nameof(WarningTextBrush));
                    break;

            }
        }

        #endregion

        #region Public Methods
        public void SetKeyList(List<List<Key>> keylist) {

            KeyList = keylist;
            OnPropertyChanged(nameof(KeyString));
            Validate();
            OnPropertyChanged(nameof(KeyItems));
        }

        private void _windowRef_Closed(object sender, EventArgs e) {
            MpMainWindowViewModel.Instance.IsShowingDialog = false;
        }

        #endregion

        #region Private Methods        

        private bool Validate() {
            //when KeysString changes check full system for duplicates, ignoring order of combinations
            WarningString = string.Empty;
            DuplicatedShortcutViewModel = null;
            _isReplacingShortcut = false;
            //iterate over ALL shortcuts
            foreach (var scvm in MpShortcutCollectionViewModel.Instance.Items) {                
                if (scvm.Command == _assigningCommand ||
                    scvm.KeyList.Count != KeyList.Count || 
                    scvm.KeyList.Count == 0) {
                    //ignore same, empty or shortcut w/ different key counts
                    continue;
                }

                _isReplacingShortcut = scvm.KeyString == KeyString;
                //int klIdx = 0;
                //foreach(var kl in scvm.KeyList) {
                //    if(kl.Count != KeyList[klIdx].Count) {
                //        _isReplacingShortcut = false;
                //        break;
                //    }
                //    foreach(var k in kl) {
                //        if(!KeyList[klIdx].Contains(k)) {
                //            _isReplacingShortcut = false;
                //        }
                //    }
                //    klIdx++;
                //}
                if (_isReplacingShortcut && KeyString != string.Empty && scvm.Command != _assigningCommand && scvm.CommandId != _commandId) {
                    DuplicatedShortcutViewModel = scvm;
                    WarningString = "This combination conflicts with '" + scvm.ShortcutDisplayName + "' which will be cleared if saved";
                    return false;
                }
            }
            if (_wasPreviouslyASequence || KeyList.Count > 1) {
                WarningString = @"Sequence hot key's require a restart to be enabled";
                return true;
            }
            return true;
        }
        
        #endregion

        #region Commands

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
            KeyList.Clear();
            KeyItems.Clear();
            Validate();
            OnPropertyChanged(nameof(KeyString));
            OnClear?.Invoke(this, null);
        }

        public ICommand OkCommand => new RelayCommand(
            async () => {
                if (DuplicatedShortcutViewModel != null) {
                    //if (DuplicatedShortcutViewModel.IsCustom()) {
                    //    if (DuplicatedShortcutViewModel.CopyItemId > 0) {
                    //        //clear input gesture text
                    //        MpClipTrayViewModel.Instance.GetContentItemViewModelById(DuplicatedShortcutViewModel.CopyItemId).ShortcutKeyString = string.Empty;
                    //    } else {
                    //        MpTagTrayViewModel.Instance.Items.Where(x => x.Tag.Id == DuplicatedShortcutViewModel.TagId).ToList()[0].ShortcutKeyString = string.Empty;
                    //    }
                    //}
                    DuplicatedShortcutViewModel.KeyString = string.Empty;
                    await DuplicatedShortcutViewModel.Shortcut.WriteToDatabaseAsync();
                    DuplicatedShortcutViewModel.Unregister();
                }
            });
        #endregion
    }
}
