
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private MpKeyGestureHelper2 _gestureHelper;

        private bool _isReplacingShortcut = false;
        private bool _wasPreviouslyASequence = false;


        private ICommand _assigningCommand = null;
        private string _commandParameter;
        #endregion

        #region Properties

        //public List<List<Key>> KeyList { get; set; } = new List<List<Key>>();

        public IEnumerable<MpAvShortcutKeyGroupViewModel> KeyItems {
            get {
                var keyItems = new List<MpAvShortcutKeyGroupViewModel>();
                var combos = KeyString.Split(new String[] { MpKeyGestureHelper2.SEQUENCE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                int maxComboIdx = combos.Length - 1;
                for (int comboIdx = 0; comboIdx < combos.Length; comboIdx++) {
                    string combo = combos[comboIdx];
                    var comboGroup = new MpAvShortcutKeyGroupViewModel();
                    var keys = combo.Split(new String[] { MpKeyGestureHelper2.COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                    
                    for (int keyIdx = 0; keyIdx < keys.Length; keyIdx++) {
                        string key = keys[keyIdx];
                        var skvm = new MpAvShortcutKeyViewModel() {
                            KeyStr = key
                        };
                        comboGroup.Items.Add(skvm);
                    }
                    comboGroup.IsPlusVisible = maxComboIdx > 0 && comboIdx < maxComboIdx;
                    keyItems.Add(comboGroup);
                }
                return keyItems;
            }
        }

        public MpAvShortcutViewModel DuplicatedShortcutViewModel { get; set; }

        public string ShortcutDisplayName { get; set; }

        public bool IsEmpty => string.IsNullOrEmpty(KeyString);

        private string _keyString = string.Empty;
        public string KeyString {
            get {
                return _keyString;
            }
            set {
                if(KeyString != value) {
                    _keyString = value;
                    OnPropertyChanged(nameof(KeyString));
                    OnPropertyChanged(nameof(KeyItems));
                }
            }
        }

        public bool IsSequence => KeyString.Contains(MpKeyGestureHelper2.SEQUENCE_SEPARATOR);

        public bool ShowWarning => IsSequence || _wasPreviouslyASequence || _isReplacingShortcut;
        public string WarningString { get; set; }

        public Bitmap WarningBmp {
            get {
                if (_isReplacingShortcut) {
                    return MpBase64Images.Warning.ToAvBitmap();//(Bitmap)new BitmapImage(new Uri(MpPrefViewModel.Instance.AbsoluteResourcesPath+@"/Images/warning.png"));
                }
                if (_wasPreviouslyASequence || IsSequence) {
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
                if(_wasPreviouslyASequence || IsSequence) {
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
                if (_wasPreviouslyASequence || IsSequence) {
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
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;
            var ascw = new MpAvAssignShortcutWindow();
            var ascwvm = new MpAvAssignShortcutModalWindowViewModel(shortcutName, keys, command, commandParameter);
            ascw.DataContext = ascwvm;

            MpMessenger.SendGlobal(MpMessageType.ShortcutAssignmentStarted);

            await ascw.ShowDialog(MpAvMainWindow.Instance);

            MpMessenger.SendGlobal(MpMessageType.ShortcutAssignmentEnded);

            var assignResult = ascw.DialogResult;
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
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

            _assigningCommand = command;
            _commandParameter = commandParameter;
            ShortcutDisplayName = shortcutName;

            OnPropertyChanged(nameof(KeyString));
            OnPropertyChanged(nameof(KeyItems));


            _gestureHelper = new MpKeyGestureHelper2();
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

        private void _windowRef_Closed(object sender, EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
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
                    scvm.KeyList.Count != KeyItems.Count() || 
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
            if (_wasPreviouslyASequence || IsSequence) {
                WarningString = @"Sequence hot key's require a restart to be enabled";
                return true;
            }
            return true;
        }
        
        #endregion

        #region Commands

        public ICommand ClearCommand => new MpCommand(
            () => {
                KeyString = String.Empty;
                OnPropertyChanged(nameof(KeyItems));
                //_gestureHelper.Reset();
                Validate();
                OnPropertyChanged(nameof(KeyString));
                OnClear?.Invoke(this, null);
            });

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

        public ICommand AddKeyDownCommand => new MpCommand<string>(
            (args) => {
                _gestureHelper.AddKeyDown(args as string);
                KeyString = _gestureHelper.GetCurrentGesture();
                OnPropertyChanged(nameof(KeyItems));
            });

        public ICommand RemoveKeyDownCommand => new MpCommand<string>(
            (args) => {
                _gestureHelper.RemoveKeyDown(args as string);
                KeyString = _gestureHelper.GetCurrentGesture();
                OnPropertyChanged(nameof(KeyItems));
            });
        #endregion
    }
}
