using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; 

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
