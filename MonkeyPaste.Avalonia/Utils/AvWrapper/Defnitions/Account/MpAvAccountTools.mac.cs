using Plugin.InAppBilling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAccountTools : MpIAccountTools {

        public string GetStoreSubscriptionUrl(MpUserAccountType uat, bool isMonthly) {
            return string.Empty;
        }

        public async Task RefreshAddOnInfoAsync() {
            var connected = await CrossInAppBilling.Current.ConnectAsync();
            if (!connected)
                return;

            var items = await CrossInAppBilling.Current.GetProductInfoAsync(ItemType.InAppPurchase, "myitem");

            // TODO setup lookup tables here
            await CrossInAppBilling.Current.DisconnectAsync();
        }

        public Task<MpSubscriptionFormat> GetStoreUserLicenseInfoAsync() {
            throw new NotImplementedException();
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
            var connected = await CrossInAppBilling.Current.ConnectAsync();
            if (!connected) {
                return null;
            }

            if (AccountTypeAddOnStoreIdLookup.FirstOrDefault(x => x.Value.Item1 == uat && x.Value.Item2 == isMonthly).Key is not string productId) {
                return null;
            }

            var purchase = await CrossInAppBilling.Current.PurchaseAsync(productId, ItemType.InAppPurchase);
            if (purchase != null & purchase.State == PurchaseState.Purchased) {
                return true;
            }

            await CrossInAppBilling.Current.DisconnectAsync();
            return false;
        }

    }
}
