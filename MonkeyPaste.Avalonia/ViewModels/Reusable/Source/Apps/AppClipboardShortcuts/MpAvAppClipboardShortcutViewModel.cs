using Avalonia.Threading;
using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppClipboardShortcutViewModel :
        MpAvViewModelBase<MpAvAppViewModel>,
        MpAvIKeyGestureViewModel {

        #region Interfaces


        #region MpAvIKeyGestureViewModel Implementation

        private ObservableCollection<MpAvShortcutKeyGroupViewModel> _keyGroups;
        public ObservableCollection<MpAvShortcutKeyGroupViewModel> KeyGroups =>
           _keyGroups;

        #endregion

        #endregion

        #region Properties

        #region State

        public bool IsCopyShortcut { get; set; }
        public bool HasShortcut =>
            !string.IsNullOrEmpty(ShortcutCmdKeyString) && ClipboardShortcutsId > 0;

        #endregion

        #region Model

        public string ShortcutCmdKeyString {
            get {
                if (ClipboardShortcuts == null) {
                    return string.Empty;
                }
                return IsCopyShortcut ?
                    ClipboardShortcuts.CopyCmdKeyString :
                    ClipboardShortcuts.PasteCmdKeyString;
            }
            set {
                if (ShortcutCmdKeyString != value) {
                    if (IsCopyShortcut) {
                        ClipboardShortcuts.CopyCmdKeyString = value;
                    } else {
                        ClipboardShortcuts.PasteCmdKeyString = value;
                    }

                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ShortcutCmdKeyString));
                }
            }
        }

        public int AppId {
            get {
                if (ClipboardShortcuts == null) {
                    return 0;
                }
                return ClipboardShortcuts.AppId;
            }
        }
        public int ClipboardShortcutsId {
            get {
                if (ClipboardShortcuts == null) {
                    return 0;
                }
                return ClipboardShortcuts.Id;
            }
        }
        public MpAppClipboardShortcuts ClipboardShortcuts { get; set; }

        #endregion

        #endregion

        #region Constructors
        public MpAvAppClipboardShortcutViewModel() : base(null) { }

        public MpAvAppClipboardShortcutViewModel(MpAvAppViewModel parent, bool isCopyShortcut) : base(parent) {
            PropertyChanged += MpPasteShortcutViewModel_PropertyChanged;
            IsCopyShortcut = isCopyShortcut;
        }


        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpAppClipboardShortcuts aps) {
            IsBusy = true;

            await Task.Delay(1);
            ClipboardShortcuts = aps;
            if (ClipboardShortcuts == null) {
                if (_keyGroups != null) {
                    _keyGroups.Clear();
                    _keyGroups = null;
                }
            } else {
                if (_keyGroups == null) {
                    _keyGroups = new ObservableCollection<MpAvShortcutKeyGroupViewModel>();
                }
                _keyGroups.Clear();
                var kivml = ShortcutCmdKeyString.ToKeyItems();
                foreach (var kivm in kivml) {
                    _keyGroups.Add(kivm);
                }
            }
            OnPropertyChanged(nameof(KeyGroups));

            IsBusy = false;
        }

        public async Task<bool> ShowAssignDialogAsync() {
            // returns false if canceled or unattached
            if (Parent == null) {
                return false;
            }
            var result_tuple = await MpAvAssignShortcutViewModel.ShowAssignShortcutDialog(
                    shortcutName: $"Record {(IsCopyShortcut ? "copy" : "paste")} shortcut for '{Parent.AppName}'",
                    keys: ShortcutCmdKeyString,
                    curShortcutId: 0,
                    assignmentType: IsCopyShortcut ? MpShortcutAssignmentType.AppCopy : MpShortcutAssignmentType.AppPaste,
                    iconResourceObj: Parent.IconId);

            if (result_tuple == null || result_tuple.Item1 == null) {
                // canceled
                return false;
            }
            if (ClipboardShortcuts == null) {
                ClipboardShortcuts = await MpAppClipboardShortcuts.CreateAsync(appId: Parent.AppId);
            }
            ShortcutCmdKeyString = result_tuple.Item1;
            while (IsBusy) {
                await Task.Delay(100);
            }
            await InitializeAsync(ClipboardShortcuts);
            return true;
        }

        #endregion

        #region Private Methods


        private void MpPasteShortcutViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        Dispatcher.UIThread.Post(async () => {
                            IsBusy = true;

                            if (string.IsNullOrWhiteSpace(ShortcutCmdKeyString) ||
                                ShortcutCmdKeyString.ToLowerInvariant() == Mp.Services.PlatformShorcuts.PasteKeys.ToLowerInvariant()) {
                                if (AppId > 0) {
                                    await ClipboardShortcuts.DeleteFromDatabaseAsync();
                                }
                            } else {
                                await ClipboardShortcuts.WriteToDatabaseAsync();
                            }
                            HasModelChanged = false;
                            IsBusy = false;

                        });
                    }
                    break;
                case nameof(KeyGroups):
                    Parent.OnPropertyChanged(nameof(KeyGroups));
                    break;
            }
        }

        #endregion

        #region Commands


        public ICommand RecordShortcutCommand => new MpAsyncCommand<object>(
            async (args) => {
                await ShowAssignDialogAsync();
            });
        #endregion
    }

}
