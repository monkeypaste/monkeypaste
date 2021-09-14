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

namespace MpWpfApp {
    public class MpShortcutViewModel : MpViewModelBase {
        #region Properties        

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

        #region Business Logic
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

        public List<string> SendKeysKeyStringList {
            get {
                var outStrList = new List<string>();
                var combos = KeyString.Split(',').ToList();
                foreach (var combo in combos) {
                    var outStr = string.Empty;
                    var keys = combo.Split('+').ToList();
                    foreach (var key in keys) {
                        switch (key) {
                            case "Control":
                                outStr += "^";
                                break;
                            case "Shift":
                                outStr += "+";
                                break;
                            case "Alt":
                                outStr += "%";
                                break; 
                            case "Enter":
                            case "Tab":
                            case "Left":
                            case "Right":
                            case "Up":
                            case "Down":
                                outStr += "{" + key.ToUpper() + "}";
                                break;
                            default:
                                if(key.ToUpper().StartsWith(@"F") && key.Length > 1) {
                                    string fVal = key.Substring(1, key.Length - 1);
                                    try {
                                        int val = Convert.ToInt32(fVal);
                                        outStr += "{F" + val + "}";
                                    }
                                    catch(Exception ex) {
                                        MonkeyPaste.MpConsole.WriteLine(@"ShortcutViewModel.SendKeys exception creating key: " + key + " with exception: " + ex);
                                        outStr += key.ToUpper();
                                        break;
                                    }
                                } else {
                                    outStr += key.ToUpper();
                                }
                                break;
                        }
                    }
                    outStrList.Add(outStr);
                }
                return outStrList;
            }
        }

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

        public string ShortcutTypeName {
            get {
                if (IsCustom()) {
                    if (Shortcut.CopyItemId > 0) {
                        return "Clip";
                    } else {
                        return "Tag";
                    }
                } else {
                    return "Application";
                }
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

        public object CommandParameter { get; set; } = null;
        #endregion

        #region State
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
                    Shortcut.WriteToDatabase();
                    OnPropertyChanged(nameof(DefaultKeyString));
                }
            }
        }

