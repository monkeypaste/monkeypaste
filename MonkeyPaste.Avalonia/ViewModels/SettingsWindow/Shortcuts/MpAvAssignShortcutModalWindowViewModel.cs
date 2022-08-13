
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SharpHook.Native;

namespace MonkeyPaste.Avalonia {
    public class MpAvAssignShortcutModalWindowViewModel : MpViewModelBase {
        #region Static Variables
        #endregion

        #region Private Variables

        private bool _isReplacingShortcut = false;
        private bool _wasPreviouslyASequence = false;


        private ICommand _assigningCommand = null;
        private string _commandParameter;
        #endregion

        #region Properties

        public List<List<Key>> KeyList { get; set; } = new List<List<Key>>();

        public ObservableCollection<MpShortcutKeyViewModel> KeyItems { get; set; } = new ObservableCollection<MpShortcutKeyViewModel>();

        public MpAvShortcutViewModel DuplicatedShortcutViewModel { get; set; }

        public string ShortcutDisplayName { get; set; }

        public bool IsEmpty => string.IsNullOrEmpty(KeyString);

        public string KeyString {
            get {
                KeyItems.Clear();
                foreach(var kl in KeyList) {
                    int seqIdx = KeyList.IndexOf(kl);
                    foreach(var k in kl) {
                        string kLiteral = MpAvKeyboardInputHelpers.GetKeyLiteral(k);
                        if (kl.Count > 1 && kl.IndexOf(k) < kl.Count - 1) {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            kLiteral,
                                            true,false,seqIdx));
                        } else if (kl.IndexOf(k) == kl.Count - 1 && KeyList.IndexOf(kl) < KeyList.Count -1) {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            kLiteral,
                                            false, true, seqIdx));
                        } else {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            kLiteral,
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

        public string WarningString { get; set; }

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

        public string WarningBorderHexColor {
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

        public string WarningTextHexColor {
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
        public static async Task<string> ShowAssignShortcutWindow(string shortcutName,string keys,ICommand command, string commandParameter) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
            var ascw = new MpAvAssignShortcutWindow();
            var ascwvm = new MpAvAssignShortcutModalWindowViewModel(shortcutName, keys, command, commandParameter);
            ascw.DataContext = ascwvm;

            await ascw.ShowDialog(MpAvMainWindow.Instance);
            
            var assignResult = ascw.DialogResult;
            MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
            if (assignResult == true) {
                return ascwvm.KeyString;
            }
            return null;
        }
        #endregion

        #region Constructors
        public MpAvAssignShortcutModalWindowViewModel() : this(string.Empty,string.Empty,null,string.Empty) { }

        private MpAvAssignShortcutModalWindowViewModel(string shortcutName, string keyString, ICommand command,string commandParameter) : base(null) {
            PropertyChanged += MpAssignShortcutModalWindowViewModel_PropertyChanged;

            if (!string.IsNullOrEmpty(keyString) && keyString.Contains(@",")) {
                _wasPreviouslyASequence = true;
                WarningString = @"Sequence hot key's require a restart to be enabled";
            }
            KeyList = MpAvKeyboardInputHelpers.ConvertStringToKeySequence(keyString);

            _assigningCommand = command;
            _commandParameter = commandParameter;
            ShortcutDisplayName = shortcutName;

            OnPropertyChanged(nameof(KeyString));
            //KeysString = keysList;
        }

        private void MpAssignShortcutModalWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(WarningString):
                    OnPropertyChanged(nameof(WarningBorderHexColor));
                    OnPropertyChanged(nameof(HasWarning));
                    OnPropertyChanged(nameof(WarningBmp));
                    OnPropertyChanged(nameof(WarningTextHexColor));
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
            foreach (var scvm in MpAvShortcutCollectionViewModel.Instance.Items) {                
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
                if (_isReplacingShortcut && KeyString != string.Empty && scvm.Command != _assigningCommand && scvm.CommandParameter != _commandParameter) {
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
