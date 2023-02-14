using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpPasteShortcutViewModel :
        MpViewModelBase<MpAvAppViewModel> {

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

        public bool EnterAfterPaste {
            get {
                if (PasteShortcut == null) {
                    return false;
                }
                return PasteShortcut.EnterAfterPaste;
            }
            set {
                if (EnterAfterPaste != value) {
                    PasteShortcut.EnterAfterPaste = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(EnterAfterPaste));
                }
            }
        }

        public MpAppPasteShortcut PasteShortcut { get; set; }

        #endregion

        #endregion

        #region Constructors
        public MpPasteShortcutViewModel() : base(null) { }

        public MpPasteShortcutViewModel(MpAvAppViewModel parent) : base(parent) {
            PropertyChanged += MpPasteShortcutViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpAppPasteShortcut aps) {
            IsBusy = true;

            await Task.Delay(1);
            PasteShortcut = aps;

            IsBusy = false;
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
    }
}
