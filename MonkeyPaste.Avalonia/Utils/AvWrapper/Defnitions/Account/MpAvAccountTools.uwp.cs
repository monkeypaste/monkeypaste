﻿using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAccountTools {
        #region Private Variables
        private bool _isContextWindowInitialized = false;
        //StoreProduct subscriptionStoreProduct;

        // Assign this variable to the Store ID of your subscription add-on.
        //private string subscriptionStoreId = "9N5X8R1C9CR4"; // unlimited-monthly
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
                    MpAvWindowManager.ActiveWindow is MpAvWindow w) {
                    nint hwnd = w.TryGetPlatformHandle().Handle;
                    if (hwnd != nint.Zero) {
                        // from https://learn.microsoft.com/en-us/windows/uwp/monetize/in-app-purchases-and-trials#desktop
                        WinRT.Interop.InitializeWithWindow.Initialize(_context, hwnd);
                        _isContextWindowInitialized = true;
                    }
                }

                return _context;
            }
        }

        Dictionary<string, (MpUserAccountType, bool)> AccountTypeAddOnStoreIdLookup { get; } =
            new Dictionary<string, (MpUserAccountType, bool)>() {
                //{"9PMDM0QVHJCS", (MpUserAccountType.Free, false)   },
                //{"9N5X8R1C9CR4", (MpUserAccountType.Unlimited, true) },

                {"9PP3W114BHL5", (MpUserAccountType.Standard, true) },
                {"9N41GXV5HQQ2", (MpUserAccountType.Standard, false) },

                {"9PGVZ60KMDQ7", (MpUserAccountType.Unlimited, true) },
                {"9NN60Z6FX02H", (MpUserAccountType.Unlimited, false) }
            };

        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        public async Task RefreshAddOnInfoAsync() {
            AccountTypeTrialAvailabilityLookup.Clear();
            AccountTypePriceLookup.Clear();

            StoreProductQueryResult spqr =
                await context.GetAssociatedStoreProductsAsync(new string[] { "Durable" });

            if (spqr.ExtendedError != null) {
                MpConsole.WriteLine($"AddOn Error: {spqr.ExtendedError}");
                return;
            }

            foreach (var at in AccountTypeAddOnStoreIdLookup) {
                var spkvp = spqr.Products.FirstOrDefault(x => x.Value.StoreId == at.Key);
                MpDebug.Assert(!spkvp.IsDefault(), $"AddOn not found StoreId: '{at.Key}'");

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
        }
        public async Task<MpSubscriptionFormat> GetStoreUserLicenseInfoAsync() {
            // get users current ms store state
            StoreAppLicense appLicense = await context.GetAppLicenseAsync();
            KeyValuePair<string, StoreLicense> user_storeid_license_kvp = default;
            if (appLicense
                .AddOnLicenses
                .Where(x => x.Value.IsActive)
                .OrderByDescending(x => (int)AccountTypeAddOnStoreIdLookup[x.Value.SkuStoreId].Item1)
                .FirstOrDefault() is var active_kvp) {
                // found most significant active license 
                user_storeid_license_kvp = active_kvp;
            } else if (appLicense
                .AddOnLicenses
                .Where(x => !x.Value.IsActive)
                .OrderByDescending(x => (int)AccountTypeAddOnStoreIdLookup[x.Value.SkuStoreId].Item1)
                .FirstOrDefault() is var inactive_kvp) {

                // find most significant inactive license 
                user_storeid_license_kvp = inactive_kvp;
            }
            if (user_storeid_license_kvp.IsDefault()) {
                // no ms store license found
                return MpSubscriptionFormat.Default;
            }

            if (AccountTypeAddOnStoreIdLookup
                .TryGetValue(user_storeid_license_kvp.Value.SkuStoreId, out var acct_type_tup)) {
                return new MpSubscriptionFormat() {
                    AccountType = acct_type_tup.Item1,
                    IsMonthly = acct_type_tup.Item2,
                    IsActive = user_storeid_license_kvp.Value.IsActive,
                    ExpireOffsetUtc = user_storeid_license_kvp.Value.ExpirationDate
                };
            }
            MpDebug.Break($"User license error. Cannot find internal ref to ms store id '{user_storeid_license_kvp.Value.SkuStoreId}'");
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
        private async Task<bool?> PerformPlatformPurchaseAsync(MpUserAccountType uat, bool isMonthly) {
            // returns:
            // true: successful purchase, already purchased or free
            // false: purchase failed, error
            // null: canceled
            Dispatcher.UIThread.VerifyAccess();

            var storeid_kvp = AccountTypeAddOnStoreIdLookup.FirstOrDefault(x => x.Value == (uat, isMonthly));
            if (string.IsNullOrEmpty(storeid_kvp.Key)) {
                return false;
            }

            StoreProduct sp = await GetAddOnByStoreIdAsync(storeid_kvp.Key);
            if (sp == null) {
                // likely offline
                return false;
            }
            if (!_isContextWindowInitialized) {
                // window handle error, should probably not happen but dunno
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

            switch (result.Status) {
                case StorePurchaseStatus.Succeeded:
                    // Show a UI to acknowledge that the customer has purchased your subscription 
                    // and unlock the features of the subscription. 
                    //SetAccountType(uat);
                    return true;

                case StorePurchaseStatus.AlreadyPurchased:
                    MpConsole.WriteLine("The customer already owns this subscription. ExtendedError: " + extendedError);
                    //SetAccountType(uat);
                    return true;
                case StorePurchaseStatus.NotPurchased:
                    MpConsole.WriteLine("The purchase did not complete. The customer may have cancelled the purchase. ExtendedError: " + extendedError);
                    return null;
                default:
                case StorePurchaseStatus.ServerError:
                case StorePurchaseStatus.NetworkError:
                    MpConsole.WriteLine("The purchase was unsuccessful due to a server or network error. ExtendedError: " + extendedError);
                    return false;
            }
        }
        #endregion

        #region Commands
        #endregion

    }
}