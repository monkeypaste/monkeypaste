using Android.Net.Wifi.Aware;
using Android.Runtime;
using Android.Webkit;
using System;
using System.Text;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdWebChromeClient : WebChromeClient {
        #region Private Variable
        private MpIHaveLog _logger;
        #endregion

        public event EventHandler ProgressDone;
        public override void OnProgressChanged(WebView view, int newProgress) {
            base.OnProgressChanged(view, newProgress);
            if (newProgress == 100) {
                ProgressDone?.Invoke(this, null);
            }
        }
        public MpAvAdWebChromeClient(MpIHaveLog logsb) : base() {
            _logger = logsb;
        }

        public override bool OnConsoleMessage(ConsoleMessage consoleMessage) {
            _logger.AppendLine(consoleMessage.Message());
            return base.OnConsoleMessage(consoleMessage);
        }

        [Obsolete]
        public override void OnConsoleMessage(string message, int lineNumber, string sourceID) {
            _logger.AppendLine(message);
            base.OnConsoleMessage(message, lineNumber, sourceID);
        }
    }

    public class MpAvAdWebViewClient : WebViewClient {
        #region Private Variable
        private MpIHaveLog _logger;
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
        public MpAvAdWebViewClient(MpIHaveLog logger) : base() {
            _logger = logger;
        }
        #endregion

        #region Public Methods
        public override void OnPageFinished(WebView view, string url) {
            base.OnPageFinished(view, url);
            PageFinished?.Invoke(this, url);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
        public override void OnReceivedError(WebView view, [GeneratedEnum] ClientError errorCode, string description, string failingUrl) {
            _logger.AppendLine("");
            _logger.AppendLine($"Error Code: {errorCode} ");
            _logger.AppendLine($"description: {description} ");
            _logger.AppendLine($"failingUrl: {failingUrl} ");
            _logger.AppendLine("");

            base.OnReceivedError(view, errorCode, description, failingUrl);
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
