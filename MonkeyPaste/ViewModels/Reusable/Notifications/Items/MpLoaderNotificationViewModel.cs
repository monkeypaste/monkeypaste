using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpLoaderNotificationViewModel : MpNotificationViewModelBase {
        #region Private Variables
        #endregion

        #region Properties

        #region Appearance

        public double ProgressTotalBarWidth { get; set; }

        public double ProgressBarCurrentWidth {
            get {
                if (ProgressLoader == null) {
                    return 0;
                }
                return ProgressTotalBarWidth * ProgressLoader.PercentLoaded;
            }
        }
        #endregion

        #region State

        public double ValueLoaded {
            get {
                if (ProgressLoader == null) {
                    return 0;
                }
                return ProgressLoader.PercentLoaded * 100;
            }
        }
        #endregion

        #region Model

        public override MpNotificationType NotificationType {
            get {
                if (ProgressLoader == null) {
                    return MpNotificationType.None;
                }
                return ProgressLoader.DialogType;
            }
        }

        public MpIProgressLoaderViewModel ProgressLoader {
            get {
                if (NotificationFormat == null) {
                    return null;
                }
                return NotificationFormat.OtherArgs as MpIProgressLoaderViewModel;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpLoaderNotificationViewModel() : base() { }

        #endregion

        #region Public Methods


        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            var base_result = await base.ShowNotificationAsync();
            if (base_result == MpNotificationDialogResultType.DoNotShow) {
                // when loader is DoNotShow base never shows it (and StartLoader is called from window)
                // so manually perform load
                await ProgressLoader.BeginLoaderAsync();
                await ProgressLoader.FinishLoaderAsync();
                return base_result;
            }
            // NOTE returning loading notifies builder not to hide loader
            return MpNotificationDialogResultType.Loading;
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
