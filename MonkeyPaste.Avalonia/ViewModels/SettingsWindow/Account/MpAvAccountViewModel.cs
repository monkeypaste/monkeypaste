using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpUserPageType {
        None = 0,
        Login,
        Register,
        Reset,
        Status
    }
    public class MpAvAccountViewModel : MpAvViewModelBase {
        #region Private Variables
        private MpUserAccountType _lastAccountType;
        private DispatcherTimer _expiration_timer;
        private DispatcherTimer _attempt_login_timer;

        #endregion

        #region Constants
        const int EXPIRATION_TIMER_CHECK_M = 5;
        const int LOGIN_TIMER_CHECK_M = 5;
        const string SUCCESS_PREFIX = "[SUCCESS]";
        const string ERROR_PREFIX = "[ERROR]";

        const string PING_URL = "https://www.monkeypaste.com/accounts/ping.php";
        const string PING_RESPONSE = "Hello";

        const string REGISTER_BASE_URL = "https://www.monkeypaste.com/accounts/register.php";
        const string LOGIN_BASE_URL = "https://www.monkeypaste.com/accounts/login.php";
        const string RESET_PASSWORD_BASE_URL = "https://www.monkeypaste.com/accounts/reset-request.php";
        #endregion

        #region Statics 
        private static MpAvAccountViewModel _instance;
        public static MpAvAccountViewModel Instance => _instance ?? (_instance = new MpAvAccountViewModel());

        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models

        #endregion

        #region Appearance

        public string AccountTypeLabel =>
            AccountType.EnumToUiString();

        public string BillingCycleLabel =>
            BillingCycleType.EnumToUiString();


        public string NextPaymentDisplayValue =>
            HasBillingCycle ?
                NextPaymentUtc.ToLocalTime().ToString(UiStrings.CommonDateFormat) :
                "♾️";

        public string AccountStateInfo {
            get {
                int content_count = MpAvAccountTools.Instance.LastContentCount;
                int cap_count = MpAvAccountTools.Instance.GetContentCapacity(AccountType);
                if (AccountType == MpUserAccountType.Unlimited) {
                    return $"{AccountType} - (Total {content_count})";
                }
                return string.Format(
                    @"{0} - ({1} total {2} capacity {3} remaining)",
                    AccountType,
                    content_count,
                    cap_count,
                    cap_count - content_count);
            }
        }

        #endregion

        #region State

        public bool HasBillingCycle =>
            BillingCycleType == MpBillingCycleType.Monthly ||
            BillingCycleType == MpBillingCycleType.Yearly;

        public bool IsPaymentPastDue =>
            HasBillingCycle && DateTime.UtcNow > NextPaymentUtc;

        public bool CanLogin =>
            !IsLoggedIn &&
            !string.IsNullOrEmpty(AccountUsername) &&
            !string.IsNullOrEmpty(AccountPassword);

        public bool IsFree =>
            AccountType == MpUserAccountType.Free;
        public bool IsStandard =>
            AccountType == MpUserAccountType.Standard;
        public bool IsUnlimited =>
            AccountType == MpUserAccountType.Unlimited;

        public bool IsLoggedIn =>
            AccountState == MpUserAccountState.Connected;
        public bool IsRegistered =>
            AccountState != MpUserAccountState.Unregistered;

        public MpUserAccountType WorkingAccountType =>
            IsExpired ? MpUserAccountType.Free : AccountType;

        public bool IsSubscriptionDevice { get; private set; }
        public MpUserAccountState AccountState { get; private set; } = MpUserAccountState.Unregistered;
        #endregion

        #region Model

        #region Request Args

        Dictionary<string, (string, string)> RegisterRequestArgs =>
            new Dictionary<string, (string, string)>() {
                    {"username", (nameof(MpAvPrefViewModel.Instance.AccountUsername),MpAvPrefViewModel.Instance.AccountUsername) },
                    {"email", (nameof(MpAvPrefViewModel.Instance.AccountEmail),MpAvPrefViewModel.Instance.AccountEmail) },
                    {"password", (nameof(MpAvPrefViewModel.Instance.AccountPassword),MpAvPrefViewModel.Instance.AccountPassword) },
                    {"password2", (nameof(MpAvPrefViewModel.Instance.AccountPassword2), MpAvPrefViewModel.Instance.AccountPassword2) },
                };


        #endregion

        public string AccountUsername => MpAvPrefViewModel.Instance.AccountUsername;

        public string AccountEmail =>
            MpAvPrefViewModel.Instance.AccountEmail;
        public string AccountPassword =>
            MpAvPrefViewModel.Instance.AccountPassword;

        public MpUserAccountType AccountType {
            get => MpAvPrefViewModel.Instance.AccountType;
            private set {
                if (AccountType != value) {
                    _lastAccountType = AccountType;
                    MpAvPrefViewModel.Instance.AccountType = value;
                    OnPropertyChanged(nameof(AccountType));
                }
            }
        }



        public MpBillingCycleType BillingCycleType {
            get => MpAvPrefViewModel.Instance.AccountBillingCycleType;
            private set {
                if (BillingCycleType != value) {
                    MpAvPrefViewModel.Instance.AccountBillingCycleType = value;
                    OnPropertyChanged(nameof(BillingCycleType));
                }
            }
        }

        public DateTime NextPaymentUtc {
            get => MpAvPrefViewModel.Instance.AccountNextPaymentDateTime;
            private set {
                if (NextPaymentUtc != value) {
                    MpAvPrefViewModel.Instance.AccountNextPaymentDateTime = value;
                    OnPropertyChanged(nameof(NextPaymentUtc));
                }
            }
        }

        bool HasShownExpiredNtf { get; set; } = false;

        public bool IsExpired =>
            DateTime.UtcNow > NextPaymentUtc;

        public bool IsYearly =>
            BillingCycleType == MpBillingCycleType.Yearly;

        #endregion
        #endregion

        #region Constructors
        public MpAvAccountViewModel() : base() {
            MpDebug.Assert(_instance == null, $"Account singleton error");
            PropertyChanged += MpAvAccountViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync() {
            IsBusy = true;
            await LoginCommand.ExecuteAsync("init");
            IsBusy = false;
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpAvAccountViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(AccountType):
                    // NOTE this maybe a good all around interface method, not sure though
                    bool changed = AccountType != _lastAccountType;
                    if (changed) {
                        bool is_upgrade = (int)_lastAccountType > (int)AccountType;
                        MpMessenger.SendGlobal(is_upgrade ? MpMessageType.AccountUpgrade : MpMessageType.AccountDowngrade);
                    }
                    break;
                case nameof(AccountState):
                    UpdateAccountViews();
                    switch (AccountState) {
                        case MpUserAccountState.Connected:
                            StartExpirationTimer();
                            break;
                    }
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsWindowOpened:

                    UpdateAccountViews();
                    break;
                case MpMessageType.AccountInfoChanged:
                    OnPropertyChanged(nameof(AccountStateInfo));
                    break;
            }
        }
        private bool ProcessServerResponse(string response, out Dictionary<string, string> args) {
            response = response.ToStringOrEmpty();
            ClearErrors();

            if (response.StartsWith(SUCCESS_PREFIX) &&
                response.SplitNoEmpty(SUCCESS_PREFIX) is string[] success_parts) {
                args = success_parts.Length <= 1 ? null : MpJsonConverter.DeserializeObject<Dictionary<string, string>>(success_parts[1]);
                return true;
            }
            if (response.StartsWith(ERROR_PREFIX) &&
                response.SplitNoEmpty(ERROR_PREFIX) is string[] error_parts) {
                response = string.Join(string.Empty, error_parts.Skip(1));
            }
            // error
            args = MpJsonConverter.DeserializeObject<Dictionary<string, string>>(response);
            return false;
        }

        private void ClearErrors() {
            var param_tup = MpAvSettingsViewModel.Instance.GetParamAndFrameViewModelsByParamId(nameof(MpAvPrefViewModel.Instance.AccountEmail));
            if (param_tup == null) {
                // occurs on startup
                return;
            }
            foreach (var pvm in param_tup.Item1.Items) {
                pvm.RemoveValidationOverride();
            }

        }

        private void ProcessResponseErrors(Dictionary<string, string> errors) {
            if (errors == null) {
                return;
            }
            foreach (var error_kvp in errors) {
                MpDebug.Assert(RegisterRequestArgs.ContainsKey(error_kvp.Key), $"Missing server input '{error_kvp.Key}'");
                string param_id = RegisterRequestArgs[error_kvp.Key].Item1;
                var param_tup = MpAvSettingsViewModel.Instance.GetParamAndFrameViewModelsByParamId(param_id);
                if (param_tup != null && param_tup.Item2 != null) {
                    param_tup.Item2.OverrideValidationMesage(error_kvp.Value);
                }
            }

        }
        private void UpdateAccountViews() {
            if (MpAvSettingsViewModel.Instance.FilteredAccountFrames == null) {
                return;
            }
            foreach (var afvm in MpAvSettingsViewModel.Instance.FilteredAccountFrames) {
                switch (afvm.FrameType) {
                    case MpSettingsFrameType.Status:
                        afvm.IsVisible = AccountState == MpUserAccountState.Connected;
                        break;
                    case MpSettingsFrameType.Login:
                        afvm.IsVisible = AccountState == MpUserAccountState.Disconnected;
                        break;
                    case MpSettingsFrameType.Register:
                        afvm.IsVisible = AccountState == MpUserAccountState.Unregistered;
                        break;
                }
            }
        }
        private void SetButtonBusy(MpRuntimePrefParamType btnType, bool is_busy) {
            if (MpAvSettingsViewModel.Instance.GetParamAndFrameViewModelsByParamId(btnType.ToString()) is var kvp && kvp != null && kvp.Item2 != null) {
                kvp.Item2.IsBusy = is_busy;
            } else {
                // MpDebug.BreakAll();
            }
        }

        private void ResetAccountPrefs() {
            MpAvPrefViewModel.Instance.AccountUsername = null;
            MpAvPrefViewModel.Instance.AccountPassword = null;
            MpAvPrefViewModel.Instance.AccountPassword2 = null;
            ResetAccountTypeToFree();
        }
        private void ResetAccountTypeToFree() {
            MpAvPrefViewModel.Instance.AccountType = MpUserAccountType.Free;
            MpAvPrefViewModel.Instance.AccountBillingCycleType = MpBillingCycleType.None;
            MpAvPrefViewModel.Instance.AccountNextPaymentDateTime = DateTime.MaxValue;
        }

        private async Task<bool> CheckIfServerIsAvailableAsync() {
            string resp = await MpFileIo.ReadTextFromUriAsync(PING_URL);
            string test = await MpFileIo.ReadTextFromUriAsync(@"https://www.monkeypaste.com/accounts/blah.php");
            MpDebug.BreakAll();
            return resp == PING_RESPONSE;

        }


        private void StartExpirationTimer() {
            if (_expiration_timer != null) {
                return;
            }
            _expiration_timer = new DispatcherTimer() {
                Interval = TimeSpan.FromMinutes(EXPIRATION_TIMER_CHECK_M),
                IsEnabled = true
            };
            _expiration_timer.Tick += CheckExpiration_tick;
            _expiration_timer.Start();

            void CheckExpiration_tick(object sender, EventArgs e) {
                if (!IsExpired) {
                    return;
                }

                if (!HasShownExpiredNtf) {
                    string msg = null;
                    if (IsLoggedIn) {
                        // verified acct is expired
                        if (IsSubscriptionDevice) {
                            // this device has subscription
                            msg = string.Format(
                                        UiStrings.AccountExpiredNtfLocalCaption,
                                        AccountType,
                                        NextPaymentUtc.ToLocalTime().ToString(UiStrings.CommonDateFormat));
                        } else {
                            // another device has subscription
                            msg = string.Format(
                                        UiStrings.AccountExpiredNtfRemoteCaption,
                                        AccountType,
                                        NextPaymentUtc.ToLocalTime().ToString(UiStrings.CommonDateFormat));
                        }
                    } else {
                        // offline expired
                        msg = string.Format(
                                        UiStrings.AccountExpiredNtfOfflineCaption,
                                        AccountType,
                                        NextPaymentUtc.ToLocalTime().ToString(UiStrings.CommonDateFormat));
                    }

                    Mp.Services.NotificationBuilder.ShowMessageAsync(
                               title: UiStrings.AccountExpiredNtfTitle,
                               body: msg,
                               msgType: MpNotificationType.SubscriptionExpired,
                               iconSourceObj: "WarningTimeImage").FireAndForgetSafeAsync();
                    HasShownExpiredNtf = true;
                }

            }
        }
        private void StartAttemptLoginTimer() {
            if (_attempt_login_timer != null) {
                return;
            }
            _attempt_login_timer = new DispatcherTimer() {
                Interval = TimeSpan.FromMinutes(LOGIN_TIMER_CHECK_M),
                IsEnabled = true
            };
            _attempt_login_timer.Tick += AttemptLogin_tick;
            _attempt_login_timer.Start();

            void AttemptLogin_tick(object sender, EventArgs e) {
                if (IsLoggedIn) {
                    if (_attempt_login_timer != null) {
                        _attempt_login_timer.Stop();
                        _attempt_login_timer.Tick -= AttemptLogin_tick;
                        _attempt_login_timer = null;
                    }
                    return;
                }

                LoginCommand.Execute("timer");
            }
        }
        #endregion

        #region Commands


        public MpIAsyncCommand<object> LoginCommand => new MpAsyncCommand<object>(
            async (args) => {
                // cases:
                // -This device has subscription
                // -Another device has subscription
                // -There is no account info
                // -There is no subscription
                // -There is no connection to store
                // -There is no connection to server
                // -There is no connection at all

                SetButtonBusy(MpRuntimePrefParamType.AccountLogin, true);
                MpSubscriptionFormat acct = await MpAvAccountTools.Instance.GetStoreUserLicenseInfoAsync();

                var req_args = new Dictionary<string, string>() {
                    {"username", MpAvPrefViewModel.Instance.AccountUsername },
                    {"password", MpAvPrefViewModel.Instance.AccountPassword },
                    {"device_guid", MpDefaultDataModelTools.ThisUserDeviceGuid },
                    {"sub_type", acct == MpSubscriptionFormat.Default ? AccountType.ToString() : acct.AccountType.ToString() },
                    {"monthly", (acct == MpSubscriptionFormat.Default ? !IsYearly : acct.IsMonthly) ? "1":"0" },
                    {"expires_utc_dt", acct == MpSubscriptionFormat.Default ? NextPaymentUtc.ToString() : acct.ExpireOffsetUtc.UtcDateTime.ToString() },
                    {"detail1", MpAvPrefViewModel.arg1 },
                    {"detail2", MpAvPrefViewModel.arg2 },
                    {"detail3", MpAvPrefViewModel.arg3 },
                };

                var sw = Stopwatch.StartNew();
                string resp = await MpHttpRequester.SubmitPostDataToUrlAsync(LOGIN_BASE_URL, req_args);

                SetButtonBusy(MpRuntimePrefParamType.AccountLogin, false);

                bool success = ProcessServerResponse(resp, out var resp_args);

                MpConsole.WriteLine($"login {success.ToTestResultLabel()}. Time: {sw.ElapsedMilliseconds}ms");
                if (success) {
                    AccountType = resp_args["sub_type"].ToEnum<MpUserAccountType>();
                    NextPaymentUtc =
                        AccountType == MpUserAccountType.Free ?
                            DateTime.MaxValue :
                            DateTime.Parse(resp_args["expires_utc_dt"]);
                    BillingCycleType =
                        AccountType == MpUserAccountType.Free ?
                            MpBillingCycleType.None :
                            resp_args["monthly"] == "1" ?
                                MpBillingCycleType.Monthly : MpBillingCycleType.Yearly;

                    IsSubscriptionDevice = AccountType == acct.AccountType;
                    AccountState = MpUserAccountState.Connected;

                    StartExpirationTimer();
                    return;
                }
                AccountState = MpUserAccountState.Disconnected;
                // user offline or unregistered
                StartExpirationTimer();

                if (args is not string login_source_type ||
                    login_source_type == "timer") {
                    // not startup or click so ignore 
                    return;
                }
                Dispatcher.UIThread.Post(async () => {
                    ProcessResponseErrors(resp_args);

                    while (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                        await Task.Delay(100);
                    }

                    Mp.Services.NotificationBuilder.ShowMessageAsync(
                                   title: UiStrings.CommonErrorLabel,
                                   body: string.Format(UiStrings.AccountLoginFailedText, AccountUsername),
                                   msgType: MpNotificationType.AccountLoginFailed,
                                   iconSourceObj: "WarningImage").FireAndForgetSafeAsync();

                    StartAttemptLoginTimer();
                });
            }, (args) => {
                //return !IsLoggedIn;
                return CanLogin;
            });


        public MpIAsyncCommand RegisterCommand => new MpAsyncCommand(
            async () => {
                SetButtonBusy(MpRuntimePrefParamType.AccountRegister, true);
                var sw = Stopwatch.StartNew();
                string response = await MpHttpRequester.SubmitPostDataToUrlAsync(REGISTER_BASE_URL, RegisterRequestArgs.ToDictionary(x => x.Key, x => x.Value.Item2));
                SetButtonBusy(MpRuntimePrefParamType.AccountRegister, false);
                bool success = ProcessServerResponse(response, out var resp_args);

                MpConsole.WriteLine($"Registration {success.ToTestResultLabel()}. Time: {sw.ElapsedMilliseconds}ms");
                if (success) {
                    AccountState = MpUserAccountState.Disconnected;
                    Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                         title: UiStrings.AccountPurchaseSuccessfulTitle,
                         message: UiStrings.AccountRegistrationSuccessText,
                         iconResourceObj: "MonkeyWinkImage").FireAndForgetSafeAsync();
                    return;
                }

                ProcessResponseErrors(resp_args);
                await Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                         title: UiStrings.CommonErrorLabel,
                         message: UiStrings.AccountRegistrationRegisterFailedText,
                         iconResourceObj: "WarningImage");
            }, () => {
                return !IsRegistered;
            });

        public ICommand ResetPasswordRequestCommand => new MpAsyncCommand(
            async () => {
                SetButtonBusy(MpRuntimePrefParamType.AccountResetPassword, true);
                var req_args = new Dictionary<string, string>() {
                    {"username", AccountUsername }
                };
                string resp = await MpHttpRequester.SubmitPostDataToUrlAsync(RESET_PASSWORD_BASE_URL, req_args);
                SetButtonBusy(MpRuntimePrefParamType.AccountResetPassword, false);

                bool success = ProcessServerResponse(resp, out var resp_args);
                if (success) {
                    Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                         title: UiStrings.AccountResetPasswordLabel,
                         message: UiStrings.AccountResetPasswordCaption,
                         iconResourceObj: "MonkeyWinkImage").FireAndForgetSafeAsync();
                    return;
                }
                await Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                         title: UiStrings.CommonErrorLabel,
                         message: UiStrings.CommonConnectionFailedCaption,
                         iconResourceObj: "WarningImage");
            },
            () => {
                return IsRegistered;
            });

        public ICommand ResetAccountCommand => new MpCommand(
            () => {
                ResetAccountPrefs();
                AccountState = MpUserAccountState.Unregistered;
            });

        public ICommand LogoutCommand => new MpCommand(
            () => {
                AccountState = MpUserAccountState.Disconnected;
            }, () => {
                return IsLoggedIn;
            });
        #endregion
    }
}
