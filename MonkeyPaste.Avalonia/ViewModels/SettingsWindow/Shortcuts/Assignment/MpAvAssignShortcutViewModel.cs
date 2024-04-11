
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpShortcutAssignmentType {
        None,
        InternalCommand,
        CanBeGlobalCommand,
        AppCopy,
        AppPaste
    }

    public class MpAvAssignShortcutViewModel :
        MpAvViewModelBase,
        MpIWantsTopmostWindowViewModel,
        MpIActiveWindowViewModel,
        MpICloseWindowViewModel,
        MpAvIKeyGestureViewModel {
        #region Static Variables
        #endregion

        #region Private Variables

        private MpAvKeyGestureHelper<KeyCode> _gestureHelper;

        private int _curShortcutId = 0;
        #endregion

        #region Statics
        public static async Task<Tuple<string, MpRoutingType>> ShowAssignShortcutDialog(
            string shortcutName,
            string keys,
            int curShortcutId,
            MpShortcutAssignmentType assignmentType = MpShortcutAssignmentType.InternalCommand,
            object iconResourceObj = null,
            Window owner = null) {
            var scavm = new MpAvAssignShortcutViewModel();
            await scavm.InitializeAsync(shortcutName, keys, curShortcutId, assignmentType, iconResourceObj);
            var ascw = new MpAvWindow() {
                DataContext = scavm,
                MinWidth = 400,
                Width = 400,
                SizeToContent = SizeToContent.Height,
                ShowInTaskbar = false,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("KeyboardImage", typeof(WindowIcon), null, null) as WindowIcon,
                Title = UiStrings.CommonAssignShortcutLabel.ToWindowTitleText(),
                Content = new MpAvAssignShortcutView()
            };
            ascw.Classes.Add("assignWindow");
            //ascw.Classes.Add("fadeIn");

            void OnActivate(object s, EventArgs e) {
                MpMessenger.SendGlobal(MpMessageType.ShortcutAssignmentActivated);
            };

            void OnDeactivate(object s, EventArgs e) {
                MpMessenger.SendGlobal(MpMessageType.ShortcutAssignmentDeactivated);
            }
            void OnClose(object s, EventArgs e) {
                ascw.Activated -= OnActivate;
                ascw.Deactivated -= OnDeactivate;
                ascw.Closed -= OnClose;
                MpMessenger.SendGlobal(MpMessageType.ShortcutAssignmentDeactivated);
            }
            ascw.Activated += OnActivate;
            ascw.Deactivated += OnDeactivate;
            ascw.Closed += OnClose;

            object result = await ascw.ShowDialogWithResultAsync(owner);

            if (result is bool assignResult && assignResult) {
                return new Tuple<string, MpRoutingType>(scavm.KeyString, scavm.SelectedRoutingType);
            }
            return null;
        }


        #endregion

        #region Interfaces

        bool MpIWantsTopmostWindowViewModel.WantsTopmost =>
            true;

        MpWindowType MpIWindowViewModel.WindowType =>
            MpWindowType.Modal;

        public bool IsWindowOpen { get; set; }

        public bool IsWindowActive { get; set; }

        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvShortcutKeyGroupViewModel> KeyGroups =>
            new ObservableCollection<MpAvShortcutKeyGroupViewModel>(KeyItems);

        public IEnumerable<MpAvShortcutKeyGroupViewModel> KeyItems =>
            KeyString.ToKeyItems();

        public MpAvShortcutViewModel DuplicatedShortcutViewModel { get; set; }

        private ObservableCollection<string> _routingTypes = null;
        public ObservableCollection<string> RoutingTypes {
            get {
                if (_routingTypes == null) {
                    _routingTypes = new ObservableCollection<string>(typeof(MpRoutingType).EnumToUiStrings());
                }
                return _routingTypes;
            }
        }
        #endregion

        #region State

        public bool IsGlobal =>
            SelectedRoutingType != MpRoutingType.Internal &&
            SelectedRoutingType != MpRoutingType.None &&
            CanBeGlobal;
        public bool CanBeGlobal =>
            AssignmentType == MpShortcutAssignmentType.CanBeGlobalCommand;
        public MpRoutingType SelectedRoutingType {
            get => (MpRoutingType)SelectedRoutingTypeIdx;
            set {
                if (SelectedRoutingType != value) {
                    SelectedRoutingTypeIdx = (int)value;
                    OnPropertyChanged(nameof(SelectedRoutingType));
                }
            }
        }
        private int _selectedRoutingTypeIdx;
        public int SelectedRoutingTypeIdx {
            get => _selectedRoutingTypeIdx;
            set {
                if (value < 1) {
                    // ignore none or ui init crap (-1 idx)
                    return;
                }
                if ((SelectedRoutingType == MpRoutingType.Internal && !CanBeGlobal)) {
                    // avoid hidden routing type combobox changing internal type
                    // when app cmd
                    return;
                }
                if (_selectedRoutingTypeIdx != value) {
                    _selectedRoutingTypeIdx = value;
                    OnPropertyChanged(nameof(SelectedRoutingTypeIdx));
                }
            }
        }
        public bool IsAppClipboardAssignment =>
            AssignmentType == MpShortcutAssignmentType.AppCopy ||
            AssignmentType == MpShortcutAssignmentType.AppPaste;
        public MpShortcutAssignmentType AssignmentType { get; set; }
        public bool IsEmpty =>
            string.IsNullOrEmpty(KeyString);
        public string WarningString { get; set; }
        public string WarningString2 { get; set; }

        public bool IsValid { get; set; } = false;

        public bool IsUserDefinedShortcut { get; set; }
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

        #region Constructors

        private MpAvAssignShortcutViewModel() : base(null) {
            PropertyChanged += MpAssignShortcutModalWindowViewModel_PropertyChanged;
        }
        #endregion

        #region Public Methods

        public async Task InitializeAsync(
            string shortcutName,
            string keyString,
            int curShortcutId,
            MpShortcutAssignmentType assignmentType,
            object iconResourceObj) {
            IsBusy = true;

            await Task.Delay(1);
            _curShortcutId = curShortcutId;
            _gestureHelper = new MpAvKeyGestureHelper<KeyCode>() { ResetAfterGesture = true };
            KeyString = keyString;
            ShortcutDisplayName = shortcutName;
            IconResourceObj = iconResourceObj;
            AssignmentType = assignmentType;

            IsUserDefinedShortcut = MpAvShortcutCollectionViewModel.Instance.Items.Any(x => x.ShortcutId == _curShortcutId && x.IsCustom);

            if (IsAppClipboardAssignment) {
                // no scvm
                SelectedRoutingType = MpRoutingType.None;
            } else if (MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == curShortcutId) is MpAvShortcutViewModel svm) {
                // existing scvm, use its routing
                SelectedRoutingType = svm.RoutingType;
            } else {
                // new sc, either passive (global) or internal
                SelectedRoutingType = CanBeGlobal ? MpRoutingType.Passive : MpRoutingType.Internal;
            }
            OnPropertyChanged(nameof(CanBeGlobal));
            OnPropertyChanged(nameof(IsGlobal));
            OnPropertyChanged(nameof(KeyString));
            OnPropertyChanged(nameof(KeyItems));
            IsValid = Validate();

            IsBusy = false;
        }
        #endregion

        #region Private Methods        

        private void MpAssignShortcutModalWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(KeyString):
                    OnPropertyChanged(nameof(KeyItems));
                    OnPropertyChanged(nameof(KeyGroups));
                    OnPropertyChanged(nameof(IsEmpty));
                    IsValid = Validate();
                    break;
                case nameof(SelectedRoutingTypeIdx):
                    OnPropertyChanged(nameof(IsGlobal));
                    break;
                case nameof(KeyItems):
                    OnPropertyChanged(nameof(KeyGroups));
                    break;
                case nameof(IsWindowOpen):
                    if (IsWindowOpen) {
                        StartKeyListener();
                    } else {
                        StopKeyListener();
                    }
                    break;

            }
        }
        private bool Validate() {
            //when KeysString changes check full system for duplicates
            WarningString = string.Empty;
            WarningString2 = string.Empty;
            DuplicatedShortcutViewModel = null;

            //if (_gestureHelper.Downs.Count > 0) {
            //    // only validate after gesture
            //    return true;
            //}

            switch (AssignmentType) {
                case MpShortcutAssignmentType.CanBeGlobalCommand:
                case MpShortcutAssignmentType.InternalCommand:
                    return ValidateCommandShortcut();
                case MpShortcutAssignmentType.AppCopy:
                case MpShortcutAssignmentType.AppPaste:
                    return ValidateAppClipboardShortcut();
            }
            return true;
        }

        private bool ValidateCommandShortcut() {
            if (string.IsNullOrEmpty(KeyString)) {
                return true;
            }

            //iterate over ALL shortcuts
            string assign_keystr = KeyString.ToLowerInvariant();
            DuplicatedShortcutViewModel =
                MpAvShortcutCollectionViewModel.Instance.Items
                .FirstOrDefault(x =>
                    x.ShortcutId != _curShortcutId &&
                    x.KeyList.Count > 0 &&
                    assign_keystr == x.KeyString.ToLowerInvariant());
            if (DuplicatedShortcutViewModel != null) {
                WarningString = string.Format(
                    UiStrings.ShortcutAssignDuplicateWarning,
                    DuplicatedShortcutViewModel.ShortcutDisplayName);
            }

            var gesture = Mp.Services.KeyConverter.ConvertStringToKeySequence<KeyCode>(assign_keystr).FirstOrDefault();
            if (!gesture.Any()) {
                return true;
            }

            int input_count = gesture.Where(x => !x.IsModKey()).Count();
            if (input_count != 1) {
                // multi-input key warning
                WarningString2 = UiStrings.ShortcutAssignInvalidGestureWarning;
            } else if (SelectedRoutingType == MpRoutingType.ExclusiveOverride &&
                        gesture.Count > 1) {
                // mulit key exclusive override warning
                WarningString2 = UiStrings.ShortcutAssignExclusiveOverrideMultiKeyWarning;
            }
            return string.IsNullOrEmpty(WarningString2);
        }

        private bool ValidateAppClipboardShortcut() {
            if (string.IsNullOrEmpty(KeyString)) {
                if (IsAppClipboardAssignment) {
                    // don't allow empty app clipboard shortcuts
                    WarningString2 = UiStrings.ShortcutAssignEmptyAppClipboardShortcutWarning;
                    return false;
                }
                return true;
            }

            return true;
        }

        private void StartKeyListener() {
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += Instance_OnGlobalKeyPressed;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += Instance_OnGlobalKeyReleased;
        }

        private void StopKeyListener() {
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed -= Instance_OnGlobalKeyPressed;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased -= Instance_OnGlobalKeyReleased;
        }
        private void Instance_OnGlobalKeyPressed(object sender, string keyStr) {
            Dispatcher.UIThread.Post(() => {
                if (!IsWindowActive) {
                    return;
                }

                _gestureHelper.AddKeyDown(Mp.Services.KeyConverter.ConvertStringToKeySequence<KeyCode>(keyStr).First().First());
                KeyString = _gestureHelper.GetCurrentGesture();
                OnPropertyChanged(nameof(KeyItems));
            });
        }
        private void Instance_OnGlobalKeyReleased(object sender, string keyStr) {
            Dispatcher.UIThread.Post(() => {
                if (!IsWindowActive) {
                    return;
                }

                _gestureHelper.RemoveKeyDown(Mp.Services.KeyConverter.ConvertStringToKeySequence<KeyCode>(keyStr).First().First());
                KeyString = _gestureHelper.GetCurrentGesture();
                OnPropertyChanged(nameof(KeyItems));
            });

        }

        #endregion

        #region Commands


        public ICommand OkCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (DuplicatedShortcutViewModel != null) {
                    DuplicatedShortcutViewModel.KeyString = string.Empty;
                    await DuplicatedShortcutViewModel.Shortcut.WriteToDatabaseAsync();
                }
                if (args is Control c &&
                    c.GetVisualRoot() is MpAvWindow w) {
                    w.DialogResult = true;
                    w.Close();
                }
            }, (args) => {
                return IsValid;
            });

        public ICommand CancelCommand => new MpCommand<object>(
            (args) => {
                if (args is Control c &&
                    c.GetVisualRoot() is MpAvWindow w) {
                    w.DialogResult = false;
                    w.Close();
                }
            });

        public ICommand ClearCommand => new MpCommand(
            () => {
                _gestureHelper.ClearCurrentGesture();
                KeyString = String.Empty;
                OnPropertyChanged(nameof(KeyItems));
                IsValid = Validate();
                OnPropertyChanged(nameof(KeyString));
            });
        #endregion
    }
}
