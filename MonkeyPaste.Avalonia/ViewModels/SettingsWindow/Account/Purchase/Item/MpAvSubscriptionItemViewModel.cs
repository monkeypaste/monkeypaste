using Avalonia.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvSubscriptionItemViewModel : MpAvViewModelBase<MpAvSubscriptionPurchaseViewModel> {
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
                    case MpUserAccountType.Standard:
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
                    case MpUserAccountType.Standard:
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
                        return UiStrings.WelcomeAccountFreeDescription;
                    case MpUserAccountType.Standard:
                        return UiStrings.WelcomeAccountStandardDescription;
                    case MpUserAccountType.Unlimited:
                        return UiStrings.WelcomeAccountUnlimitedDescription;
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
                    MpAvAccountTools.Instance.GetTrashCapacity(MpUserAccountType.Standard));
            }
        }
        string MonthlyRateText =>
            MpAvAccountTools.Instance.GetAccountRate(AccountType, true);

        string YearlyRateText =>
            MpAvAccountTools.Instance.GetAccountRate(AccountType, false);

        public string RateText =>
            IsMonthlyEnabled ?
                MonthlyRateText :
                YearlyRateText;

        string MonthlyTrialText =>
            HasMonthlyTrial ?
                string.Format(UiStrings.AccountFreeTrialLabel, MonthlyTrialDayCount) :
                null;

        string YearlyTrialText =>
            HasYearlyTrial ?
                string.Format(UiStrings.AccountFreeTrialLabel, YearlyTrialDayCount) :
                null;

        public string TrialText =>
            IsMonthlyEnabled ?
                MonthlyTrialText :
                YearlyTrialText;

        #endregion

        #region State
        public bool DoFocusPulse { get; set; }

        public bool IsMonthlyEnabled =>
            Parent != null && Parent.IsMonthlyEnabled;

        public bool IsVisible =>
            AccountType != MpUserAccountType.None;

        public bool IsChecked { get; set; }
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
        public MpAvWelcomeOptionItemViewModel ToWelcomeOptionItem(bool isMonthly) {
            return new MpAvWelcomeOptionItemViewModel(MpAvWelcomeNotificationViewModel.Instance, AccountType) {
                IconSourceObj = IconSourceObj,
                LabelText = LabelText,
                LabelText2 = isMonthly ? MonthlyTrialText : YearlyTrialText,
                DescriptionText = DescriptionText,
                DescriptionText2 = isMonthly ? MonthlyRateText : YearlyRateText,
                IsChecked = AccountType == MpUserAccountType.Unlimited && !isMonthly
            };
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpAvAccountItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsChecked):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    break;
                case nameof(DoFocusPulse):
                    if (!DoFocusPulse) {
                        break;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        var sw = Stopwatch.StartNew();
                        while (sw.ElapsedMilliseconds < MpAvThemeViewModel.Instance.FocusPulseDurMs) {
                            await Task.Delay(100);
                        }
                        DoFocusPulse = false;
                    });
                    break;
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
