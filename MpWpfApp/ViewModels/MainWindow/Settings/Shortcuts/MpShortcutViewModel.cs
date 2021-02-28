using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using MouseKeyHook.Rx;

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
                                outStr += "{" + key.ToUpper() + "}";
                                break;
                            default:
                                outStr += key.ToUpper();
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
                    OnPropertyChanged(nameof(ShortcutId));
                }
            }
        }

        public List<List<Key>> KeyList {
            get {
                if(Shortcut == null) {
                    return new List<List<Key>>();
                }
                return Shortcut.KeyList;
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
                                var ctvm = MainWindowViewModel.ClipTrayViewModel.GetClipTileByCopyItemId(Shortcut.CopyItemId);
                                ctvm.ShortcutKeyString = Shortcut.KeyString;
                            } else {
                                var ttvm = MainWindowViewModel.TagTrayViewModel.Where(x => x.Tag.TagId == Shortcut.TagId).Single();
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
                    var hook = RoutingType == MpRoutingType.Internal ? MainWindowViewModel.ApplicationHook : MainWindowViewModel.GlobalHook;

                    if (IsSequence()) {
                        if(MainWindowViewModel.IsLoading) {
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
                    Console.WriteLine("Error creating shortcut: " + ex.ToString());
                    return;
                }
                Shortcut.WriteToDatabase();
                Console.WriteLine("Shortcut Successfully registered for '" + ShortcutDisplayName + "' with hotkeys: " + KeyString);
                return;
            }
        }

        public void Unregister() {
            if(KeysObservable != null) {
                KeysObservable.Dispose();
                Console.WriteLine("Unregistering shortcut " + Shortcut.ToString() + " was successful");
            } else {
                //either not previously registered or a sequence that won't be unregistered until app shutdown
            }
        }

        public void PassKeysToForegroundWindow() {
            var mwvm = (MpMainWindowViewModel)System.Windows.Application.Current.MainWindow.DataContext;
            WinApi.SetForegroundWindow(MpClipboardManager.Instance.LastWindowWatcher.LastHandle);
            foreach (var str in SendKeysKeyStringList) {
                System.Windows.Forms.SendKeys.SendWait(str);
            }
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
               MainWindowViewModel.ClipTrayViewModel.IsPastingTemplate ||
               MainWindowViewModel.ClipTrayViewModel.IsEditingClipTile ||
               MainWindowViewModel.ClipTrayViewModel.IsEditingClipTitle ||
               MainWindowViewModel.TagTrayViewModel.IsEditingTagName ||
               MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused) {
                return false;
            }
            //otherwise check basic type routing for validity
            if(RoutingType == MpRoutingType.Internal) {
                return MpMainWindowViewModel.IsOpen;
            } else {
                return !MpMainWindowViewModel.IsOpen;
            }
        }
        private void PeformShortcut() {
            if (RoutingType == MpRoutingType.Bubble) {
                PassKeysToForegroundWindow();
            }
                        
            Command?.Execute(CommandParameter);

            if (RoutingType == MpRoutingType.Tunnel) {
                PassKeysToForegroundWindow();
            }
        }
        #endregion
    }
}
