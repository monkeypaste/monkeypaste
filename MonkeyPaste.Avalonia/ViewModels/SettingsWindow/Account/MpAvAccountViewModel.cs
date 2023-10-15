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
        #endregion

        #region Model

        public string AccountUsername =>
            MpAvPrefViewModel.Instance.AccountUsername;
        public string AccountEmail =>
            MpAvPrefViewModel.Instance.AccountEmail;
        public string AccountPassword =>
            MpAvPrefViewModel.Instance.AccountPassword;


        public bool IsActive {
            get {
                if (UserAccount == null) {
                    return false;
                }
                return UserAccount.IsActive;
            }
        }

        public bool IsYearly {
            get {
                if (UserAccount == null) {
                    return false;
                }
                return UserAccount.IsYearly;
            }
        }

        public DateTimeOffset NextPaymentUtc {
            get {
                if (UserAccount == null) {
                    return DateTimeOffset.MaxValue;
                }
                return UserAccount.ExpireOffsetUtc;
            }
        }

        public MpBillingCycleType BillingCycleType {
            get {
                if (UserAccount == null) {
                    return MpBillingCycleType.None;
                }
                return UserAccount.BillingCycleType;
            }
        }

        public MpUserAccountType AccountType {
            get {
                if (UserAccount == null) {
                    return MpUserAccountType.None;
                }
                return UserAccount.AccountType;
            }
        }
        public MpSubscriptionFormat UserAccount { get; private set; }
        #endregion
        #endregion

        #region Constructors
        public MpAvAccountViewModel() : base() {
            RegistrationViewModel = new MpAvAccountRegistrationViewModel(this);
            LoginViewModel = new MpAvAccountLoginViewModel(this);
            InitializeAsync().FireAndForgetSafeAsync();
        }
        #endregion

        #region Public Methods
        public async Task InitializeAsync() {
            IsBusy = true;

            UserAccount = await MpAvAccountTools.Instance.GetUserSubscriptionAsync();

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
        #endregion

        #region Commands


        public ICommand ResetAccountPasswordCommand => new MpCommand(
            () => {

            }, () => {
                return IsRegistered;
            });

        //public MpIAsyncCommand LoginCommand => new MpAsyncCommand(
        //    async () => {
        //        WasLoginSuccessful = null;
        //        WasLoginSuccessful = await MpAvAccountTools.Instance.RegisterUserAsync(AccountEmail, AccountPassword, Remember, Parent.UserAccount);
        //        RefreshAccountPage();
        //    },
        //    () => {
        //        return CanAttempLogin;
        //    });

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
