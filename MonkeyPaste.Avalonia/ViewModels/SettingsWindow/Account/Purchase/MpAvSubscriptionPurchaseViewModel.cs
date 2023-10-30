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
        static string ACCOUNT_HELP_URI = $"{MpServerConstants.DOMAIN_URL}/help#recycling";
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
                Caption = UiStrings.WelcomeAccountCaption,
                NeedsSkip = !IsStoreAvailable
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
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsMonthlyEnabled)));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.MatchesAccount)));
                    break;
                case nameof(SelectedItem):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.CanBuy)));
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
                bool is_monthly = false;
                MpAvSubscriptionItemViewModel purchase_vm = null;
                if (args is object[] argParts &&
                    argParts[0] is MpUserAccountType welcome_purchase_uat &&
                    argParts[1] is bool welcome_is_monthly) {
                    purchase_vm = Items.FirstOrDefault(x => x.AccountType == welcome_purchase_uat);
                    is_monthly = welcome_is_monthly;
                } else if (SelectedItem != null) {
                    purchase_vm = SelectedItem;
                    is_monthly = IsMonthlyEnabled;
                }
                if (purchase_vm == null) {
                    return;
                }
                MpUserAccountType purchase_uat = purchase_vm.AccountType;
                IsMonthlyEnabled = is_monthly;
                if (purchase_uat == MpUserAccountType.Free) {
                    return;
                }
                if (!purchase_vm.CanBuy) {
                    MpConsole.WriteLine($"Cannot buy {purchase_vm} monthly: {is_monthly}");
                    return;
                }

                if (!string.IsNullOrEmpty(purchase_vm.PrePurchaseMessage)) {
                    // theres store specific logic user should know before attempting purchase,
                    await Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                            title: UiStrings.AccountPrePurchaseNtfTitle,
                            message: purchase_vm.PrePurchaseMessage,
                            iconResourceObj: "WarningImage");
                }


                // NOTE to work around login failures or no selection, just default to free i guess
                bool? success = await MpAvAccountTools.Instance.PurchaseSubscriptionAsync(purchase_uat, is_monthly);
                if (success.IsTrue()) {


                    await MpAvAccountViewModel.Instance.SubscribeCommand.ExecuteAsync(purchase_uat);
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
