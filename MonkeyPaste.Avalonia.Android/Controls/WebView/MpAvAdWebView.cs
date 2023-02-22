using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Webkit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdWebView : WebView, MpIWebViewNavigator {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIWebViewNavigator Implementation

        void MpIWebViewNavigator.Navigate(string url) {
            this.LoadUrl(url);
        }

        #endregion

        #endregion

        #region Properties
        private MpAvAdWebViewClient _webViewClient = new MpAvAdWebViewClient();
        public new MpAvAdWebViewClient WebViewClient =>
            _webViewClient;

        #endregion

        #region Constructors

        public MpAvAdWebView(Context context) : base(context) {

        }

        public MpAvAdWebView(Context context, IAttributeSet attrs) : base(context, attrs) {
        }

        public MpAvAdWebView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) {
        }

        [System.Obsolete]
        public MpAvAdWebView(Context context, IAttributeSet attrs, int defStyleAttr, bool privateBrowsing) : base(context, attrs, defStyleAttr, privateBrowsing) {
        }

        public MpAvAdWebView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes) {
        }

        protected MpAvAdWebView(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
