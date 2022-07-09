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
        public static async Task<MpUrl> CreateUrl(string sourceUrl, string title = "") {
            if(string.IsNullOrWhiteSpace(sourceUrl)) {
                return null;
            }
            string sourceUrlTitle = title;
            if (string.IsNullOrEmpty(sourceUrlTitle)) {
                sourceUrlTitle = await MpUrlHelpers.GetUrlTitle(sourceUrl);
            }

            var result = await MpUrl.Create(sourceUrl, sourceUrlTitle);
            return result;
        }

        public async Task<MpUrl> CreateAsync(string uri, string title = "") {
            var url = await MpUrlBuilder.CreateUrl(uri, title);
            return url;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
