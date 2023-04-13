using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Key = Avalonia.Input.Key;

namespace MonkeyPaste.Avalonia {

    public class MpAvShortcutViewModel : MpViewModelBase<MpAvShortcutCollectionViewModel>,
        //MpIActionComponent,
        MpIFilterMatch,
        MpAvIShortcutCommandViewModel,
        MpAvIKeyGestureViewModel,
        MpISelectableViewModel {

        #region Interfaces

        #region MpIFilterMatch Implementation
        bool MpIFilterMatch.IsMatch(string filter) {
            if (string.IsNullOrEmpty(filter)) {
                return true;
            }
            return
                ShortcutTypeName.ToLower().Contains(filter.ToLower()) ||
                ShortcutDisplayName.ToLower().Contains(filter.ToLower()) ||
                RoutingType.ToString().ToLower().Contains(filter.ToLower());
        }

        #endregion

        #region MpAvIShortcutCommandViewModel Implementation

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
                    _routingTypes = new ObservableCollection<string>();
                    if (IsGlobalShortcut) {
                        //_routingTypes.Add("Direct");
                        _routingTypes.Add("Bubble");
                        _routingTypes.Add("Tunnel");
                        _routingTypes.Add("Override");
                    } else {
                        _routingTypes.Add("Internal");
                    }
                }
                return _routingTypes;
            }
        }

        #endregion

        #region State
        public bool IsCustom {
            get {
                if (Shortcut == null) {
                    return false;
                }
                return (int)ShortcutType >= MpShortcut.MIN_USER_SHORTCUT_TYPE;
            }
        }
        public ICommand ShortcutCommand { get; set; }

        public IDisposable KeysObservable { get; set; }

        public bool IsGlobalShortcut =>
            ShortcutType.IsGlobal();

        public bool CanDelete =>
            IsCustom;

        public bool CanReset =>
            !IsCustom && KeyString != DefaultKeyString;

        public bool CanDeleteOrReset =>
            CanDelete || CanReset;

        public bool IsNew =>
            ShortcutId == 0;


        public string ShortcutDisplayName { get; private set; }
        public MpShortcutType ShortcutType {
            get {
                if (Shortcut == null) {
                    return MpShortcutType.None;
                }
                return Shortcut.ShortcutType;
            }
            set {
                if (Shortcut.ShortcutType != value) {
                    Shortcut.ShortcutType = value;
                    OnPropertyChanged(nameof(ShortcutType));
                }
            }
        }

        public string ShortcutTypeName {
            get {
                if (IsCustom) {
                    switch (ShortcutType) {
                        case MpShortcutType.PasteCopyItem:
                            return "Clip";
                        case MpShortcutType.SelectTag:
                            return "Tag";
                        case MpShortcutType.AnalyzeCopyItemWithPreset:
                            return "Analyzer";
                    }
                }
                return "Application";
            }
        }

        public int SelectedRoutingTypeIdx {
            get {
                if (Shortcut == null) {
                    return 0;
                }
                return RoutingTypes.IndexOf(RoutingType.ToString());
                //return (int)RoutingType - (int)MpRoutingType.Internal - 1;
            }
            set {
                if (RoutingType == MpRoutingType.Internal) {
                    return;
                }
                if (SelectedRoutingTypeIdx != value) {
                    value = Math.Max(0, value);
                    //RoutingType = (MpRoutingType)(value + (int)MpRoutingType.Internal + 1);
                    RoutingType = RoutingTypes[value].ToString().ToEnum<MpRoutingType>();
                    OnPropertyChanged(nameof(SelectedRoutingTypeIdx));
                }
            }
        }

        public bool IsEmpty => KeyItems.Count() == 0;

        #endregion

        #region Model

        public string DefaultKeyString {
            get {
                if (Shortcut == null) {
                    return String.Empty;
                }
                return Shortcut.DefaultKeyString;
            }
            set {
                if (Shortcut != null && Shortcut.DefaultKeyString != value) {
                    Shortcut.DefaultKeyString = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(DefaultKeyString));
                }
            }
        }

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
                if (Shortcut != null && Shortcut.Id != value) {
                    Shortcut.Id = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ShortcutId));
                }
            }
        }

        private List<List<Key>> _keyList;
        public List<List<Key>> KeyList {
            get {
                if (_keyList == null) {
                    _keyList = new List<List<Key>>();
                    if (Shortcut == null) {
                        return _keyList;
                    }
                    Mp.Services.KeyConverter
                        .ConvertStringToKeySequence<Key>(KeyString)
                        .ForEach(x => _keyList.Add(x.ToList()));
                }

                return _keyList;
            }
        }

        public string KeyString {
            get {
                if (Shortcut == null) {
                    return string.Empty;
                }
                if (Shortcut.KeyString == null) {
                    // avoid null errors for input matching
                    return string.Empty;
                }
                return Shortcut.KeyString;
            }
            set {
                if (Shortcut.KeyString != value) {
                    Shortcut.KeyString = value;
                    // flag keylist to reset
                    _keyList = null;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(KeyString));
                    OnPropertyChanged(nameof(KeyList));
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
                if (Shortcut != null && Shortcut.RoutingType != value) {
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
                if (Shortcut.RoutingDelayMs != value) {
                    Shortcut.RoutingDelayMs = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(RoutingDelayMs));
                }
            }
        }
        public bool IsModelReadOnly =>
            Shortcut == null || Shortcut.IsReadOnly;

        private MpShortcut _shortcut = null;
        public MpShortcut Shortcut {
            get {
                return _shortcut;
            }
            set {
                if (_shortcut != value) {
                    _shortcut = value;
                    OnPropertyChanged(nameof(Shortcut));
                    OnPropertyChanged(nameof(RoutingType));
                    OnPropertyChanged(nameof(KeyString));
                    OnPropertyChanged(nameof(KeyList));
                    OnPropertyChanged(nameof(ShortcutDisplayName));
                    OnPropertyChanged(nameof(ShortcutId));
                    OnPropertyChanged(nameof(CommandParameter));
                    OnPropertyChanged(nameof(ShortcutType));
                    OnPropertyChanged(nameof(DefaultKeyString));
                    OnPropertyChanged(nameof(SelectedRoutingTypeIdx));
                }
            }
        }
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

            OnPropertyChanged(nameof(IsGlobalShortcut));
            OnPropertyChanged(nameof(KeyItems));
            OnPropertyChanged(nameof(IsEmpty));
        }

        //public void RegisterActionComponent(MpIInvokableAction mvm) {
        //    //by design this only can occur for shortcuts with a selected item as its context

        //    OnShortcutExecuted += mvm.OnActionInvoked;
        //    MpConsole.WriteLine($"ClipTray Registered {mvm.Label} matcher");
        //}

        //public void UnregisterActionComponent(MpIInvokableAction mvm) {
        //    OnShortcutExecuted -= mvm.OnActionInvoked;
        //    MpConsole.WriteLine($"Matcher {mvm.Label} Unregistered from OnShortcutExecuted");
        //}

        public void Unregister() {
            if (KeysObservable != null) {
                KeysObservable.Dispose();
                MpConsole.WriteLine("Unregistering shortcut " + Shortcut.ToString() + " was successful");
            } else {
                //either not previously registered or a sequence that won't be unregistered until app shutdown
            }
        }

        public bool IncludesKeyLiteral(string keyliteral) {
            var keys = Mp.Services.KeyConverter.ConvertStringToKeySequence<Key>(keyliteral);
            if (keys.FirstOrDefault() is IEnumerable<Key> combo &&
                combo.FirstOrDefault() is Key key) {
                return KeyList.Any(x => x.Any(y => y == key));
            }
            return false;
        }

        public void PassKeysToForegroundWindow() {
            //MpHelpers.PassKeysListToWindow(MpProcessHelper.MpProcessManager.LastHandle,KeyList);
        }

        public bool IsSequence() {
            return KeyString.Contains(",");
        }


        public void ClearShortcutKeyString() {
            KeyString = string.Empty;
        }

        public object Clone() {
            return new MpAvShortcutViewModel(Parent);
        }

        public override string ToString() {
            return $"{ShortcutType} - '{KeyString}'";
        }


        public async Task SetShortcutNameAsync() {
            Dispatcher.UIThread.CheckAccess();
            ShortcutDisplayName = await ShortcutType.GetShortcutTitleAsync(CommandParameter);
        }
        #endregion

        #region Protected Methods

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            //bool wasChanged = false;
            //if (int.TryParse(CommandParameter, out int cmd_param_int)) {
            //    if (e is MpCopyItem ci) {
            //        if (ShortcutType == MpShortcutType.PasteCopyItem &&
            //            cmd_param_int == ci.Id) {
            //            ShortcutDisplayName = $"Paste {ci.Title}";
            //            wasChanged = true;
            //        }
            //    } else if (e is MpTag t) {
            //        if (ShortcutType == MpShortcutType.SelectTag &&
            //            cmd_param_int == t.Id) {
            //            ShortcutDisplayName = $"Select {t.TagName}";
            //            wasChanged = true;
            //        }
            //    } else if (e is MpPluginPreset aip) {
            //        if (ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset &&
            //            cmd_param_int == aip.Id) {
            //            ShortcutDisplayName = $"User {aip.Label} analyzer";
            //            wasChanged = true;
            //        }
            //    }
            //}

            //if (wasChanged) {
            //    Task.Run(async () => {
            //        await Shortcut.WriteToDatabaseAsync();
            //    });
            //}
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
                    OnPropertyChanged(nameof(KeyItems));
                    OnPropertyChanged(nameof(KeyGroups));
                    OnPropertyChanged(nameof(IsEmpty));
                    OnPropertyChanged(nameof(CanReset));
                    break;
                case nameof(IsBusy):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    break;
                case nameof(ShortcutType):
                    OnPropertyChanged(nameof(IsGlobalShortcut));
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
                    return typeof(MpPluginPreset);
                case MpShortcutType.InvokeTrigger:
                    return typeof(MpAction);
            }
            return typeof(MpShortcut);
        }
        #endregion

        #region Commands
        public ICommand PerformShortcutCommand => new MpCommand(
            () => {
                ShortcutCommand?.Execute(CommandParameter);

                if (ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset) {
                    //var aipvm = MpAvAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(CommandId);
                    //if (aipvm != null) {
                    //    Task.Run(async () => {
                    //        await Task.Delay(300);
                    //        while (aipvm.IsBusy) { await Task.Delay(100); }

                    //        OnShortcutExecuted?.Invoke(this, aipvm.Parent.LastTransaction == null ? null : aipvm.Parent.LastTransaction.ResponseContent);
                    //    });
                    //}
                } else if (ShortcutType == MpShortcutType.PasteCopyItem || ShortcutType == MpShortcutType.PasteSelectedItems) {
                    OnShortcutExecuted?.Invoke(this, MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem);
                }
            },
            () => {
                var mwvm = MpAvMainWindowViewModel.Instance;
                var ctrvm = MpAvClipTrayViewModel.Instance;
                var ttrvm = MpAvTagTrayViewModel.Instance;
                var sbvm = MpAvSearchBoxViewModel.Instance;
                var acvm = MpAvTriggerCollectionViewModel.Instance;

                bool canPerformShortcut = true;

                //if(IsGlobalShortcut && !mwvm.IsMainWindowActive) {
                //    // should be fine and up to commands when its gloabl and app isn't active
                //} else {
                //    // when mw is active treat global shortcuts like any other
                //    if(mwvm.IsMainWindowActive) {
                //        if (mwvm.IsAnyDialogOpen ||
                //            mwvm.IsAnyItemDragging ||
                //            mwvm.IsAnyMainWindowTextBoxFocused ||
                //            !mwvm.IsMainWindowActive) {
                //            canPerformShortcut = false;
                //        }
                //    } else {
                //        canPerformShortcut = false;
                //    }
                //}

                if (!IsGlobalShortcut) {
                    if (!mwvm.IsMainWindowActive) {
                        canPerformShortcut = false;
                    }
                }

                MpConsole.WriteLine($"CanPerformShortcut '{ShortcutType}': {canPerformShortcut.ToString().ToUpper()}", true);

                if (!canPerformShortcut) {
                    MpConsole.WriteLine($"IsGlobalShortcut: " + IsGlobalShortcut);
                    MpConsole.WriteLine($"IsMainWindowActive: " + mwvm.IsMainWindowActive);
                    MpConsole.WriteLine($"IsAnyItemDragging: " + mwvm.IsAnyItemDragging);
                    MpConsole.WriteLine($"IsAnyTextBoxFocused: " + mwvm.IsAnyMainWindowTextBoxFocused, false, true);
                }
                return canPerformShortcut;
            });

        public ICommand DeleteOrResetThisShortcutCommand => new MpCommand(
            () => {
                if (IsCustom) {
                    Parent.DeleteShortcutCommand.Execute(this);
                    return;
                }
                Parent.ResetShortcutCommand.Execute(this);
            },
            () => {
                return Parent != null && IsCustom ? CanDelete : CanReset;
            });

        public ICommand ReassignShortcutCommand => new MpCommand(
            () => {
                if (IsCustom) {

                }
            });

        #endregion
    }
}
