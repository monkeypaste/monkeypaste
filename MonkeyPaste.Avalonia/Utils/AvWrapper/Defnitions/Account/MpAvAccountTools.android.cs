using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAccountTools {
        #region Private Variables

        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        Dictionary<string, (MpUserAccountType, bool)> AccountTypeAddOnStoreIdLookup { get; } =
            new Dictionary<string, (MpUserAccountType, bool)>() {
#if DEBUG
                {"9N0M0CF894CV", (MpUserAccountType.Standard, true) },
                {"9NTBHV933F76", (MpUserAccountType.Standard, false) },

                {"9P06QJ00F7Q8", (MpUserAccountType.Unlimited, true) },
                {"9N2BVBP6MSP6", (MpUserAccountType.Unlimited, false) }
#else
                {"9PP3W114BHL5", (MpUserAccountType.Standard, true) },
                {"9N41GXV5HQQ2", (MpUserAccountType.Standard, false) },

                {"9PGVZ60KMDQ7", (MpUserAccountType.Unlimited, true) },
                {"9NN60Z6FX02H", (MpUserAccountType.Unlimited, false) }

#endif
            };

        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        public string GetStoreSubscriptionUrl(MpUserAccountType uat, bool isMonthly) {
            return $"https://www.monkeypaste.com";
        }

        public async Task RefreshAddOnInfoAsync() {
            await Task.Delay(0);
        }
        public async Task<bool> CanConnectToStoreAsync() {
            await Task.Delay(0);
            return false;
        }
        public async Task<MpSubscriptionFormat> GetStoreUserLicenseInfoAsync() {
            await Task.Delay(0);
            return MpSubscriptionFormat.Default;
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods



        private async Task<bool?> PerformPlatformPurchaseAsync(MpUserAccountType uat, bool isMonthly) {
            await Task.Delay(1);
            return false;
        }
        #endregion

        #region Commands
        #endregion

    }
}
