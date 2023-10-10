using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAccountViewModel : MpAvViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        const string ACCOUNT_HELP_URI = @"https://www.monkeypaste.com/help#recycling";
        #endregion

        #region Statics
        private static MpAvAccountViewModel _instance;
        public static MpAvAccountViewModel Instance => _instance ?? (_instance = new MpAvAccountViewModel());
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #region View Models
        public ObservableCollection<MpAvAccountItemViewModel> Items { get; } = new ObservableCollection<MpAvAccountItemViewModel>();

        public MpAvAccountItemViewModel SelectedItem =>
            Items.FirstOrDefault(x => x.IsChecked);
        #endregion
        #region Appearance


        #endregion

        #region State
        public MpUserAccountType CurrentAccountType =>
            Mp.Services.AccountTools.CurrentAccountType;

        public bool IsSubscriptionPanelVisible { get; set; } = true;
        public bool IsMonthlyEnabled { get; set; } = true;
        public bool IsContentAddPausedByAccount { get; private set; }
        public bool CanBuy {
            get {
                if (SelectedItem == null) {
                    return false;
                }
                if (UserAccount.IsYearly && UserAccount.IsActive) {
                    return false;
                }
                if ((int)SelectedItem.AccountType > (int)UserAccount.AccountType) {
                    // allow higher
                    return true;
                }
                if ((int)SelectedItem.AccountType == (int)UserAccount.AccountType) {
                    if (UserAccount.IsMonthly && !IsMonthlyEnabled) {
                        // allow monthly to yearly
                        return true;
                    }

                }
                return false;
            }
        }
        #endregion

        #region Model
        public MpUserAccountFormat UserAccount { get; private set; }
        #endregion

        #endregion

        #region Constructors
        public MpAvAccountViewModel() {
            PropertyChanged += MpAvAccountViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            IsBusy = true;

            UserAccount = await MpAvAccountTools.Instance.GetUserAccountAsync();

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

        private async Task<MpAvAccountItemViewModel> CreateAccountItemViewModelAsync(MpUserAccountType acctType) {
            var aivm = new MpAvAccountItemViewModel(this);
            await aivm.InitializeAsync(acctType);
            return aivm;
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsWindowOpened:
                    Items.ForEach(x => x.IsChecked = x.AccountType == Mp.Services.AccountTools.CurrentAccountType);
                    MpAvAccountTools.Instance.RefreshPricingInfoAsync().FireAndForgetSafeAsync();
                    break;
            }
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
