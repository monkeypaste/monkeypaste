
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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
        AppPaste
    }

    public class MpAvAssignShortcutViewModel :
        MpViewModelBase,
        MpIWantsTopmostWindowViewModel,
        MpIActiveWindowViewModel,
        MpIChildWindowViewModel,
        MpAvIKeyGestureViewModel {
        #region Static Variables
        #endregion

        #region Private Variables

        private MpKeyGestureHelper _gestureHelper;

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
                MinHeight = 300,
                MinWidth = 400,
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowInTaskbar = false,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("KeyboardImage", typeof(WindowIcon), null, null) as WindowIcon,
                Title = "Assign Shortcut".ToWindowTitleText(),
                Content = new MpAvAssignShortcutView()
            };
            ascw.Classes.Add("assignWindow");
            ascw.Classes.Add("fadeIn");
            MpMessenger.SendGlobal(MpMessageType.ShortcutAssignmentStarted);

            var result = await ascw.ShowChildDialogWithResultAsync(owner);

            MpMessenger.SendGlobal(MpMessageType.ShortcutAssignmentEnded);

            if (owner is Window w) {
                w.Activate();
                w.Focus();
            }
            if (result is bool assignResult && assignResult && ascw.DataContext is MpAvAssignShortcutViewModel ascwvm) {
                MpRoutingType routing_type = MpRoutingType.None;
                if (ascwvm.AssignmentType != MpShortcutAssignmentType.AppPaste) {
                    routing_type = ascwvm.RoutingTypes[ascwvm.SelectedRoutingTypeIdx].ToEnum<MpRoutingType>();
                }
                return new Tuple<string, MpRoutingType>(ascwvm.KeyString, routing_type);
            }
            return null;
        }


        #endregion

        #region Interfaces

        bool MpIWantsTopmostWindowViewModel.WantsTopmost =>
            true;

        MpWindowType MpIWindowViewModel.WindowType =>
            MpWindowType.Modal;

        public bool IsChildWindowOpen { get; set; }

        public bool IsActive { get; set; }

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
                    _routingTypes = new ObservableCollection<string>(
                        typeof(MpRoutingType).EnumToLabels(hideFirst: true));
                }
                return _routingTypes;
            }
        }
        #endregion

        #region State

        public bool IsGlobal =>
            RoutingTypes[SelectedRoutingTypeIdx] != MpRoutingType.Internal.ToString() &&
            RoutingTypes[SelectedRoutingTypeIdx] != MpRoutingType.None.ToString() &&
            CanBeGlobal;
        public bool CanBeGlobal { get; set; }

        public int SelectedRoutingTypeIdx { get; set; } = 0;
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
        public string ClearButtonLabel =>
            IsUserDefinedShortcut ?
                "Delete" :
                _curShortcutId > 0 ?
                    "Clear" :
                    string.Empty;
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
            _gestureHelper = new MpKeyGestureHelper();
            KeyString = keyString;
            ShortcutDisplayName = shortcutName;
            IconResourceObj = iconResourceObj;
            AssignmentType = assignmentType;

            IsUserDefinedShortcut = MpAvShortcutCollectionViewModel.Instance.Items.Any(x => x.ShortcutId == _curShortcutId && x.IsCustom);

            if (AssignmentType == MpShortcutAssignmentType.CanBeGlobalCommand) {
                CanBeGlobal = true;
                if (MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == curShortcutId) is MpAvShortcutViewModel svm) {
                    SelectedRoutingTypeIdx = svm.SelectedRoutingTypeIdx;
                } else {
                    // default globals to passive routing
                    SelectedRoutingTypeIdx = RoutingTypes.IndexOf(MpRoutingType.Passive.ToString());
                }

            } else {
                CanBeGlobal = false;
                SelectedRoutingTypeIdx = 0;
            }
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
                case nameof(IsChildWindowOpen):
                    if (IsChildWindowOpen) {
                        StartKeyListener();
                    } else {
                        StopKeyListener();
                    }
                    break;

            }
        }
        private bool Validate() {
            //when KeysString changes check full system for duplicates, ignoring order of combinations
            WarningString = string.Empty;
            WarningString2 = string.Empty;
            DuplicatedShortcutViewModel = null;
            if (string.IsNullOrEmpty(KeyString)) {
                return true;
            }

            switch (AssignmentType) {
                case MpShortcutAssignmentType.CanBeGlobalCommand:
                case MpShortcutAssignmentType.InternalCommand:
                    return ValidateCommandShortcut();
                case MpShortcutAssignmentType.AppPaste:
                    return ValidateAppPasteShortcut();
            }
            return true;
        }

        private bool ValidateCommandShortcut() {
            //iterate over ALL shortcuts
            string assign_keystr = KeyString.ToLower();
            DuplicatedShortcutViewModel =
                MpAvShortcutCollectionViewModel.Instance.Items
                .FirstOrDefault(x =>
                    x.ShortcutId != _curShortcutId && x.KeyList.Count > 0 && assign_keystr == x.KeyString);
            if (DuplicatedShortcutViewModel != null) {
                WarningString = $"This combination conflicts with '{DuplicatedShortcutViewModel.ShortcutDisplayName}' which will be cleared if saved";
            }

            var gesture = Mp.Services.KeyConverter.ConvertStringToKeySequence<KeyCode>(assign_keystr).FirstOrDefault();
            if (!gesture.Any()) {
                return true;
            }

            int input_count = gesture.Where(x => !x.IsModKey()).Count();
            if (input_count != 1) {
                WarningString2 = $"Shortcut must contain ONE and only ONE input key. (Any key that is not {string.Join(",", MpInputConstants.MOD_LITERALS)})";
            }
            return string.IsNullOrEmpty(WarningString2);
        }

        private bool ValidateAppPasteShortcut() {
            if (KeyString.ToLower() == Mp.Services.PlatformShorcuts.PasteKeys.ToLower()) {
                WarningString = $"This is the default paste shortcut for the '{Mp.Services.PlatformInfo.OsType}' platform. It does not need to be set.";
                return false;
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
                if (!IsActive) {
                    return;
                }
                AddKeyDownCommand.Execute(keyStr);
            });
        }
        private void Instance_OnGlobalKeyReleased(object sender, string keyStr) {
            Dispatcher.UIThread.Post(() => {
                if (!IsActive) {
                    return;
                }
                RemoveKeyDownCommand.Execute(keyStr);
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

        public ICommand CancelCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (DuplicatedShortcutViewModel != null) {
                    DuplicatedShortcutViewModel.KeyString = string.Empty;
                    await DuplicatedShortcutViewModel.Shortcut.WriteToDatabaseAsync();
                }
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
