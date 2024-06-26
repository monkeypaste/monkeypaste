﻿using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if WAP
using Windows.Services.Store;
#endif

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAccountTools {
#if WAP
        #region Private Variables
        private bool _isContextWindowInitialized = false;
        //private string _WindowsStoreId = "9MZRBMH3JT75";
        private string _WindowsStoreId = "9MW0FPDSHMJK";
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion


        #region Properties

        StoreContext _context;
        StoreContext context {
            get {
                if (_context == null) {
                    _context = StoreContext.GetDefault();
                }

                if (!_isContextWindowInitialized &&
                    MpAvWindowManager.PrimaryHandle != IntPtr.Zero) {
                    // from https://learn.microsoft.com/en-us/windows/uwp/monetize/in-app-purchases-and-trials#desktop
                    WinRT.Interop.InitializeWithWindow.Initialize(_context, MpAvWindowManager.PrimaryHandle);
                    _isContextWindowInitialized = true;
                }

                return _context;
            }
        }

        public string RateAppUri =>
            $"ms-windows-store://review/?PFN={Windows.ApplicationModel.Package.Current.Id.FamilyName}";

        public string ThisProductUri =>
            $"ms-windows-store://pdp/?productid={_WindowsStoreId}";

        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        public string GetStoreSubscriptionUrl(MpUserAccountType uat, bool isMonthly) {
            var kvp = AccountTypeAddOnStoreIdLookup.FirstOrDefault(x => x.Value.Item1 == uat && x.Value.Item2 == isMonthly);
            if (kvp.IsDefault()) {
                return null;
            }
            return $"https://account.microsoft.com/services/{kvp.Key}/details#billing";
        }

        public async Task<bool> RefreshAddOnInfoAsync() {
            AccountTypeTrialAvailabilityLookup.Clear();
            AccountTypePriceLookup.Clear();

            StoreProductQueryResult spqr =
                await context.GetAssociatedStoreProductsAsync(new string[] { "Durable" });

            if (spqr.ExtendedError != null && !string.IsNullOrEmpty(spqr.ExtendedError.Message)) {
                MpConsole.WriteLine($"AddOn Error: {spqr.ExtendedError}");
                return false;
            }

            foreach (var at in AccountTypeAddOnStoreIdLookup) {
                var spkvp = spqr.Products.FirstOrDefault(x => x.Value.StoreId == at.Key);
                if (spkvp.IsDefault()) {
                    MpDebug.Break($"AddOn not found StoreId: '{at.Key}'", silent: true);
                    continue;
                }

                StoreProduct sp = spkvp.Value;
                AccountTypePriceLookup.AddOrReplace(at.Value, sp.Price.FormattedRecurrencePrice);

                if (sp.Skus.Count < 2) {
                    // no trial available
                    // example https://learn.microsoft.com/en-us/windows/uwp/monetize/enable-subscription-add-ons-for-your-app#purchase-a-subscription-add-on
                    continue;
                }
                var trial_sku = sp.Skus[0];
                if (trial_sku.SubscriptionInfo.HasTrialPeriod) {
                    // presumes either weekly or monthly (currently 1 week)
                    int unit_to_days =
                        trial_sku.SubscriptionInfo.TrialPeriodUnit == StoreDurationUnit.Week ?
                            7 :
                            30;
                    int trial_day_count = (int)trial_sku.SubscriptionInfo.TrialPeriod * unit_to_days;
                    AccountTypeTrialAvailabilityLookup.AddOrReplace(at.Value, trial_day_count);
                }
            }
            return true;
        }

        public async Task<MpSubscriptionFormat> GetStoreUserLicenseInfoAsync() {
            // get users current ms store state
            StoreAppLicense appLicense = await context.GetAppLicenseAsync();
            KeyValuePair<string, StoreLicense> user_storeid_license_kvp = default;
            if (appLicense
                .AddOnLicenses
                .Where(x => x.Value.IsActive && AccountTypeAddOnStoreIdLookup.ContainsKey(ParseSkuStoreId(x)))
                .OrderByDescending(x => GetAccountPriority(AccountTypeAddOnStoreIdLookup[ParseSkuStoreId(x)].Item1, AccountTypeAddOnStoreIdLookup[ParseSkuStoreId(x)].Item2))
                .FirstOrDefault() is var active_kvp) {
                // found most significant active license 
                user_storeid_license_kvp = active_kvp;
            } else if (appLicense
                .AddOnLicenses
                .Where(x => !x.Value.IsActive && AccountTypeAddOnStoreIdLookup.ContainsKey(ParseSkuStoreId(x)))
                .OrderByDescending(x => GetAccountPriority(AccountTypeAddOnStoreIdLookup[ParseSkuStoreId(x)].Item1, AccountTypeAddOnStoreIdLookup[ParseSkuStoreId(x)].Item2))
                .FirstOrDefault() is var inactive_kvp) {

                // find most significant inactive license 
                user_storeid_license_kvp = inactive_kvp;
            }
            if (user_storeid_license_kvp.IsDefault()) {
                // no ms store license found
                return MpSubscriptionFormat.Default;
            }

            if (AccountTypeAddOnStoreIdLookup
                .TryGetValue(ParseSkuStoreId(user_storeid_license_kvp), out var acct_type_tup)) {
                return new MpSubscriptionFormat() {
                    AccountType = acct_type_tup.Item1,
                    IsMonthly = acct_type_tup.Item2,
                    IsActive = user_storeid_license_kvp.Value.IsActive,
                    ExpireOffsetUtc = user_storeid_license_kvp.Value.ExpirationDate
                };
            }
            MpDebug.Break($"User license error. Cannot find internal ref to ms store id '{ParseSkuStoreId(user_storeid_license_kvp)}'");
            return MpSubscriptionFormat.Default;
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods



        private async Task<StoreProduct> GetAddOnByStoreIdAsync(string storeId) {
            // Load the sellable add-ons for this app and check if the trial is still 
            // available for this customer. If they previously acquired a trial they won't 
            // be able to get a trial again, and the StoreProduct.Skus property will 
            // only contain one SKU.

            StoreProductQueryResult result =
                await context.GetAssociatedStoreProductsAsync(new string[] { "Durable" });

            if (result.ExtendedError != null) {
                MpConsole.WriteLine("Something went wrong while getting the add-ons. " +
                    "ExtendedError:" + result.ExtendedError);
                return null;
            }

            // Look for the product that represents the subscription.
            foreach (var item in result.Products) {
                StoreProduct product = item.Value;
                if (product.StoreId == storeId) {
                    return product;
                }
            }

            MpConsole.WriteLine("The subscription was not found.");
            return null;
        }
        public async Task<bool?> PurchaseSubscriptionAsync(MpUserAccountType uat, bool isMonthly) {
            // returns:
            // true: successful purchase, already purchased or free
            // false: purchase failed, error
            // null: canceled

            if (uat == MpUserAccountType.None ||
                uat == MpUserAccountType.Free) {
                // ignore free or none
                return true;
            }
            Dispatcher.UIThread.VerifyAccess();

            var storeid_kvp = AccountTypeAddOnStoreIdLookup.FirstOrDefault(x => x.Value == (uat, isMonthly));
            if (string.IsNullOrEmpty(storeid_kvp.Key)) {
                // shouldn't happen, 
                Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                        title: UiStrings.CommonErrorLabel,
                        message: UiStrings.CommonErrorCodeText.Format(3.ToErrorCode()),
                        iconResourceObj: "WarningImage").FireAndForgetSafeAsync();
                return false;
            }

            StoreProduct sp = await GetAddOnByStoreIdAsync(storeid_kvp.Key);
            if (sp == null) {
                // likely offline
                Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                        title: UiStrings.CommonErrorLabel,
                        message: UiStrings.CommonErrorCodeText.Format(UiStrings.AccountOfflineLabel),
                        iconResourceObj: "WarningImage").FireAndForgetSafeAsync();
                return false;
            }
            MpDebug.Assert(_isContextWindowInitialized, "StoreContext not initialized");
            if (!_isContextWindowInitialized) {
                // window handle error, should probably not happen but dunno
                Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                        title: UiStrings.CommonErrorLabel,
                        message: UiStrings.CommonErrorCodeText.Format(4.ToErrorCode()),
                        iconResourceObj: "WarningImage").FireAndForgetSafeAsync();
                return false;
            }
            // Request a purchase of the subscription product. If a trial is available it will be offered 
            // to the customer. Otherwise, the non-trial SKU will be offered.
            StorePurchaseResult result = await sp.RequestPurchaseAsync();

            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null) {
                extendedError = result.ExtendedError.Message;
            }
            MpConsole.WriteLine($"Purchase for {uat} Monthly: {isMonthly} result: {result.Status}");

            switch (result.Status) {
                case StorePurchaseStatus.Succeeded:
                    // Show a UI to acknowledge that the customer has purchased your subscription 
                    // and unlock the features of the subscription. 
                    return true;

                case StorePurchaseStatus.AlreadyPurchased:
                    MpConsole.WriteLine("The customer already owns this subscription. ExtendedError: " + extendedError);
                    return true;
                case StorePurchaseStatus.NotPurchased:
                    // NOTE This can happen for a lot of reasons apparently like:
                    // clicked cancel
                    // using a gift card
                    // pay pal
                    // has overdue payments
                    // virus protection

                    MpConsole.WriteLine("The purchase did not complete. The customer may have cancelled the purchase. ExtendedError: " + extendedError);
                    //Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                    //    title: UiStrings.CommonErrorLabel,
                    //    message: UiStrings.PurchaseFailureText.Format(extendedError),
                    //    iconResourceObj: "WarningImage").FireAndForgetSafeAsync();
                    return null;
                default:
                case StorePurchaseStatus.ServerError:
                case StorePurchaseStatus.NetworkError:
                    MpConsole.WriteLine("The purchase was unsuccessful due to a server or network error. ExtendedError: " + extendedError);
                    return false;
            }
        }

        private string ParseSkuStoreId(KeyValuePair<string, StoreLicense> kvp) {
            if (kvp.Value == null || string.IsNullOrEmpty(kvp.Value.SkuStoreId)) {
                return string.Empty;
            }
            return kvp.Value.SkuStoreId.SplitNoEmpty(@"/")[0];
        }
        #endregion

        #region Commands
        #endregion
#else
        public string RateAppUri =>
            "https://localhost";
        public string ThisProductUri =>
            "https://localhost";

        public string GetStoreSubscriptionUrl(MpUserAccountType uat, bool isMonthly) {
            return "https://localhost";
        }

        public async Task<bool> RefreshAddOnInfoAsync() {
            await Task.Delay(1);
            return false;
        }

        public async Task<MpSubscriptionFormat> GetStoreUserLicenseInfoAsync() {
            await Task.Delay(1);
            return MpSubscriptionFormat.Default;
        }

        public async Task<bool?> PurchaseSubscriptionAsync(MpUserAccountType uat, bool isMonthly) {
            await Task.Delay(1);
            return true;
        }
#endif

    }
}
