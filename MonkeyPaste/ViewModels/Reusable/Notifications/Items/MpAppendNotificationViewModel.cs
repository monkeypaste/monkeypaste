using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpAppendNotificationViewModel : MpMessageNotificationViewModel {
        #region Statics

        private static MpAppendNotificationViewModel _instance;
        public static MpAppendNotificationViewModel Instance => _instance;

        public static async Task<MpAppendNotificationViewModel> InitAsync(MpNotificationFormat nf) {
            _instance = new MpAppendNotificationViewModel();
            await _instance.InitializeAsync(nf);
            return _instance;
        }
        #endregion

        #region Properties

        #region State

        public bool IsExpanded { get; set; } = false;
        #endregion

        #region Model

        public override string Title {
            get {
                if (Body is MpIAppendTitleViewModel atvm) {
                    return atvm.AppendTitle;
                }
                return string.Empty;
            }
        }

        #endregion

        #endregion

        #region Constructors
        private MpAppendNotificationViewModel() : base() { }
        #endregion


        #region Public Methods
        #endregion
    }
}
