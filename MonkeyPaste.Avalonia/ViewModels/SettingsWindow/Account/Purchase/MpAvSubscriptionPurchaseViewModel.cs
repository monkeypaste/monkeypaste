using Avalonia.Threading;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvSubscriptionPurchaseViewModel : MpAvViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        const string ACCOUNT_HELP_URI = @"https://www.monkeypaste.com/help#recycling";
        #endregion

        #region Statics
        private static MpAvSubscriptionPurchaseViewModel _instance;
        public static MpAvSubscriptionPurchaseViewModel Instance => _instance ?? (_instance = new MpAvSubscriptionPurchaseViewModel());
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #region View Models
        public ObservableCollection<MpAvSubscriptionItemViewModel> Items { get; } = new ObservableCollection<MpAvSubscriptionItemViewModel>();

        public MpAvSubscriptionItemViewModel SelectedItem { get; set; }
        public MpAvSubscriptionItemViewModel UnlimitedItem =>
            Items.FirstOrDefault(x => x.AccountType == MpUserAccountType.Unlimited);
        #endregion
        #region Appearance


        #endregion

        #region State
        public bool IsSubscriptionPanelVisible { get; set; } = true;
        public bool IsMonthlyEnabled { get; set; } = false;
        public bool IsStoreAvailable =>
            !Items.All(x => string.IsNullOrEmpty(x.RateText) || x.RateText == MpAvAccountTools.EMPTY_RATE_TEXT);

        public bool CanBuy {
            get {
                if (SelectedItem == null ||
                    !IsStoreAvailable ||
                    SelectedItem.AccountType == MpUserAccountType.Free) {
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
        public MpAvSubscriptionPurchaseViewModel() {
            PropertyChanged += MpAvAccountViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            if (IsBusy) {
                return;
            }
            IsBusy = true;

            var sw = Stopwatch.StartNew();
            await MpAvAccountTools.Instance.RefreshAddOnInfoAsync();
            MpConsole.WriteLine($"AddOn Refresh: {sw.ElapsedMilliseconds}ms");
            Items.Clear();
            int test = typeof(MpUserAccountType).Length();
            for (int i = 0; i < 4; i++) {
                var aivm = await CreateAccountItemViewModelAsync((MpUserAccountType)i);
                Items.Add(aivm);
            }
            SelectedItem = Items.FirstOrDefault(x => x.AccountType == MpAvAccountViewModel.Instance.AccountType);
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(IsStoreAvailable));
            IsBusy = false;
        }
        public MpAvWelcomeOptionGroupViewModel ToWelcomeOptionGroup() {
            // NOTE yearly comes first
            var wogvm = new MpAvWelcomeOptionGroupViewModel(
                MpAvWelcomeNotificationViewModel.Instance,
                MpWelcomePageType.Account) {
                Title = UiStrings.WelcomeAccountTitle,
                Caption = UiStrings.WelcomeAccountCaption
            };
            wogvm.Items = new List<MpAvWelcomeOptionItemViewModel>();
            foreach (var item in Items) {
                wogvm.Items.Add(item.ToWelcomeOptionItem(false));
            }
            foreach (var item in Items) {
                wogvm.Items.Add(item.ToWelcomeOptionItem(true));
                wogvm.Items.Last().IsOptionVisible = false;
            }
            return wogvm;
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
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    break;
            }
        }

        private async Task<MpAvSubscriptionItemViewModel> CreateAccountItemViewModelAsync(MpUserAccountType acctType) {
            var aivm = new MpAvSubscriptionItemViewModel(this);
            await aivm.InitializeAsync(acctType);
            return aivm;
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsWindowOpened:
                    if (!IsStoreAvailable) {
                        InitializeAsync().FireAndForgetSafeAsync();
                    }
                    break;
            }
        }

        #endregion

        #region Commands
        public MpIAsyncCommand ReinitializeCommand => new MpAsyncCommand(
            async () => {
                Dispatcher.UIThread.VerifyAccess();
                await InitializeAsync();
            });

        public MpIAsyncCommand<object> PurchaseSubscriptionCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (MpAvPrefViewModel.Instance.AccountType != MpUserAccountType.Free) {
                    // TODO if there's an active subscription (on microsoft at least) 
                    // need to either automate or explain that current must be cancelled and then subscribe (i think)
                }
                bool is_monthly = false;
                MpUserAccountType purchase_uat = MpUserAccountType.None;
                if (args is object[] argParts &&
                    argParts[0] is MpUserAccountType welcome_purchase_uat &&
                    argParts[1] is bool welcome_is_monthly) {
                    purchase_uat = welcome_purchase_uat;
                    is_monthly = welcome_is_monthly;
                } else if (SelectedItem != null) {
                    purchase_uat = SelectedItem.AccountType;
                    is_monthly = IsMonthlyEnabled;
                }
                if (purchase_uat == MpUserAccountType.None) {
                    return;
                }


                // NOTE to work around login failures or no selection, just default to free i guess
                bool? success = await MpAvAccountTools.Instance.PurchaseSubscriptionAsync(purchase_uat, is_monthly);
                if (success.IsTrue()) {

                    if (purchase_uat != MpUserAccountType.Free) {
                        await Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                            title: UiStrings.AccountPurchaseSuccessfulTitle,
                            message: string.Format(
                                UiStrings.AccountPurchaseSuccessfulCaption,
                                purchase_uat.EnumToUiString(),
                                is_monthly ? UiStrings.AccountMonthlyLabel : UiStrings.AccountYearlyLabel),
                            iconResourceObj: "MonkeyWinkImage");
                    }

                    // refresh account vm w/ new license
                    await MpAvAccountViewModel.Instance.InitializeAsync();
                    return;
                }

                if (success.IsFalse()) {
                    // error
                    bool retry = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                        title: UiStrings.CommonErrorLabel,
                        message: UiStrings.AccountPurchaseErrorCaption,
                        iconResourceObj: "WarningImage");
                    if (retry) {
                        // opted to try again
                        await PurchaseSubscriptionCommand.ExecuteAsync(args);
                    }
                    return;
                }

            });

        public MpIAsyncCommand NavigateToBuyUpgradeCommand => new MpAsyncCommand(
            async () => {
                // open/activate settings window and select acct tab...
                await MpAvSettingsViewModel.Instance
                .ShowSettingsWindowCommand.ExecuteAsync(MpSettingsTabType.Account);

                // wait for store update
                while (IsBusy) {
                    await Task.Delay(100);
                }
                if (!IsStoreAvailable) {
                    // no store available
                    return;
                }
                // flag unlimited yearly to pulse
                IsMonthlyEnabled = false;
                SelectedItem = UnlimitedItem;
                UnlimitedItem.DoFocusPulse = true;
            });
        #endregion
    }
}
