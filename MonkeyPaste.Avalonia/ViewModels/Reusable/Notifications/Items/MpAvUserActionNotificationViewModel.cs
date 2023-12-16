using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvUserActionNotificationViewModel : MpAvNotificationViewModelBase, MpIProgressIndicatorViewModel {
        #region Private Variables
        bool _wasIgnoreHiddenToFix = false;
        #endregion

        #region Interfaces
        public double PercentLoaded { get; set; }

        #endregion

        #region Properties

        #region State
        public override bool CanPin => true;

        public override bool ShowOptionsButton =>
            //NotificationType == MpNotificationType.RateApp ||
            NotificationType == MpNotificationType.ContentCapReached ||
            NotificationType == MpNotificationType.TrashCapReached ||
            NotificationType == MpNotificationType.ContentAddBlockedByAccount ||
            NotificationType == MpNotificationType.ContentRestoreBlockedByAccount ||
            NotificationType == MpNotificationType.ConfirmEndAppend ||
            NotificationType == MpNotificationType.ModalContentFormatDegradation;

        public bool IsFixing { get; set; } = false;

        public bool CanFix => FixCommand != null && FixCommand.CanExecute(FixCommandArgs);

        public bool ShowIgnoreButton { get; set; }
        public bool ShowFixButton => CanFix && !IsFixing;
        public bool ShowSubmitButton { get; set; }
        public bool ShowRetryButton => IsFixing;

        public bool ShowShutdownButton => ShowIgnoreButton && !CanFix;

        public bool ShowYesButton { get; set; }
        public bool ShowNoButton { get; set; }
        public bool ShowCancelButton { get; set; }
        public bool ShowProgressSpinner { get; set; }
        public bool ShowBusySpinner { get; set; }
        public bool ShowOkButton { get; set; }
        public bool ShowRateButton { get; set; }
        public bool ShowUpgradeButton { get; set; }
        public bool ShowLearnMoreButton { get; set; }
        public bool ShowTextBox { get; set; }
        public MpNotificationDialogResultType DialogResult { get; private set; }
        public string InputResult { get; private set; }
        public string BoundInputText { get; set; }
        public bool RememberInputText { get; set; }

        public string ValidationText { get; set; }

        public bool IsInputValid => string.IsNullOrEmpty(ValidationText);

        public bool CanSubmit { get; set; } = true;
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
                        ShowCancelButton =
                            OtherArgs is CancellationToken ||
                            (OtherArgs is object[] argParts &&
                             argParts.Any(x => x is CancellationToken));
                        break;
                    }

                case MpNotificationButtonsType.Busy: {
                        ShowBusySpinner = true;

                        ShowCancelButton =
                            OtherArgs is CancellationToken ||
                            (OtherArgs is object[] argParts &&
                             argParts.Any(x => x is CancellationToken));
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
                case MpNotificationButtonsType.IgnoreRetryFix:
                    ShowIgnoreButton = true;
                    // others based on args (fix,retry non-null)
                    break;
            }
            IsBusy = false;
        }

        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            var base_result = await base.ShowNotificationAsync();

            if (base_result == MpNotificationDialogResultType.DoNotShow) {
                // NOTE pretty sure do not show isn't allowed for action notifications 
                // so this shouldn't happen
                HideNotification();
                return base_result;
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
                    CloseNotificationCommand.Execute(null);
                    return DialogResult;
                }
                if (MaxShowTimeMs > 0) {
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
                        }
                    } else {
                        DialogResult = MpNotificationDialogResultType.Dismiss;
                    }
                }

                if (ShowProgressSpinner || ShowBusySpinner) {
                    object[] argParts = OtherArgs as object[];
                    if (argParts == null) {
                        argParts = new[] { OtherArgs };
                    }
                    while (true) {
                        if (argParts.OfType<MpIProgressIndicatorViewModel>().FirstOrDefault() is MpIProgressIndicatorViewModel prog_vm) {
                            PercentLoaded = prog_vm.PercentLoaded;
                        }
                        if (argParts.OfType<CancellationToken>().FirstOrDefault() is CancellationToken ct &&
                            ct.IsCancellationRequested) {
                            MpConsole.WriteLine($"Progress canceled by token!");
                            DialogResult = MpNotificationDialogResultType.Cancel;
                            return DialogResult;
                        }
                        MpConsole.WriteLine($"Cur Percent loaded: " + PercentLoaded);
                        if (PercentLoaded >= 1.0) {
                            DialogResult = MpNotificationDialogResultType.Dismiss;
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
                //Dispatcher.UIThread.Post(async () => {
                while (IsFixing) {
                    await Task.Delay(100);
                }
                HideNotification();
                var result = RetryAction?.Invoke(RetryActionObj);
                //});
            } else {
                HideNotification();
                //while (IsClosing) {
                //    await Task.Delay(100);
                //}
            }

            //if (DialogResult == MpNotificationDialogResultType.Retry) {
            //    RetryAction?.Invoke(RetryActionObj);
            //} else if(DialogResult != MpNotificationDialogResultType.Fix) {
            //    HideNotification();
            //}

            return DialogResult;
        }

        public async Task<string> ShowInputResultNotificationAsync() {
            _ = await base.ShowNotificationAsync();

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
