using Avalonia.Threading;
using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvPasteShortcutViewModel :
        MpViewModelBase<MpAvAppViewModel>,
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

        public bool HasPasteShortcut =>
            !string.IsNullOrEmpty(PasteCmdKeyString) && PasteShortcutId > 0;
        #endregion

        #region Model

        public int PasteShortcutId {
            get {
                if (PasteShortcut == null) {
                    return 0;
                }
                return PasteShortcut.Id;
            }
        }
        public int AppId {
            get {
                if (PasteShortcut == null) {
                    return 0;
                }
                return PasteShortcut.AppId;
            }
        }
        public string PasteCmdKeyString {
            get {
                if (PasteShortcut == null) {
                    return string.Empty;
                }
                return PasteShortcut.PasteCmdKeyString;
            }
            set {
                if (PasteCmdKeyString != value) {
                    if (PasteShortcut == null) {
                        PasteShortcut = new MpAppPasteShortcut() {
                            Guid = System.Guid.NewGuid().ToString(),
                            AppId = Parent == null ? 0 : Parent.AppId,
                        };
                    }
                    PasteShortcut.PasteCmdKeyString = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(PasteCmdKeyString));
                }
            }
        }

        public MpAppPasteShortcut PasteShortcut { get; set; }

        #endregion

        #endregion

        #region Constructors
        public MpAvPasteShortcutViewModel() : base(null) { }

        public MpAvPasteShortcutViewModel(MpAvAppViewModel parent) : base(parent) {
            PropertyChanged += MpPasteShortcutViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpAppPasteShortcut aps) {
            IsBusy = true;

            await Task.Delay(1);
            PasteShortcut = aps;
            if (aps == null) {
                if (_keyGroups != null) {
                    _keyGroups.Clear();
                    _keyGroups = null;
                }
            } else {
                if (_keyGroups == null) {
                    _keyGroups = new ObservableCollection<MpAvShortcutKeyGroupViewModel>();
                }
                _keyGroups.Clear();
                var kivml = aps.PasteCmdKeyString.ToKeyItems();
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
                    shortcutName: $"Record paste shortcut for '{Parent.AppName}'",
                    keys: PasteCmdKeyString,
                    curShortcutId: 0,
                    assignmentType: MpShortcutAssignmentType.AppPaste, Parent.IconId,
                    owner: MpAvWindowManager.AllWindows.FirstOrDefault(x => x.DataContext == MpAvSettingsViewModel.Instance));

            if (result_tuple == null || result_tuple.Item1 == null) {
                // canceled
                return false;
            }

            PasteCmdKeyString = result_tuple.Item1;
            while (IsBusy) {
                await Task.Delay(100);
            }
            await InitializeAsync(PasteShortcut);
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

                            if (string.IsNullOrWhiteSpace(PasteCmdKeyString) ||
                                PasteCmdKeyString.ToLower() == Mp.Services.PlatformShorcuts.PasteKeys.ToLower()) {
                                if (AppId > 0) {
                                    await PasteShortcut.DeleteFromDatabaseAsync();
                                }
                            } else {
                                await PasteShortcut.WriteToDatabaseAsync();
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
        //public ICommand AssignPasteShortcutCommand => new MpCommand(
        //    () => {
        //        ShowAssignDialogAsync().FireAndForgetSafeAsync(this);
        //    });

        //public ICommand DeletePasteShortcutCommand => new MpAsyncCommand(
        //    async () => {
        //        var result = await Mp.Services.NativeMessageBox.ShowYesNoCancelMessageBoxAsync(
        //            title: $"Confirm",
        //            message: $"Are you sure want to remove the paste shortcut for '{Parent.AppName}'",
        //            iconResourceObj: Parent.IconId);
        //        if (result.IsNull()) {
        //            // canceled
        //            return;
        //        }
        //        PasteCmdKeyString = null;
        //    });
        #endregion
    }
}
