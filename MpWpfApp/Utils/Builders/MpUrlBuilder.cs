using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpUrlBuilder : MpIUrlBuilder {
        #region Statics
        #endregion

        #region Private Variables
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public static async Task<MpUrl> CreateFromHtmlData(string htmlDataStr) {
            string sourceTag = "SourceURL:";
            string sourceUrlLine = string.Empty;

            if (!htmlDataStr.ToLower().Contains(sourceTag)) {
                // NOTE this occurs when copying images
                var m = MpRegEx.GetRegExForTokenType(MpSubTextTokenType.Uri).Match(htmlDataStr);
                if(m.Success) {
                    sourceUrlLine = m.ToString();
                    if(sourceUrlLine.EndsWith("\"")) {
                        // removes trailing " from <img src>
                        sourceUrlLine = sourceUrlLine.Substring(0, sourceUrlLine.Length - 1);
                    }
                }
            } else {
                var htmlParts = htmlDataStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                sourceUrlLine = htmlParts.Where(x => x.StartsWith(sourceTag)).FirstOrDefault();
            }
            if(string.IsNullOrWhiteSpace(sourceUrlLine)) {
                return null;
            }

            string sourceUrl = sourceUrlLine.Replace(sourceTag, string.Empty).Replace(Environment.NewLine, string.Empty);

            string sourceUrlTitle = await MonkeyPaste.MpHelpers.GetUrlTitleAsync(sourceUrl);

            var result = await MpUrl.Create(sourceUrl, sourceUrlTitle);
            return result;
        }

        public static async Task<MpUrl> Create(string url, string title = "") {
            if (string.IsNullOrEmpty(url)) {
                return null;
            }
            string sourceUrlTitle = title;
            if(string.IsNullOrEmpty(sourceUrlTitle)) {
                sourceUrlTitle = await MonkeyPaste.MpHelpers.GetUrlTitleAsync(url);
            }

            var result = await MpUrl.Create(url, sourceUrlTitle);

            return result;
        }

        public async Task<MpUrl> Create(string url) {
            await Task.Delay(1);
            return null;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
