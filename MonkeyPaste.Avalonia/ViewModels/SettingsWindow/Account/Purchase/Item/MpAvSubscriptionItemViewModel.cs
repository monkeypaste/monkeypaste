using System;
using System.Threading.Tasks;
namespace MonkeyPaste.Avalonia {
    public static class MpAvAccountExtensions {
        public static bool IsPaidType(this MpUserAccountType uat) {
            return uat == MpUserAccountType.Basic || uat == MpUserAccountType.Unlimited;
        }
    }
    public class MpAvSubscriptionItemViewModel :
        MpAvViewModelBase<MpAvSubscriptionPurchaseViewModel>,
        MpAvIPulseViewModel {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region Appearance
        public object IconSourceObj {
            get {
                switch (AccountType) {
                    case MpUserAccountType.None:
                        return "LoginImage";
                    case MpUserAccountType.Free:
                        return "StarGoldImage";
                    case MpUserAccountType.Basic:
                        return "StarYellow2Image";
                    case MpUserAccountType.Unlimited:
                        return "TrophyImage";
                    default:
                        return $"QuestionMarkImage";
                }
            }
        }
        public string LabelText {
            get {
                switch (AccountType) {
                    case MpUserAccountType.None:
                        return UiStrings.WelcomeAccountRestoreLabel;
                    case MpUserAccountType.Free:
                        return UiStrings.WelcomeAccountFreeLabel;
                    case MpUserAccountType.Basic:
                        return UiStrings.WelcomeAccountStandardLabel;
                    case MpUserAccountType.Unlimited:
                        return UiStrings.WelcomeAccountUnlimitedLabel;
                    default:
                        return $"Unknown label for type '{AccountType}'";
                }
            }
        }

        string DescriptionTemplate {
            get {
                switch (AccountType) {
                    case MpUserAccountType.None:
                        return UiStrings.WelcomeAccountRestoreDescription;
                    case MpUserAccountType.Free:
                        return UiStrings.AccountFreeDescription;
                    case MpUserAccountType.Basic:
                        return UiStrings.AccountStandardDescription;
                    case MpUserAccountType.Unlimited:
                        return UiStrings.AccountUnlimitedDescription;
                    default:
                        return $"Unknown description for type '{AccountType}'";
                }
            }
        }

        public string DescriptionText {
            get {
                return string.Format(
                    DescriptionTemplate,
                    MpAvAccountTools.Instance.GetContentCapacity(MpUserAccountType.Free),
                    MpAvAccountTools.Instance.GetTrashCapacity(MpUserAccountType.Free),
                    MpAvAccountTools.Instance.GetContentCapacity(MpUserAccountType.Basic));
            }
        }
        string MonthlyRateText =>
            AccountType.IsPaidType() ?
            MpAvAccountTools.Instance.GetAccountRate(AccountType, true) :
            AccountType == MpUserAccountType.None ?
                string.Empty :
                UiStrings.AccountFreePriceText;

        string YearlyRateText =>
            AccountType.IsPaidType() ?
            MpAvAccountTools.Instance.GetAccountRate(AccountType, false) :
            AccountType == MpUserAccountType.None ?
                string.Empty :
                UiStrings.AccountFreePriceText;

        public string RateText =>
            IsMonthlyEnabled ?
                MonthlyRateText :
                YearlyRateText;

        string MonthlyTrialText =>
            HasMonthlyTrial ?
                string.Format(UiStrings.AccountFreeTrialLabel, MonthlyTrialDayCount) :
                string.Empty;

        string YearlyTrialText =>
            HasYearlyTrial ?
                string.Format(UiStrings.AccountFreeTrialLabel, YearlyTrialDayCount) :
                string.Empty;

        public string TrialText =>
            IsMonthlyEnabled ?
                MonthlyTrialText :
                YearlyTrialText;


        public string PostPurchaseActionMessage {
            get {
                if (!CanBuy ||
                    MpAvAccountViewModel.Instance.IsFree ||
                    !OperatingSystem.IsWindows()) {
                    return string.Empty;
                }
                // microsoft doesn't allow directly change subscription
                // need to cancel current and then buy new (or I guess it won't work? should test...don't pay though?)
                // from https://learn.microsoft.com/en-us/windows/uwp/monetize/enable-subscription-add-ons-for-your-app#unsupported-scenarios
                return string.Format(
                    UiStrings.AccountPrePurchaseWindowsNtfCaption,
                    MpAvAccountViewModel.Instance.AccountType.EnumToUiString(),
                    MpAvAccountViewModel.Instance.BillingCycleType.EnumToUiString());
            }
        }

        public int ClipCapCount =>
            MpAvAccountTools.Instance.GetContentCapacity(AccountType);

        public int TrashCapCount =>
            MpAvAccountTools.Instance.GetTrashCapacity(AccountType);
        #endregion

        #region State
        public bool DoFocusPulse { get; set; }

        public bool IsMonthlyEnabled =>
            Parent != null && Parent.IsMonthlyEnabled;

        public bool IsVisible =>
            AccountType != MpUserAccountType.None;

        public bool IsSelected {
            get => Parent == null || Parent.SelectedItem != this ? false : true;
            set {
                if (IsSelected != value && value && Parent != null) {
                    Parent.SelectedItem = this;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }


        public bool IsHovering { get; set; }

        public bool IsTrialAvailable => true;
        public bool HasTrial =>
            IsMonthlyEnabled ?
                HasMonthlyTrial :
                HasYearlyTrial;

        bool HasMonthlyTrial =>
            MonthlyTrialDayCount > 0;
        bool HasYearlyTrial =>
            YearlyTrialDayCount > 0;

        int MonthlyTrialDayCount =>
            MpAvAccountTools.Instance.GetSubscriptionTrialLength(AccountType, true);

        int YearlyTrialDayCount =>
            MpAvAccountTools.Instance.GetSubscriptionTrialLength(AccountType, false);

        int MonthlyPriority =>
            MpAvAccountTools.Instance.GetAccountPriority(AccountType, true);
        int YearlyPriority =>
            MpAvAccountTools.Instance.GetAccountPriority(AccountType, false);
        public bool IsUnlimited =>
            AccountType == MpUserAccountType.Unlimited;

        bool CanBuyMonthly {
            get {
                if (Parent == null ||
                    !Parent.IsStoreAvailable) {
                    return false;
                }
                if (AccountType == MpUserAccountType.None &&
                    !MpAvAccountViewModel.Instance.IsRegistered) {
                    // allow restore account
                    return true;
                }
                return MonthlyPriority > MpAvAccountViewModel.Instance.AccountPriority;
            }
        }
        bool CanBuyYearly {
            get {
                if (Parent == null ||
                    !Parent.IsStoreAvailable) {
                    return false;
                }
                if (AccountType == MpUserAccountType.None &&
                    !MpAvAccountViewModel.Instance.IsRegistered) {
                    // allow restore account
                    return true;
                }
                return YearlyPriority > MpAvAccountViewModel.Instance.AccountPriority;
            }
        }
        public bool CanBuy {
            get {
                return IsMonthlyEnabled ? CanBuyMonthly : CanBuyYearly;
            }
        }

        public bool MatchesAccount =>
            AccountType == MpAvAccountViewModel.Instance.AccountType &&
            IsMonthlyEnabled == MpAvAccountViewModel.Instance.IsMonthly;

        #endregion

        #region Model
        public MpUserAccountType AccountType { get; private set; }
        #endregion

        #endregion

        #region Constructors
        public MpAvSubscriptionItemViewModel(MpAvSubscriptionPurchaseViewModel parent) : base(parent) {
            PropertyChanged += MpAvAccountItemViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpUserAccountType accountType) {
            await Task.Delay(1);
            AccountType = accountType;
            OnPropertyChanged(nameof(MonthlyRateText));
            OnPropertyChanged(nameof(YearlyRateText));
            OnPropertyChanged(nameof(IconSourceObj));
        }
        public MpAvWelcomeOptionItemViewModel ToWelcomeOptionItem(MpAvWelcomeOptionGroupViewModel group, bool isMonthly) {
            var woivm = new MpAvWelcomeOptionItemViewModel(MpAvWelcomeNotificationViewModel.Instance, AccountType, group) {
                IconSourceObj = IconSourceObj,
                LabelText = LabelText,
                LabelText2 = isMonthly ? MonthlyTrialText : YearlyTrialText,
                DescriptionText = DescriptionText,
                DescriptionText2 = isMonthly ? MonthlyRateText : YearlyRateText,
                //IsEnabled = isMonthly ? CanBuyMonthly : CanBuyYearly,
                IsEnabled = true,
                IsChecked = AccountType == MpUserAccountType.Unlimited && !isMonthly
            };

            if (MpAvAccountViewModel.Instance.AccountType != MpUserAccountType.Free &&
                MpAvAccountViewModel.Instance.AccountType == AccountType &&
                MpAvAccountViewModel.Instance.IsMonthly == isMonthly) {
                // mark plan as active, no need for trial info
                woivm.LabelText3 = UiStrings.AccountActiveLabel;
                woivm.LabelText2 = null;
            }
            return woivm;
        }

        public override string ToString() {
            return $"Subscription Type: {AccountType}";
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpAvAccountItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    break;
                case nameof(DoFocusPulse):
                    MpAvThemeViewModel.Instance.HandlePulse(this);
                    break;
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
