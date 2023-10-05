using System.Collections.Generic;

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
    }
}
