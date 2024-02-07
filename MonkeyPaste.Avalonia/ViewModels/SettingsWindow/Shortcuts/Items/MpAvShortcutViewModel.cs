using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Key = Avalonia.Input.Key;

namespace MonkeyPaste.Avalonia {

    public class MpAvShortcutViewModel : MpAvViewModelBase<MpAvShortcutCollectionViewModel>,
        MpIActionComponent,
        MpIFilterMatch,
        MpIPopupMenuPicker,
        MpIShortcutCommandViewModel,
        MpAvIKeyGestureViewModel,
        MpISelectableViewModel {

        #region Interfaces

        #region MpIPopupMenuPicker Implementation

        MpAvMenuItemViewModel MpIPopupMenuPicker.GetMenu(ICommand cmd, object cmdArg, IEnumerable<int> selectedShortcutIds, bool recursive) {
            // used for component parameter picker
            return new MpAvMenuItemViewModel() {
                Header = ShortcutDisplayName,
                Command = cmd,
                CommandParameter = ShortcutId,
                IsChecked = selectedShortcutIds.Contains(ShortcutId),
            };
        }
        #endregion

        #region MpIActionComponent Implementation
        List<MpIInvokableAction> _registeredActions = new List<MpIInvokableAction>();
        void MpIActionComponent.RegisterActionComponent(MpIInvokableAction mvm) {
            if (OnShortcutExecuted.HasInvoker(mvm)) {
                return;
            }
            OnShortcutExecuted += mvm.OnActionInvoked;
            MpConsole.WriteLine($"{nameof(OnShortcutExecuted)} Registered {mvm.Label}");
        }

        void MpIActionComponent.UnregisterActionComponent(MpIInvokableAction mvm) {
            if (!OnShortcutExecuted.HasInvoker(mvm)) {
                return;
            }
            OnShortcutExecuted -= mvm.OnActionInvoked;
            MpConsole.WriteLine($"{nameof(OnShortcutExecuted)} Unregistered {mvm.Label}");
        }
        #endregion

        #region MpIFilterMatch Implementation
        public bool IsFilterMatch(string filter) {
            if (string.IsNullOrEmpty(filter)) {
                return true;
            }
            return
                ShortcutDisplayName.ToLower().Contains(filter.ToLower()) ||
                RoutingTypeDisplayValue.ToString().ToLower().Contains(filter.ToLower());
        }

        #endregion

        #region MpIShortcutCommandViewModel Implementation

        // NOTE since shortcut represents application commands and has a unique shortcut type
        // its parameter is always null
        public object ShortcutCommandParameter =>
            CommandParameter;


        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #endregion

        #region Properties        

        #region View Models

        public ObservableCollection<MpAvShortcutKeyGroupViewModel> KeyGroups =>
            new ObservableCollection<MpAvShortcutKeyGroupViewModel>(KeyItems);
        public IEnumerable<MpAvShortcutKeyGroupViewModel> KeyItems =>
            KeyString.ToKeyItems();

        private ObservableCollection<string> _routingTypes = null;
        public ObservableCollection<string> RoutingTypes {
            get {
                if (_routingTypes == null) {
                    _routingTypes = new ObservableCollection<string>(
                        typeof(MpRoutingType).EnumToUiStrings());
                }
                return _routingTypes;
            }
        }

        #endregion

        #region State
        public bool IsEditorShortcut =>
            ShortcutType.IsEditorShortcut();

        public bool IsCustom {
            get {
                if (Shortcut == null) {
                    return false;
                }
                return (int)ShortcutType >= MpShortcut.MIN_USER_SHORTCUT_TYPE;
            }
        }
        public ICommand ShortcutCommand { get; set; }

        public bool CanBeGlobalShortcut =>
            ShortcutType.CanBeGlobal();

        public bool IsGlobal =>
            RoutingType != MpRoutingType.Internal &&
            RoutingType != MpRoutingType.None;

        public bool SuppressesKeys => //false;
            IsGlobal &&
            RoutingType != MpRoutingType.Passive;

        public bool CanDelete =>
            IsCustom;


        public bool CanReset =>
            !IsCustom && (KeyString != DefaultKeyString || RoutingType != DefaultRoutingType);

        public bool CanDeleteOrReset =>
            CanDelete || CanReset;

        public bool IsNew =>
            ShortcutId == 0;

        public string ShortcutDisplayName { get; private set; }

        public string ShortcutTypeName {
            get {
                if (IsCustom) {
                    switch (ShortcutType) {
                        case MpShortcutType.PasteCopyItem:
                            return UiStrings.ShortcutPasteCopyItemName;
                        case MpShortcutType.SelectTag:
                            return UiStrings.ShortcutSelectTagName;
                        case MpShortcutType.AnalyzeCopyItemWithPreset:
                            return UiStrings.ShortcutRunAnalyzerName;
                        case MpShortcutType.InvokeTrigger:
                            return UiStrings.ShortcutInvokeTriggerName;

                    }
                }
                return UiStrings.ShortcutInternalName;
            }
        }
        public string SelectedRoutingTypeStr => RoutingType.ToString();

        public int SelectedRoutingTypeIdx {
            get {
                if (Shortcut == null) {
                    return 0;
                }
                return (int)RoutingType;// RoutingTypes.IndexOf(RoutingType.ToString());
            }
            set {
                if (value < 0) {
                    // BUG when DataGrid virtualizes it pre-sets idx to -1 which is impossible
                    int test = RoutingTypes.IndexOf(RoutingType.ToString());
                    MpConsole.WriteLine($"Ignoring negative combo box idx!! Set Value: {value} ItemsIdx: {test} RoutingType: '{RoutingType.ToString()}'");
                    return;
                }
                if (!CanBeGlobalShortcut) {
                    return;
                }
                //if (Mp.Services.FocusMonitor.FocusElement is Control c) {
                //    bool reject = true;
                //    if (c.DataContext is MpAvAssignShortcutViewModel) {
                //        reject = false;
                //    } else if (c is DataGrid dg && dg.SelectedItem is MpAvShortcutViewModel svm && svm.ShortcutId == ShortcutId) {
                //        reject = false;
                //    }
                //    if (reject) {
                //        return;
                //    }
                if (SelectedRoutingTypeIdx != value) {
                    value = Math.Max(0, value);
                    //RoutingType = RoutingTypes[paramValue].ToString().ToEnum<MpRoutingType>();
                    RoutingType = (MpRoutingType)value;//RoutingTypes[paramValue].ToString().ToEnum<MpRoutingType>();
                    OnPropertyChanged(nameof(SelectedRoutingTypeIdx));
                }
                //}

            }
        }

        public IReadOnlyList<IReadOnlyList<Key>> KeyList { get; private set; }

        public IReadOnlyList<IReadOnlyList<KeyCode>> GlobalKeyList { get; private set; }
        public bool IsEmpty => !KeyItems.Any();

        #endregion

        #region Appearance

        public string RoutingTypeDisplayValue {
            get {
                if (CanBeGlobalShortcut && (RoutingType == MpRoutingType.Internal || RoutingType == MpRoutingType.None)) {
                    return UiStrings.CommonDisabledLabel;
                }
                return RoutingType.EnumToUiString();
            }
        }
        #endregion

        #region Model

        public MpShortcutType ShortcutType {
            get {
                if (Shortcut == null) {
                    return MpShortcutType.None;
                }
                return Shortcut.ShortcutType;
            }
            set {
                if (ShortcutType != value) {
                    Shortcut.ShortcutType = value;
                    OnPropertyChanged(nameof(ShortcutType));
                }
            }
        }
        (string, MpRoutingType) _defaultInfo;
        (string, MpRoutingType) DefaultInfo {
            get {
                if (IsCustom) {
                    return default;
                }
                if (_defaultInfo.IsDefault()) {
                    var def_ref = MpAvDefaultDataCreator.DefaultShortcutDefinitions.FirstOrDefault(x => x[2] == ShortcutType.ToString());
                    if (def_ref != null) {
                        _defaultInfo.Item1 = def_ref[1];
                        _defaultInfo.Item2 = def_ref[3].ToEnum<MpRoutingType>();
                    } else {
                        MpDebug.Break($"Shortcut error, cannot find default def for '{ShortcutType}'");
                    }
                }
                return _defaultInfo;
            }
        }
        public MpRoutingType DefaultRoutingType =>
            DefaultInfo.Item2;
        public string DefaultKeyString =>
            DefaultInfo.Item1;

        public string CommandParameter {
            get {
                if (Shortcut == null) {
                    return null;
                }
                //if(IsCustom) {
                //    return Shortcut.CommandId;
                //}
                //return ShortcutId;
                return Shortcut.CommandParameter;
            }
            set {
                if (CommandParameter != value) {
                    if (!IsCustom) {
                        throw new Exception("Application shortcuts use pk not command id");
                    }
                    Shortcut.CommandParameter = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CommandParameter));
                }
            }
        }

        public int ShortcutId {
            get {
                if (Shortcut == null) {
                    return 0;
                }
                return Shortcut.Id;
            }
            set {
                if (ShortcutId != value) {
                    Shortcut.Id = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ShortcutId));
                }
            }
        }


        public string KeyString {
            get {
                if (Shortcut == null) {
                    return string.Empty;
                }
                return Shortcut.KeyString;
            }
            set {
                if (KeyString != value) {
                    Shortcut.KeyString = value;
                    // flag keylist to reset
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(KeyString));
                }
            }
        }


        public MpRoutingType RoutingType {
            get {
                if (Shortcut == null) {
                    return MpRoutingType.None;
                }
                return Shortcut.RoutingType;
            }
            set {
                if (RoutingType != value) {
                    Shortcut.RoutingType = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(RoutingType));
                    OnPropertyChanged(nameof(SelectedRoutingTypeIdx));
                }
            }
        }

        public int RoutingDelayMs {
            // NOTE only applies to non-direct routing types
            get {
                if (Shortcut == null) {
                    return 0;
                }
                return Shortcut.RoutingDelayMs;
            }
            set {
                if (RoutingDelayMs != value) {
                    Shortcut.RoutingDelayMs = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(RoutingDelayMs));
                }
            }
        }

        //private MpShortcut _shortcut = null;
        //public MpShortcut Shortcut {
        //    get {
        //        return _shortcut;
        //    }
        //    set {
        //        if (_shortcut != paramValue) {
        //            _shortcut = paramValue;
        //            OnPropertyChanged(nameof(Shortcut));
        //            OnPropertyChanged(nameof(RoutingType));
        //            OnPropertyChanged(nameof(KeyString));
        //            OnPropertyChanged(nameof(KeyList));
        //            OnPropertyChanged(nameof(ShortcutDisplayName));
        //            OnPropertyChanged(nameof(ShortcutId));
        //            OnPropertyChanged(nameof(CommandParameter));
        //            OnPropertyChanged(nameof(ShortcutType));
        //            OnPropertyChanged(nameof(DefaultKeyString));
        //            OnPropertyChanged(nameof(SelectedRoutingTypeIdx));
        //        }
        //    }
        //}
        public MpShortcut Shortcut { get; set; } = new MpShortcut();
        #endregion

        #endregion

        #region Events

        public event EventHandler<MpCopyItem> OnShortcutExecuted;

        #endregion

        #region Public Methods
        public MpAvShortcutViewModel(MpAvShortcutCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpShortcutViewModel_PropertyChanged;
        }


        public async Task InitializeAsync(MpShortcut s, ICommand command) {
            //only register if non-empty keysstring
            await Task.Delay(1);

            Shortcut = s;
            ShortcutCommand = command;

            await SetShortcutNameAsync();

            OnPropertyChanged(nameof(CanBeGlobalShortcut));
            OnPropertyChanged(nameof(KeyItems));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(KeyString));
            OnPropertyChanged(nameof(SelectedRoutingTypeIdx));
            OnPropertyChanged(nameof(CanDeleteOrReset));
        }


        public void ClearShortcutKeyString() {
            KeyString = string.Empty;
        }

        public override string ToString() {
            return $"{ShortcutType} - '{KeyString}'";
        }


        public async Task SetShortcutNameAsync() {
            Dispatcher.UIThread.CheckAccess();
            ShortcutDisplayName = await ShortcutType.GetShortcutTitleAsync(CommandParameter);
        }

        public bool IsMatch(string keystr) {
            if (RoutingType != MpRoutingType.ExclusiveOverride) {
                return keystr == KeyString;
            }
            return keystr.SplitNoEmpty(MpInputConstants.COMBO_SEPARATOR).Any(x => x == KeyString);
        }

        #endregion

        #region Protected Methods

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (IsModelAffectThisShortcut(e)) {
                Dispatcher.UIThread.Post(() => {
                    SetShortcutNameAsync().FireAndForgetSafeAsync(this);
                }, DispatcherPriority.Background);
            }
        }

        #endregion

        #region Private Methods

        private void MpShortcutViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        Task.Run(async () => {
                            IsBusy = true;
                            await Shortcut.WriteToDatabaseAsync();
                            HasModelChanged = false;
                            IsBusy = false;
                        });
                    }
                    break;
                case nameof(KeyItems):

                    OnPropertyChanged(nameof(KeyGroups));
                    break;
                case nameof(KeyString):
                    //if (IsCustom) {
                    //    if (Shortcut.CopyItemId > 0) {
                    //        var ctvm = MpAvClipTrayViewModel.Instance.GetContentItemViewModelById(Shortcut.CopyItemId);
                    //        ctvm.ShortcutKeyString = Shortcut.KeyString;
                    //    } else {
                    //        var ttvm = MpAvTagTrayViewModel.Instance.Items.Where(x => x.Tag.Id == Shortcut.TagId).Single();
                    //        ttvm.ShortcutKeyString = Shortcut.KeyString;
                    //    }
                    //}

                    KeyList = Mp.Services.KeyConverter.ConvertStringToKeySequence<Key>(KeyString);
                    GlobalKeyList = Mp.Services.KeyConverter.ConvertStringToKeySequence<KeyCode>(KeyString);
                    OnPropertyChanged(nameof(KeyItems));
                    OnPropertyChanged(nameof(KeyGroups));
                    OnPropertyChanged(nameof(IsEmpty));
                    OnPropertyChanged(nameof(CanReset));
                    break;
                case nameof(IsBusy):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    break;
                case nameof(ShortcutType):
                    OnPropertyChanged(nameof(CanBeGlobalShortcut));
                    break;
                case nameof(RoutingType):
                    OnPropertyChanged(nameof(RoutingTypeDisplayValue));
                    break;
            }
        }


        private bool IsModelAffectThisShortcut(MpDbModelBase model) {
            if (model.GetType() == GetShortcutCommandType()) {
                if (IsCustom) {
                    return model.Id.ToString() == CommandParameter;
                }
                return true;
            }
            return false;
        }

        private Type GetShortcutCommandType() {
            switch (ShortcutType) {
                case MpShortcutType.PasteCopyItem:
                    return typeof(MpCopyItem);
                case MpShortcutType.SelectTag:
                    return typeof(MpTag);
                case MpShortcutType.AnalyzeCopyItemWithPreset:
                    return typeof(MpPreset);
                case MpShortcutType.InvokeTrigger:
                    return typeof(MpAction);
            }
            return typeof(MpShortcut);
        }

        #endregion

        #region Commands
        public MpIAsyncCommand PerformShortcutCommand => new MpAsyncCommand(
            async () => {
                if (ShortcutCommand is MpIAsyncCommand ac) {
                    await ac.ExecuteAsync();
                } else if (ShortcutCommand is MpIAsyncCommand<object> ac2) {
                    await ac2.ExecuteAsync(CommandParameter);
                } else {
                    ShortcutCommand?.Execute(CommandParameter);
                }


                if (ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset) {
                    //var aipvm = MpAvAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(CommandId);
                    //if (aipvm != null) {
                    //    Task.Run(async () => {
                    //        await Task.Delay(300);
                    //        while (aipvm.IsBusy) { await Task.Delay(100); }

                    //        OnShortcutExecuted?.Invoke(this, aipvm.Parent.LastTransaction == null ? null : aipvm.Parent.LastTransaction.ResponseContent);
                    //    });
                    //}
                } else if (ShortcutType == MpShortcutType.PasteCopyItem ||
                            ShortcutType == MpShortcutType.PasteToExternal) {
                    OnShortcutExecuted?.Invoke(this, MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem);
                }
            },
            () => {
                bool is_this_app_active = MpAvMainWindowViewModel.Instance.IsAnyAppWindowActive;
                bool canPerformShortcut = true;
                if (!IsGlobal && !is_this_app_active) {

                    canPerformShortcut = false;
                }

                MpConsole.WriteLine($"CanPerformShortcut '{ShortcutType}': {canPerformShortcut.ToString().ToUpper()}", true, canPerformShortcut);

                if (!canPerformShortcut) {
                    MpConsole.WriteLine($"IsGlobal: " + IsGlobal);
                    MpConsole.WriteLine($"IsAnyAppWindowActive: " + MpAvMainWindowViewModel.Instance.IsAnyAppWindowActive, false, true);
                }
                return canPerformShortcut;
            });

        public ICommand DeleteOrResetThisShortcutCommand => new MpAsyncCommand(
            async () => {
                if (IsCustom) {
                    await Parent.DeleteShortcutCommand.ExecuteAsync(this);
                } else {

                    await Parent.ResetShortcutCommand.ExecuteAsync(this);
                }
                Parent.RefreshFilters();
            },
            () => {
                return Parent != null && IsCustom ? CanDelete : CanReset;
            });

        #endregion
    }
}
