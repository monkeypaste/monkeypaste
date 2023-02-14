using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvCefNetPlainHtmlConverterWebView :
        MpAvCefNetWebView {
        #region Private Variables

        #endregion

        #region Statics
        public static string HTML_CONVERTER_PARAMS => "converter=true";

        #endregion

        #region Properties
        public override string ContentUrl => base.ContentUrl + $"?{HTML_CONVERTER_PARAMS}";
        #endregion

        #region Constructors
        public MpAvCefNetPlainHtmlConverterWebView() : base() {
            MpPlatform.Services.ContentViewLocator = this;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
