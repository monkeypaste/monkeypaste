
using Avalonia.Controls;
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
    public enum MpShortcutAssignmentType {
        InternalOrGlobalCommand,
        AppPaste
    }

    public class MpAvAssignShortcutViewModel :
        MpViewModelBase,
        MpIWantsTopmostWindowViewModel,
        MpAvIKeyGestureViewModel {
        #region Static Variables
        #endregion

        #region Private Variables

        private MpKeyGestureHelper _gestureHelper;

        private int _curShortcutId = 0;
        #endregion

        #region Interfaces

        bool MpIWantsTopmostWindowViewModel.WantsTopmost =>
            true;

        MpWindowType MpIWindowViewModel.WindowType =>
            MpWindowType.Modal;

        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvShortcutKeyGroupViewModel> KeyGroups =>
            new ObservableCollection<MpAvShortcutKeyGroupViewModel>(KeyItems);

        public IEnumerable<MpAvShortcutKeyGroupViewModel> KeyItems =>
            KeyString.ToKeyItems();

        public MpAvShortcutViewModel DuplicatedShortcutViewModel { get; set; }
        #endregion

        #region State

        public MpShortcutAssignmentType AssignmentType { get; set; }
        public bool IsEmpty => string.IsNullOrEmpty(KeyString);

        public bool IsSequence =>
            KeyString != null && KeyString.Contains(MpInputConstants.SEQUENCE_SEPARATOR);
        public string WarningString { get; set; }
        public string WarningString2 { get; set; }
        #endregion

        #region Appearance

        public string ShortcutDisplayName { get; set; }

        public object IconResourceObj { get; set; }
        #endregion

        #region Model

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

        #endregion

        #endregion

        #region Static Methods
        public static async Task<string> ShowAssignShortcutDialog(
            string shortcutName,
            string keys,
            int curShortcutId,
            MpShortcutAssignmentType assignmentType = MpShortcutAssignmentType.InternalOrGlobalCommand,
            object iconResourceObj = null,
            Window owner = null) {
            var ascw = new MpAvAssignShortcutDialog();
            var ascwvm = new MpAvAssignShortcutViewModel(shortcutName, keys, curShortcutId, assignmentType, iconResourceObj);
            ascw.DataContext = ascwvm;

            MpMessenger.SendGlobal(MpMessageType.ShortcutAssignmentStarted);

            var result = await ascw.ShowChildDialogWithResultAsync(owner);

            MpMessenger.SendGlobal(MpMessageType.ShortcutAssignmentEnded);

            if (owner is Window w) {
                w.Activate();
                w.Focus();
            }
            if (result is bool assignResult && assignResult) {
                return ascwvm.KeyString;
            }
            return null;
        }
        #endregion

        #region Constructors
        public MpAvAssignShortcutViewModel() : this(string.Empty, string.Empty, 0, MpShortcutAssignmentType.InternalOrGlobalCommand, null) { }

        private MpAvAssignShortcutViewModel(
            string shortcutName,
            string keyString,
            int curShortcutId,
            MpShortcutAssignmentType assignmentType,
            object iconResourceObj) : base(null) {
            PropertyChanged += MpAssignShortcutModalWindowViewModel_PropertyChanged;

            _curShortcutId = curShortcutId;
            KeyString = keyString;
            ShortcutDisplayName = shortcutName;
            IconResourceObj = iconResourceObj;
            AssignmentType = assignmentType;
            OnPropertyChanged(nameof(KeyString));
            OnPropertyChanged(nameof(KeyItems));


            _gestureHelper = new MpKeyGestureHelper();
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

            switch (AssignmentType) {
                case MpShortcutAssignmentType.InternalOrGlobalCommand:
                    return ValidateCommandShortcut();
                case MpShortcutAssignmentType.AppPaste:
                    return ValidateAppPasteShortcut();
            }
            return true;
        }

        private bool ValidateCommandShortcut() {
            //iterate over ALL shortcuts
            string assign_keystr = KeyString.ToLower();
            foreach (var scvm in MpAvShortcutCollectionViewModel.Instance.Items) {
                if (scvm.ShortcutId == _curShortcutId ||
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

        private bool ValidateAppPasteShortcut() {
            if (KeyString.ToLower() == Mp.Services.PlatformShorcuts.PasteKeys.ToLower()) {
                WarningString = $"This is the default paste shortcut for the '{Mp.Services.PlatformInfo.OsType}' platform. It does not need to be set.";
                return false;
            }
            return true;
        }

        #endregion

        #region Commands

        public ICommand ClearCommand => new MpCommand(
            () => {
                KeyString = String.Empty;
                OnPropertyChanged(nameof(KeyItems));
                Validate();
                OnPropertyChanged(nameof(KeyString));
            });

        public ICommand OkCommand => new MpCommand(
            async () => {
                if (DuplicatedShortcutViewModel != null) {
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
