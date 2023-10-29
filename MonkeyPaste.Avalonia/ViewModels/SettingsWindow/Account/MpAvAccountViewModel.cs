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
        ExistingLoginSuccessful,
        LoginFailedUser,
        LoginFailedStartup,
        ResetSent,
        ResetError,
        RegistrationSuccessful,
        RegistrationError,
        AccountExpiredLocal,
        AccountExpiredRemote,
        AccountExpiredOffline,
        SubscriptionSuccessful,
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

        #endregion

        #region Statics 

        static string PRIVACY_POLICY_URL = $"{MpServerConstants.DOMAIN_URL}/legal/privacy.html";
        static string SUBSCRIBE_URI = $"{MpServerConstants.DOMAIN_URL}/accounts/subscribe.php";
        static string REGISTER_BASE_URL = $"{MpServerConstants.DOMAIN_URL}/accounts/register.php";
        static string LOGIN_BASE_URL = $"{MpServerConstants.DOMAIN_URL}/accounts/login.php";
        static string RESET_PASSWORD_BASE_URL = $"{MpServerConstants.DOMAIN_URL}/accounts/reset-request.php";

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
                string offline = AccountState == MpUserAccountState.Connected ?
                    string.Empty : $"{UiStrings.AccountOfflineLabel} ";
                string acct_name = IsRegistered ? AccountUsername : UiStrings.AccountUnregisteredLabel;
                int content_count = MpAvAccountTools.Instance.LastContentCount;
                int cap_count = MpAvAccountTools.Instance.GetContentCapacity(AccountType);
                if (AccountType != MpUserAccountType.Unlimited) {
                    return $"{AccountType} - (Total {content_count})";
                }
                return string.Format(
                    @"{0} [{1}] {2} - ({3} total {4} capacity {5} remaining)",
                    offline,
                    acct_name,
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

        public bool CanAttemptLogin =>
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
            MpAvPrefViewModel.Instance.LastLoginDateTimeUtc > DateTime.MinValue;

        public MpUserAccountType WorkingAccountType =>
            IsExpired ? MpUserAccountType.Free : AccountType;


        public bool IsSubscriptionDevice { get; private set; }
        public MpUserAccountState AccountState { get; private set; } = MpUserAccountState.Unregistered;
        #endregion

        #region Model

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

        public async Task ShowExistingAccountLoginWindowAsync() {
            MpDebug.Assert(MpAvWelcomeNotificationViewModel.Instance.IsWindowOpen, $"Existing acct window error. Only supposed to be used during welcome");

            AccountState = MpUserAccountState.Disconnected;
            var svm = MpAvSettingsViewModel.Instance;
            await svm.InitAsync();
            await svm.ShowSettingsWindowCommand.ExecuteAsync(MpSettingsTabType.Account);


            //while (true) {
            //    if (!svm.IsWindowOpen) {
            //        // closed window ie cancel
            //        ResetAccountPrefs();
            //        AccountState = MpUserAccountState.Unregistered;
            //        return MpUserAccountType.None;
            //    }
            //    if (AccountState == MpUserAccountState.Connected) {
            //        // login successful show confirmation and close window to return welcome
            //        ShowAccountNotficationAsync(MpAccountNtfType.ExistingLoginSuccessful).FireAndForgetSafeAsync();
            //        svm.IsWindowOpen = false;
            //        return AccountType;
            //    }
            //    await Task.Delay(300);
            //}
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

                        MpMessenger.SendGlobal(MpMessageType.AccountStateChanged);
                        MpMessenger.SendGlobal(MpMessageType.AccountInfoChanged);
                    }
                    break;
                case nameof(AccountState):
                    UpdateAccountViews();
                    switch (AccountState) {
                        case MpUserAccountState.Connected:
                            MpAvPrefViewModel.Instance.LastLoginDateTimeUtc = DateTime.UtcNow;

                            if (!MpAvPrefViewModel.Instance.IsWelcomeComplete &&
                                MpAvWindowManager.LocateWindow(MpAvSettingsViewModel.Instance) is MpAvWindow stw) {
                                Dispatcher.UIThread.Post(async () => {
                                    await ShowAccountNotficationAsync(MpAccountNtfType.ExistingLoginSuccessful);
                                    // close login window programmatically
                                    stw.Close();
                                    MpAvWelcomeNotificationViewModel.Instance.SelectNextPageCommand.Execute(null);
                                });
                            }
                            break;
                    }
                    MpMessenger.SendGlobal(MpMessageType.AccountStateChanged);
                    MpMessenger.SendGlobal(MpMessageType.AccountInfoChanged);
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsWindowOpened:
                    UpdateAccountViews();
                    break;
            }
        }
        private bool ProcessServerResponse(string response, out Dictionary<string, string> args) {
            response = response.ToStringOrEmpty();
            MpConsole.WriteLine($"Server response: '{response}'", level: MpLogLevel.Error);
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
            } else {
                msg_suffix = response;
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
            MpSettingsFrameType frameType = GetNtfParamFrame(ntf_type);
            var err_sb = new StringBuilder();
            foreach (var error_kvp in errors) {
                if (param_id_lookup.TryGetValue(error_kvp.Key, out string param_id)) {
                    if (MpAvSettingsViewModel.Instance.TryGetParamAndFrameViewModelsByParamId(frameType, param_id, out var param_tup)) {
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
        private MpSettingsFrameType GetNtfParamFrame(MpAccountNtfType ntfType) {
            switch (ntfType) {
                case MpAccountNtfType.ExistingLoginSuccessful:
                case MpAccountNtfType.LoginFailedStartup:
                case MpAccountNtfType.LoginFailedUser:
                case MpAccountNtfType.ResetSent:
                case MpAccountNtfType.ResetError:
                    return MpSettingsFrameType.Login;
                case MpAccountNtfType.RegistrationError:
                case MpAccountNtfType.RegistrationSuccessful:
                    return MpSettingsFrameType.Register;
                case MpAccountNtfType.AccountExpiredLocal:
                case MpAccountNtfType.AccountExpiredRemote:
                case MpAccountNtfType.AccountExpiredOffline:
                    return MpSettingsFrameType.Status;
                default:
                    return MpSettingsFrameType.None;
            }
        }
        private void UpdateAccountViews() {
            if (MpAvSettingsViewModel.Instance.FilteredAccountFrames == null) {
                return;
            }
            foreach (var afvm in MpAvSettingsViewModel.Instance.FilteredAccountFrames) {
                switch (afvm.FrameType) {
                    case MpSettingsFrameType.Register:
                        afvm.IsVisible = AccountState == MpUserAccountState.Unregistered;
                        break;
                    case MpSettingsFrameType.Login:
                        afvm.IsVisible = AccountState == MpUserAccountState.Disconnected;
                        break;
                    case MpSettingsFrameType.Status:
                        afvm.IsVisible = AccountState == MpUserAccountState.Connected;
                        UpdateStatusData(afvm);
                        break;
                }
            }
        }
        private void UpdateStatusData(MpAvSettingsFrameViewModel sfvm) {
            var status_args = new Dictionary<string, string>() {
                    {nameof(MpAvPrefViewModel.Instance.AccountUsername),MpAvPrefViewModel.Instance.AccountUsername },
                    {nameof(MpAvPrefViewModel.Instance.AccountEmail),MpAvPrefViewModel.Instance.AccountEmail },
                    {nameof(MpAvPrefViewModel.Instance.AccountType),MpAvPrefViewModel.Instance.AccountType.EnumToUiString() },
                    {nameof(MpAvPrefViewModel.Instance.AccountBillingCycleType), BillingCycleType.EnumToUiString() },
                    {nameof(MpAvPrefViewModel.Instance.AccountNextPaymentDateTime), NextPaymentDisplayValue },
                };

            foreach (var pvm in sfvm.Items) {
                if (status_args.TryGetValue(pvm.ParamId.ToStringOrEmpty(), out string param_val)) {
                    pvm.CurrentValue = param_val;
                } else {
                    MpConsole.WriteLine($"no reg status param for update '{pvm.ParamId}'");
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


        private async Task ShowAccountNotficationAsync(MpAccountNtfType ant, params string[] args) {
            string title = null;
            string msg = null;
            object icon = null;
            MpNotificationType ntf_type = MpNotificationType.None;
            switch (ant) {
                default:
                    return;
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
                    msg = string.Format(msg, AccountType.EnumToUiString(), NextPaymentDisplayValue);
                    ntf_type = MpNotificationType.SubscriptionExpired;
                    break;
                case MpAccountNtfType.LoginFailedUser:
                case MpAccountNtfType.LoginFailedStartup:
                    title = UiStrings.AccountLoginFailedTitle;
                    msg = UiStrings.AccountLoginFailedText;
                    icon = "ErrorImage";
                    if (ant == MpAccountNtfType.LoginFailedStartup) {
                        ntf_type = MpNotificationType.AccountLoginFailed;
                    }
                    break;
                case MpAccountNtfType.ExistingLoginSuccessful:
                    title = UiStrings.AccountExistingLoginTitle;
                    msg = string.Format(UiStrings.AccountExistingLoginText, AccountType.EnumToUiString(), BillingCycleType.EnumToUiString());
                    icon = "MonkeyWinkImage";
                    break;
                case MpAccountNtfType.RegistrationError:
                    title = UiStrings.AccountRegistrationFailedTitle;
                    msg = args[0];
                    icon = "ErrorImage";
                    break;
                case MpAccountNtfType.RegistrationSuccessful:
                    title = UiStrings.AccountRegistrationSuccessfulNtfTitle;
                    msg = UiStrings.AccountRegistrationSuccessText;
                    icon = "MonkeyWinkImage";
                    break;
                case MpAccountNtfType.ResetSent:
                    title = UiStrings.AccountResetPasswordLabel;
                    msg = UiStrings.AccountResetPasswordCaption;
                    icon = "MonkeyWinkImage";
                    break;
                case MpAccountNtfType.ResetError:
                    title = UiStrings.AccountResetPasswordErrorTitle;
                    msg = UiStrings.CommonConnectionFailedCaption;
                    icon = "ErrorImage";
                    break;
                case MpAccountNtfType.SubscriptionSuccessful:
                    title = UiStrings.AccountPurchaseSuccessfulTitle;
                    msg = string.Format(
                                UiStrings.AccountPurchaseSuccessfulCaption,
                                AccountType.EnumToUiString(),
                                IsMonthly ? UiStrings.AccountMonthlyLabel : UiStrings.AccountYearlyLabel);
                    icon = "MonkeyWinkImage";
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

        private void UpdateAccountState(MpUserAccountState acct_state, MpUserAccountType acct_type, DateTime expires, bool monthly, bool is_sub_device) {

            IsSubscriptionDevice = is_sub_device;
            AccountType = acct_type;
            NextPaymentUtc =
                AccountType == MpUserAccountType.Free ?
                    DateTime.MaxValue :
                    expires;
            BillingCycleType =
                AccountType == MpUserAccountType.Free ?
                    MpBillingCycleType.None :
                    monthly ?
                        MpBillingCycleType.Monthly : MpBillingCycleType.Yearly;
            AccountState = acct_state;
        }

        #endregion

        #region Commands


        public MpIAsyncCommand<object> SubscribeCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not MpUserAccountType uat) {
                    return;
                }

                MpSubscriptionFormat acct = await MpAvAccountTools.Instance.GetStoreUserLicenseInfoAsync();
                if (acct == MpSubscriptionFormat.Default) {
                    MpDebug.Break("Error cannot subscribe to free acct");
                    return;
                }
                var req_args = new Dictionary<string, string>() {
                    {"device_guid", MpDefaultDataModelTools.ThisUserDeviceGuid },
                    {"sub_type", acct.AccountType.ToString()},
                    {"monthly", acct.IsMonthly ? "1":"0"},
                    {"expires_utc_dt", acct.ToString()},
                };
                string resp = await MpHttpRequester.SubmitPostDataToUrlAsync(SUBSCRIBE_URI, req_args);
                bool success = ProcessServerResponse(resp, out var resp_args);
                await InitializeAsync();
                if (success) {
                    ShowAccountNotficationAsync(MpAccountNtfType.SubscriptionSuccessful).FireAndForgetSafeAsync();
                    return;
                }
#if DEBUG
                // acct should be updated only report errors for debug here
                MpDebug.Break($"Subscribe error");
#endif

            });

        public MpIAsyncCommand<object> LoginCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpLoginSourceType login_type = (MpLoginSourceType)args;

                SetButtonBusy(MpRuntimePrefParamType.AccountLogin, true);

                MpSubscriptionFormat acct = await MpAvAccountTools.Instance.GetStoreUserLicenseInfoAsync();
                bool is_sub_device = acct != MpSubscriptionFormat.Default;
                MpUserAccountType acct_type = AccountType;
                bool monthly = !IsYearly;
                DateTime expires = NextPaymentUtc;

                if (is_sub_device) {
                    // this device has active subscription
                    acct_type = acct.AccountType;
                    monthly = !acct.IsYearly;
                    expires = acct.ExpireOffsetUtc.DateTime;
                }

                var req_args = new Dictionary<string, (string, string)>() {
                    {"username", (nameof(MpAvPrefViewModel.Instance.AccountUsername),MpAvPrefViewModel.Instance.AccountUsername) },
                    {"password", (nameof(MpAvPrefViewModel.Instance.AccountPassword),MpAvPrefViewModel.Instance.AccountPassword) },
                    {"device_guid", (nameof(MpDefaultDataModelTools.ThisUserDeviceGuid), MpDefaultDataModelTools.ThisUserDeviceGuid) },
                    {"detail1", (null,MpAvPrefViewModel.arg1)},
                    {"detail2", (null,MpAvPrefViewModel.arg2)},
                    {"detail3", (null,MpAvPrefViewModel.arg3)},
                    //{"sub_type",(null, acct_type.ToString())},
                    //{"monthly", (null, monthly ? "1" : "0")},
                    //{"expires_utc_dt", (null, expires.ToString())},
                };
                if (is_sub_device) {
                    req_args.Add("sub_type", (null, acct_type.ToString()));
                    req_args.Add("monthly", (null, monthly ? "1" : "0"));
                    req_args.Add("expires_utc_dt", (null, expires.ToString()));
                }

                var sw = Stopwatch.StartNew();
                string resp = null;

                bool try_login = CanAttemptLogin;
                if (try_login && !MpAvPrefViewModel.Instance.IsWelcomeComplete && login_type != MpLoginSourceType.Click) {
                    // workaround to prevent login timer if trying existing account and fails or canceling during welcome
                    try_login = false;
                }

                if (try_login) {
                    // NOTE avoid unnecessary requests for unregistered users
                    resp = await MpHttpRequester.SubmitPostDataToUrlAsync(LOGIN_BASE_URL, req_args.ToDictionary(x => x.Key, x => x.Value.Item2));
                }

                SetButtonBusy(MpRuntimePrefParamType.AccountLogin, false);

                bool success = ProcessServerResponse(resp, out var resp_args);

                MpConsole.WriteLine($"login {success.ToTestResultLabel()}. Time: {sw.ElapsedMilliseconds}ms");
                MpUserAccountState acct_state = MpUserAccountState.Unregistered;
                if (success) {
                    MpDebug.Assert(resp_args != null, $"subscription error, login resp wrong");
                    acct_type = resp_args["sub_type"].ToEnum<MpUserAccountType>();
                    expires = DateTime.Parse(resp_args["expires_utc_dt"]);
                    monthly = resp_args["monthly"] == "1";

                    acct_state = MpUserAccountState.Connected;
                } else {
                    acct_state = IsRegistered ? MpUserAccountState.Disconnected : MpUserAccountState.Unregistered;
                }
                UpdateAccountState(acct_state, acct_type, expires, monthly, is_sub_device);
                StartExpirationTimer();
                if (success) {
                    return;
                }

                // user offline or unregistered

                if (login_type == MpLoginSourceType.Timer) {
                    return;
                }
                if (login_type == MpLoginSourceType.Init && !IsRegistered) {
                    return;
                }
                ProcessResponseErrors(
                    login_type == MpLoginSourceType.Init ? MpAccountNtfType.LoginFailedStartup : MpAccountNtfType.LoginFailedUser,
                    req_args.ToDictionary(x => x.Key, x => x.Value.Item1), resp_args);

                if (IsRegistered) {
                    StartAttemptLoginTimer();
                }
            }, (args) => {
                //return !IsLoggedIn;
                return !IsLoggedIn && args is MpLoginSourceType;
            });

        public MpIAsyncCommand RegisterCommand => new MpAsyncCommand(
            async () => {
                SetButtonBusy(MpRuntimePrefParamType.AccountRegister, true);
                var request_args = new Dictionary<string, (string, string)>() {
                    {"username", (nameof(MpAvPrefViewModel.Instance.AccountUsername),MpAvPrefViewModel.Instance.AccountUsername) },
                    {"email", (nameof(MpAvPrefViewModel.Instance.AccountEmail),MpAvPrefViewModel.Instance.AccountEmail) },
                    {"password", (nameof(MpAvPrefViewModel.Instance.AccountPassword),MpAvPrefViewModel.Instance.AccountPassword) },
                    {"confirm", (nameof(MpAvPrefViewModel.Instance.AccountPassword2), MpAvPrefViewModel.Instance.AccountPassword2) },
                };

                var sw = Stopwatch.StartNew();
                string response = await MpHttpRequester.SubmitPostDataToUrlAsync(REGISTER_BASE_URL, request_args.ToDictionary(x => x.Key, x => x.Value.Item2));
                SetButtonBusy(MpRuntimePrefParamType.AccountRegister, false);
                bool success = ProcessServerResponse(response, out var resp_args);

                MpConsole.WriteLine($"Registration {success.ToTestResultLabel()}. Time: {sw.ElapsedMilliseconds}ms");
                if (success) {
                    AccountState = MpUserAccountState.Disconnected;
                    ShowAccountNotficationAsync(MpAccountNtfType.RegistrationSuccessful).FireAndForgetSafeAsync();
                    return;
                }

                ProcessResponseErrors(MpAccountNtfType.RegistrationError, request_args.ToDictionary(x => x.Key, x => x.Value.Item1), resp_args);

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
                var req_args = new Dictionary<string, (string, string)>() {
                    {"username", (nameof(MpAvPrefViewModel.Instance.AccountUsername),MpAvPrefViewModel.Instance.AccountUsername) },
                };
                string resp = await MpHttpRequester.SubmitPostDataToUrlAsync(RESET_PASSWORD_BASE_URL, req_args.ToDictionary(x => x.Key, x => x.Value.Item2));
                SetButtonBusy(MpRuntimePrefParamType.AccountResetPassword, false);

                bool success = ProcessServerResponse(resp, out var resp_args);
                if (success) {
                    ShowAccountNotficationAsync(MpAccountNtfType.ResetSent).FireAndForgetSafeAsync();
                    return;
                }
                ProcessResponseErrors(MpAccountNtfType.ResetError, req_args.ToDictionary(x => x.Key, x => x.Value.Item1), resp_args);
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

        public ICommand ShowRegisterPanelCommand => new MpCommand(
            () => {
                AccountState = MpUserAccountState.Unregistered;
            });

        public ICommand ShowLoginPanelCommand => new MpCommand(
            () => {
                AccountState = MpUserAccountState.Disconnected;
            });

        #endregion
    }
}
