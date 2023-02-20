using Android.Webkit;
using System;

namespace MonkeyPaste.Avalonia.Android {
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
        public event EventHandler<string> Navigated;

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public override void OnPageFinished(WebView view, string url) {
            base.OnPageFinished(view, url);
            Navigated?.Invoke(this, url);
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