        public int CopyItemId {
            get {
                if (Shortcut == null) {
                    return 0;
                }
                return Shortcut.CopyItemId;
            }
            set {
                if (Shortcut != null && Shortcut.CopyItemId != value) {
                    Shortcut.CopyItemId = value;
                    Shortcut.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        public int TagId {
            get {
                if (Shortcut == null) {
                    return 0;
                }
                return Shortcut.TagId;
            }
            set {
                if (Shortcut != null && Shortcut.TagId != value) {
                    Shortcut.TagId = value;
                    Shortcut.WriteToDatabase();
                    OnPropertyChanged(nameof(TagId));
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
                    Shortcut.WriteToDatabase();
                    OnPropertyChanged(nameof(ShortcutId));
                }
            }
        }

        public List<List<Key>> KeyList {
            get {
                if(Shortcut == null) {
                    return new List<List<Key>>();
                }
                var kl = new List<List<Key>>();
                for (int i = 0; i < Shortcut.KeyList.Count; i++) {
                    kl.Add(new List<Key>());
                    for (int j = 0; j < Shortcut.KeyList[i].Count; j++) {
                        kl[i].Add((Key)Shortcut.KeyList[i][j]);
                    }
                }
                return kl;
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
                    Shortcut.WriteToDatabase();
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
                    Shortcut.WriteToDatabase();
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
                    Shortcut.WriteToDatabase();
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
                    OnPropertyChanged(nameof(TagId));
                    OnPropertyChanged(nameof(CopyItemId));
                    OnPropertyChanged(nameof(DefaultKeyString));
                    OnPropertyChanged(nameof(ResetButtonVisibility));
                    OnPropertyChanged(nameof(DeleteButtonVisibility));
                    OnPropertyChanged(nameof(SelectedRoutingType));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpShortcutViewModel(MpShortcut s, ICommand command, object commandParameter) {
            PropertyChanged += (s1, e) => {
                switch (e.PropertyName) {
                    case nameof(KeyString):
                        if (IsCustom()) {
                            if (Shortcut.CopyItemId > 0) {
                                var ctvm = MpClipTrayViewModel.Instance.GetClipTileByCopyItemId(Shortcut.CopyItemId);
                                if (ctvm == null) {
                                    var ci = MpCopyItem.GetCopyItemById(Shortcut.CopyItemId);
                                    if (ci == null) {
                                        MonkeyPaste.MpConsole.WriteLine("SHortcut init error cannot find copy item w/ id: " + Shortcut.CopyItemId);
                                        break;
                                    }
                                    ctvm = MpClipTrayViewModel.Instance.GetClipTileByCopyItemId(ci.CompositeParentCopyItemId);
                                    if (ctvm == null) {
                                        MonkeyPaste.MpConsole.WriteLine("SHortcut init error cannot find hostclip w/ id: " + ci.CompositeParentCopyItemId);
                                        break;
                                    }
                                    var rtbvm = ctvm.ContentContainerViewModel.GetContentItemByCopyItemId(ci.Id);
                                    rtbvm.ShortcutKeyString = Shortcut.KeyString;
                                } else {
                                    ctvm.ShortcutKeyString = Shortcut.KeyString;
                                }
                            } else {
                                var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.Where(x => x.Tag.Id == Shortcut.TagId).Single();
                                ttvm.ShortcutKeyString = Shortcut.KeyString;
                            }
                        }
                        break;
                }
            };

            Shortcut = s;
            Command = command;
            CommandParameter = commandParameter;
        }

        public void Register() {
            //only register if non-empty keysstring
            if (string.IsNullOrEmpty(KeyString)) {
                if(!IsSequence()) {
                    Unregister();
                }
                Shortcut.WriteToDatabase();
                return;
            } else {
                try {
                    if (RoutingType == MpRoutingType.None) {
                        throw new Exception("ShortcutViewModel error, routing type cannot be none");
                    }
                    var hook = RoutingType == MpRoutingType.Internal ? MpShortcutCollectionViewModel.Instance.ApplicationHook : MpShortcutCollectionViewModel.Instance.GlobalHook;
                    
                    if (IsSequence()) {
                        if(MpMainWindowViewModel.IsMainWindowLoading) {
                            //only register sequences at startup
                            hook.OnSequence(new Dictionary<Sequence, Action> {
                                {
                                    Sequence.FromString(KeyString),
                                    () => PerformShortcutCommand.Execute(null)
                                }
                            });
                        }
                    } else {
                        //unregister if already exists
                        Unregister();
                        var t = new MouseKeyHook.Rx.Trigger[] { MouseKeyHook.Rx.Trigger.FromString(KeyString) };
                        KeysObservable = hook.KeyDownObservable().Matching(t).Subscribe(
                            (trigger) => PerformShortcutCommand.Execute(null)
                        );
                    }
                }
                catch (Exception ex) {
                    MonkeyPaste.MpConsole.WriteLine("Error creating shortcut: " + ex.ToString());
                    return;
                }
                Shortcut.WriteToDatabase();
                MonkeyPaste.MpConsole.WriteLine("Shortcut Successfully registered for '" + ShortcutDisplayName + "' with hotkeys: " + KeyString);
                return;
            }
        }

        public void Unregister() {
            if(KeysObservable != null) {
                KeysObservable.Dispose();
                MonkeyPaste.MpConsole.WriteLine("Unregistering shortcut " + Shortcut.ToString() + " was successful");
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
            MpHelpers.Instance.PassKeysListToWindow(MpClipboardManager.Instance.LastWindowWatcher.LastHandle,KeyList);
        }

        public bool IsSequence() {
            return KeyString.Contains(",");
        }
        public bool IsCustom() {
            return Shortcut.CopyItemId > 0 || Shortcut.TagId > 0;
        }
        public void ClearShortcutKeyString() {
            KeyString = string.Empty;
        }

        public object Clone() {
            return new MpShortcutViewModel(Shortcut, Command, CommandParameter);
        }
        #endregion

        #region Private Methods
        
        #endregion

        #region Commands
        private RelayCommand _performShortcutCommand = null;
        public ICommand PerformShortcutCommand {
            get {
                if(_performShortcutCommand == null) {
                    _performShortcutCommand = new RelayCommand(PeformShortcut, CanPerformShortcut);
                }
                return _performShortcutCommand;
            }
        }
        private bool CanPerformShortcut() {
            //never perform shortcuts in the following states
            if(MpAssignShortcutModalWindowViewModel.IsOpen ||
               MpSettingsWindowViewModel.IsOpen ||
               MpClipTrayViewModel.Instance.IsPastingTemplate ||
               MpClipTrayViewModel.Instance.IsEditingClipTile ||
               MpClipTrayViewModel.Instance.IsEditingClipTitle ||
               MainWindowViewModel.TagTrayViewModel.IsEditingTagName ||
               MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused) {
                return false;
            }
            //otherwise check basic type routing for validity
            if(RoutingType == MpRoutingType.Internal) {
                return MpMainWindowViewModel.IsMainWindowOpen;
            } else {
                return !MpMainWindowViewModel.IsMainWindowOpen;
            }
        }
        private void PeformShortcut() {
            Command?.Execute(CommandParameter);
        }
        #endregion
    }
}
