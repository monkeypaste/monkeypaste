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
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using CefNet.Avalonia;

namespace MonkeyPaste.Avalonia {
    public enum MpAvEditorBindingFunctionType {
        GetDragData,
        GetAllTemplatesFromDb,
        NotifyEditorSelectionChanged,
        NotifyContentLengthChanged,
        NotifySubSelectionEnabledChanged,
        NotifyContentDraggableChanged,
        NotifyDropEffectChanged,
        NotifyException,
        NotifyDragStartOrEnd,
        NotifyReadOnlyChanged,
        NotifyDomLoaded
    }
    public class MpAvCefNetApplication : CefNetApplication {
        #region Private Variables

        private string _dbPath;

        private Timer messagePump;
        private const int messagePumpDelay = 10;
        #endregion

        #region Constants

        public const string JS_REF_ERROR = "JS_REF_ERROR";

        public static bool UseCefNet { get; private set; } = true;
        #endregion

        #region Statics


        #endregion

        #region Properties

        private static string[] BindingFunctionNames = new string[] {
            "getDragData",
            "getAllTemplatesFromDb",
            "notifyEditorSelectionChanged",
            "notifyContentLengthChanged",
            "notifySubSelectionEnabledChanged",
            "notifyContentDraggableChanged",
            "notifyDropEffectChanged",
            "notifyException",
            "notifyDragStartOrEnd",
            "notifyReadOnlyChanged",
            "notifyDomLoaded"
        };

        public static Dictionary<string, MpAvEditorBindingFunctionType> BindingFunctionLookup {
            get {
                if(typeof(MpAvEditorBindingFunctionType).Length() != BindingFunctionNames.Length) {
                    // mismatch in count
                    Debugger.Break();
                }
                var bfl = new Dictionary<string, MpAvEditorBindingFunctionType>();
                for (int i = 0; i < BindingFunctionNames.Length; i++) {
                    bfl.Add(BindingFunctionNames[i], (MpAvEditorBindingFunctionType)i);
                }
                return bfl;
            }
        }

        #endregion

        #region Events

        private bool _useExternalMessagePump = false;

        private async void OnScheduleMessagePumpWorAsync(long delayMs) {
            await Task.Delay((int)delayMs);
            Dispatcher.UIThread.Post(CefApi.DoMessageLoopWork);
        }


        private void App_FrameworkInitialized(object sender, EventArgs e) {
            if (Instance.UsesExternalMessageLoop) {
                messagePump = new Timer(_ => Dispatcher.UIThread.Post(CefApi.DoMessageLoopWork), null, messagePumpDelay, messagePumpDelay);
            }
        }

        protected override void OnScheduleMessagePumpWork(long delayMs) {
            ScheduleMessagePumpWorkCallback(delayMs);
        }
        public Action<long> ScheduleMessagePumpWorkCallback { get; set; }

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

        public static void InitCefNet() {
            _ = new MpAvCefNetApplication();
        }

        public static void ShutdownCefNet() {
            if(Instance == null) {
                return;
            }
            Instance.Shutdown();
            MpConsole.WriteLine("CefNet Successfully shutdown");
        }

        
        private MpAvCefNetApplication() {
            _dbPath = new MpAvDbInfo().DbPath;

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

            if(_useExternalMessagePump) {
                // when true will only work on windows or linux, also maybe pointless...trying to configure for single process
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux()) {
                    settings.MultiThreadedMessageLoop = false;
                    settings.ExternalMessagePump = true;
                    ScheduleMessagePumpWorkCallback = OnScheduleMessagePumpWorAsync;
                    App.FrameworkInitialized += App_FrameworkInitialized;
                } else {
                    settings.MultiThreadedMessageLoop = true;
                    settings.ExternalMessagePump = false;
                }
            } else {
                settings.MultiThreadedMessageLoop = true;
                settings.ExternalMessagePump = false;
            }
            
            settings.WindowlessRenderingEnabled = false;
            settings.LocalesDirPath = localDirPath;
            settings.ResourcesDirPath = resourceDirPath;
            settings.LogSeverity = CefLogSeverity.Error;

