using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Apis.PeopleService.v1.Data;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    public class MpUrlBuilder : MpIUrlBuilder {
        #region Statics
        #endregion

        #region Private Variables
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public async Task<MpUrl> CreateAsync(string url, int appId = 0, bool suppressWrite = false) {
            if (string.IsNullOrWhiteSpace(url)) {
                return null;
            }

            var urlProps = await MpUrlHelpers.DiscoverUrlProperties(url);
            if(urlProps == null) {
                return null;
            }

            MpIcon icon = await MpPlatformWrapper.Services.IconBuilder.CreateAsync(
                    iconBase64: urlProps.IconBase64,
                    suppressWrite: suppressWrite);


            var result = await MpUrl.CreateAsync(
                urlPath: urlProps.FullyFormattedUriStr,
                title: urlProps.Title,
                domain: urlProps.DomainStr,
                iconId: icon.Id,
                appId: appId,
                suppressWrite: suppressWrite);

            return result;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
