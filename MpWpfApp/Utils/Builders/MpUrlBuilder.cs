using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
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
        public static async Task<MpUrl> CreateFromHtmlData(string htmlDataStr, MpApp app) {
            string sourceTag = "SourceURL:";
            var htmlParts = htmlDataStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var sourceUrlLine = htmlParts.Where(x => x.StartsWith(sourceTag)).FirstOrDefault();
            if(sourceUrlLine == null) {
                return null;
            }

            string sourceUrl = sourceUrlLine.Replace(sourceTag, string.Empty).Replace(Environment.NewLine, string.Empty);
            string sourceUrlTitle = await MonkeyPaste.MpHelpers.Instance.GetUrlTitleAsync(sourceUrl);

            var result = await MpUrl.Create(sourceUrl, sourceUrlTitle,app);
            return result;
        }

        public static async Task<MpUrl> Create(string url, MpApp app) {
            if (string.IsNullOrEmpty(url)) {
                return null;
            }
            string sourceUrlTitle = await MonkeyPaste.MpHelpers.Instance.GetUrlTitleAsync(url);

            var result = await MpUrl.Create(url, sourceUrlTitle, app);

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
