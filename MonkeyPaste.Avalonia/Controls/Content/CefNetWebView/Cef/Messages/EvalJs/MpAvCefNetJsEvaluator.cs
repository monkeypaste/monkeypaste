#if CEFNET_WV

using Avalonia.Threading;
using CefNet;
using CefNet.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvCefNetJsEvaluator {

        #region Public Methods

        public bool HandleCefMessage(CefProcessMessageReceivedEventArgs e) {
            bool isRequest = e.Name == "EvaluateScript";
            bool isResponse = e.Name == "ScriptEvaluation";
            if (!isRequest && !isResponse) {
                return false;
            }
            bool wasSuccess = false;
            if (isRequest) {
                wasSuccess = HandleRequest(e.Frame, e.Message);
            } else {
                wasSuccess = HandleResponse(e.Frame, e.Message);
            }
            if (!wasSuccess) {
                //MpConsole.WriteTraceLine("Error processing evalJs msg");
            }
            return true;
        }

        #endregion

        #region Private Methods

        private bool HandleRequest(CefFrame frame, CefProcessMessage requestFromHost) {
            // renderer thread
            string evalKey = requestFromHost.ArgumentList.GetString(0);
            string script = requestFromHost.ArgumentList.GetString(1);

            CefV8Context context = frame.V8Context;

            if (!context.Enter()) {
                return false;
            }

            string jsRespStr_renderer = null;
            try {
                CefV8Value result = context.Eval(script, null);
                if (result != null) {
                    jsRespStr_renderer = result.GetStringValue();
                }
            }
            catch (CefNet.CefNetJSExcepton) {// ex) {
                //MpConsole.WriteTraceLine("EvalJs Exception: ",ex);
                // MpConsole.WriteLine($"Source Line: {ex.SourceLine}");
                // MpConsole.WriteLine($"Script Name: {ex.ScriptName}");
                // MpConsole.WriteLine($"Line: {ex.Line}");
                // MpConsole.WriteLine($"Column: {ex.Column}");
                jsRespStr_renderer = MpAvCefNetApplication.JS_REF_ERROR;
            }
            finally {
                context.Exit();
            }

            var response = new CefProcessMessage("ScriptEvaluation");
            response.ArgumentList.SetString(0, evalKey);
            response.ArgumentList.SetString(1, jsRespStr_renderer);
            frame.SendProcessMessage(CefProcessId.Browser, response);
            return true;
        }

        private bool HandleResponse(CefFrame frame, CefProcessMessage responseFromCef) {
            bool wasHandled = false;
            string evalKey = responseFromCef.ArgumentList.GetString(0);
            string jsRespStr_browser = responseFromCef.ArgumentList.GetString(1);
            Dispatcher.UIThread.Post(() => {
                if (frame.Browser.Host.Client.GetWebView() is WebView wv) {
                    wv.SetJavascriptResult(evalKey, jsRespStr_browser);
                    wasHandled = true;
                }
            });
            return wasHandled;
        }
        #endregion
    }
}

#endif