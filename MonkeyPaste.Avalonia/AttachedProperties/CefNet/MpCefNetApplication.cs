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
        #endregion

        #region Constants

        public const string JS_REF_ERROR = "JS_REF_ERROR";

        #endregion

        #region Statics
        #endregion


        public static void ResetEnv() {
            //if(OperatingSystem.IsWindows()) {
            //    //int HWND = WinApi.FindWindow(null, "WebViewHost");//window title
            //    var wvhp = Process.GetProcesses().FirstOrDefault(x => x.ProcessName.ToLower() == "webviewhost");
            //    if(wvhp != null) {
            //        WinApi.SendMessage(wvhp.Handle.ToInt32(), WinApi.WM_SYSCOMMAND, WinApi.SC_CLOSE, 0);
            //    }
                
            //}

            //string cefNetLogPath = Path.Combine(Environment.CurrentDirectory, "debug.log");
            //if (File.Exists(cefNetLogPath)) {
            //    File.Delete(cefNetLogPath);
            //}
        }

        public static void InitCefNet(IClassicDesktopStyleApplicationLifetime desktop) {
            _ = new MpCefNetApplication(desktop);
        }
        
        private MpCefNetApplication(IClassicDesktopStyleApplicationLifetime desktop) {
            string datFileName = "icudtl.dat";
            string cefRootDir = @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Avalonia\cef";

            string localDirPath = string.Empty;
            string resourceDirPath = string.Empty;
            string releaseDir = string.Empty;
            string datFileSourcePath = string.Empty;
            string datFileTargetPath = string.Empty;

            if (OperatingSystem.IsWindows()) {
                cefRootDir = Path.Combine(cefRootDir, "win");
                localDirPath = Path.Combine(cefRootDir, "Resources", "locales");
                resourceDirPath = Path.Combine(cefRootDir, "Resources");
                releaseDir = Path.Combine(cefRootDir, "Release");
                datFileSourcePath = Path.Combine(resourceDirPath, datFileName);
                datFileTargetPath = Path.Combine(releaseDir, datFileName);
            } else if(OperatingSystem.IsMacOS()) {
                cefRootDir = Path.Combine(cefRootDir, "mac");
            } else if(OperatingSystem.IsLinux()) {
                cefRootDir = Path.Combine(cefRootDir, "linux");
            } else {
                throw new Exception("No cef implementation found for this architecture");
            }

            if(!File.Exists(datFileTargetPath)) {
                // NOTE this would/will occur when a new cef version is installed
                if(!File.Exists(datFileSourcePath)) {
                    throw new Exception($"'CefNet cannot initialize, '{datFileSourcePath}' cannot be found");
                }
                try {
                    File.Copy(datFileSourcePath, datFileTargetPath);
                }catch(Exception ex) {
                    throw new Exception($"'CefNet cannot initialize, '{datFileSourcePath}' cannot be written to '{datFileTargetPath}'"+Environment.NewLine,ex);
                }
            }
            

            var settings = new CefSettings();
            settings.NoSandbox = true;
            settings.MultiThreadedMessageLoop = true;
            settings.WindowlessRenderingEnabled = false;
            settings.LocalesDirPath = localDirPath;
            settings.ResourcesDirPath = resourceDirPath;
            settings.LogSeverity = CefLogSeverity.Error;

            desktop.Exit += (s,e) => Shutdown();            
            
            CefProcessMessageReceived += CefApp_CefProcessMessageReceived;

            Initialize(Path.Combine(cefRootDir, "Release"), settings);
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
