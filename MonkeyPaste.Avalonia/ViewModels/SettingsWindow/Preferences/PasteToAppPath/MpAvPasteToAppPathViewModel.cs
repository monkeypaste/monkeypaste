using MonkeyPaste.Common;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using static MonkeyPaste.Common.Wpf.WinApi;

namespace MonkeyPaste.Avalonia {
    public class MpAvPasteToAppPathViewModel :
        MpViewModelBase<MpAvPasteToAppPathViewModelCollection>,
        MpIUserIconViewModel,
        MpIMenuItemViewModel,
        MpISelectableViewModel {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                return new MpMenuItemViewModel() {
                    Header = IsRuntime ? Label + (IsAdmin ? " (Admin)" : string.Empty) : Label,
                    IconId = this.IconId,
                    Command = MpAvClipTrayViewModel.Instance.PasteSelectedClipTileFromContextMenuCommand,
                    CommandParameter = Handle,
                    IsPasteToPathRuntimeItem = IsRuntime,
                    ShortcutArgs = new object[] { MpShortcutType.PasteSelectedItems },
                    IsVisible = !IsHidden
                };
            }
        }
        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region MpIMatcherTriggerViewModel Implementation

        #region MpIUserIcon Implementation

        public async Task<MpIcon> GetIcon() {
            var icon = await MpDataModelProvider.GetItemAsync<MpIcon>(IconId);
            return icon;
        }

        public ICommand SetIconCommand => new MpCommand<object>(
            async (args) => {
                IconId = (args as MpIcon).Id;
                await PasteToAppPath.WriteToDatabaseAsync();

            });
        #endregion

        public void RegisterMatcher(MpAvActionViewModelBase mvm) {
            //AddWatcher(mvm.MatchData, mvm);
            MpConsole.WriteLine($"FileSystemWatcher Registered {mvm.Label} matcher");
        }

        public void UnregisterMatcher(MpAvActionViewModelBase mvm) {
            //RemoveWatcher(mvm.MatchData);
            MpConsole.WriteLine($"FileSystemWatcher Unregistered {mvm.Label} matcher");
        }

        public ObservableCollection<MpAvActionViewModelBase> MatcherViewModels => null;// new ObservableCollection<MpMatcherViewModel>(
                                                                                       //MpMatcherCollectionViewModel.Instance.Matchers.Where(x =>
                                                                                       //    x.Matcher.MatcherTriggerType == MpMatcherTriggerType.WatchFileChanged ||
                                                                                       //     x.Matcher.MatcherTriggerType == MpMatcherTriggerType.WatchFolderChange).ToList());

        #endregion

        #region Appearance
        public Brush PasteToAppPathDataRowBorderBrush {
            get {
                if (IsValid) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
            }
        }
        #endregion

        #region State

        private int _selectedWindowState = -1;
        public int SelectedWindowState {
            get {
                if (PasteToAppPath == null) {
                    return 0;
                }
                if (_selectedWindowState < 0) {
                    _selectedWindowState = (int)PasteToAppPath.WindowState;
                }
                return _selectedWindowState;
            }
            set {
                if (_selectedWindowState != value) {
                    _selectedWindowState = value;
                    WindowState = (ShowWindowCommands)_selectedWindowState;
                    OnPropertyChanged(nameof(SelectedWindowState));
                }
            }
        }


        public bool IsReadOnly {
            get {
                return !IsSelected;
            }
        }

        private bool _isValid = false;
        public bool IsValid {
            get {
                return _isValid;
            }
            set {
                if (_isValid != value) {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                    OnPropertyChanged(nameof(PasteToAppPathDataRowBorderBrush));
                }
            }
        }

        public bool IsRuntime => Handle != IntPtr.Zero;

        public bool IsHidden => IsRuntime && Parent.HiddenHandles.Contains(Handle);

        public IntPtr Handle { get; set; } = IntPtr.Zero;
        #endregion

        #region Model Properties

        public bool PressEnter {
            get {
                if (PasteToAppPath == null) {
                    return false;
                }
                return PasteToAppPath.PressEnter;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.PressEnter != value) {
                    PasteToAppPath.PressEnter = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(PressEnter));
                }
            }
        }

        public ShowWindowCommands WindowState {
            get {
                if (PasteToAppPath == null) {
                    return ShowWindowCommands.Normal;
                }
                return (ShowWindowCommands)PasteToAppPath.WindowState;
            }
            set {
                if (PasteToAppPath.WindowState != (int)value) {
                    PasteToAppPath.WindowState = (int)value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(WindowState));
                }
            }
        }
        public int IconId {
            get {
                if (PasteToAppPath == null) {
                    return 0;
                }
                return PasteToAppPath.IconId;
            }
            set {
                if (IconId != value) {
                    PasteToAppPath.IconId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IconId));
                }
            }
        }

        public string Args {
            get {
                if (PasteToAppPath == null) {
                    return string.Empty;
                }
                if (PasteToAppPath.Args == null) {
                    return string.Empty;
                }
                return PasteToAppPath.Args;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.Args != value) {
                    PasteToAppPath.Args = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Args));
                }
            }
        }

        public string Label {
            get {
                if (PasteToAppPath == null) {
                    return string.Empty;
                }
                //if(string.IsNullOrEmpty(PasteToAppPath.Label)) {
                //    return AppName;
                //}
                return PasteToAppPath.Label;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.Label != value) {
                    PasteToAppPath.Label = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public bool IsSilent {
            get {
                if (PasteToAppPath == null) {
                    return false;
                }
                return PasteToAppPath.IsSilent;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.IsSilent != value) {
                    PasteToAppPath.IsSilent = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsSilent));
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }

        public bool IsAdmin {
            get {
                if (PasteToAppPath == null) {
                    return false;
                }
                return PasteToAppPath.IsAdmin;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.IsAdmin != value) {
                    PasteToAppPath.IsAdmin = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsAdmin));
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }

        public string AppName {
            get {
                if (PasteToAppPath == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(PasteToAppPath.AppName)) {
                    return Path.GetFileName(PasteToAppPath.AppPath) + (IsAdmin ? " (Admin)" : string.Empty) + (IsSilent ? " (Silent)" : string.Empty);
                }
                return PasteToAppPath.AppName + (IsAdmin ? " (Admin)" : string.Empty) + (IsSilent ? " (Silent)" : string.Empty);
            }
            set {
                if (PasteToAppPath.AppName != value && PasteToAppPath.AppPath != value) {
                    PasteToAppPath.AppName = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }

        public string AppPath {
            get {
                if (PasteToAppPath == null) {
                    return String.Empty;
                }
                return PasteToAppPath.AppPath;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.AppPath != value) {
                    PasteToAppPath.AppPath = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(AppPath));
                }
            }
        }

        public int PasteToAppPathId {
            get {
                if (PasteToAppPath == null) {
                    return 0;
                }
                return PasteToAppPath.PasteToAppPathId;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.PasteToAppPathId != value) {
                    PasteToAppPath.PasteToAppPathId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(PasteToAppPathId));
                }
            }
        }

        public MpPasteToAppPath PasteToAppPath { get; set; }
        #endregion

        #endregion

        #region Public Methods
        public MpAvPasteToAppPathViewModel() : base(null) { }

        public MpAvPasteToAppPathViewModel(MpAvPasteToAppPathViewModelCollection parent, MpPasteToAppPath pasteToAppPath) : base(parent) {
            //constructor used for user defined paste to applications
            PropertyChanged += MpPasteToAppPathViewModel_PropertyChanged;
            PasteToAppPath = pasteToAppPath;
        }

        public MpAvPasteToAppPathViewModel(MpAvPasteToAppPathViewModelCollection parent, MpPasteToAppPath pasteToAppPath, IntPtr handle) : this(parent, pasteToAppPath) {
            //constructor used for running applications
            Handle = handle;
        }

        private void MpPasteToAppPathViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasModelChanged):
                    if (HasModelChanged) {

                        Task.Run(async () => {
                            await PasteToAppPath.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    break;
            }
        }

        public string Validate() {
            if (string.IsNullOrEmpty(Args)) {
                return string.Empty;
            }
            if (Args.Length > MpPrefViewModel.Instance.MaxCommandLineArgumentLength) {
                return @"Max length of Commandline args is " + MpPrefViewModel.Instance.MaxCommandLineArgumentLength + " this is " + Args.Length;
            }
            return string.Empty;
        }

        #endregion

        #region Commands

        #endregion
    }
}