            App.FrameworkShutdown += App_FrameworkShutdown;

            CefProcessMessageReceived += CefApp_CefProcessMessageReceived;

            Initialize(Path.Combine(cefRootDir, "Release"), settings);
        }


        private void App_FrameworkShutdown(object sender, EventArgs e) {
            messagePump?.Dispose();
            ShutdownCefNet();
        }



        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context) {
            if (!context.Enter()) {
                return;
            }
            try {
                CefV8Value window = context.GetGlobal();
                var fnhandler = new MpAvCefV8Func(_dbPath);
                foreach(var bf_kvp in BindingFunctionLookup) {
                    window.SetValue(bf_kvp.Key, CefV8Value.CreateFunction(bf_kvp.Key, fnhandler), CefV8PropertyAttribute.ReadOnly);
                }
            }
            catch (CefNet.CefNetJSExcepton ex) {
                MpConsole.WriteTraceLine("CefNet Context created exception: ", ex);
            }
            finally {
                context.Exit();
            }
        }


        private void CefApp_CefProcessMessageReceived(object sender, CefProcessMessageReceivedEventArgs e) {            
            if (e.Name == "EvaluateScript") {
                // renderer thread
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
                    MpConsole.WriteLine("EvalJs Exception: "+ ex.ToString());
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
                        wv.SetJavascriptResult(evalKey, jsRespStr_browser);
                    }
                });
                e.Handled = true;
                return;
            }

            if(e.Name == "WindowBindingResponse") {
                string msgType = e.Message.ArgumentList.GetString(0);
                string msgJsonStr = e.Message.ArgumentList.GetString(1);
                Dispatcher.UIThread.Post(async() => {
                    if(!BindingFunctionLookup.TryGetValue(msgType, out var funcType)) {
                        // function name mismatch
                        Debugger.Break();
                        return;
                    }

                    MpAvCefNetWebView wv = e.Frame.Browser.Host.Client.GetWebView() as MpAvCefNetWebView;
                    if(wv == null) {
                        // occurs for converter (not subclassed webview)
                        return;
                    }
                    //while (wv == null) {
                    //    await Task.Delay(100);
                    //    wv = e.Frame.Browser.Host.Client.GetWebView() as MpAvCefNetWebView;
                    //}
                    var ctvm = wv.DataContext as MpAvClipTileViewModel;
                    switch (funcType) {
                        case MpAvEditorBindingFunctionType.NotifyContentDraggableChanged: 
                            var draggableChanged = MpJsonObject.DeserializeBase64Object<MpQuillContentDraggableChangedMessage>(msgJsonStr);
                            wv.UpdateDraggable(draggableChanged.isDraggable);
                            break;
                        case MpAvEditorBindingFunctionType.NotifyEditorSelectionChanged: 
                            var selChangedJsonMsgObj = MpJsonObject.DeserializeBase64Object<MpQuillContentSelectionChangedMessage>(msgJsonStr);
                            wv.UpdateSelection(selChangedJsonMsgObj.index, selChangedJsonMsgObj.length, selChangedJsonMsgObj.selText, true, selChangedJsonMsgObj.isChangeBegin);
                            break;
                        case MpAvEditorBindingFunctionType.NotifyContentLengthChanged:
                            var contentLengthMsgObj = MpJsonObject.DeserializeBase64Object<MpQuillContentLengthChangedMessage>(msgJsonStr);
                            wv.Document.ContentEnd.Offset = contentLengthMsgObj.length;
                            break;
                        case MpAvEditorBindingFunctionType.NotifyDropEffectChanged:
                            var dropEffectChangedNtf = MpJsonObject.DeserializeBase64Object<MpQuillDropEffectChangedNotification>(msgJsonStr);
                            wv.UpdateDropEffect(dropEffectChangedNtf.dropEffect);
                            MpConsole.WriteLine($"{ctvm.CopyItemTitle} dropEffects: {dropEffectChangedNtf.dropEffect}");
                            break;
                        case MpAvEditorBindingFunctionType.NotifyDragStartOrEnd:
                            //if(wv.GetVisualAncestor<MpAvClipTileView>() is MpAvClipTileView ctv) {
                            //    var dddmsg = MpJsonObject.DeserializeBase64Object<MpQuillDragDropDataObjectMessage>(msgJsonStr);
                            //    ctv.UpdateSubSelectDragDataObject(dddmsg);
                            //}
                            var dragStartOrEndMsg = MpJsonObject.DeserializeBase64Object<MpQuillDragStartOrEndNotification>(msgJsonStr);
                            wv.WasDragStartedFromEditor = dragStartOrEndMsg.isStart;
                            break;
                        case MpAvEditorBindingFunctionType.NotifyReadOnlyChanged:
                            // TODO coordinate readOnly and sub-selection processing w/ webview extension..

                            if(ctvm.IsContentReadOnly) {
                                var disableReadOnlyMsg = MpJsonObject.DeserializeBase64Object<MpQuillDisableReadOnlyResponseMessage>(msgJsonStr);
                                ctvm.IsContentReadOnly = false;
                            } else {
                                var enableReadOnlyMsg = MpJsonObject.DeserializeBase64Object<MpQuillEnableReadOnlyResponseMessage>(msgJsonStr);
                                ctvm.IsContentReadOnly = true;
                            }
                            break;

                        case MpAvEditorBindingFunctionType.NotifySubSelectionEnabledChanged:
                            var subSelChangedNtf = MpJsonObject.DeserializeBase64Object<MpQuillSubSelectionChangedNotification>(msgJsonStr);
                            ctvm.IsSubSelectionEnabled = subSelChangedNtf.isSubSelectionEnabled;
                            break;
                        case MpAvEditorBindingFunctionType.NotifyDomLoaded:
                            wv.IsDomLoaded = true;
                            break;
                        case MpAvEditorBindingFunctionType.NotifyException:
                            var exceptionMsgObj = MpJsonObject.DeserializeBase64Object<MpQuillExceptionMessage>(msgJsonStr);
                            MpConsole.WriteLine(exceptionMsgObj);
                            break;
                    }
                });
                

                e.Handled = true;
                return;
            }

        }
    }
    class MpAvCefV8Func : CefV8Handler {
        private string _dbPath;


        public MpAvCefV8Func(string dbPath) : base() {
            _dbPath = dbPath;
        }

        protected override bool Execute(string name, CefV8Value @object, CefV8Value[] arguments, ref CefV8Value retval, ref string exception) {
            MpConsole.WriteLine("Received window binding msg name: " + name);
            if(arguments != null) {
                arguments.ForEach((x, i) => MpConsole.WriteLine("Arg " + i + ": " + x.ToString()));
            }
            if(name.StartsWith("get")) {
                // js is accessing data from cs...
                var test = MpAvCefNetApplication.BindingFunctionLookup;
                // is it accessible?
                Debugger.Break();

                if (name == "getAllTemplatesFromDb") {
                    List<MpTextTemplate> citl = MpDb.GetItems<MpTextTemplate>(_dbPath);

                    exception = null;
                    retval = CefV8Value.CreateString(JsonConvert.SerializeObject(citl));
                    return true;
                }
                if(name == "getDragData") {

                }
            } else if(name.StartsWith("notify")) {
                // js is setting cs data..

                CefProcessMessage browserProcMsg = new CefProcessMessage("WindowBindingResponse");
                browserProcMsg.ArgumentList.SetString(0, name);
                if(arguments != null) {
                    for (int i = 0; i < arguments.Length; i++) {
                        browserProcMsg.ArgumentList.SetString(i+1, arguments[i].GetStringValue());
                    }
                }else {
                    browserProcMsg.ArgumentList.SetString(1, "<NO PARAM>");
                }
                
                CefV8Context.GetCurrentContext().Frame.SendProcessMessage(CefProcessId.Browser, browserProcMsg);

                exception = null;
                retval = null;
                return true;
            }

            // unknown msg name
            MpConsole.WriteTraceLine("Uknown msg name: " + name);

            Debugger.Break();

            return false;
        }
    }
}
