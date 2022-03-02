using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    public enum MpDialogResultType {
        None = 0,
        Yes,
        No,
        Cancel,
        Ignore,
        Retry,
        Shutdown
    }

    public class MpUserActionNotificationViewModel : MpNotificationViewModelBase {
        #region Properties

        #region State

        public bool ShowIgnoreButton => ExceptionType != MpNotificationExceptionSeverityType.ErrorAndShutdown;

        public bool ShowRetryButton => ExceptionType != MpNotificationExceptionSeverityType.ErrorAndShutdown;

        public bool ShowShutdownButton => ExceptionType != MpNotificationExceptionSeverityType.None;

        public MpDialogResultType DialogResult { get; private set; }

        #endregion

        #region Model

        public override string IconImageBase64 {
            get {
                switch(ExceptionType) {
                    case MpNotificationExceptionSeverityType.Error:
                    case MpNotificationExceptionSeverityType.ErrorAndShutdown:
                    case MpNotificationExceptionSeverityType.ErrorWithOption:
                        return MpBase64Images.Error;
                    case MpNotificationExceptionSeverityType.Warning:
                    case MpNotificationExceptionSeverityType.WarningWithOption:
                        return MpBase64Images.Warning;
                    default:
                        return MpBase64Images.AppIcon;
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpUserActionNotificationViewModel() : base(null) { }

        public MpUserActionNotificationViewModel(MpNotificationBalloonViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        #endregion


        #region Commands

        public ICommand IgnoreCommand => new MpCommand(
            () => {
                DialogResult = MpDialogResultType.Ignore;
            });

        public ICommand RetryCommand => new MpCommand(
            () => {
                DialogResult = MpDialogResultType.Retry;
            });

        public ICommand ShutdownCommand => new MpCommand(
            () => {
                DialogResult = MpDialogResultType.Shutdown;
                // TODO should have global shutdown workflow instead of just shutting down maybe
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            });

        #endregion
    }
}
