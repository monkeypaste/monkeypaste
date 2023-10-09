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

        private Dictionary<string, MpUserAccountType> _accountTypeAddOnStoreIdLookup;
        Dictionary<string, MpUserAccountType> AccountTypeAddOnStoreIdLookup {
            get {
                if (_accountTypeAddOnStoreIdLookup == null) {
                    _accountTypeAddOnStoreIdLookup = new() {
                        //9MZRBMH3JT75/0010

                        {"9PMDM0QVHJCS", MpUserAccountType.Test   },
                        {"9N5X8R1C9CR4", MpUserAccountType.Unlimited },

                        //{MpUserAccountType.Free,"f921e300-6325-4574-a850-a896a31c816f" },
                        //{MpUserAccountType.Standard,"5d8f2bac-662c-4075-a3e9-0495a7757a3d" },
                        //{MpUserAccountType.Unlimited,"ba104e40-7859-478a-890b-f308678ca0bc" },
                        //{MpUserAccountType.Trial,"f0ae6390-93fd-4055-9442-afb45b21dd63" },
                        //{MpUserAccountType.Admin,"e67c9b90-3746-4986-9f1e-82ff8877cf52" }
                    };
                }
                return _accountTypeAddOnStoreIdLookup;
            }
        }

        private Dictionary<MpUserAccountType, string> _accountTypeProductPriceTierLookup;
        Dictionary<MpUserAccountType, string> AccountTypeProductPriceTierLookup {
            get {
                // Each price tier corresponds to a unique numerical identifier,
                // which can be used with the Store submission API.

                // full list:
                // https://partner.microsoft.com/en-us/dashboard/availability/api/product/9N5X8R1C9CR4/submissions/1152921505696858501/tiers/download?category=consumer
                if (_accountTypeProductPriceTierLookup == null) {
                    _accountTypeProductPriceTierLookup = new() {
                        {MpUserAccountType.Free,"1" },
                        {MpUserAccountType.Standard,"1012" }, // $0.99
                        {MpUserAccountType.Unlimited,"1032" }, // $2.99
                        {MpUserAccountType.Trial,"1" },
                        {MpUserAccountType.Admin,"1" }
                    };
                }
                return _accountTypeProductPriceTierLookup;
            }
        }
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public async Task InitAsync() {
            if (Mp.Services.StartupState.StartupFlags.HasFlag(MpStartupFlags.Initial)) {
                return;
            }
            await RefreshAccountTypeAsync();
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private async Task RefreshAccountTypeAsync() {
            MpUserAccountType acct_type = MpUserAccountType.Free;

            MpUserAccountFormat store_lic = await GetStoreUserLicenseInfoAsync();
            if (store_lic == default) {
                MpUserAccountType server_lic = await GetServerAccountTypeAsync();
                if (server_lic != MpUserAccountType.None) {
                    acct_type = server_lic;
                }
            } else {
                if (store_lic.IsActive) {
                    acct_type = store_lic.AccountType;
                }
            }
            if (acct_type == MpUserAccountType.Test) {
                acct_type = TEST_ACCOUNT_TYPE;
            }
            SetAccountType(acct_type);
        }

        private async Task<MpUserAccountFormat> GetStoreUserLicenseInfoAsync() {
            StoreAppLicense appLicense = await context.GetAppLicenseAsync();
            foreach (var addOnLicense in appLicense.AddOnLicenses) {
                StoreLicense license = addOnLicense.Value;
                if (AccountTypeAddOnStoreIdLookup.TryGetValue(license.SkuStoreId, out MpUserAccountType platAcctType)) {
                    return new MpUserAccountFormat() {
                        AccountType = platAcctType,
                        IsActive = license.IsActive,
                        ExpireOffset = license.ExpirationDate
                    };
                }
            }
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
        private async Task PerformPlatformPurchaseAsync(MpUserAccountType uat) {
            var acc_kvp = AccountTypeAddOnStoreIdLookup.FirstOrDefault(x => x.Value == uat);
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
