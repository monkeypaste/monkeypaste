using CefNet;

namespace MonkeyPaste.Avalonia {
    public class MpAvCefNetMessageHub {
        #region Private Variables

        public MpAvCefNetWindowBinder WindowBinder { get; private set; }
        private MpAvCefNetJsEvaluator _jsEvaluator;

        #endregion

        #region Properties
        #endregion

        #region Constructors


        public MpAvCefNetMessageHub(MpAvCefNetApplication cefNetApp) {
            WindowBinder = new MpAvCefNetWindowBinder();
            _jsEvaluator = new MpAvCefNetJsEvaluator();
            cefNetApp.CefProcessMessageReceived += MessageReceived;
        }
        #endregion

        #region Public Methods

        public void MessageReceived(object sender, CefProcessMessageReceivedEventArgs e) {
            if (_jsEvaluator.HandleCefMessage(e)) {
                e.Handled = true;
                return;
            }
            if (WindowBinder.HandleCefMessage(e)) {
                e.Handled = true;
                return;
            }
            e.Handled = false;
        }
        #endregion
    }
}
