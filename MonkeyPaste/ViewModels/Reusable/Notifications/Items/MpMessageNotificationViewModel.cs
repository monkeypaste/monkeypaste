using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpMessageNotificationViewModel : MpNotificationViewModelBase {
        #region Constructors
        public MpMessageNotificationViewModel() : base() { }
        #endregion

        #region Public Methods

        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            await base.ShowNotificationAsync();
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
