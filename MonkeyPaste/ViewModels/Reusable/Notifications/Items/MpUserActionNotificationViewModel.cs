using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpUserActionNotificationViewModel : MpNotificationViewModelBase {
        #region Properties

        #region State

        public bool ShowIgnoreButton => ExceptionType != MpNotificationExceptionSeverityType.ErrorAndShutdown;

        public bool ShowRetryButton => ExceptionType != MpNotificationExceptionSeverityType.ErrorAndShutdown;

        public bool ShowShutdownButton => ExceptionType != MpNotificationExceptionSeverityType.None;

        #endregion

        #region Model

        #endregion

        #endregion
        #region Constructors

        public MpUserActionNotificationViewModel() : base(null) { }

        public MpUserActionNotificationViewModel(MpNotificationBalloonViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        #endregion
    }
}
