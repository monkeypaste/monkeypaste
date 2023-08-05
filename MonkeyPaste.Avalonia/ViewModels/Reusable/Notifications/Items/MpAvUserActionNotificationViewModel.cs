using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvUserActionNotificationViewModel : MpAvNotificationViewModelBase, MpIProgressIndicatorViewModel {
        #region Private Variables
        #endregion

        #region Interfaces
        public double PercentLoaded { get; set; }

        #endregion

        #region Properties

        #region State

        public override bool ShowOptionsButton =>
            NotificationType == MpNotificationType.ContentCapReached ||
            NotificationType == MpNotificationType.TrashCapReached ||
            NotificationType == MpNotificationType.ContentAddBlockedByAccount ||
            NotificationType == MpNotificationType.ContentRestoreBlockedByAccount ||
            NotificationType == MpNotificationType.ModalContentFormatDegradation;

        public bool IsFixing { get; set; } = false;

        public bool CanFix => FixCommand != null && FixCommand.CanExecute(FixCommandArgs);

        public bool ShowIgnoreButton { get; set; }
        public bool ShowFixButton => CanFix && !IsFixing;
        public bool ShowSubmitButton { get; set; }
        public bool ShowRetryButton => IsFixing;

        public bool ShowShutdownButton => ShowIgnoreButton && !CanFix;

        public bool ShowYesButton { get; set; } = false;
        public bool ShowNoButton { get; set; } = false;
        public bool ShowCancelButton { get; set; } = false;
        public bool ShowProgressSpinner { get; set; } = false;
        public bool ShowOkButton { get; set; } = false;
        public bool ShowUpgradeButton { get; set; } = false;
        public bool ShowLearnMoreButton { get; set; } = false;
        public bool ShowTextBox { get; set; } = false;
        public MpNotificationDialogResultType DialogResult { get; private set; }
        public string InputResult { get; private set; }
        public string BoundInputText { get; set; }

        public string ValidationText { get; set; }

        public bool IsInputValid => string.IsNullOrEmpty(ValidationText);

        public bool CanSubmit { get; set; } = true;
        #endregion

        #region Model

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
                if (IsErrorNotification) {
                    nf.IconSourceObj = MpBase64Images.Error;
                } else if (IsWarningNotification) {
                    nf.IconSourceObj = MpBase64Images.Warning;
                } else {
                    nf.IconSourceObj = MpBase64Images.QuestionMark;
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

                case MpNotificationButtonsType.ProgressCancel:
                    ShowCancelButton = true;
                    ShowProgressSpinner = true;
                    break;
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
            DateTime startTime = DateTime.Now;
            while (true) {
                if (DialogResult != MpNotificationDialogResultType.None) {
                    break;
                }
                if (ShowProgressSpinner) {
                    if (OtherArgs is object[] argParts) {
                        if (argParts.OfType<MpAvProgressViewModel>().FirstOrDefault() is MpAvProgressViewModel prog_vm) {
                            PercentLoaded = prog_vm.Progress;
                        }
                        if (argParts.OfType<CancellationToken>().FirstOrDefault() is CancellationToken ct &&
                            ct.IsCancellationRequested) {
                            MpConsole.WriteLine($"Progress canceled by token!");
                            DialogResult = MpNotificationDialogResultType.Cancel;
                        }
                    }
                    MpConsole.WriteLine($"Cur Percent loaded: " + PercentLoaded);
                    if (PercentLoaded >= 1.0) {
                        DialogResult = MpNotificationDialogResultType.Dismiss;
                    }
                } else if (MaxShowTimeMs > 0) {
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
                } else if (DoNotShowAgain) {
                    // set from CheckDoNotShowAgainCmd
                    DialogResult = MpNotificationDialogResultType.DoNotShow;
                    CloseNotificationCommand.Execute(null);
                    return DialogResult;

                }
                await Task.Delay(100);
            }

            if (DialogResult == MpNotificationDialogResultType.Fix) {
                // if fix is result, fix button becomes retry 
                // either wait for retry to become result or immediatly trigger
                // retry where caller should block return until retry invoked (isFixing becomes false)
                _ = Task.Run(async () => {
                    while (IsFixing) {
                        await Task.Delay(100);
                    }
                    HideNotification();
                    var result = RetryAction?.Invoke(RetryActionObj);
                });
            } else {
                HideNotification();
                while (IsClosing) {
                    await Task.Delay(100);
                }
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

            return InputResult;
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
                //DialogResult = MpNotificationDialogResultType.Retry;
            });


        public ICommand ShutdownCommand => new MpCommand(
            () => {
                DialogResult = MpNotificationDialogResultType.Shutdown;
                Mp.Services.ShutdownHelper.ShutdownApp("userAction cmd");
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
                Mp.Services.SettingsTools
                    .ShowSettingsWindowCommand.Execute(
                    new object[] {
                        MpSettingsTabType.Account,
                        nameof(MpAvPrefViewModel.Instance.UserEmail) });
                DialogResult = MpNotificationDialogResultType.Dismiss;
            });

        public ICommand LearnMoreCommand => new MpCommand(
            () => {
                Mp.Services.SettingsTools
                    .ShowSettingsWindowCommand.Execute(
                    new object[] {
                        MpSettingsTabType.Account,
                        nameof(MpAvPrefViewModel.Instance.UserEmail) });
                DialogResult = MpNotificationDialogResultType.Dismiss;
            });

        #endregion
    }
}
