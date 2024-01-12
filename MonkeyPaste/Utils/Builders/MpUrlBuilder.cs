using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Threading.Tasks;

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
            if (MpUrlHelpers.IsBlankUrl(url)) {
                MpConsole.WriteLine($"Blank url '{url}' rejected.");
                return null;
            }

            if (Mp.Services.SourceRefTools.IsInternalUrl(url)) {
                MpDebug.Break($"Should not be adding internal urls, check stack and fix case");
                return null;
            }

            var urlProps = await MpUrlHelpers.DiscoverUrlPropertiesAsync(url);
            if (urlProps == null) {
                return null;
            }

            int icon_id = 0;
            if (!string.IsNullOrEmpty(urlProps.FavIconBase64)) {
                MpIcon icon = await Mp.Services.IconBuilder.CreateAsync(
                    iconBase64: urlProps.FavIconBase64,
                    suppressWrite: suppressWrite);
                if (icon != null) {
                    icon_id = icon.Id;
                }
            }

            var result = await MpUrl.CreateAsync(
                urlPath: urlProps.FullyFormattedUriStr,
                title: urlProps.Title,
                iconId: icon_id,
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
