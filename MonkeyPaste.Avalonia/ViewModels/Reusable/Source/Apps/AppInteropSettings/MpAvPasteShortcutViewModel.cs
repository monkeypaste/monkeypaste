using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
           // new ObservableCollection<MpAvShortcutKeyGroupViewModel>(PasteCmdKeyString.ToKeyItems());
           _keyGroups;

        #endregion

        #endregion

        #region Properties

        #region Model

        // Arg2
        public string PasteCmdKeyString {
            get {
                if (PasteShortcut == null) {
                    return string.Empty;
                }
                return PasteShortcut.PasteCmdKeyString;
            }
            set {
                if (PasteCmdKeyString != value) {
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
                _keyGroups.AddRange(aps.PasteCmdKeyString.ToKeyItems());
            }
            OnPropertyChanged(nameof(KeyGroups));

            IsBusy = false;
        }

        public async Task ShowAssignDialogAsync() {
            if (Parent == null) {
                return;
            }
            string shortcutKeyString = await MpAvAssignShortcutViewModel.ShowAssignShortcutDialog(
                    $"Record paste shortcut for '{Parent.AppName}'",
                    PasteCmdKeyString,
                    0,
                    MpShortcutAssignmentType.AppPaste, Parent.IconId,
                    MpAvWindowManager.AllWindows.FirstOrDefault(x => x.DataContext == MpAvSettingsViewModel.Instance));

            if (shortcutKeyString == null) {
                // canceled
                return;
            }
            if (shortcutKeyString == string.Empty ||
                shortcutKeyString.ToLower() == Mp.Services.PlatformShorcuts.PasteKeys.ToLower()) {
                // cleared or os default
                if (PasteShortcut.Id > 0) {
                    await PasteShortcut.DeleteFromDatabaseAsync();
                }
                InitializeAsync(null).FireAndForgetSafeAsync(this);
                return;
            }
            PasteCmdKeyString = shortcutKeyString;
            InitializeAsync(PasteShortcut).FireAndForgetSafeAsync(this);
        }
        #endregion

        #region Private Methods


        private void MpPasteShortcutViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        Task.Run(async () => {
                            await PasteShortcut.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
            }
        }



        #endregion

        #region Commands
        //public ICommand AssignPasteShortcutCommand => new MpCommand(
        //    () => {
        //        ShowAssignDialogAsync().FireAndForgetSafeAsync(this);
        //    });
        #endregion
    }
}
