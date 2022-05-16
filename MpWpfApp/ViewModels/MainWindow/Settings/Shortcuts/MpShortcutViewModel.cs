using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using MouseKeyHook.Rx;
using WindowsInput;
using MonkeyPaste;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public interface MpIShortcutCommandViewModel<T> where T:struct,Enum {
        int CommandId { get; }
        ICommand Command { get; }
        object CommandParameter { get; }
    }

    public class MpShortcutViewModel : MpViewModelBase<MpShortcutCollectionViewModel>, MpIActionComponent {
        #region Properties        

        #region View Models

        public ObservableCollection<MpShortcutKeyViewModel> KeyItems {
            get {
                var keyItems = new ObservableCollection<MpShortcutKeyViewModel>();
                foreach (var kl in KeyList) {
                    int seqIdx = KeyList.IndexOf(kl);
                    foreach (var k in kl) {
                        if (kl.Count > 1 && kl.IndexOf(k) < kl.Count - 1) {
                            keyItems.Add(new MpShortcutKeyViewModel(this,
                                            MpWpfKeyboardInputHelpers.GetKeyLiteral(k),
                                            true, false, seqIdx));
                        } else if (kl.IndexOf(k) == kl.Count - 1 && KeyList.IndexOf(kl) < KeyList.Count - 1) {
                            keyItems.Add(new MpShortcutKeyViewModel(this,
                                            MpWpfKeyboardInputHelpers.GetKeyLiteral(k),
                                            false, true, seqIdx));
                        } else {
                            keyItems.Add(new MpShortcutKeyViewModel(this,
                                            MpWpfKeyboardInputHelpers.GetKeyLiteral(k),
                                            false, false, seqIdx));
                        }

                    }
                }
                return keyItems;
            }
        }


        #endregion

        #region Visibility 

        public Visibility GlobalRoutingTypeComboItemVisibility {
            get {
                return IsRoutable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility InternalRoutingTypeComboItemVisibility {
            get {
                return IsRoutable ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility DeleteButtonVisibility {
            get {
                return IsCustom() ? Visibility.Visible:Visibility.Collapsed;
            }
        }

        public Visibility ResetButtonVisibility {
            get {
                return IsCustom() ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        #endregion

        #region State

        private string _menuItemTag = string.Empty;
        public string MenuItemTag {
            get {
                return _menuItemTag;
            }
            set {
                if(_menuItemTag != value) {
                    _menuItemTag = value;
                    OnPropertyChanged(nameof(MenuItemTag));
                }
            }
        }

        private ICommand _command = null;
        public ICommand Command {
            get {
                return _command;
            }
            set {
                if (_command != value) {
                    _command = value;
                    OnPropertyChanged(nameof(Command));
                }
            }
        }

        public IDisposable KeysObservable { get; set; }


        public bool IsRoutable {
            get {
                return RoutingType != MpRoutingType.None && RoutingType != MpRoutingType.Internal;
            }
        }

        public bool IsNew {
            get {
                return Shortcut.ShortcutId == 0;
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

        public string SelectedRoutingType {
            get {
                if (Shortcut == null) {
                    return "None";
                }
                return Enum.GetName(typeof(MpRoutingType), Shortcut.RoutingType);
            }
            set {
                if (Shortcut != null && Enum.GetName(typeof(MpRoutingType), Shortcut.RoutingType) != value) {
                    for (int i = 0; i < Enum.GetNames(typeof(MpRoutingType)).Length; i++) {
                        var rt = Enum.GetNames(typeof(MpRoutingType))[i];
                        if (rt == value) {
                            RoutingType = (MpRoutingType)i;
                        }
                    }
                    OnPropertyChanged(nameof(SelectedRoutingType));
                }
            }
        }

        private ObservableCollection<string> _routingTypes = null;
        public ObservableCollection<string> RoutingTypes {
            get {
                if(_routingTypes == null) {
                    _routingTypes = new ObservableCollection<string>();
                    if(IsRoutable) {
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


        public bool IsEmpty => KeyItems.Count == 0;

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

        public int CommandId {
            get {
                if(Shortcut == null) {
                    return 0;
                }
                //if(IsCustom()) {
                //    return Shortcut.CommandId;
                //}
                //return ShortcutId;
                return Shortcut.CommandId;
            }
            set {
                if(CommandId != value) {
                    if(!IsCustom()) {
                        throw new Exception("Application shortcuts use pk not command id");
                    }
                    Shortcut.CommandId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CommandId));
                }
            }
        }

        public int ShortcutId {
            get {
                if (Shortcut == null) {
                    return 0;
                }
                return Shortcut.ShortcutId;
            }
            set {
                if (Shortcut != null && Shortcut.ShortcutId != value) {
                    Shortcut.ShortcutId = value;
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
                return MpWpfKeyboardInputHelpers.ConvertStringToKeySequence(KeyString);
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
                if (Shortcut != null && Shortcut.KeyString != value) {
                    Shortcut.KeyString = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(KeyString));
                    OnPropertyChanged(nameof(KeyList));
                }
            }
        }

        public string ShortcutDisplayName {
            get {
                if(Shortcut == null) {
                    return string.Empty;
                }
                return Shortcut.ShortcutName;
            }
            set {
                if (Shortcut != null && Shortcut.ShortcutName != value) {
                    Shortcut.ShortcutName = value;
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
                    OnPropertyChanged(nameof(SelectedRoutingType));
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
                    OnPropertyChanged(nameof(CommandId));
                    OnPropertyChanged(nameof(ShortcutType));
                    OnPropertyChanged(nameof(DefaultKeyString));
                    OnPropertyChanged(nameof(ResetButtonVisibility));
                    OnPropertyChanged(nameof(DeleteButtonVisibility));
                    OnPropertyChanged(nameof(SelectedRoutingType));
                }
            }
        }
        #endregion

        #endregion

        #region Events

        public event EventHandler<MpCopyItem> OnShortcutExecuted;

        #endregion

        #region Public Methods
        public MpShortcutViewModel(MpShortcutCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpShortcutViewModel_PropertyChanged;
        }


        public async Task InitializeAsync(MpShortcut s, ICommand command) {
            //only register if non-empty keysstring
            Shortcut = s;
            Command = command;

            OnPropertyChanged(nameof(KeyItems));
            OnPropertyChanged(nameof(IsEmpty));

            if (string.IsNullOrEmpty(KeyString)) {
                if (!IsSequence()) {
                    Unregister();
                }
                await Shortcut.WriteToDatabaseAsync();
                return;
            } else {
                try {
                    if (RoutingType == MpRoutingType.None) {
                        throw new Exception("ShortcutViewModel error, routing type cannot be none");
                    }
                    var hook = RoutingType == MpRoutingType.Internal ? Parent.ApplicationHook : Parent.GlobalHook;

                    var cl = MpWpfKeyboardInputHelpers.ConvertStringToKeySequence(KeyString);
                    var wfcl = cl.Select(x => x.Select(y => MpWpfKeyboardInputHelpers.WpfKeyToWinformsKey(y)).ToList()).ToList();
                    string keyValStr = string.Join(",", wfcl.Select(x =>
                                                 string.Join("+", x.Select(y =>
                                                    Enum.GetName(typeof(System.Windows.Forms.Keys), y)))));

                    //keyValStr = KeyString;
                    if (IsSequence()) {
                        if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                            //only register sequences at startup
                            hook.OnSequence(new Dictionary<Sequence, Action> {
                                {
                                    Sequence.FromString(keyValStr),
                                    () => PerformShortcutCommand.Execute(null)
                                }
                            });
                        }
                    } else {
                        //unregister if already exists
                        Unregister();
                        var t = new MouseKeyHook.Rx.Trigger[] { MouseKeyHook.Rx.Trigger.FromString(keyValStr) };
                        
                        KeysObservable = hook.KeyUpObservable().Matching(t).Subscribe(
                            (trigger) => PerformShortcutCommand.Execute(null)
                        );
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteLine("Error creating shortcut: " + ex.ToString());
                    return;
                }
                //MpConsole.WriteLine("Shortcut Successfully registered for '" + ShortcutDisplayName + "' with hotkeys: " + KeyString);
                return;
            }
        }

        public void Register(MpIActionComponentHandler mvm) {
            //by design this only can occur for shortcuts with a selected item as its context

            OnShortcutExecuted += mvm.OnActionTriggered;
            MpConsole.WriteLine($"ClipTray Registered {mvm.Label} matcher");
        }

        public void Unregister(MpIActionComponentHandler mvm) {
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
                    //        var ctvm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(Shortcut.CopyItemId);
                    //        ctvm.ShortcutKeyString = Shortcut.KeyString;
                    //    } else {
                    //        var ttvm = MpTagTrayViewModel.Instance.Items.Where(x => x.Tag.Id == Shortcut.TagId).Single();
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

        public bool StartsWith(List<List<Key>> otherKeyList) {
            if(otherKeyList == null || 
               otherKeyList.Count == 0 || 
               otherKeyList[0].Count == 0) {
                return false;
            }
            for (int i = 0; i < otherKeyList.Count; i++) {
                if(KeyList.Count <=i) {
                    return false;
                }
                for (int j = 0; j < otherKeyList[i].Count; j++) {
                    if(KeyList[i].Count <= j) {
                        return false;
                    }
                    if(KeyList[i][j] != otherKeyList[i][j]) {
                        return false;
                    }
                }
            }
            return true;
        }

        public void PassKeysToForegroundWindow() {
            MpHelpers.PassKeysListToWindow(MpProcessHelper.MpProcessManager.LastHandle,KeyList);
        }

        public bool IsSequence() {
            return KeyString.Contains(",");
        }
        public bool IsCustom() {
            if(Shortcut == null) {
                return false;
            }
            return Shortcut.CommandId > 0;
        }

        public void ClearShortcutKeyString() {
            KeyString = string.Empty;
        }

        public object Clone() {
            return new MpShortcutViewModel(Parent);
        }
        #endregion

        #region Protected Methods

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            bool wasChanged = false;
            if(e is MpCopyItem ci) {
                if(ShortcutType == MpShortcutType.PasteCopyItem && 
                    CommandId == ci.Id) {
                    ShortcutDisplayName = $"Paste {ci.Title}";
                    wasChanged = true;
                }
            } else if (e is MpTag t) {
                if (ShortcutType == MpShortcutType.SelectTag &&
                    CommandId == t.Id) {
                    ShortcutDisplayName = $"Select {t.TagName}";
                    wasChanged = true;
                }
            } else if (e is MpAnalyticItemPreset aip) {
                if (ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset &&
                    CommandId == aip.Id) {
                    ShortcutDisplayName = $"User {aip.Label} analyzer";
                    wasChanged = true;
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
        public ICommand PerformShortcutCommand => new RelayCommand(
            () => {
                if (IsCustom()) {
                    Command?.Execute(CommandId);
                } else {
                    Command?.Execute(null);
                }

                if (ShortcutType == MpShortcutType.AnalyzeCopyItemWithPreset) {
                    var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(CommandId);
                    if (aipvm != null) {
                        Task.Run(async () => {
                            await Task.Delay(300);
                            while (aipvm.IsBusy) { await Task.Delay(100); }

                            OnShortcutExecuted?.Invoke(this, aipvm.Parent.LastResultContentItem);
                        });
                    }
                } else if (ShortcutType == MpShortcutType.PasteCopyItem || ShortcutType == MpShortcutType.PasteSelectedItems) {
                    OnShortcutExecuted?.Invoke(this, MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItem);
                }
            },
            () => {
                if(MpIsFocusedExtension.IsAnyTextBoxFocused) {
                    return false;
                }
                var mwvm = MpMainWindowViewModel.Instance;
                var ctrvm = MpClipTrayViewModel.Instance;
                var ttrvm = MpTagTrayViewModel.Instance;
                var sbvm = MpSearchBoxViewModel.Instance;
                var acvm = MpActionCollectionViewModel.Instance;
                //never perform shortcuts in the following states
                if (mwvm.IsShowingDialog ||
                   ctrvm.IsAnyPastingTemplate ||
                   ctrvm.IsAnyEditingClipTile ||
                   ctrvm.IsAnyEditingClipTitle ||
                   ttrvm.IsEditingTagName) {
                    return false;
                }
                if(mwvm.IsMainWindowOpen) {
                    if(sbvm.IsTextBoxFocused) {
                        return false;
                    }
                }
                //otherwise check basic type routing for validity
                if (RoutingType == MpRoutingType.Internal) {
                    return MpMainWindowViewModel.Instance.IsMainWindowOpen;
                } else {
                    return !MpMainWindowViewModel.Instance.IsMainWindowOpen;
                }
            });
        
        #endregion
    }
}
