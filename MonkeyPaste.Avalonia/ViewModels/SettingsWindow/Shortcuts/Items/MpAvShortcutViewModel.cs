using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using MonkeyPaste;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using SharpHook.Native;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Controls;
using System.Windows.Input;
using Key = Avalonia.Input.Key;
using SkiaSharp;

namespace MonkeyPaste.Avalonia {

    public class MpAvShortcutViewModel : MpViewModelBase<MpAvShortcutCollectionViewModel>, 
        MpIActionComponent,
        MpAvIShortcutCommand,
        MpISelectableViewModel {
        #region Properties        

        #region MpAvIShortcutCommand Implementation

        public ICommand AssignCommand => new MpCommand(() => {
            Parent.SelectedItem = this;
            Parent.ReassignSelectedShortcutCommand.Execute(null);
        });

        public MpAvShortcutViewModel ShortcutViewModel => this;
        public string ShortcutKeyString => KeyString;

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region View Models

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
                            KeyStr = key,
                        };
                        comboGroup.Items.Add(skvm);
                    }
                    comboGroup.IsPlusVisible = maxComboIdx > 0 && comboIdx < maxComboIdx;
                    keyItems.Add(comboGroup);
                }
                return keyItems;
            }
        }


        #endregion

        #region State

        public ICommand Command { get; set; }

        public IDisposable KeysObservable { get; set; }

        public bool IsGlobalShortcut => RoutingType != MpRoutingType.None && RoutingType != MpRoutingType.Internal;



        public bool CanDelete => IsCustom();

        public bool IsNew {
            get {
                return ShortcutId == 0;
            }
        }

        public MpShortcutType ShortcutType {
            get {
                if(Shortcut == null) {
                    return MpShortcutType.None;
                }
                return Shortcut.ShortcutType;
            }
            set {
                if(Shortcut.ShortcutType != value) {
                    Shortcut.ShortcutType = value;
                    OnPropertyChanged(nameof(ShortcutType));
                }
            }
        }

        public string ShortcutTypeName {
            get {
                if (IsCustom()) {
                    switch(ShortcutType) {
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
                return (int)RoutingType;
            }
            set {
                if (SelectedRoutingTypeIdx != value) {
                    RoutingType = (MpRoutingType)value;
                    OnPropertyChanged(nameof(SelectedRoutingTypeIdx));                    
                }
            }
        }

        private ObservableCollection<string> _routingTypes = null;
        public ObservableCollection<string> RoutingTypes {
            get {
                if(_routingTypes == null) {
                    _routingTypes = new ObservableCollection<string>();
                    if(IsGlobalShortcut) {
                        _routingTypes.Add("Direct");
                        _routingTypes.Add("Bubble");
                        _routingTypes.Add("Tunnel");
                    } else {
                        _routingTypes.Add("Internal");
                    }
                }
                return _routingTypes;
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
                if(Shortcut == null) {
                    return null;
                }
                //if(IsCustom()) {
                //    return Shortcut.CommandId;
                //}
                //return ShortcutId;
                return Shortcut.CommandParameter;
            }
            set {
                if(CommandParameter != value) {
                    if(!IsCustom()) {
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

        public List<List<Key>> KeyList {
            get {
                if(Shortcut == null) {
                    return new List<List<Key>>();
                }
                //var kl = new List<List<Key>>();
                //for (int i = 0; i < Shortcut.KeyList.Count; i++) {
                //    kl.Add(new List<Key>());
                //    for (int j = 0; j < Shortcut.KeyList[i].Count; j++) {
                //        kl[i].Add((Key)Shortcut.KeyList[i][j]);
                //    }
                //}
                //return kl;
                return MpAvKeyboardInputHelpers.ConvertStringToKeySequence(KeyString);
            }
        }

        public string KeyString {
            get {
                if(Shortcut == null) {
                    return string.Empty;
                }
                return Shortcut.KeyString;
            }
            set {
                if (Shortcut.KeyString != value) {
                    Shortcut.KeyString = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(KeyString));
                    OnPropertyChanged(nameof(SendKeyStr));
                    OnPropertyChanged(nameof(KeyList));
                }
            }
        }


        private string _sendKeyStr;
        public string SendKeyStr => _sendKeyStr;

        public string ShortcutDisplayName {
            get {
                if(Shortcut == null) {
                    return string.Empty;
                }
                return Shortcut.ShortcutLabel;
            }
            set {
                if (Shortcut != null && Shortcut.ShortcutLabel != value) {
                    Shortcut.ShortcutLabel = value;
                    HasModelChanged = true; 
                    OnPropertyChanged(nameof(ShortcutDisplayName));
                }
            }
        }

        public MpRoutingType RoutingType {
            get {
                if(Shortcut == null) {
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

        private MpShortcut _shortcut = null;
        public MpShortcut Shortcut {
            get {
                return _shortcut;
            }
            set {
                if(_shortcut != value) {
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
            Command = command;
            _sendKeyStr = MpAvKeyboardInputHelpers.ConvertKeyStringToSendKeysString(KeyString);

            OnPropertyChanged(nameof(KeyItems));
            OnPropertyChanged(nameof(IsEmpty));
        }

        public void RegisterActionComponent(MpIActionTrigger mvm) {
            //by design this only can occur for shortcuts with a selected item as its context

            OnShortcutExecuted += mvm.OnActionTriggered;
            MpConsole.WriteLine($"ClipTray Registered {mvm.Label} matcher");
        }

        public void UnregisterActionComponent(MpIActionTrigger mvm) {
            OnShortcutExecuted -= mvm.OnActionTriggered;
            MpConsole.WriteLine($"Matcher {mvm.Label} Unregistered from OnShortcutExecuted");
        }

        private void MpShortcutViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasModelChanged):
                    if(HasModelChanged) {
                        Task.Run(async () => {
                            await Shortcut.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
                case nameof(KeyString):
                    //if (IsCustom()) {
                    //    if (Shortcut.CopyItemId > 0) {
                    //        var ctvm = MpAvClipTrayViewModel.Instance.GetContentItemViewModelById(Shortcut.CopyItemId);
                    //        ctvm.ShortcutKeyString = Shortcut.KeyString;
                    //    } else {
                    //        var ttvm = MpAvTagTrayViewModel.Instance.Items.Where(x => x.Tag.Id == Shortcut.TagId).Single();
                    //        ttvm.ShortcutKeyString = Shortcut.KeyString;
                    //    }
                    //}
                    OnPropertyChanged(nameof(KeyItems));
                    OnPropertyChanged(nameof(IsEmpty));
                    break;
            }
        }

        public void Unregister() {
            if(KeysObservable != null) {
                KeysObservable.Dispose();
                MpConsole.WriteLine("Unregistering shortcut " + Shortcut.ToString() + " was successful");
            } else {
                //either not previously registered or a sequence that won't be unregistered until app shutdown
            }
        }

        //public bool StartsWith(List<List<Key>> otherKeyList) {
        //    if(otherKeyList == null || 
        //       otherKeyList.Count == 0 || 
        //       otherKeyList[0].Count == 0) {
        //        return false;
        //    }
        //    for (int i = 0; i < otherKeyList.Count; i++) {
        //        if(KeyList.Count <=i) {
        //            return false;
        //        }
        //        for (int j = 0; j < otherKeyList[i].Count; j++) {
        //            if(KeyList[i].Count <= j) {
        //                return false;
        //            }
        //            if(KeyList[i][j] != otherKeyList[i][j]) {
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}

        public void PassKeysToForegroundWindow() {
            //MpHelpers.PassKeysListToWindow(MpProcessHelper.MpProcessManager.LastHandle,KeyList);
        }

        public bool IsSequence() {
            return KeyString.Contains(",");
        }
        public bool IsCustom() {
            if(Shortcut == null) {
                return false;
            }
            return (int)ShortcutType >= MpShortcut.MIN_USER_SHORTCUT_TYPE;
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
        #endregion

        #region Protected Methods

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            bool wasChanged = false;
            if (int.TryParse(CommandParameter, out int cmd_param_int)) {
                if (e is MpCopyItem ci) {
                    if (ShortcutType == MpShortcutType.PasteCopyItem &&
                        cmd_param_int == ci.Id) {
                        ShortcutDisplayName = $"Paste {ci.Title}";
                        wasChanged = true;
                    }
                } else if (e is MpTag t) {
                    if (ShortcutType == MpShortcutType.SelectTag &&
                        cmd_param_int == t.Id) {
                        ShortcutDisplayName = $"Select {t.TagName}";
                        wasChanged = true;
                    }
                } else if (e is MpPluginPreset aip) {
                    if (ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset &&
                        cmd_param_int == aip.Id) {
                        ShortcutDisplayName = $"User {aip.Label} analyzer";
                        wasChanged = true;
                    }
                }
            }
            
            if(wasChanged) {
                Task.Run(async () => {
                    await Shortcut.WriteToDatabaseAsync();
                });
            }
        }
        #endregion

        #region Private Methods

        #endregion

        #region Commands
        public ICommand PerformShortcutCommand => new MpCommand(
            () => {

                Command?.Execute(CommandParameter);

                if (ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset) {
                    //var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(CommandId);
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
                var acvm = MpAvActionCollectionViewModel.Instance;

                bool canPerformShortcut = true;

                if(IsGlobalShortcut && !mwvm.IsMainWindowActive) {
                    // should be fine and up to commands when its gloabl and app isn't active
                } else {
                    // when mw is active treat global shortcuts like any other
                    if(mwvm.IsMainWindowActive) {
                        if (mwvm.IsAnyDialogOpen ||
                            mwvm.IsAnyItemDragging ||
                            mwvm.IsAnyMainWindowTextBoxFocused ||
                            !mwvm.IsMainWindowActive) {
                            canPerformShortcut = false;
                        }
                    } else {
                        canPerformShortcut = false;
                    }
                }

                MpConsole.WriteLine($"CanPerformShortcut '{ShortcutType}': {canPerformShortcut.ToString().ToUpper()}");

                if(!canPerformShortcut) {
                    MpConsole.WriteLine($"IsGlobalShortcut: "+IsGlobalShortcut);
                    MpConsole.WriteLine($"IsMainWindowActive: "+mwvm.IsMainWindowActive);
                    MpConsole.WriteLine($"IsShowingDialog: "+mwvm.IsAnyDialogOpen);
                    MpConsole.WriteLine($"IsAnyItemDragging: "+mwvm.IsAnyItemDragging);
                    MpConsole.WriteLine($"IsAnyTextBoxFocused: "+mwvm.IsAnyMainWindowTextBoxFocused);
                    MpConsole.WriteLine($"IsMainWindowActive: "+mwvm.IsMainWindowActive);
                }
                return canPerformShortcut;
            });


        #endregion
    }
}
