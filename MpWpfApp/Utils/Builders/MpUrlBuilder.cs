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
        public static MpUrl CreateFromHtmlData(string htmlDataStr) {
            string sourceTag = "SourceURL:";
            var htmlParts = htmlDataStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var sourceUrlLine = htmlParts.Where(x => x.StartsWith(sourceTag)).FirstOrDefault();
            if(sourceUrlLine == null) {
                return null;
            }

            string sourceUrl = sourceUrlLine.Replace(sourceTag, string.Empty).Replace(Environment.NewLine, string.Empty);
            string sourceUrlTitle = MonkeyPaste.MpHelpers.Instance.GetUrlTitle(sourceUrl);

            return MpUrl.Create(sourceUrl, sourceUrlTitle);
        }

        public MpUrl Create(string url) {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
