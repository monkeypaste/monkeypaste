using MonkeyPaste;

namespace MpWpfApp {
    public class MpTooltipInfoCollectionViewModel : MpViewModelBase,
        MpISingletonViewModel<MpTooltipInfoCollectionViewModel> {

        #region Statics

        private static MpTooltipInfoCollectionViewModel _instance;
        public static MpTooltipInfoCollectionViewModel Instance => _instance ?? (_instance = new MpTooltipInfoCollectionViewModel());

        #endregion

        #region Properties

        #region View Models
        public MpDateTimeFormatInfoViewModel DateTimeFormatInfoViewModel { get; private set; }

        public MpContactFieldInfoViewModel ContactFieldInfoViewModel { get; private set; }

        public MpClipboardFormatInfoViewModel ClipboardFormatInfoViewModel { get; private set; }
        #endregion

        #endregion

        #region Public Methods
        public void Init() {
            DateTimeFormatInfoViewModel = new MpDateTimeFormatInfoViewModel();
            ContactFieldInfoViewModel = new MpContactFieldInfoViewModel();
            ClipboardFormatInfoViewModel = new MpClipboardFormatInfoViewModel();
        }

        #endregion

    }
}
