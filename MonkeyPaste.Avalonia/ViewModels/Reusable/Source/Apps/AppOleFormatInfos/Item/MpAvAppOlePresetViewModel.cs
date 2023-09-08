using MonkeyPaste.Common;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOlePresetViewModel :
        MpAvViewModelBase<MpAvAppOleFormatInfoCollectionViewModel> {

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public MpAvClipboardFormatPresetViewModel ClipboardPresetViewModel =>
            MpAvClipboardHandlerCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.PresetId == PresetId);

        #endregion

        #region State
        public bool IsReaderAppPreset =>
            IsReaderNoOp ?
                true :
                ClipboardPresetViewModel == null ?
                    false :
                    ClipboardPresetViewModel.IsReader;

        public bool IsWriterAppPreset =>
            IsWriterNoOp ?
                true :
                ClipboardPresetViewModel == null ?
                    false :
                    ClipboardPresetViewModel.IsWriter;

        public bool IsNoOpReaderOrWriter =>
            IsReaderNoOp || IsWriterNoOp;
        #endregion

        #region Appearance

        #endregion

        #region Model

        public bool IsReaderNoOp {
            get {
                if (AppOlePreset == null) {
                    return false;
                }
                return AppOlePreset.IsNoOpReader;
            }
        }
        public bool IsWriterNoOp {
            get {
                if (AppOlePreset == null) {
                    return false;
                }
                return AppOlePreset.IsNoOpWriter;
            }
        }
        public int PresetId {
            get {
                if (AppOlePreset == null) {
                    return 0;
                }
                return AppOlePreset.PresetId;
            }
        }

        public int AppId {
            get {
                if (AppOlePreset == null) {
                    return 0;
                }
                return AppOlePreset.AppId;
            }
        }

        public int AppOlePresetId {
            get {
                if (AppOlePreset == null) {
                    return 0;
                }
                return AppOlePreset.Id;
            }
        }
        public MpAppOlePreset AppOlePreset { get; set; }

        #endregion

        #endregion

        #region Constructors
        public MpAvAppOlePresetViewModel() : base(null) { }

        public MpAvAppOlePresetViewModel(MpAvAppOleFormatInfoCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAppClipboardFormatInfoViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpAppOlePreset ais) {
            IsBusy = true;

            await Task.Delay(1);
            AppOlePreset = ais;

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpAppClipboardFormatInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        if (AppId < 0) {
                            MpDebug.Break("Trying to set non-app specific clipboard format, ignoring");
                            break;
                        }
                        Task.Run(async () => {
                            await AppOlePreset.WriteToDatabaseAsync();
                            HasModelChanged = false;

                        });
                    }
                    break;
            }
        }
        #endregion
    }
}
