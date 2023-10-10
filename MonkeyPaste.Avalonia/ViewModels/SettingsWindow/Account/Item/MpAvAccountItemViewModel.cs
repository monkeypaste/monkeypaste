using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAccountItemViewModel : MpAvViewModelBase<MpAvAccountViewModel> {
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
                        return new object[] { "bronze", "StarYellowImage" };
                    case MpUserAccountType.Standard:
                        return new object[] { "gold1", "StarYellowImage" };
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

        public string DescriptionTemplate {
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
        public string MonthlyRateText =>
            Mp.Services.AccountTools.GetAccountRate(AccountType, true);

        public string YearlyRateText =>
            Mp.Services.AccountTools.GetAccountRate(AccountType, false);

        public string RateText =>
            Parent == null ||
            Parent.IsMonthlyEnabled ?
                MonthlyRateText :
                YearlyRateText;
        public string DescriptionText {
            get {
                return string.Format(
                    DescriptionTemplate,
                    Mp.Services.AccountTools.GetContentCapacity(MpUserAccountType.Free),
                    Mp.Services.AccountTools.GetTrashCapacity(MpUserAccountType.Free),
                    Mp.Services.AccountTools.GetTrashCapacity(MpUserAccountType.Standard));
            }
        }

        #endregion

        #region State
        public bool IsVisible =>
            AccountType != MpUserAccountType.None;
        public bool IsChecked { get; set; }
        public bool IsHovering { get; set; }
        #endregion

        #region Model
        public MpUserAccountType AccountType { get; private set; }
        #endregion

        #endregion

        #region Constructors
        public MpAvAccountItemViewModel(MpAvAccountViewModel parent) : base(parent) {
            PropertyChanged += MpAvAccountItemViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpUserAccountType accountType) {
            await Task.Delay(1);
            AccountType = accountType;
            OnPropertyChanged(nameof(IconSourceObj));
        }
        public MpAvWelcomeOptionItemViewModel ToWelcomeOptionItem(bool isMonthly) {
            return new MpAvWelcomeOptionItemViewModel(MpAvWelcomeNotificationViewModel.Instance, AccountType) {
                IconSourceObj = IconSourceObj,
                LabelText = LabelText,
                DescriptionText = DescriptionText,
                IsChecked = AccountType == MpUserAccountType.Unlimited
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
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
