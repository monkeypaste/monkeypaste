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
                    MpAvWindowManager.MainWindow != null) {
                    nint hwnd = MpAvWindowManager.MainWindow.TryGetPlatformHandle().Handle;
                    if (hwnd != nint.Zero) {
                        // from https://aka.ms/storecontext-for-desktop.
                        WinRT.Interop.InitializeWithWindow.Initialize(_context, hwnd);
                        _isContextWindowInitialized = true;
                    }
                }

                return _context;
            }
        }

        Dictionary<string, (MpUserAccountType, bool)> AccountTypeAddOnStoreIdLookup { get; } =
            new Dictionary<string, (MpUserAccountType, bool)>() {
                {"9PMDM0QVHJCS", (MpUserAccountType.Free, false)   },
                {"9N5X8R1C9CR4", (MpUserAccountType.Unlimited, true) },

                //{"9PP3W114BHL5", (MpUserAccountType.Standard, true) },
                //{"9N41GXV5HQQ2", (MpUserAccountType.Standard, false) },

                //{"9PGVZ60KMDQ7", (MpUserAccountType.Unlimited, true) },
                //{"9NN60Z6FX02H", (MpUserAccountType.Unlimited, false) }
            };

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public async Task InitAsync() {
            if (Mp.Services.StartupState.StartupFlags.HasFlag(MpStartupFlags.Initial)) {
                await RefreshPricingInfoAsync();
                return;
            }
            await RefreshUserAccountStateAsync();
        }

        public async Task RefreshPricingInfoAsync() {
            AccountTypePriceLookup.Clear();
            foreach (var at in AccountTypeAddOnStoreIdLookup) {
                var sp = await GetAddOnByStoreIdAsync(at.Key);
                AccountTypePriceLookup.Add(at.Value, sp.Price.FormattedPrice);
            }
        }
        public async Task<MpUserAccountStateFormat> RefreshUserAccountStateAsync() {
            var acct = await GetUserAccountAsync();
            SetAccountType(acct.AccountType);
            return acct;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private async Task<MpUserAccountStateFormat> GetStoreUserLicenseInfoAsync() {
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
                return null;
            }

            if (AccountTypeAddOnStoreIdLookup
                .TryGetValue(user_storeid_license_kvp.Value.SkuStoreId, out var acct_type_tup)) {
                return new MpUserAccountStateFormat() {
                    AccountType = acct_type_tup.Item1,
                    IsMonthly = acct_type_tup.Item2,
                    IsActive = user_storeid_license_kvp.Value.IsActive,
                    ExpireOffset = user_storeid_license_kvp.Value.ExpirationDate
                };
            }
            MpDebug.Break($"User license error. Cannot find internal ref to ms store id '{user_storeid_license_kvp.Value.SkuStoreId}'");
            return null;
        }

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
        private async Task PerformPlatformPurchaseAsync(MpUserAccountType uat, bool isMonthly) {
            var acc_kvp = AccountTypeAddOnStoreIdLookup.FirstOrDefault(x => x.Value == (uat, isMonthly));
            if (string.IsNullOrEmpty(acc_kvp.Key)) {
                return;
            }
            StoreProduct sp = await GetAddOnByStoreIdAsync(acc_kvp.Key);

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
                    SetAccountType(uat);
                    break;

                case StorePurchaseStatus.AlreadyPurchased:
                    MpConsole.WriteLine("The customer already owns this subscription. ExtendedError: " + extendedError);
                    SetAccountType(uat);
                    break;
                case StorePurchaseStatus.NotPurchased:
                    MpConsole.WriteLine("The purchase did not complete. The customer may have cancelled the purchase. ExtendedError: " + extendedError);
                    break;

                case StorePurchaseStatus.ServerError:
                case StorePurchaseStatus.NetworkError:
                    MpConsole.WriteLine("The purchase was unsuccessful due to a server or network error. ExtendedError: " + extendedError);
                    break;

            }
        }
        #endregion

        #region Commands
        #endregion

    }
}
