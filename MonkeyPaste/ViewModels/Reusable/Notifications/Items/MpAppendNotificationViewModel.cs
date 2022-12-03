using System;
using System.Reflection;
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

        #region Constructors
        private MpAppendNotificationViewModel() : base() { }
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion
    }
}
