using Android.Webkit;
using System;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdWebChromeClient : WebChromeClient {
        public event EventHandler ProgressDone;
        public override void OnProgressChanged(WebView view, int newProgress) {
            base.OnProgressChanged(view, newProgress);
            if (newProgress == 100) {
                ProgressDone?.Invoke(this, null);
            }
        }
    }

    public class MpAvAdWebViewClient : WebViewClient {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #endregion

        #region Events
        public event EventHandler<string> PageFinished;

        #endregion

        #region Constructors
        public MpAvAdWebViewClient() : base() {

        }
        #endregion

        #region Public Methods
        public override void OnPageFinished(WebView view, string url) {
            base.OnPageFinished(view, url);
            PageFinished?.Invoke(this, url);
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
