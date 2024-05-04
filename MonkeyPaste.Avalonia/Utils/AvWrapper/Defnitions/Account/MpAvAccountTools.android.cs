using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia
{
    public partial class MpAvAccountTools
    {
        public string RateAppUri =>
            "https://www.monkeypaste.com";
        public string ThisProductUri =>
            "https://www.monkeypaste.com";
        public string GetStoreSubscriptionUrl(MpUserAccountType uat, bool isMonthly) {
            return string.Empty;
        }

        public async Task<bool> RefreshAddOnInfoAsync() {
            await Task.Delay(1);
            return true;
        }

        public async Task<MpSubscriptionFormat> GetStoreUserLicenseInfoAsync() {
            await Task.Delay(1);
            return MpSubscriptionFormat.Default;
        }

        public async Task<bool?> PurchaseSubscriptionAsync(MpUserAccountType uat, bool isMonthly) {
            await Task.Delay(1);
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
