using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    public class MpUserActionNotificationViewModel : MpNotificationViewModelBase {
        #region Private Variables

        private Action<object> _retryAction;
        private object _retryActionObj;


        #endregion

        #region Properties

        #region State

        public bool IsFixing { get; set; } = false;

        public bool CanFix => FixCommand != null && FixCommandArgs != null;

        public bool ShowIgnoreButton { get; set; }
        public bool ShowFixButton => CanFix && !IsFixing;

        public bool ShowRetryButton => IsFixing;

        public bool ShowShutdownButton => ShowIgnoreButton && !CanFix;

        public bool ShowYesButton { get; set; } = false;
        public bool ShowNoButton { get; set; } = false;
        public bool ShowCancelButton { get; set; } = false;
        public bool ShowOkButton { get; set; } = false;
        public MpNotificationDialogResultType DialogResult { get; private set; }

        #endregion

        #region Model

        public object FixCommandArgs { get; set; }

        public ICommand FixCommand { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpUserActionNotificationViewModel() : base() { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpNotificationFormat nf, object nfArgs) {
            IsBusy = true;
            if(string.IsNullOrEmpty(nf.IconSourceStr)) {
                if(IsErrorNotification) {
                    nf.IconSourceStr = MpBase64Images.Error;
                } else if(IsWarningNotification) {
                    nf.IconSourceStr = MpBase64Images.Warning;
                } else {
                    nf.IconSourceStr = MpBase64Images.QuestionMark;
                }
            }
            await base.InitializeAsync(nf, nfArgs);

            if(nfArgs is Object[] argParts) {
                if(argParts.Length > 0) {
                    if (argParts[0] is Object[] retryParts) {
                        _retryAction = retryParts[0] as Action<object>;
                        _retryActionObj = retryParts[1];
                    }
                    if (argParts[1] is object[] fixParts) {
                        FixCommand = fixParts[0] as ICommand;
                        FixCommandArgs = fixParts[1];
                    }
                }
            }
            switch(ButtonsType) {
                case MpNotificationButtonsType.YesNoCancel:
                    ShowYesButton = true;
                    ShowNoButton = true;
                    ShowCancelButton = true;
                    break;
                case MpNotificationButtonsType.Ok:
                    ShowOkButton = true;
                    break;
                case MpNotificationButtonsType.OkCancel:
                    ShowOkButton = true;
                    ShowCancelButton = true;
                    break;
                case MpNotificationButtonsType.IgnoreRetryShutdown:
                case MpNotificationButtonsType.IgnoreRetryFix:
                    ShowIgnoreButton = true;
                    // others based on args (fix,retry non-null)
                    break;
            }
            IsBusy = false;
        }

        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            ShowBalloon();
            while (DialogResult == MpNotificationDialogResultType.None) {
                await Task.Delay(100);
            }
            if (DialogResult == MpNotificationDialogResultType.Retry) {
                _retryAction?.Invoke(_retryActionObj);
            } else {
                HideNotification();
            }
            return DialogResult;
        }

        #endregion

        #region Commands

        public ICommand IgnoreCommand => new MpCommand(
            () => {
                DialogResult = MpNotificationDialogResultType.Ignore;                
            });

        public ICommand RetryCommand => new MpCommand(
            () => {
                DialogResult = MpNotificationDialogResultType.Retry;
            });


        public ICommand ShutdownCommand => new MpCommand(
            () => {
                DialogResult = MpNotificationDialogResultType.Shutdown;
                // TODO should have global shutdown workflow instead of just shutting down maybe
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            });

        public ICommand YesCommand => new MpCommand(
            () => {
                DialogResult = MpNotificationDialogResultType.Yes;
            });
        public ICommand NoCommand => new MpCommand(
            () => {
                DialogResult = MpNotificationDialogResultType.No;
            });
        public ICommand CancelCommand => new MpCommand(
            () => {
                DialogResult = MpNotificationDialogResultType.Cancel;
            });

        public ICommand OkCommand => new MpCommand(
            () => {
                DialogResult = MpNotificationDialogResultType.Ok;
            });

        #endregion
    }
}
