using Avalonia.Controls;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        #endregion

        #region Constants
        const string SUCCESS_PREFIX = "[SUCCESS]";
        const string REGISTER_URL = "https://www.monkeypaste.com/accounts/register.php";
        const string LOGIN_URL = "https://www.monkeypaste.com/accounts/login.php";
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

        public bool IsLoggedIn =>
            AccountState == MpUserAccountState.Connected;
        public bool IsRegistered =>
            AccountState != MpUserAccountState.Unregistered;


        public MpUserAccountState AccountState { get; private set; } = MpUserAccountState.Unregistered;
        #endregion

        #region Model

        #region Request Args

        Dictionary<string, string> RegisterRequestArgs =>
            new Dictionary<string, string>() {
                    {"username", MpAvPrefViewModel.Instance.AccountUsername },
                    {"email", MpAvPrefViewModel.Instance.AccountEmail },
                    {"password", MpAvPrefViewModel.Instance.AccountPassword },
                    {"password2", MpAvPrefViewModel.Instance.AccountPassword2 },
                };


        #endregion

        public string AccountUsername =>
            MpAvPrefViewModel.Instance.AccountUsername;
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

        public bool IsActive =>
            DateTime.UtcNow < NextPaymentUtc;

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

            await LoginCommand.ExecuteAsync(null);
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
            ClearErrors();

            if (response.StartsWith(SUCCESS_PREFIX) &&
                response.SplitNoEmpty(SUCCESS_PREFIX) is string[] success_parts) {
                args = success_parts.Length == 1 ? null : MpJsonConverter.DeserializeObject<Dictionary<string, string>>(success_parts[1]);
                return true;
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
            foreach (var error_kvp in errors) {
                MpDebug.Assert(RegisterRequestArgs.ContainsKey(error_kvp.Key), $"Missing server input '{error_kvp.Key}'");
                string param_id = RegisterRequestArgs[error_kvp.Key];
                var param_tup = MpAvSettingsViewModel.Instance.GetParamAndFrameViewModelsByParamId(param_id);
                MpDebug.Assert(param_tup != null && param_tup.Item2 != null, $"Could not locate param w/ id '{param_id}'");
                param_tup.Item2.OverrideValidationMesage(error_kvp.Value);
            }

        }
        private void UpdateAccountViews() {
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
        #endregion

        #region Commands


        public MpIAsyncCommand<object> LoginCommand => new MpAsyncCommand<object>(
            async (args) => {
                // check platform store for subscription, if none found default is returned
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
                string resp = await MpHttpRequester.PostDataToUrlAsync(LOGIN_URL, req_args);

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

                    AccountState = MpUserAccountState.Connected;

                    return;
                }
                ProcessResponseErrors(resp_args);

                if (args is not Control anchor_c) {
                    // startup login, no msg
                    return;
                }
                bool retry = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                         title: UiStrings.CommonErrorLabel,
                         message: UiStrings.AccountLoginFailedText,
                         iconResourceObj: "WarningImage",
                         anchor: anchor_c);
                if (retry) {
                    // opted to try again
                    await LoginCommand.ExecuteAsync(args);
                }
            }, (args) => {
                //return !IsLoggedIn;
                return CanLogin;
            });


        public MpIAsyncCommand RegisterCommand => new MpAsyncCommand(
            async () => {
                var sw = Stopwatch.StartNew();
                string response = await MpHttpRequester.PostDataToUrlAsync(REGISTER_URL, RegisterRequestArgs);

                bool success = ProcessServerResponse(response, out var resp_args);

                MpConsole.WriteLine($"Registration {success.ToTestResultLabel()}. Time: {sw.ElapsedMilliseconds}ms");
                if (success) {
                    AccountState = MpUserAccountState.Disconnected;
                }

                ProcessResponseErrors(resp_args);
                bool retry = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                         title: UiStrings.CommonErrorLabel,
                         message: UiStrings.AccountRegistrationRegisterFailedText,
                         iconResourceObj: "WarningImage");
                if (retry) {
                    // opted to try again
                    await RegisterCommand.ExecuteAsync();
                }
            }, () => {
                return !IsRegistered;
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
