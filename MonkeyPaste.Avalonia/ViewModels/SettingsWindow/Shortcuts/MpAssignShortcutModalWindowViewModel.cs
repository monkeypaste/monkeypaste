
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SharpHook.Native;

namespace MonkeyPaste.Avalonia {

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

        public List<List<KeyCode>> KeyList { get; set; } = new List<List<KeyCode>>();

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
                                            MpAvKeyboardInputHelpers.GetKeyLiteral(k),
                                            true,false,seqIdx));
                        } else if (kl.IndexOf(k) == kl.Count - 1 && KeyList.IndexOf(kl) < KeyList.Count -1) {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpAvKeyboardInputHelpers.GetKeyLiteral(k),
                                            false, true, seqIdx));
                        } else {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpAvKeyboardInputHelpers.GetKeyLiteral(k),
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
                
                return MpAvKeyboardInputHelpers.ConvertKeySequenceToString(KeyList);
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

        public Bitmap WarningBmp {
            get {
                if (_isReplacingShortcut) {
                    return MpBase64Images.Warning.ToAvBitmap();//(Bitmap)new BitmapImage(new Uri(MpPrefViewModel.Instance.AbsoluteResourcesPath+@"/Images/warning.png"));
                }
                if (_wasPreviouslyASequence || KeyList.Count > 1) {
                    return MpBase64Images.QuestionMark.ToAvBitmap(); //(Bitmap)new BitmapImage(new Uri(MpPrefViewModel.Instance.AbsoluteResourcesPath + @"/Images/info.png"));
                }
                return null;// new BitmapImage();
            }
        }

        public bool HasWarning => !string.IsNullOrEmpty(WarningString);

        public string WarningBorderBrush {
            get {
                if (_isReplacingShortcut) {
                    return MpSystemColors.indianred;
                }
                if(_wasPreviouslyASequence || KeyList.Count > 1) {
                    return MpSystemColors.lightblue;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string WarningTextBrush {
            get {
                if (_isReplacingShortcut) {
                    return MpSystemColors.antiquewhite;
                }
                if (_wasPreviouslyASequence || KeyList.Count > 1) {
                    return MpSystemColors.dimgray;
                }
                return MpSystemColors.Transparent;
            }
        }
        #endregion

        #region Events
        
        public event EventHandler OnClear;

        #endregion

        #region Static Methods
        public static string ShowAssignShortcutWindow(string shortcutName,string keys,ICommand command, int commandId) {
            //MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
            //var ascw = new MpAssignHotkeyModalWindow();
            //var ascwvm = new MpAssignShortcutModalWindowViewModel(shortcutName,keys,command, commandId);
            //ascw.DataContext = ascwvm;
            //ascw.Loaded += ascwvm.Ascw_Loaded;
            //ascw.Unloaded += ascwvm.Ascw_Unloaded;
            //var assignResult = ascw.ShowDialog();
            //MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
            //ascw.KeyGestureBehavior.StopListening();
            //ascw.Loaded -= ascwvm.Ascw_Loaded;
            //ascw.Unloaded -= ascwvm.Ascw_Unloaded;
            //if (assignResult == true) {
            //    return ascwvm.KeyString;
            //} else {
            //    return null;
            //}
            return null;
        }

        //public void Ascw_Unloaded(object sender, RoutedEventArgs e) {
        //    (sender as MpAssignHotkeyModalWindow).KeyGestureBehavior.StopListening();
        //}

        //public void Ascw_Loaded(object sender, RoutedEventArgs e) {
        //    (sender as MpAssignHotkeyModalWindow).KeyGestureBehavior.StartListening(this);
        //}
        #endregion

        #region Constructors
        public MpAssignShortcutModalWindowViewModel() : this(string.Empty,string.Empty,null,0) { }

        private MpAssignShortcutModalWindowViewModel(string shortcutName, string keyString, ICommand command,int commandId) : base(null) {
            PropertyChanged += MpAssignShortcutModalWindowViewModel_PropertyChanged;

            if (!string.IsNullOrEmpty(keyString) && keyString.Contains(@",")) {
                _wasPreviouslyASequence = true;
                WarningString = @"Sequence hot key's require a restart to be enabled";
            }
            KeyList = MpAvKeyboardInputHelpers.ConvertStringToKeySequence(keyString);
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
        public void SetKeyList(List<List<KeyCode>> keylist) {
            KeyList = keylist;
            OnPropertyChanged(nameof(KeyString));
            Validate();
            OnPropertyChanged(nameof(KeyItems));
        }

        private void _windowRef_Closed(object sender, EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
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

        private MpCommand _clearCommand;
        public ICommand ClearCommand {
            get {
                if (_clearCommand == null) {
                    _clearCommand = new MpCommand(Clear);
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

        public ICommand OkCommand => new MpCommand(
            async () => {
                if (DuplicatedShortcutViewModel != null) {
                    //if (DuplicatedShortcutViewModel.IsCustom()) {
                    //    if (DuplicatedShortcutViewModel.CopyItemId > 0) {
                    //        //clear input gesture text
                    //        MpAvClipTrayViewModel.Instance.GetContentItemViewModelById(DuplicatedShortcutViewModel.CopyItemId).ShortcutKeyString = string.Empty;
                    //    } else {
                    //        MpAvTagTrayViewModel.Instance.Items.Where(x => x.Tag.Id == DuplicatedShortcutViewModel.TagId).ToList()[0].ShortcutKeyString = string.Empty;
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
