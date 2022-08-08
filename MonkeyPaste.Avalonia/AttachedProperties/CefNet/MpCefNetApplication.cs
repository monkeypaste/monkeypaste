using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CefNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpCefNetApplication : CefNetApplication {
        #region Private Variables

        private Timer messagePump;
        private const int messagePumpDelay = 10;

        #endregion

        #region Constants

        public const string JS_REF_ERROR = "JS_REF_ERROR";
        #endregion

        #region Statics


        #endregion

        #region Events

        public static event EventHandler<object> ProcessMessageReceived;

        #endregion

        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context) {
            base.OnContextCreated(browser, frame, context);
        }


        public Action<long> ScheduleMessagePumpWorkCallback { get; set; }

        protected override void OnScheduleMessagePumpWork(long delayMs) {
            ScheduleMessagePumpWorkCallback(delayMs);
        }

        public static void ResetEnv() {
            if(OperatingSystem.IsWindows()) {
                //int HWND = WinApi.FindWindow(null, "WebViewHost");//window title
                var wvhp = Process.GetProcesses().FirstOrDefault(x => x.ProcessName.ToLower() == "webviewhost");
                if(wvhp != null) {
                    WinApi.SendMessage(wvhp.Handle.ToInt32(), WinApi.WM_SYSCOMMAND, WinApi.SC_CLOSE, 0);
                }
                
            }

            string cefNetLogPath = Path.Combine(Environment.CurrentDirectory, "debug.log");
            if (File.Exists(cefNetLogPath)) {
                File.Delete(cefNetLogPath);
            }
        }

        public static void InitCefNet(IClassicDesktopStyleApplicationLifetime desktop) {
            _ = new MpCefNetApplication(desktop);
        }
        
        private MpCefNetApplication(IClassicDesktopStyleApplicationLifetime desktop) {
            string cefRootDir = @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Avalonia\cef";

            var settings = new CefSettings();
            settings.NoSandbox = true;

            //settings.ExternalMessagePump = true;
            settings.MultiThreadedMessageLoop = true;
            settings.WindowlessRenderingEnabled = false;
            settings.LocalesDirPath = Path.Combine(cefRootDir, "Resources", "locales");
            settings.ResourcesDirPath = Path.Combine(cefRootDir, "Resources");
            settings.LogSeverity = CefLogSeverity.Error;

            desktop.Startup += Desktop_Startup;
            desktop.Exit += (s,e) => Shutdown();            
            
            CefProcessMessageReceived += CefApp_CefProcessMessageReceived;

            ScheduleMessagePumpWorkCallback = ScheduleMessagePumpCallback;

            Initialize(Path.Combine(cefRootDir, "Release"), settings);
        }

        private void Desktop_Startup(object sender, ControlledApplicationLifetimeStartupEventArgs e) {
            if (UsesExternalMessageLoop) {
                messagePump = new Timer(_ => Dispatcher.UIThread.Post(CefApi.DoMessageLoopWork), null, messagePumpDelay, messagePumpDelay);
            }
        }


        public void OnFrameworkShutdown() {
            messagePump?.Dispose();
        }

        private async void ScheduleMessagePumpCallback(long delayMs) {
            await Task.Delay((int)delayMs);
            Dispatcher.UIThread.Post(CefApi.DoMessageLoopWork);
        }

        private void CefApp_CefProcessMessageReceived(object sender, CefProcessMessageReceivedEventArgs e) {            
            if (e.Name == "EvaluateScript") {
                string evalKey = e.Message.ArgumentList.GetString(0);
                string script = e.Message.ArgumentList.GetString(1);

                CefV8Context context = e.Frame.V8Context;

                if (!context.Enter()) {
                    return;
                }

                string jsRespStr_renderer = null;
                try {
                    CefV8Value result = context.Eval(script, null);
                    if (result != null) {
                        jsRespStr_renderer = result.GetStringValue();
                    }
                }catch(CefNet.CefNetJSExcepton ex) {
                    jsRespStr_renderer = JS_REF_ERROR;
                }
                finally {
                    context.Exit();
                }

                var message = new CefProcessMessage("ScriptEvaluation");
                message.ArgumentList.SetString(0, evalKey);
                message.ArgumentList.SetString(1, jsRespStr_renderer);
                e.Frame.SendProcessMessage(CefProcessId.Browser, message);

                e.Handled = true;
                return;
            }

            if (e.Name == "ScriptEvaluation") {
                string evalKey = e.Message.ArgumentList.GetString(0);
                string jsRespStr_browser = e.Message.ArgumentList.GetString(1);
                Dispatcher.UIThread.Post(() => {
                    if (e.Frame.Browser.Host.Client.GetWebView() is MpAvCefNetWebView wv) {
                        wv.SetJavascriptResult(evalKey,jsRespStr_browser);
                    }
                });
                e.Handled = true;
                return;
            }

            //if (e.Name == "MessageBox.Show") {
            //    string message = e.Message.ArgumentList.GetString(0);
            //    Dispatcher.UIThread.Post(() => {
            //        var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
            //            .GetMessageBoxStandardWindow("title", message);
            //        messageBoxStandardWindow.Show();
            //    });
            //    e.Handled = true;
            //    return;
            //}
        }

        private void EvaluateScript(CefFrame frame, string script) {
            //var sb = new StringBuilder();
            CefV8Context context = frame.V8Context;
            if (!context.Enter()) {
                return;
            }

            try {
                //    sb.Append("typeof 1 = ").Append(context.Eval("1", null).Type).AppendLine();
                //    sb.Append("typeof true = ").Append(context.Eval("true", null).Type).AppendLine();
                //    sb.Append("typeof 'string' = ").Append(context.Eval("'string'", null).Type).AppendLine();
                //    sb.Append("typeof 2.2 = ").Append(context.Eval("2.2", null).Type).AppendLine();
                //    sb.Append("typeof null = ").Append(context.Eval("null", null).Type).AppendLine();
                //    sb.Append("typeof new Object() = ").Append(context.Eval("new Object()", null).Type).AppendLine();
                //    sb.Append("typeof undefined = ").Append(context.Eval("undefined", null).Type).AppendLine();
                //    sb.Append("typeof new Date() = ").Append(context.Eval("new Date()", null).Type).AppendLine();
                //    sb.Append("(window == window) = ").Append(context.Eval("window", null) == context.Eval("window", null)).AppendLine();
                context.Eval(script, null);
            }
            finally {
                context.Exit();
            }
            var message = new CefProcessMessage("ScriptEvaluation");
            //message.ArgumentList.SetString(0, sb.ToString());
            frame.SendProcessMessage(CefProcessId.Browser, message);
        }

    }
}
