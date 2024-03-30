using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvUserActionNotificationViewModel : MpAvNotificationViewModelBase {
        #region Private Variables
        bool _wasIgnoreHiddenToFix = false;
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region OtherArgs
        public MpIProgressIndicatorViewModel ProgressIndicatorViewModel {
            get {
                if (OtherArgs is MpIProgressIndicatorViewModel pivm) {
                    return pivm;
                }
                if (OtherArgs is object[] argParts && argParts.OfType<MpIProgressIndicatorViewModel>().FirstOrDefault() is { } pivm2) {
                    return pivm2;
                }
                return null;
            }
        }

        public CancellationToken? BusyCancellationToken {
            get {
                if (ProgressIndicatorViewModel is MpICancelableProgressIndicatorViewModel cpivm) {
                    return cpivm.CancellationToken;
                }
                if (OtherArgs is CancellationToken ct) {
                    return ct;
                }
                if (OtherArgs is object[] argParts && argParts.OfType<CancellationToken>().FirstOrDefault() is { } ct2) {
                    return ct2;
                }
                return null;
            }
        }
        bool? OtherBoolArg {
            get {
                if (OtherArgs is bool boolArg) {
                    return boolArg;
                }
                if (OtherArgs is not object[] argParts) {
                    return default;
                }
                return argParts.OfType<bool>().FirstOrDefault();
            }
        }
        #endregion

        #region State
        public bool HasParams =>
            Body is MpAvAnalyticItemPresetViewModel;
        public override bool CanPin => true;

        public override bool ShowOptionsButton =>
            NotificationType == MpNotificationType.AlertAction ||
            NotificationType == MpNotificationType.UpdateAvailable ||
            NotificationType == MpNotificationType.ContentCapReached ||
            NotificationType == MpNotificationType.TrashCapReached ||
            NotificationType == MpNotificationType.ContentAddBlockedByAccount ||
            NotificationType == MpNotificationType.ContentRestoreBlockedByAccount ||
            NotificationType == MpNotificationType.ConfirmEndAppend ||
            NotificationType == MpNotificationType.AppendModeChanged ||
            NotificationType == MpNotificationType.ModalContentFormatDegradation;

        public bool IsFixing { get; set; } = false;

        public bool CanFix => FixCommand != null && FixCommand.CanExecute(FixCommandArgs);

        public bool ShowUpdateButton { get; set; }
        public bool ShowIgnoreButton { get; set; }
        public bool ShowFixButton => CanFix && !IsFixing;
        public bool ShowSubmitButton { get; set; }
        public bool ShowRetryButton => IsFixing && RetryAction != null;
        public bool ShowDeleteButton { get; set; }
        public bool ShowRestartButton { get; set; }
        public bool ShowShutdownButton { get; set; }
        public bool ShowRestartNowButton { get; set; }
        public bool ShowLaterButton { get; set; }

        public bool ShowYesButton { get; set; }
        public bool ShowNoButton { get; set; }
        public bool ShowCancelButton { get; set; }
        public bool ShowResetPresetButtons { get; set; }
        public bool ShowProgressSpinner { get; set; }
        public bool ShowBusySpinner { get; set; }
        public bool ShowOkButton { get; set; }
        public bool ShowRateButton { get; set; }
        public bool ShowUpgradeButton { get; set; }
        public bool ShowLearnMoreButton { get; set; }
        public bool ShowTextBox { get; set; }
        public string InputResult { get; private set; }
        public string BoundInputText { get; set; }
        public bool RememberInputText { get; set; }

        public string ValidationText { get; set; }

        public bool IsInputValid => string.IsNullOrEmpty(ValidationText);

        public bool CanSubmit { get; set; } = true;
        #endregion

        #region Appearance

        #endregion

        #region Model

        public bool CanRemember {
            get {
                if (NotificationFormat == null) {
                    return default;
                }
                return NotificationFormat.CanRemember;
            }
        }
        public char PasswordChar {
            get {
                if (NotificationFormat == null) {
                    return default;
                }
                return NotificationFormat.PasswordChar;
            }
        }
        public object FixCommandArgs {
            get {
                if (NotificationFormat == null) {
                    return null;
                }
                return NotificationFormat.FixCommandArgs;
            }
        }

        public ICommand FixCommand {
            get {
                if (NotificationFormat == null) {
                    return null;
                }
                return NotificationFormat.FixCommand;
            }
        }

        public Func<object, object> RetryAction {
            get {
                if (NotificationFormat == null) {
                    return null;
                }
                return NotificationFormat.RetryAction;
            }
        }
        public object RetryActionObj {
            get {
                if (NotificationFormat == null) {
                    return null;
                }
                return NotificationFormat.RetryActionObj;
            }
        }
        #endregion

        #endregion

        #region Constructors

        public MpAvUserActionNotificationViewModel() : base() {
            PropertyChanged += MpUserActionNotificationViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpNotificationFormat nf) {
            IsBusy = true;
            if (nf.IconSourceObj == null) {
                if (GetIsErrorNotification(nf)) {
                    nf.IconSourceObj = "ErrorImage";
                } else if (GetIsWarningNotification(nf)) {
                    nf.IconSourceObj = "WarningImage";
                } else {
                    nf.IconSourceObj = "QuestionMarkImage";
                }
            }

            await base.InitializeAsync(nf);
            switch (ButtonsType) {
                case MpNotificationButtonsType.DeleteIgnoreFix:
                    ShowDeleteButton = true;
                    ShowIgnoreButton = true;
                    break;
                case MpNotificationButtonsType.ModalShutdownLater:
                    ShowShutdownButton = true;
                    ShowLaterButton = true;
                    break;
                case MpNotificationButtonsType.RestartNowLater:
                    ShowRestartNowButton = true;
                    ShowLaterButton = true;
                    break;
                case MpNotificationButtonsType.RestartIgnoreCancel:
                    ShowRestartButton = true;
                    ShowIgnoreButton = true;
                    ShowCancelButton = true;
                    break;
                case MpNotificationButtonsType.ResetAllResetSharedResetUnsharedCancel:
                    ShowResetPresetButtons = true;
                    ShowCancelButton = true;
                    break;
                case MpNotificationButtonsType.Update:
                    ShowUpdateButton = true;
                    break;
                case MpNotificationButtonsType.YesNo:
                    ShowYesButton = true;
                    ShowNoButton = true;
                    break;
                case MpNotificationButtonsType.YesNoCancel:
                    ShowYesButton = true;
                    ShowNoButton = true;
                    ShowCancelButton = true;
                    break;
                case MpNotificationButtonsType.Rate:
                    ShowRateButton = true;
                    break;
                case MpNotificationButtonsType.Progress: {
                        ShowProgressSpinner = true;
                        ShowCancelButton = OtherBoolArg.IsTrue() || ProgressIndicatorViewModel is MpICancelableProgressIndicatorViewModel;
                        break;
                    }

                case MpNotificationButtonsType.Busy: {
                        ShowBusySpinner = true;
                        ShowCancelButton = OtherBoolArg.IsTrue();
                        break;
                    }

                case MpNotificationButtonsType.SubmitCancel:
                    ShowSubmitButton = true;
                    ShowCancelButton = true;
                    break;
                case MpNotificationButtonsType.Ok:
                    ShowOkButton = true;
                    break;
                case MpNotificationButtonsType.OkCancel:
                    ShowOkButton = true;
                    ShowCancelButton = true;
                    break;
                case MpNotificationButtonsType.UpgradeLearnMore:
                    ShowUpgradeButton = true;
                    ShowLearnMoreButton = true;
                    break;
                case MpNotificationButtonsType.TextBoxOkCancel:
                    if (OtherArgs is string curText) {
                        BoundInputText = curText;
                    } else {
                        BoundInputText = string.Empty;
                    }
                    ShowTextBox = true;
                    ShowOkButton = true;
                    ShowCancelButton = true;
                    break;
                case MpNotificationButtonsType.IgnoreRetryShutdown:
                    ShowIgnoreButton = true;
                    ShowShutdownButton = true;
                    break;
                case MpNotificationButtonsType.IgnoreRetryFix:
                    ShowIgnoreButton = true;
                    // others based on args (fix,retry non-null)
                    break;
            }
            SetupParams();
            IsBusy = false;
        }

        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            DialogResult = BeginShow();

            if (DialogResult == MpNotificationDialogResultType.DoNotShow) {
                // NOTE pretty sure do not show isn't allowed for action notifications 
                // so this shouldn't happen
                return DialogResult;
            }
            await WaitForFullVisibilityAsync();
            DateTime startTime = DateTime.Now;
            while (true) {
                if (DialogResult != MpNotificationDialogResultType.None) {
                    break;
                }
                if (DoNotShowAgain) {
                    // set from CheckDoNotShowAgainCmd
                    DialogResult = MpNotificationDialogResultType.DoNotShow;
                    return DialogResult;
                }
                if (MaxShowTimeMs > 0) {
                    MpDebug.Assert(!ShowProgressSpinner && !ShowBusySpinner, "Spinner ntfs shouldn't have max show time");

                    if (DateTime.Now - startTime <= TimeSpan.FromMilliseconds(MaxShowTimeMs)) {
                        // max show not reache yet
                        while (IsFadeDelayFrozen) {
                            // reset wait while over
                            startTime = DateTime.Now;
                            await Task.Delay(100);
                            if (DoNotShowAgain) {
                                // DoNotShow clicked
                                return MpNotificationDialogResultType.DoNotShow;
                            }
                            if (DialogResult != MpNotificationDialogResultType.None) {
                                break;
                            }
                        }
                    } else {
                        DialogResult = MpNotificationDialogResultType.Dismiss;
                        return DialogResult;
                    }
                }

                if (ShowProgressSpinner || ShowBusySpinner) {
                    MpDebug.Assert(ProgressIndicatorViewModel != null || BusyCancellationToken.HasValue, "For busy or progress, must have ct or progress vm in OtherArgs");

                    while (true) {
                        if (BusyCancellationToken.HasValue && BusyCancellationToken.Value.IsCancellationRequested) {
                            MpConsole.WriteLine($"Progress canceled by token!");
                            DialogResult = MpNotificationDialogResultType.Cancel;
                            return DialogResult;
                        }

                        if (ProgressIndicatorViewModel != null) {
                            MpConsole.WriteLine(ProgressIndicatorViewModel.ToString());

                            if (ProgressIndicatorViewModel.PercentLoaded >= 1.0) {
                                MpConsole.WriteLine($"Progress completed by percent!");
                                DialogResult = MpNotificationDialogResultType.Dismiss;
                                return DialogResult;
                            }
                        }
                        if (DialogResult == MpNotificationDialogResultType.Cancel) {
                            // user canceled
                            return DialogResult;
                        }
                        await Task.Delay(100);
                    }
                }
                await Task.Delay(100);
            }

            if (DialogResult == MpNotificationDialogResultType.Fix) {
                // if fix is result, fix button becomes retry 
                // either wait for retry to become result or immediatly trigger
                // retry where caller should block return until retry invoked (isFixing becomes false)
                while (IsFixing) {
                    await Task.Delay(100);
                }
            }

            HideNotification();
            var test = RetryAction?.Invoke(RetryActionObj);

            return DialogResult;
        }

        public async Task<string> ShowInputResultNotificationAsync() {
            if (!Dispatcher.UIThread.CheckAccess()) {
                string result = await Dispatcher.UIThread.InvokeAsync(async () => {
                    return await ShowInputResultNotificationAsync();
                });
                return result;
            }
            DialogResult = BeginShow();

            while (DialogResult == MpNotificationDialogResultType.None) {
                await Task.Delay(100);
            }
            HideNotification();

            // NOTE ensure null only returned by cancel
            return DialogResult == MpNotificationDialogResultType.Cancel ?
                null :
                InputResult == null ?
                string.Empty : InputResult;
        }

        #endregion

        #region Private Methods

        private void MpUserActionNotificationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(BoundInputText):
                    if (IsInputValid) {
                        return;
                    }
                    // NOTE trigger validate here when already flagged invalid (via OkCommand)
                    Validate();
                    break;
                case nameof(IsFixing):
                    // HACK hide ignore while fixing...
                    // most if not all retry thread loops don't account
                    // for clicking ignore AFTER fix instead of retry
                    // but retry should reset if not fixed in which case
                    // ignore goes through intended logic 
                    if (IsFixing) {
                        if (ShowIgnoreButton) {
                            _wasIgnoreHiddenToFix = true;
                            ShowIgnoreButton = false;
                        }
                    } else {
                        if (_wasIgnoreHiddenToFix) {
                            _wasIgnoreHiddenToFix = false;
                            ShowIgnoreButton = true;
                        }
                    }
                    break;
                case nameof(IsWindowOpen):
                    if (IsWindowOpen) {
                        break;
                    }
                    CleanupParams();
                    break;
            }
        }

        private void SetupParams() {
            if (Body is not MpAvAnalyticItemPresetViewModel aipvm) {
                return;
            }
            aipvm.ExecuteItems.ForEach(x => x.PropertyChanged += HandleParamPropChange);
            //CanSubmit = aipvm.Parent.PerformAnalysisCommand.CanExecute(aipvm.Parent.CurrentExecuteArgs);
            CanSubmit = aipvm.IsAllValid;
        }
        private void CleanupParams() {
            if (Body is not MpAvAnalyticItemPresetViewModel aipvm) {
                return;
            }
            aipvm.ExecuteItems.ForEach(x => x.PropertyChanged -= HandleParamPropChange);
        }
        private void HandleParamPropChange(object s, PropertyChangedEventArgs e) {
            if (Body is not MpAvAnalyticItemPresetViewModel aipvm ||
                s is not MpAvParameterViewModelBase pvm) {
                return;
            }
            if (e.PropertyName == nameof(pvm.CurrentValue)) {
                //CanSubmit = aipvm.Parent.PerformAnalysisCommand.CanExecute(aipvm.Parent.CurrentExecuteArgs);
                CanSubmit = aipvm.IsAllValid;
            }
        }

        private bool Validate() {
            ValidationText = string.Empty;
            if (ShowTextBox && string.IsNullOrEmpty(BoundInputText)) {
                ValidationText = $"Value required";
            }
            OnPropertyChanged(nameof(IsInputValid));
            return IsInputValid;
        }

        #endregion

        #region Commands

        public ICommand ResetPresetCommand => new MpCommand<object>(
            (args) => {
                if (args is not string resetType) {
                    return;
                }
                switch (resetType) {
                    case "shared":
                        DialogResult = MpNotificationDialogResultType.ResetShared;
                        break;
                    case "unshared":
                        DialogResult = MpNotificationDialogResultType.ResetUnshared;
                        break;
                    case "all":
                        DialogResult = MpNotificationDialogResultType.ResetAll;
                        break;
                    default:
                        MpDebug.Break($"unhandled reset type '{resetType}'");
                        DialogResult = MpNotificationDialogResultType.Cancel;
                        break;
                }
            });

        public ICommand IgnoreCommand => new MpCommand(
            () => {
                IsFixing = false;
                DialogResult = MpNotificationDialogResultType.Ignore;
            });

        public ICommand FixWrapperCommand => new MpCommand(
            () => {
                IsFixing = true;
                FixCommand.Execute(FixCommandArgs);
                DialogResult = MpNotificationDialogResultType.Fix;
            }, () => CanFix);

        public ICommand RetryCommand => new MpCommand(
            () => {
                IsFixing = false;
                DialogResult = MpNotificationDialogResultType.Retry;
            });


        public ICommand ShutdownCommand => new MpCommand(
            () => {
                DialogResult = MpNotificationDialogResultType.Shutdown;
                Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.UserNtfCmd, "userAction cmd");
            });

        public ICommand DeleteCommand => new MpCommand(
            () => {
                DialogResult = MpNotificationDialogResultType.Delete;
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
                InputResult = null;
                if (ProgressIndicatorViewModel is MpICancelableProgressIndicatorViewModel cpivm) {
                    cpivm.CancelCommand.Execute(null);
                }
                DialogResult = MpNotificationDialogResultType.Cancel;
            });

        public ICommand RestartCommand => new MpCommand(
            () => {
                InputResult = null;
                MpAvAppRestarter.ShutdownWithRestartTaskAsync("By ntf").FireAndForgetSafeAsync();
                // NOTE settings result as Cancel so follow up code doesn't shut down but 
                // restarter is going to keep triggering itself until it can restart
                DialogResult = MpNotificationDialogResultType.Cancel;
            });

        public ICommand OkCommand => new MpCommand(
            () => {
                InputResult = BoundInputText;
                if (!Validate()) {
                    return;
                }
                DialogResult = MpNotificationDialogResultType.Ok;
            });

        public ICommand SubmitCommand => new MpCommand(
            () => {
                DialogResult = MpNotificationDialogResultType.Submitted;
            },
            () => {
                return CanSubmit;
            });

        public ICommand UpgradeCommand => new MpCommand(
            () => {
                MpAvSubscriptionPurchaseViewModel.Instance.NavigateToBuyUpgradeCommand.Execute(null);
                DialogResult = MpNotificationDialogResultType.Dismiss;
            });

        public ICommand LearnMoreCommand => new MpCommand(
            () => {
                MpAvHelpViewModel.Instance.NavigateToHelpLinkCommand.Execute(MpHelpLinkType.ContentLimits);
                DialogResult = MpNotificationDialogResultType.Dismiss;
            });

        public ICommand RateAppCommand => new MpCommand(
            () => {
                MpAvPrefViewModel.Instance.HasRated = true;
                MpAvAccountViewModel.Instance.RateAppCommand.Execute(null);
                DialogResult = MpNotificationDialogResultType.Dismiss;
            });

        #endregion
    }
}
