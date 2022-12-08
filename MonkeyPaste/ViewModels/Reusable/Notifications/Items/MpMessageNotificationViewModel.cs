using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpMessageNotificationViewModel : MpNotificationViewModelBase {
        #region Properties

        public override int MaxShowTimeMs => DEFAULT_NOTIFICATION_SHOWTIME_MS;

        #endregion

        #region Constructors
        public MpMessageNotificationViewModel() : base() { }
        #endregion

        #region Public Methods

        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            var base_result = await base.ShowNotificationAsync();
            if(base_result == MpNotificationDialogResultType.DoNotShow) {
                HideNotification();
                return base_result;
            }

            if (MaxShowTimeMs > 0) {
                DateTime startTime = DateTime.Now;
                while (DateTime.Now - startTime <= TimeSpan.FromMilliseconds(MaxShowTimeMs)) {
                    await Task.Delay(100);

                    while (IsHovering) {
                        await Task.Delay(100);
                        if(DoNotShowAgain) {
                            return MpNotificationDialogResultType.DoNotShow;
                        }
                    }
                }
                HideNotification();
            }
            return MpNotificationDialogResultType.None;
        }
        #endregion
    }
}
