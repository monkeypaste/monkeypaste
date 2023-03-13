
using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAssignShortcutViewModel :
        MpViewModelBase,
        MpAvIKeyGestureViewModel {
        #region Static Variables
        #endregion

        #region Private Variables

        private MpKeyGestureHelper _gestureHelper;

        private ICommand _assigningCommand = null;
        private string _commandParameter;
        #endregion

        #region Properties

        //public List<List<Key>> KeyList { get; set; } = new List<List<Key>>();
        public ObservableCollection<MpAvShortcutKeyGroupViewModel> KeyGroups =>
            new ObservableCollection<MpAvShortcutKeyGroupViewModel>(KeyItems);

        public IEnumerable<MpAvShortcutKeyGroupViewModel> KeyItems {
            get {
                var keyItems = new List<MpAvShortcutKeyGroupViewModel>();
                if (KeyString == null) {
                    return keyItems;
                }
                var combos = KeyString.SplitNoEmpty(MpInputConstants.SEQUENCE_SEPARATOR);
                int maxComboIdx = combos.Length - 1;
                for (int comboIdx = 0; comboIdx < combos.Length; comboIdx++) {
                    string combo = combos[comboIdx];
                    var comboGroup = new MpAvShortcutKeyGroupViewModel();
                    var keys = combo.SplitNoEmpty(MpInputConstants.COMBO_SEPARATOR);

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
                if (KeyString != value) {
                    _keyString = value;
                    OnPropertyChanged(nameof(KeyString));
                    OnPropertyChanged(nameof(KeyItems));
                }
            }
        }

        public bool IsSequence =>
            KeyString != null && KeyString.Contains(MpInputConstants.SEQUENCE_SEPARATOR);


        public string WarningString { get; set; }
        public string WarningString2 { get; set; }


        #endregion

        #region Events

        public event EventHandler OnClear;

        #endregion

        #region Static Methods
        public static async Task<string> ShowAssignShortcutDialog(string shortcutName, string keys, ICommand command, string commandParameter) {
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;
            var ascw = new MpAvAssignShortcutDialog();
            var ascwvm = new MpAvAssignShortcutViewModel(shortcutName, keys, command, commandParameter);
            ascw.DataContext = ascwvm;

            MpMessenger.SendGlobal(MpMessageType.ShortcutAssignmentStarted);

            await ascw.ShowDialog(MpAvWindowManager.MainWindow);

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
        public MpAvAssignShortcutViewModel() : this(string.Empty, string.Empty, null, string.Empty) { }

        private MpAvAssignShortcutViewModel(string shortcutName, string keyString, ICommand command, string commandParameter) : base(null) {
            PropertyChanged += MpAssignShortcutModalWindowViewModel_PropertyChanged;

            _assigningCommand = command;
            _commandParameter = commandParameter;
            KeyString = keyString;
            ShortcutDisplayName = shortcutName;

            OnPropertyChanged(nameof(KeyString));
            OnPropertyChanged(nameof(KeyItems));


            _gestureHelper = new MpKeyGestureHelper();
            //KeysString = keysList;
        }

        private void MpAssignShortcutModalWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(KeyString):
                    OnPropertyChanged(nameof(KeyItems));
                    OnPropertyChanged(nameof(KeyGroups));
                    OnPropertyChanged(nameof(IsEmpty));
                    Validate();
                    break;
                case nameof(KeyItems):

                    OnPropertyChanged(nameof(KeyGroups));
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
            WarningString2 = string.Empty;
            DuplicatedShortcutViewModel = null;
            if (string.IsNullOrEmpty(KeyString)) {
                return true;
            }

            //iterate over ALL shortcuts
            string assign_keystr = KeyString.ToLower();
            foreach (var scvm in MpAvShortcutCollectionViewModel.Instance.Items) {
                if ((scvm.Command == _assigningCommand && scvm.CommandParameter == _commandParameter) ||
                    //scvm.KeyList.Count != KeyItems.Count() ||
                    scvm.KeyList.Count == 0) {
                    //ignore same, empty or shortcut w/ different key counts
                    continue;
                }
                string cur_keystr = scvm.KeyString.ToLower();

                if (cur_keystr == assign_keystr) {
                    DuplicatedShortcutViewModel = scvm;
                    WarningString = "This combination conflicts with '" + scvm.ShortcutDisplayName + "' which will be cleared if saved";
                    return false;
                } else if (IsSequence && assign_keystr.StartsWith(cur_keystr)) {
                    WarningString = "This sequence starts with '" + scvm.ShortcutDisplayName + "' which will still occur when executing this sequence";
                    // NOTE don't return here to continue checking for dups
                }
            }
            if (!string.IsNullOrEmpty(assign_keystr)) {
                bool has_mods = MpInputConstants.MOD_LITERALS.Any(x => assign_keystr.Contains(x.ToLower()));
                if (!has_mods) {
                    WarningString2 = "Warning! Confirm at your own risk, this shortcut has no modifier keys and may interfere with standard input.";
                }

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
