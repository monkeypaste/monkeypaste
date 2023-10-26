using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    public enum MpLoginSourceType {
        Init,
        Timer,
        Click
    }
    public enum MpAccountRequestType {
        None,
        Login,
        Register
    }
    public enum MpAccountNtfType {
        None = 0,
        LoginSuccessful,
        LoginFailedUser,
        LoginFailedStartup,
        RegistrationSuccessful,
        RegistrationError,
        AccountExpiredLocal,
        AccountExpiredRemote,
        AccountExpiredOffline,
        ResetSent,
        ResetError
    }
    public class MpAvAccountViewModel :
        MpAvViewModelBase {
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

        const string PRIVACY_POLICY_URL = "https://www.monkeypaste.com/legal/privacy.html";

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

        //private List<MpAvSettingsFrameViewModel> _accountFrameViewModels;
        //public List<MpAvSettingsFrameViewModel> AccountFrameViewModels {
        //    get {
        //        if(_accountFrameViewModels == null) {
        //            _a
        //        }
        //    }
        //}

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

        bool HasConfirmedAccount =>
            MpAvPrefViewModel.Instance.LastLoginDateTimeUtc > DateTime.MinValue;
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
                    {"confirm", (nameof(MpAvPrefViewModel.Instance.AccountPassword2), MpAvPrefViewModel.Instance.AccountPassword2) },
                    {"agree", (nameof(MpAvPrefViewModel.Instance.AccountPrivacyPolicyAccepted), MpAvPrefViewModel.Instance.AccountPrivacyPolicyAccepted ? "1":"0") },
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
        public bool IsMonthly =>
            BillingCycleType == MpBillingCycleType.Monthly;

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
            await LoginCommand.ExecuteAsync(MpLoginSourceType.Init);
            IsBusy = false;
        }

        public async Task<MpUserAccountType> ShowExistingAccountLoginWindowAsync() {
            MpDebug.Assert(MpAvWelcomeNotificationViewModel.Instance.IsWindowOpen, $"Existing acct window error. Only supposed to be used during welcome");

            AccountState = MpUserAccountState.Disconnected;
            var svm = MpAvSettingsViewModel.Instance;
            await svm.InitAsync();
            await svm.ShowSettingsWindowCommand.ExecuteAsync(MpSettingsTabType.Account);


            while (true) {
                if (!svm.IsWindowOpen) {
                    // closed window ie cancel
                    ResetAccountPrefs();
                    AccountState = MpUserAccountState.Unregistered;
                    return MpUserAccountType.None;
                }
                if (AccountState == MpUserAccountState.Connected) {
                    // login successful show confirmation and close window to return welcome
                    await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                         title: string.Empty,
                         message: UiStrings.AccountLoginSuccessfulText,
                         iconResourceObj: "MonkeyWinkImage");
                    svm.IsWindowOpen = false;
                    return AccountType;
                }
                await Task.Delay(300);
            }
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
                            MpAvPrefViewModel.Instance.LastLoginDateTimeUtc = DateTime.UtcNow;
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
            string msg_suffix = string.Empty;
            bool success = false;
            ClearErrors();

            if (response.StartsWith(SUCCESS_PREFIX) &&
                response.SplitNoEmpty(SUCCESS_PREFIX) is string[] success_parts) {
                msg_suffix = string.Join(string.Empty, success_parts);
                success = true;
            } else if (response.StartsWith(ERROR_PREFIX) &&
                response.SplitNoEmpty(ERROR_PREFIX) is string[] error_parts) {
                msg_suffix = string.Join(string.Empty, error_parts);
            }

            args = MpJsonConverter.DeserializeObject<Dictionary<string, string>>(msg_suffix);
            if (!string.IsNullOrWhiteSpace(msg_suffix) && args.Count == 0) {
                // shouldnon-input error, add it to empty key
                MpDebug.Assert(!success, $"Should only have non-lookup result for error");
                args = new() { { string.Empty, msg_suffix } };
            }
            return success;
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

        private void ProcessResponseErrors(MpAccountNtfType ntf_type, Dictionary<string, string> param_id_lookup, Dictionary<string, string> errors) {
            if (errors == null) {
                return;
            }
            var err_sb = new StringBuilder();
            foreach (var error_kvp in errors) {
                if (param_id_lookup.TryGetValue(error_kvp.Key, out string param_id)) {
                    if (MpAvSettingsViewModel.Instance.TryGetParamAndFrameViewModelsByParamId(param_id, out var param_tup)) {
                        param_tup.Item2.OverrideValidationMesage(error_kvp.Value);
                    } else {
                        MpConsole.WriteLine($"Cannot find settings param '{param_id}'");
                    }
                } else {
                    MpConsole.WriteLine($"Non-req server resp error found. key '{error_kvp.Key.ToStringOrEmpty()}' value '{error_kvp.Value.ToStringOrEmpty()}'");
                    err_sb.AppendLine(error_kvp.Value);
                }
            }
            if (err_sb.ToString() is not string err_str ||
                string.IsNullOrWhiteSpace(err_str)) {
                // no general error;
                return;
            }
            ShowAccountNotficationAsync(ntf_type, err_str).FireAndForgetSafeAsync();
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

        private async Task ShowAccountNotficationAsync(MpAccountNtfType ant, params string[] args) {
            string title = null;
            string msg = null;
            object icon = null;
            MpNotificationType ntf_type = MpNotificationType.None;
            switch (ant) {
                case MpAccountNtfType.AccountExpiredRemote:
                case MpAccountNtfType.AccountExpiredOffline:
                case MpAccountNtfType.AccountExpiredLocal:
                    icon = "WarningTimeImage";
                    title = UiStrings.AccountExpiredNtfTitle;
                    msg = ant == MpAccountNtfType.AccountExpiredLocal ?
                        UiStrings.AccountExpiredNtfLocalCaption :
                        ant == MpAccountNtfType.AccountExpiredRemote ?
                        UiStrings.AccountExpiredNtfRemoteCaption :
                        UiStrings.AccountExpiredNtfOfflineCaption;
                    msg = string.Format(msg, AccountType, NextPaymentDisplayValue);
                    ntf_type = MpNotificationType.SubscriptionExpired;
                    break;
                case MpAccountNtfType.LoginFailedUser:
                case MpAccountNtfType.LoginFailedStartup:
                    title = UiStrings.CommonErrorLabel;
                    msg = string.Format(UiStrings.AccountLoginFailedText, AccountUsername);
                    icon = "ErrorImage";
                    if (ant == MpAccountNtfType.LoginFailedStartup) {
                        ntf_type = MpNotificationType.AccountLoginFailed;
                    }
                    break;
                case MpAccountNtfType.LoginSuccessful:
                    title = UiStrings.AccountLoginSuccessfulText;
                    icon = "MonkeyWinkImage";
                    break;
                case MpAccountNtfType.RegistrationError:
                    title = UiStrings.CommonErrorLabel;
                    msg = UiStrings.AccountRegistrationRegisterFailedText;
                    icon = "ErrorImage";
                    break;
                case MpAccountNtfType.RegistrationSuccessful:
                    msg = string.Empty;
                    title = UiStrings.AccountRegistrationSuccessText;
                    icon = "MonkeyWinkImage";
                    break;
                case MpAccountNtfType.ResetSent:
                    title = UiStrings.AccountResetPasswordLabel;
                    msg = UiStrings.AccountResetPasswordCaption;
                    icon = "MonkeyWinkImage";
                    break;
                case MpAccountNtfType.ResetError:
                    title = UiStrings.CommonErrorLabel;
                    msg = UiStrings.CommonConnectionFailedCaption;
                    icon = "ErrorImage";
                    break;
            }

            if (ntf_type == MpNotificationType.None) {
                await Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                         title: title,
                         message: msg,
                         iconResourceObj: icon);
            } else {
                await Mp.Services.NotificationBuilder.ShowMessageAsync(
                       title: title,
                       body: msg,
                       msgType: ntf_type,
                       iconSourceObj: icon);
            }
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
                    MpAccountNtfType ant = MpAccountNtfType.None;

                    if (IsLoggedIn) {
                        // verified acct is expired
                        if (IsSubscriptionDevice) {
                            // this device has subscription
                            ant = MpAccountNtfType.AccountExpiredLocal;
                        } else {
                            // another device has subscription
                            ant = MpAccountNtfType.AccountExpiredRemote;
                        }
                    } else {
                        // offline expired
                        ant = MpAccountNtfType.AccountExpiredOffline;
                    }
                    ShowAccountNotficationAsync(ant).FireAndForgetSafeAsync();
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

                LoginCommand.Execute(MpLoginSourceType.Timer);
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
                MpLoginSourceType login_type = (MpLoginSourceType)args;

                SetButtonBusy(MpRuntimePrefParamType.AccountLogin, true);
                MpSubscriptionFormat acct = await MpAvAccountTools.Instance.GetStoreUserLicenseInfoAsync();


                var req_args = new Dictionary<string, (string, string)>() {
                    {"username", (nameof(MpAvPrefViewModel.Instance.AccountUsername),MpAvPrefViewModel.Instance.AccountUsername) },
                    {"password", (nameof(MpAvPrefViewModel.Instance.AccountPassword),MpAvPrefViewModel.Instance.AccountPassword) },
                    {"device_guid", (nameof(MpDefaultDataModelTools.ThisUserDeviceGuid), MpDefaultDataModelTools.ThisUserDeviceGuid) },
                    {"sub_type", (null,acct == MpSubscriptionFormat.Default ? AccountType.ToString() : acct.AccountType.ToString())},
                    {"monthly", (null, (acct == MpSubscriptionFormat.Default ? !IsYearly : acct.IsMonthly) ? "1" : "0")},
                    {"expires_utc_dt", (null,acct == MpSubscriptionFormat.Default? NextPaymentUtc.ToString() : acct.ExpireOffsetUtc.UtcDateTime.ToString())},
                    {"detail1", (null,MpAvPrefViewModel.arg1)},
                    {"detail2", (null,MpAvPrefViewModel.arg2)},
                    {"detail3", (null,MpAvPrefViewModel.arg3)},
                };

                var sw = Stopwatch.StartNew();
                string resp = await MpHttpRequester.SubmitPostDataToUrlAsync(LOGIN_BASE_URL, req_args.ToDictionary(x => x.Key, x => x.Value.Item2));

                SetButtonBusy(MpRuntimePrefParamType.AccountLogin, false);

                bool success = ProcessServerResponse(resp, out var resp_args);

                MpConsole.WriteLine($"login {success.ToTestResultLabel()}. Time: {sw.ElapsedMilliseconds}ms");
                if (success) {
                    MpDebug.Assert(resp_args != null, $"subscription error, login resp wrong");
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
                    if (login_type == MpLoginSourceType.Click) {
                        ShowAccountNotficationAsync(MpAccountNtfType.LoginSuccessful).FireAndForgetSafeAsync();
                    }
                    StartExpirationTimer();
                    return;
                }

                AccountState = MpUserAccountState.Disconnected;
                // user offline or unregistered
                StartExpirationTimer();

                if (login_type == MpLoginSourceType.Timer) {
                    return;
                }
                if (login_type == MpLoginSourceType.Init && !HasConfirmedAccount) {
                    return;
                }
                ProcessResponseErrors(
                    login_type == MpLoginSourceType.Init ? MpAccountNtfType.LoginFailedStartup : MpAccountNtfType.LoginFailedUser,
                    req_args.ToDictionary(x => x.Key, x => x.Value.Item1), resp_args);

                StartAttemptLoginTimer();
            }, (args) => {
                //return !IsLoggedIn;
                return CanLogin && args is MpLoginSourceType;
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
                    ShowAccountNotficationAsync(MpAccountNtfType.RegistrationSuccessful).FireAndForgetSafeAsync();
                    return;
                }

                ProcessResponseErrors(MpAccountNtfType.RegistrationError, RegisterRequestArgs.ToDictionary(x => x.Key, x => x.Value.Item1), resp_args);

            }, () => {
                return !IsRegistered;
            });

        public ICommand ShowPrivacyPolicyCommand => new MpCommand(
            () => {
                MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(PRIVACY_POLICY_URL);
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
                    ShowAccountNotficationAsync(MpAccountNtfType.ResetSent).FireAndForgetSafeAsync();
                    return;
                }
                ShowAccountNotficationAsync(MpAccountNtfType.ResetError).FireAndForgetSafeAsync();
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
