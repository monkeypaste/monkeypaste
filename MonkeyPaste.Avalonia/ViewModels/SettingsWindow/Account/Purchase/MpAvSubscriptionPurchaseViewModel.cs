﻿using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
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
        public bool IsMonthlyEnabled { get; set; } = false;
        public bool IsStoreAvailable { get; private set; }

        public bool IsSubscriptionTabSelected { get; set; } = true;
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
            IsStoreAvailable = await MpAvAccountTools.Instance.RefreshAddOnInfoAsync();
            MpConsole.WriteLine($"Store available: {IsStoreAvailable.ToTestResultLabel()} AddOn Refresh: {sw.ElapsedMilliseconds}ms");
            Items.Clear();
            int test = typeof(MpUserAccountType).Length();
            for (int i = 0; i < 4; i++) {
                var aivm = await CreateAccountItemViewModelAsync((MpUserAccountType)i);
                Items.Add(aivm);
            }
            // toggle so active is visible
            IsMonthlyEnabled = MpAvAccountViewModel.Instance.IsMonthly;

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
                //NeedsSkip = !IsStoreAvailable || MpAvAccountViewModel.Instance.AccountType.IsPaidType()
            };
            wogvm.Items = new List<MpAvWelcomeOptionItemViewModel>();
            foreach (var item in Items) {
                wogvm.Items.Add(item.ToWelcomeOptionItem(wogvm, false));
            }
            foreach (var item in Items) {
                wogvm.Items.Add(item.ToWelcomeOptionItem(wogvm, true));
                wogvm.Items.Last().IsOptionVisible = false;
            }
            return wogvm;
        }
        #endregion


        #region Private Methods

        private void MpAvAccountViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsMonthlyEnabled):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrialText)));
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
                case MpMessageType.AccountStateChanged:
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.MatchesAccount)));
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
                    Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                        title: UiStrings.CommonErrorLabel,
                        message: UiStrings.CommonErrorCodeText.Format(1.ToErrorCode()),
                        iconResourceObj: "WarningImage").FireAndForgetSafeAsync();
                    return;
                }
                if (!purchase_vm.CanBuy) {
                    Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                        title: UiStrings.CommonErrorLabel,
                        message: UiStrings.CommonErrorCodeText.Format(2.ToErrorCode()),
                        iconResourceObj: "WarningImage").FireAndForgetSafeAsync();
                    MpConsole.WriteLine($"Cannot buy {purchase_vm} monthly: {is_monthly}");
                    return;
                }

                // NOTE get purchase action info before changing account state
                string post_action_url = MpAvAccountTools.Instance.GetStoreSubscriptionUrl(
                    MpAvAccountViewModel.Instance.AccountType,
                    MpAvAccountViewModel.Instance.BillingCycleType == MpBillingCycleType.Monthly);
                string post_action_msg = purchase_vm.PostPurchaseActionMessage;


                // NOTE to work around login failures or no selection, just default to free i guess
                bool? success = await MpAvAccountTools.Instance.PurchaseSubscriptionAsync(purchase_uat, is_monthly);
                if (success.IsTrue()) {
                    await MpAvAccountViewModel.Instance.SubscribeCommand.ExecuteAsync(new object[] { purchase_uat, is_monthly });

                    if (!string.IsNullOrEmpty(post_action_msg)) {
                        // theres store specific logic user should know after purchase,
                        var result = await Mp.Services.PlatformMessageBox.ShowYesNoMessageBoxAsync(
                                title: UiStrings.AccountPrePurchaseNtfTitle,
                                message: post_action_msg,
                                iconResourceObj: "WarningImage");
                        if (result) {
                            MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(post_action_url);
                        }
                    }
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
                await Task.Delay(300);
                // flag unlimited yearly to pulse
                IsSubscriptionTabSelected = true;
                IsMonthlyEnabled = false;
                SelectedItem = UnlimitedItem;
                UnlimitedItem.DoFocusPulse = true;
            });
        #endregion
    }
}
