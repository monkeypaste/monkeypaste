using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSubcriptionPurchaseViewModel : MpAvViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        const string ACCOUNT_HELP_URI = @"https://www.monkeypaste.com/help#recycling";
        #endregion

        #region Statics
        private static MpAvSubcriptionPurchaseViewModel _instance;
        public static MpAvSubcriptionPurchaseViewModel Instance => _instance ?? (_instance = new MpAvSubcriptionPurchaseViewModel());
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #region View Models
        public ObservableCollection<MpAvSubscriptionItemViewModel> Items { get; } = new ObservableCollection<MpAvSubscriptionItemViewModel>();

        public MpAvSubscriptionItemViewModel SelectedItem =>
            Items.FirstOrDefault(x => x.IsChecked);
        #endregion
        #region Appearance


        #endregion

        #region State
        public bool IsSubscriptionPanelVisible { get; set; } = true;
        public bool IsMonthlyEnabled { get; set; } = false;
        public bool CanBuy {
            get {
                if (SelectedItem == null) {
                    return false;
                }
                var ua = MpAvAccountViewModel.Instance;
                if (ua.IsYearly && ua.IsActive) {
                    return false;
                }
                if ((int)SelectedItem.AccountType > (int)ua.AccountType) {
                    // allow higher
                    return true;
                }
                if ((int)SelectedItem.AccountType == (int)ua.AccountType) {
                    if (!ua.IsYearly && !IsMonthlyEnabled) {
                        // allow monthly to yearly
                        return true;
                    }

                }
                return false;
            }
        }
        #endregion

        #region Model
        #endregion

        #endregion

        #region Constructors
        public MpAvSubcriptionPurchaseViewModel() {
            PropertyChanged += MpAvAccountViewModel_PropertyChanged;
            InitializeAsync().FireAndForgetSafeAsync();
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            IsBusy = true;

            await MpAvAccountTools.Instance.RefreshPricingInfoAsync();

            Items.Clear();

            var acct_vml = await Task.WhenAll(
                new[] {
                    MpUserAccountType.None,
                    MpUserAccountType.Free,
                    MpUserAccountType.Standard,
                    MpUserAccountType.Unlimited,
                }
                .Select(x => CreateAccountItemViewModelAsync(x)));
            acct_vml.ForEach(x => Items.Add(x));

            Items.ForEach(x => x.IsChecked = x.AccountType == Mp.Services.AccountTools.CurrentAccountType);
            OnPropertyChanged(nameof(Items));
            IsBusy = false;
        }
        public MpAvWelcomeOptionGroupViewModel ToWelcomeOptionGroup(bool isMonthly) {
            return new MpAvWelcomeOptionGroupViewModel(
                MpAvWelcomeNotificationViewModel.Instance,
                MpWelcomePageType.Account) {
                Title = UiStrings.WelcomeAccountTitle,
                Caption = UiStrings.WelcomeAccountCaption,
                Items = Items.Select(x => x.ToWelcomeOptionItem(isMonthly)).ToList()
            };
        }
        #endregion


        #region Private Methods

        private void MpAvAccountViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsMonthlyEnabled):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.RateText)));
                    break;
                case nameof(SelectedItem):
                    OnPropertyChanged(nameof(CanBuy));
                    break;
            }
        }

        private async Task<MpAvSubscriptionItemViewModel> CreateAccountItemViewModelAsync(MpUserAccountType acctType) {
            var aivm = new MpAvSubscriptionItemViewModel(this);
            await aivm.InitializeAsync(acctType);
            return aivm;
        }
        #endregion

        #region Commands
        public ICommand UpgradeAccountCommand => new MpAsyncCommand(
            async () => {
                // TODO if there's an active subscription (on microsoft at least) 
                // need to either automate or explain that current must be cancelled and then subscribe (i think)

                await MpAvAccountTools.Instance.PurchaseSubscriptionAsync(SelectedItem.AccountType, IsMonthlyEnabled);
            });
        #endregion
    }
}
