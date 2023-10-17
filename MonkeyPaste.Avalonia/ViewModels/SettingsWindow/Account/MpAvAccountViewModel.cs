using MonkeyPaste.Common;
using System;
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
        #endregion

        #region Constants
        #endregion

        #region Statics 
        private static MpAvAccountViewModel _instance;
        public static MpAvAccountViewModel Instance => _instance ?? (_instance = new MpAvAccountViewModel());

        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public MpAvAccountRegistrationViewModel RegistrationViewModel { get; }
        public MpAvAccountLoginViewModel LoginViewModel { get; }
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
        public string LoginResultMessage =>
            WasLoginSuccessful.IsNull() ?
                string.Empty :
                WasLoginSuccessful.Value ?
                    UiStrings.AccountLoginSuccessfulText :
                    UiStrings.AccountLoginFailedText;

        #endregion

        #region State

        public bool HasBillingCycle =>
            BillingCycleType == MpBillingCycleType.Monthly ||
            BillingCycleType == MpBillingCycleType.Yearly;

        public bool IsPaymentPastDue =>
            HasBillingCycle && DateTime.UtcNow > NextPaymentUtc;

        public bool IsLoggedIn =>
            UserAccount != MpSubscriptionFormat.Default;

        public MpUserPageType CurrentPageType { get; set; }
        public bool IsRegistered =>
            !string.IsNullOrEmpty(AccountEmail) &&
            !string.IsNullOrEmpty(AccountPassword);

        public bool? WasLoginSuccessful { get; set; }

        public bool CanLogin =>
            !WasLoginSuccessful.IsTrue() &&
            !string.IsNullOrEmpty(AccountUsername) &&
            !string.IsNullOrEmpty(AccountPassword);
        #endregion

        #region Model

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

        public bool IsActive {
            get {
                if (UserAccount == null) {
                    return false;
                }
                return UserAccount.IsActive;
            }
        }

        public bool IsYearly =>
            BillingCycleType == MpBillingCycleType.Yearly;

        //public DateTimeOffset NextPaymentUtc {
        //    get {
        //        if (UserAccount == null) {
        //            return DateTimeOffset.MaxValue;
        //        }
        //        return UserAccount.ExpireOffsetUtc;
        //    }
        //}

        //public MpBillingCycleType BillingCycleType {
        //    get {
        //        if (UserAccount == null) {
        //            return MpBillingCycleType.None;
        //        }
        //        return UserAccount.BillingCycleType;
        //    }
        //}

        //public MpUserAccountType AccountType {
        //    get {
        //        if (UserAccount == null) {
        //            return MpUserAccountType.None;
        //        }
        //        return UserAccount.AccountType;
        //    }
        //}
        public MpSubscriptionFormat UserAccount { get; private set; }
        #endregion
        #endregion

        #region Constructors
        public MpAvAccountViewModel() : base() {
            MpDebug.Assert(_instance == null, $"Account singleton error");
            RegistrationViewModel = new MpAvAccountRegistrationViewModel(this);
            LoginViewModel = new MpAvAccountLoginViewModel(this);
        }
        #endregion

        #region Public Methods
        public async Task InitializeAsync() {
            IsBusy = true;

            var acct = await MpAvAccountTools.Instance.GetUserSubscriptionAsync();
            if (acct != default) {
                AccountType = acct.AccountType;
                NextPaymentUtc = acct.ExpireOffsetUtc.UtcDateTime;
                BillingCycleType =
                    acct.AccountType == MpUserAccountType.Free ?
                        MpBillingCycleType.None :
                        acct.IsMonthly ?
                            MpBillingCycleType.Monthly :
                            MpBillingCycleType.Yearly;
            }

            await LoginCommand.ExecuteAsync();

            RefreshAccountPage();

            IsBusy = false;
        }

        public void RefreshAccountPage(MpUserPageType forcePage = MpUserPageType.None) {
            if (forcePage != MpUserPageType.None) {
                CurrentPageType = forcePage;
                return;
            }
            if (IsRegistered) {
                if (IsLoggedIn) {
                    CurrentPageType = MpUserPageType.Status;
                } else {
                    CurrentPageType = MpUserPageType.Login;
                }
            } else {
                CurrentPageType = MpUserPageType.Register;
            }
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void SetAccountType(MpUserAccountType newType) {
            // NOTE this maybe a good all around interface method, not sure though
            bool changed = AccountType != newType;
            if (changed) {
                bool is_upgrade = (int)newType > (int)AccountType;
                AccountType = newType;
                MpMessenger.SendGlobal(is_upgrade ? MpMessageType.AccountUpgrade : MpMessageType.AccountDowngrade);
            }
        }
        #endregion

        #region Commands


        public ICommand ResetAccountPasswordCommand => new MpCommand(
            () => {

            }, () => {
                return IsRegistered;
            });

        public MpIAsyncCommand LoginCommand => new MpAsyncCommand(
            async () => {
                WasLoginSuccessful = await MpAvAccountTools.Instance.LoginUserAsync();
                MpConsole.WriteLine($"login {WasLoginSuccessful.Value.ToTestResultLabel()}");
                RefreshAccountPage();
            }, () => {
                return CanLogin;
            });

        public ICommand LogOutCommand => new MpCommand(
            () => {
                //MpAvPrefViewModel.Instance.AccountEmail = null;
                //MpAvPrefViewModel.Instance.AccountPassword = null;
                UserAccount = MpSubscriptionFormat.Default;
                CurrentPageType = MpUserPageType.Login;
            }, () => {
                return IsLoggedIn;
            });

        #endregion
    }
}
