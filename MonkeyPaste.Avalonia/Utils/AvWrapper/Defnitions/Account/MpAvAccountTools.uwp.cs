using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAccountTools {

        private Dictionary<MpUserAccountType, string> _accountTypeProductIdLookup;
        Dictionary<MpUserAccountType, string> AccountTypeProductIdLookup {
            get {
                if (_accountTypeProductIdLookup == null) {
                    _accountTypeProductIdLookup = new() {
                        {MpUserAccountType.Free,"f921e300-6325-4574-a850-a896a31c816f" },
                        {MpUserAccountType.Standard,"5d8f2bac-662c-4075-a3e9-0495a7757a3d" },
                        {MpUserAccountType.Unlimited,"ba104e40-7859-478a-890b-f308678ca0bc" },
                        {MpUserAccountType.Trial,"f0ae6390-93fd-4055-9442-afb45b21dd63" },
                        {MpUserAccountType.Admin,"e67c9b90-3746-4986-9f1e-82ff8877cf52" }
                    };
                }
                return _accountTypeProductIdLookup;
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


        private StoreContext context = null;
        StoreProduct subscriptionStoreProduct;

        // Assign this variable to the Store ID of your subscription add-on.
        private string subscriptionStoreId = "9N5X8R1C9CR4"; // unlimited-monthly

        public async void PurchaseAddOnAsync(string storeId) {
            if (context == null) {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

            //workingProgressRing.IsActive = true;
            StorePurchaseResult result = await context.RequestPurchaseAsync(storeId);
            //workingProgressRing.IsActive = false;

            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null) {
                extendedError = result.ExtendedError.Message;
            }

            MpConsole.WriteLine($"Purchase result: {result.Status}");

            switch (result.Status) {
                case StorePurchaseStatus.AlreadyPurchased:
                    //textBlock.Text = "The user has already purchased the product.";
                    break;

                case StorePurchaseStatus.Succeeded:
                    //textBlock.Text = "The purchase was successful.";
                    break;

                case StorePurchaseStatus.NotPurchased:
                    //textBlock.Text = "The purchase did not complete. The user may have cancelled the purchase. ExtendedError: " + extendedError;
                    break;

                case StorePurchaseStatus.NetworkError:
                    //textBlock.Text = "The purchase was unsuccessful due to a network error. ExtendedError: " + extendedError;
                    break;

                case StorePurchaseStatus.ServerError:
                    //textBlock.Text = "The purchase was unsuccessful due to a server error. ExtendedError: " + extendedError;
                    break;

                default:
                    //textBlock.Text = "The purchase was unsuccessful due to an unknown error. ExtendedError: " + extendedError;
                    break;
            }
        }

        // This is the entry point method for the example.
        public async Task SetupSubscriptionInfoAsync() {
            if (context == null) {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.

                // Obtain window handle by passing in pointer to the window object
                //nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(windowObject);
                nint hwnd = MpAvWindowManager.MainWindow.TryGetPlatformHandle().Handle;
                // Initialize the dialog using wrapper funcion for IInitializeWithWindow
                WinRT.Interop.InitializeWithWindow.Initialize(context, hwnd);

            }

            var test = await context.GetAppLicenseAsync();
            var test2 = context.User;
            var test3 = await context.GetUserCollectionAsync(new[] { "Durable", "Consumable", "UnmanagedConsumable" });
            string[] productKinds = { "Durable" };
            List<String> filterList = new List<string>(productKinds);

            // Specify the Store IDs of the products to retrieve.
            string[] storeIds = new string[] { "9MZRBMH3JT75" };

            StoreProductQueryResult queryResult =
                await context.GetStoreProductsAsync(filterList, storeIds);
            if (queryResult.ExtendedError != null) {
                // The user may be offline or there might be some other server failure.
                return;
            }

            foreach (KeyValuePair<string, StoreProduct> item in queryResult.Products) {
                // Access the Store info for the product.
                StoreProduct product = item.Value;

                // Use members of the product object to access info for the product...
            }

            bool userOwnsSubscription = await CheckIfUserHasSubscriptionAsync();
            if (userOwnsSubscription) {
                // Unlock all the subscription add-on features here.
                return;
            }

            // Get the StoreProduct that represents the subscription add-on.
            subscriptionStoreProduct = await GetSubscriptionProductAsync();
            if (subscriptionStoreProduct == null) {
                StoreProductResult spr = await context.GetStoreProductForCurrentAppAsync();
                if (spr != null &&
                    spr.Product != null) {
                    subscriptionStoreProduct = spr.Product;
                } else {
                    return;
                }
                return;
            }

            // Check if the first SKU is a trial and notify the customer that a trial is available.
            // If a trial is available, the Skus array will always have 2 purchasable SKUs and the
            // first one is the trial. Otherwise, this array will only have one SKU.
            StoreSku sku = subscriptionStoreProduct.Skus[0];
            if (sku.SubscriptionInfo.HasTrialPeriod) {
                // You can display the subscription trial info to the customer here. You can use 
                // sku.SubscriptionInfo.TrialPeriod and sku.SubscriptionInfo.TrialPeriodUnit 
                // to get the trial details.
            } else {
                // You can display the subscription purchase info to the customer here. You can use 
                // sku.SubscriptionInfo.BillingPeriod and sku.SubscriptionInfo.BillingPeriodUnit
                // to provide the renewal details.
            }

            // Prompt the customer to purchase the subscription.
            await PromptUserToPurchaseAsync();
            return;
        }

        private async Task<bool> CheckIfUserHasSubscriptionAsync() {
            StoreAppLicense appLicense = await context.GetAppLicenseAsync();

            // Check if the customer has the rights to the subscription.
            foreach (var addOnLicense in appLicense.AddOnLicenses) {
                StoreLicense license = addOnLicense.Value;
                if (license.SkuStoreId.StartsWith(subscriptionStoreId)) {
                    if (license.IsActive) {
                        // The expiration date is available in the license.ExpirationDate property.
                        return true;
                    }
                }
            }

            // The customer does not have a license to the subscription.
            return false;
        }

        private async Task<StoreProduct> GetSubscriptionProductAsync() {
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
                if (product.StoreId == subscriptionStoreId) {
                    return product;
                }
            }

            MpConsole.WriteLine("The subscription was not found.");
            return null;
        }

        private async Task PromptUserToPurchaseAsync() {
            // Request a purchase of the subscription product. If a trial is available it will be offered 
            // to the customer. Otherwise, the non-trial SKU will be offered.
            StorePurchaseResult result = await subscriptionStoreProduct.RequestPurchaseAsync();

            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null) {
                extendedError = result.ExtendedError.Message;
            }

            switch (result.Status) {
                case StorePurchaseStatus.Succeeded:
                    // Show a UI to acknowledge that the customer has purchased your subscription 
                    // and unlock the features of the subscription. 
                    break;

                case StorePurchaseStatus.NotPurchased:
                    MpConsole.WriteLine("The purchase did not complete. " +
                        "The customer may have cancelled the purchase. ExtendedError: " + extendedError);
                    break;

                case StorePurchaseStatus.ServerError:
                case StorePurchaseStatus.NetworkError:
                    MpConsole.WriteLine("The purchase was unsuccessful due to a server or network error. " +
                        "ExtendedError: " + extendedError);
                    break;

                case StorePurchaseStatus.AlreadyPurchased:
                    MpConsole.WriteLine("The customer already owns this subscription." +
                            "ExtendedError: " + extendedError);
                    break;
            }
        }
    }
}
