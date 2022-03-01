using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpNotificationViewModelBase : MpViewModelBase<MpNotificationBalloonViewModel> {
        #region Properties

        #region Appearance

        public string NotificationTextForegroundColor {
            get {
                if (ExceptionType == MpNotificationExceptionSeverityType.Warning ||
                    ExceptionType == MpNotificationExceptionSeverityType.WarningWithOption) {
                    return MpSystemColors.Yellow;
                }
                if (ExceptionType == MpNotificationExceptionSeverityType.ErrorAndShutdown ||
                    ExceptionType == MpNotificationExceptionSeverityType.ErrorWithOption) {
                    return MpSystemColors.Red;
                }
                if (ExceptionType != MpNotificationExceptionSeverityType.None) {
                    return MpSystemColors.royalblue;
                }
                return MpSystemColors.Black;
            }
        }

        #endregion

        #region State

        public bool IsVisible { get; set; } = false;

        #endregion

        #region Model

        public MpNotificationExceptionSeverityType ExceptionType {
            get {
                if(Notification == null) {
                    return MpNotificationExceptionSeverityType.None;
                }
                return Notification.SeverityType;
            }
        }

        public MpNotification Notification { get; set; }

        #endregion

        #endregion
        #region Constructors

        public MpNotificationViewModelBase() : base(null) { }

        public MpNotificationViewModelBase(MpNotificationBalloonViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpNotification notification) {
            IsBusy = true;

            await Task.Delay(1);
            Notification = notification;

            IsBusy = false;
        }

        #endregion
    }
}
