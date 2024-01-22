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
            ClipboardPresetViewModel == null ?
                    false :
                    ClipboardPresetViewModel.IsReader;

        public bool IsWriterAppPreset =>
           ClipboardPresetViewModel == null ?
                    false :
                    ClipboardPresetViewModel.IsWriter;
        #endregion

        #region Appearance

        #endregion

        #region Model

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
