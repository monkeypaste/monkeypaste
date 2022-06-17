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
        Fix,
        Shutdown
    }
    public class MpUserActionNotificationViewModel : MpNotificationViewModelBase {
        #region Properties

        #region State

        public bool IsFixing { get; set; } = false;

        public bool CanFix => FixCommand != null && FixCommandArgs != null;

        public bool ShowFixButton => CanFix && !IsFixing;

        public bool ShowRetryButton => IsFixing;

        public bool ShowShutdownButton => !CanFix;
        public MpDialogResultType DialogResult { get; private set; }

        #endregion

        #region Model

        public object FixCommandArgs { get; set; }

        public ICommand FixCommand { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpUserActionNotificationViewModel() : base(null) { }

        public MpUserActionNotificationViewModel(MpNotificationCollectionViewModel parent) : base(parent) { }

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
