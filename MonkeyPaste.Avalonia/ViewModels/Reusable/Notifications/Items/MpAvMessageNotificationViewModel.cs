using MonkeyPaste.Common;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvMessageNotificationViewModel : MpAvNotificationViewModelBase {
        #region Properties

        #region State

        public bool IsOpenAnimated =>
            OpenStartX.HasValue;

        #endregion

        #region Layout

        public int? OpenStartX { get; set; }
        public int? OpenEndX { get; set; }

        #endregion

        public override bool CanPin => true;

        #endregion

        #region Constructors
        public MpAvMessageNotificationViewModel() : base() { }
        #endregion

        #region Public Methods

        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            var base_result = BeginShow();
            if (base_result == MpNotificationDialogResultType.DoNotShow) {
                return base_result;
            }

            if (MaxShowTimeMs > 0) {
                await WaitForFullVisibilityAsync();
                DateTime startTime = DateTime.Now;
                while (DateTime.Now - startTime <= TimeSpan.FromMilliseconds(MaxShowTimeMs)) {
                    await Task.Delay(100);

                    while (IsFadeDelayFrozen) {
                        startTime = DateTime.Now;
                        await Task.Delay(100);
                        if (DoNotShowAgain) {
                            return MpNotificationDialogResultType.DoNotShow;
                        }
                    }
                }
                HideNotification();
            }
            return MpNotificationDialogResultType.None;
        }

        public override void HideNotification(bool force = false) {
            if (IsPinned && !force) {
                // ignore until unpinned
                return;
            }
            base.HideNotification(force);
        }
        #endregion
    }
}
