﻿#if CEFNET_WV
using Avalonia.Threading;
using CefNet;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {

    public class MpAvCefNetWindowBinder {
        #region Private Variables

        private string _dbPath;

        #endregion

        #region Constructors

        public MpAvCefNetWindowBinder() {
            _dbPath = new MpAvDbInfo().DbPath;
            //cefNetApp.OnCefNetContextCreated += CefNetApp_OnCefNetContextCreated;
        }

        #endregion

        #region Public Methods

        public void CefNetApp_OnCefNetContextCreated(object sender, CefV8Context context) {
            if (!context.Enter()) {
                return;
            }
            try {
                CefV8Value window = context.GetGlobal();
                var fnhandler = new MpAvCefV8Func(_dbPath);
                foreach (string functionName in typeof(MpEditorBindingFunctionType).GetEnumNames()) {
                    window.SetValue(functionName, CefV8Value.CreateFunction(functionName, fnhandler), CefV8PropertyAttribute.ReadOnly);
                }
            }
            catch (CefNet.CefNetJSExcepton ex) {
                MpConsole.WriteTraceLine("CefNet Context created exception: ", ex);
            }
            finally {
                context.Exit();
            }
        }

        public bool HandleCefMessage(CefProcessMessageReceivedEventArgs e) {
            if (e.Name != "WindowBindingResponse") {
                return false;
            }

            string msgTypeStr = e.Message.ArgumentList.GetString(0);
            string msgJsonStr = e.Message.ArgumentList.GetString(1);
            string contentHandle = e.Message.ArgumentList.GetString(2);
            var funcType = msgTypeStr.ToEnum<MpEditorBindingFunctionType>();

            Dispatcher.UIThread.Post(() => {
                if (e.Frame == null ||
                    e.Frame.Browser == null ||
                    e.Frame.Browser.Host == null ||
                    e.Frame.Browser.Host.Client == null) {
                    // occurs when control was detached (as a known case)
                    return;
                }
                if (e.Frame.Browser.Host.Client.GetWebView() is MpAvIWebViewBindingResponseHandler respHandler) {
                    respHandler.HandleBindingNotification(funcType, msgJsonStr, contentHandle);
                }
            });


            return true;
        }


        #endregion

        #region Private Methods

        #endregion
    }
}
#endif