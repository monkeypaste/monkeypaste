using System.Collections.Generic;
using System.Globalization;

namespace MonkeyPaste.Common.Plugin {
    public abstract class MpPluginMessageFormatBase {
        private string _cultureCode;
        public string cultureCode {
            get {
                if (string.IsNullOrEmpty(_cultureCode)) {
                    //if (MpCommonTools.Services == null ||
                    //    MpCommonTools.Services.UserCultureInfo == null) {
                    //    return CultureInfo.CurrentCulture.Name;
                    //}
                    //return MpCommonTools.Services.UserCultureInfo.CultureCode;
                    return CultureInfo.CurrentUICulture.Name;
                }
                return _cultureCode;
            }
            set => _cultureCode = value;
        }
        public Dictionary<string, object> dataObjectLookup { get; set; }
    }
}
