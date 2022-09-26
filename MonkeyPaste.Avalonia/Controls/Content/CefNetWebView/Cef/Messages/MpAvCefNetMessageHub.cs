using CefNet;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvCefNetMessageHub {
        #region Private Variables

        private MpAvCefNetWindowBinder _windowBinder;
        private MpAvCefNetJsEvaluator _jsEvaluator;

        #endregion

        #region Properties
        #endregion


        #region Constructors


        public MpAvCefNetMessageHub(MpAvCefNetApplication cefNetApp) {
            _windowBinder = new MpAvCefNetWindowBinder(cefNetApp);
            _jsEvaluator = new MpAvCefNetJsEvaluator();
            cefNetApp.CefProcessMessageReceived += MessageReceived;
        }
        #endregion

        #region Public Methods

        public void MessageReceived(object sender, CefProcessMessageReceivedEventArgs e) {
            if(_jsEvaluator.HandleCefMessage(e)) {
                e.Handled = true;
                return;
            }
            if(_windowBinder.HandleCefMessage(e)) {
                e.Handled = true;
                return;
            }
            e.Handled = false;
        }
        #endregion
    }
}
