using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Threading.Tasks;
using System.IO;
using Avalonia.VisualTree;
using System.Diagnostics;
using System.Collections.Concurrent;

#if CEFNET_WV
using CefNet;
using CefNet.Avalonia;
using CefNet.Internal;
#elif OUTSYS_WV
using WebViewControl;
#elif SUGAR_WV
#if MAC
using Foundation;
using WkWebview = WebKit.WKWebView;
using Avalonia.WebView.MacCatalyst.Core;
#endif
using WebViewCore.Configurations;
using AvaloniaWebView;
#endif

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public class MpAvWebView :
#if CEFNET_WV
        WebView
#elif OUTSYS_WV
        WebView, MpIWebViewNavigator
#elif SUGAR_WV
        UserControl
#else
        MpAvNativeWebViewHost
#endif
        ,
        MpICanExecuteJavascript,
        MpAvIWebViewBindingResponseHandler,
        MpIHasDevTools {


        #region Private Variables
#if CEFNET_WV
        private bool _isBrowserCreated = false;
#endif
        #endregion

        #region Constants
        #endregion

        #region Statics
        static MpAvWebView() {
#if OUTSYS_WV
            WebView.Settings.OsrEnabled = true;
            WebView.Settings.AddCommandLineSwitch("use-mock-keychain", null);
            WebView.Settings.AddCommandLineSwitch("process-per-site", null);
#elif SUGAR_WV


#endif
        }

#if SUGAR_WV
        public static void ConfigureWebViewCreationProperties(WebViewCreationProperties config) {
            config.AreDevToolEnabled =
#if DEBUG
            true;
#else
            false; 
#endif
            config.AreDefaultContextMenusEnabled = false;
            config.IsStatusBarEnabled = false;
            config.DefaultWebViewBackgroundColor = System.Drawing.Color.FromArgb(System.Drawing.Color.Transparent.ToArgb());
            config.AdditionalBrowserArguments = MpAvCefCommandLineArgs.ToArgString();
            config.BrowserExecutableFolder = Path.GetDirectoryName(new MpAvPlatformInfoBase().EditorPath);

            MpConsole.WriteLine($"Cef args: '{config.AdditionalBrowserArguments}'");
            //config.UserDataFolder = _creationProperties.UserDataFolder;
            //config.Language = MpAvCurrentCultureViewModel.Instance.CurrentCulture.Name;
            //config.ProfileName = _creationProperties.ProfileName;
            //config.IsInPrivateModeEnabled = _creationProperties.IsInPrivateModeEnabled;
        }
#endif
#endregion

            #region Interfaces
#if CEFNET_WV || OUTSYS_WV || SUGAR_WV
            public void OpenDevTools() {
#if RELEASE
            return;
#elif OUTSYS_WV
            ShowDeveloperTools();
            return;
#elif CEFNET_WV
            if (!_isBrowserCreated) {
                Dispatcher.UIThread.Post(async () => {
                    while (!_isBrowserCreated) {
                        await Task.Delay(100);
                    }
                    base.ShowDevTools();
                });
                return;
            }
            base.ShowDevTools();
            return;
#elif SUGAR_WV
            if (InnerWebView != null) {
#if MAC
                Dispatcher.UIThread.Post(async () => {
                    InnerWebView.Focus();
                    Mp.Services.PointerSimulator.SimulateRightClick(InnerWebView);
                    await Task.Delay(300);
                    Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence("Down");
                    await Task.Delay(50);
                    Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence("Down");
                    await Task.Delay(50);
                    Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence("Enter");
                });

#else
                InnerWebView.OpenDevToolsWindow(); 
#endif
            }
            return;
#endif
        }
#endif

        #region MpAvIWebViewBindingResponseHandler Implemention

#if CEFNET_WV
#elif OUTSYS_WV || SUGAR_WV
        public MpAvIWebViewBindingResponseHandler BindingHandler =>
            this;
#else
        public override MpAvIWebViewBindingResponseHandler BindingHandler =>
            this;
#endif

        public virtual void HandleBindingNotification(MpEditorBindingFunctionType notificationType, string msgJsonBase64Str, string contentHandle) {
#if DEBUG
            LogResponse(notificationType);
            object ntf = null;
            switch (notificationType) {
                case MpEditorBindingFunctionType.notifyShowDebugger:
                    ntf = msgJsonBase64Str.DeserializeBase64Object<MpQuillShowDebuggerNotification>();
                    if (ntf is MpQuillShowDebuggerNotification showDebugNtf) {
                        MpConsole.WriteLine($"WebView ShowDebugger Request Received [{DataContext}] {showDebugNtf.reason}");
                        OpenDevTools();
                    }
                    break;
                case MpEditorBindingFunctionType.notifyException:
                    OpenDevTools();
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillExceptionMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillExceptionMessage exceptionMsgObj) {
                        MpConsole.WriteLine($"WebView Exception ntf Received [{DataContext}] {exceptionMsgObj}");

                    }

                    break;
            }
#endif
        }
        #endregion

        #region MpIWebViewNavigator 
#if OUTSYS_WV || SUGAR_WV
        public virtual void Navigate(string urlStr) {
            // NOTE outsys has an address prop, this does nothing but is called when address changes
            // so treating it like Navigating cefnet event

#if OUTSYS_WV
            IsNavigating = true;
#elif SUGAR_WV
            if (!Uri.IsWellFormedUriString(urlStr, UriKind.Absolute)) {
                return;
            }
            if(!MpAvAssetMover.IsLoaded) {
                Dispatcher.UIThread.Post(async () => {
                    while(!MpAvAssetMover.IsLoaded) {
                        await Task.Delay(100);
                    }
                    Navigate(urlStr);
                });
                return;
            }
            InnerWebView.Url = new Uri(urlStr, UriKind.Absolute);
#endif
        }
#endif
        #endregion

        #region MpICanExecuteJavascript 
#if SUGAR_WV
        async 
#endif
        void MpICanExecuteJavascript.ExecuteJavascript(string script) {
#if CEFNET_WV
            this.GetMainFrame().ExecuteJavaScript(script, this.GetMainFrame().Url, 0);
#elif SUGAR_WV
            var sw = Stopwatch.StartNew();
            bool needed_wait = !CanMakeRequest(script);
            while (!CanMakeRequest(script)) {
                await Task.Delay(50);
                //MpConsole.WriteLine($"{DataContext} Waiting to execute script: {script}");
                if (sw.Elapsed > TimeSpan.FromSeconds(5)) {
                    //MpConsole.WriteLine($"{DataContext} Wait to executeJavascript timeout for script: {script}");
                    return;
                }
            }
            if (needed_wait) {
                //MpConsole.WriteLine($"{DataContext} Wait complete ({sw.ElapsedMilliseconds}ms) for script: {script}");
            }
            try {
                LogRequest(script);

                if (DataContext is MpAvClipTileViewModel ctvm &&
                    ctvm.CopyItemTitle == "Text12" &&
                    ctvm.IsWindowOpen) {
                    //return;
                }
                Dispatcher.UIThread.Post(() => InnerWebView.ExecuteScriptAsync(script).FireAndForgetSafeAsync());
            }
            catch (Exception ex) {
                MpConsole.WriteLine($"Error executing script!", true);
                MpConsole.WriteLine($"Script: '{script}'");
                MpConsole.WriteLine($"Ex: {ex}", false, true);

            }
#endif
        }
#if SUGAR_WV
        public async Task<string> ExecuteJavascriptAsync(string msgJsonBase64Str) {
            try {

                string result = await InnerWebView.ExecuteScriptAsync(msgJsonBase64Str);
                return result.Replace("\"", string.Empty).ToStringFromBase64();
            }
            catch (Exception ex) {

                MpConsole.WriteLine($"Error executing script!", true);
                MpConsole.WriteLine($"Script: '{msgJsonBase64Str}'");
                MpConsole.WriteLine($"Ex: {ex}", false, true);
            }
            return string.Empty;
        }
#endif
        #endregion
        #endregion

        #region Properties

#if SUGAR_WV
        protected WebView InnerWebView =>
            Content as WebView;
#endif

        #region State

        protected List<string> RequestLookup { get; set; } = [];
        protected List<string> ResponseLookup { get; set; } = [];
        protected void LogRequest(string script) {
            if (script.SplitNoEmpty("(") is not { } script_parts ||
                        script_parts.Length < 1 ||
                    script_parts[0] is not string reqTypeName) {
                return;
            }
            RequestLookup.Add(reqTypeName);
        }

        protected void LogResponse(MpEditorBindingFunctionType notificationType) {
            ResponseLookup.Add(notificationType.ToString());
        }

        protected bool CanMakeRequest(string script) {
            if (script.SplitNoEmpty("(") is not { } script_parts ||
                        script_parts.Length == 0 ||
                        script_parts[0] is not string reqTypeName) {
                return false;
            }

#if SUGAR_WV
            if (this.InnerWebView == null ||
                        !this.InnerWebView.IsLoaded ||
                        this.InnerWebView.Url == null ||
                        this.InnerWebView.Url.AbsoluteUri != Address) {
                // webview not loaded
                return false;
            } 
#endif
            if (this is MpAvContentWebView cwv2 &&
                !cwv2.IsDomLoaded) {
                // dom not loaded
                return false;
            }
            string main_req_name = "initMain_ext";
            string content_req_name = "loadContentAsync_ext";

            bool is_main_loaded = RequestLookup.Contains(main_req_name);
            if (reqTypeName == main_req_name) {
                // should be initial setup request
                if (is_main_loaded) {
                    // already made initial setup request, which can be resent but shouldn't that often
                    //MpConsole.WriteLine($"Warning! WebView {DataContext} has already made a '{reqTypeName}'. Should not need repeating but allowing");
                }
                return true;
            }
            if (this is MpAvPlainHtmlConverterWebView) {
                // no further checks needed
                return true;
            }
            if (is_main_loaded) {
                if (this is not MpAvContentWebView cwv) {
                    return true;
                }
                if (reqTypeName == content_req_name && cwv.IsEditorInitialized) {
                    // only allow loadContent when editor initialized
                    return true;
                }
                if (cwv.IsEditorInitialized && cwv.IsEditorLoaded) {
                    // allow any other request after init and load are complete
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Address

#if !OUTSYS_WV
        private string _Address;
        public string Address {
            get { return _Address; }
            set { SetAndRaise(AddressProperty, ref _Address, value); }
        }
        public static readonly DirectProperty<MpAvWebView, string> AddressProperty =
            AvaloniaProperty.RegisterDirect<MpAvWebView, string>(
                nameof(Address),
                x => x.Address,
                (x, o) => x.Address = o,
                defaultBindingMode: BindingMode.TwoWay);

#endif

        private void OnAddressChanged() {
#if CEFNET_WV
            if (!_isBrowserCreated) {
                return;
            }
#endif
            if (!Uri.IsWellFormedUriString(Address, UriKind.Absolute) ||
                MpUrlHelpers.IsBlankUrl(Address)) {
                return;
            }
            Navigate(Address);
        }

#if CEFNET_WV
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            _isBrowserCreated = false;
            this.ClearDisposables();
        }
#endif
        #endregion

        #region IsNavigating

        private bool _IsNavigating;
        public bool IsNavigating {
            get { return _IsNavigating; }
            set { SetAndRaise(IsNavigatingProperty, ref _IsNavigating, value); }
        }

        public static readonly DirectProperty<MpAvWebView, bool> IsNavigatingProperty =
            AvaloniaProperty.RegisterDirect<MpAvWebView, bool>(
                nameof(IsNavigating),
                x => x.IsNavigating,
                (x, o) => x.IsNavigating = o,
                defaultBindingMode: BindingMode.TwoWay);


        #endregion

        #region DocumentTitle

        private string _DocumentTitle;
        public string DocumentTitle {
            get { return _DocumentTitle; }
            private set { SetAndRaise(DocumentTitleProperty, ref _DocumentTitle, value); }
        }

        public static readonly DirectProperty<MpAvWebView, string> DocumentTitleProperty =
            AvaloniaProperty.RegisterDirect<MpAvWebView, string>(
                nameof(DocumentTitle),
                x => x.DocumentTitle,
                (x, o) => x.DocumentTitle = o,
                defaultBindingMode: BindingMode.OneWay);


        #endregion

        #region LoadErrorInfo

        private string _LoadErrorInfo;
        public string LoadErrorInfo {
            get { return _LoadErrorInfo; }
            set { SetAndRaise(LoadErrorInfoProperty, ref _LoadErrorInfo, value); }
        }

        public static readonly DirectProperty<MpAvWebView, string> LoadErrorInfoProperty =
            AvaloniaProperty.RegisterDirect<MpAvWebView, string>(
                nameof(LoadErrorInfo),
                x => x.LoadErrorInfo,
                (x, o) => x.LoadErrorInfo = o,
                defaultBindingMode: BindingMode.TwoWay);


        #endregion

        #endregion

        #region Constructors
        public MpAvWebView() : base() {
            this.GetObservable(MpAvWebView.AddressProperty).Subscribe(value => OnAddressChanged()).AddDisposable(this);
#if SUGAR_WV
            this.Content = new WebView();
            InnerWebView.WebViewCreated += InnerWebView_WebViewCreated;
            InnerWebView.NavigationStarting += InnerWebView_NavigationStarting;
            InnerWebView.NavigationCompleted += InnerWebView_NavigationCompleted;
            InnerWebView.WebMessageReceived += InnerWebView_WebMessageReceived;
#elif CEFNET_WV
            Navigating += MpAvWebView_Navigating;
            Navigated += MpAvWebView_Navigated;
            LoadError += MpAvWebView_LoadError;
#elif OUTSYS_WV
            LoadFailed += MpAvWebView_LoadFailed;
            Navigated += MpAvWebView_Navigated;

            void SendMessage(string fn, string msg, string handle) {
                Dispatcher.UIThread.Post(() => {

                    //var resp = MpJsonConverter.DeserializeBase64Object<MpQuillPostMessageResponse>(msg);
                    //MpEditorBindingFunctionType funcType = resp.msgType.ToEnum<MpEditorBindingFunctionType>();
                    //HandleBindingNotification(funcType, resp.msgData, resp.handle);
                    MpEditorBindingFunctionType funcType = fn.ToEnum<MpEditorBindingFunctionType>();
                    HandleBindingNotification(funcType, msg, handle);
                });
            }

#pragma warning disable CS8974 // Converting method group to non-delegate type
            this.RegisterJavascriptObject("SendMessage", SendMessage);
#pragma warning restore CS8974 // Converting method group to non-delegate type
#endif

        }
        #endregion

        #region Public Methods

#if OUTSYS_WV || SUGAR_WV
        public virtual void OnNavigated(string url) { }
#endif
        #endregion

        #region Protected Methods


        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);
            if (!e.IsMiddleRelease(this)) {
                return;
            }
            OpenDevTools();
        }

#if CEFNET_WV
        protected override void OnDocumentTitleChanged(DocumentTitleChangedEventArgs e) {
            base.OnDocumentTitleChanged(e);
            DocumentTitle = e.Title;
        }
        protected override WebViewGlue CreateWebViewGlue() {
            return new MpAvCefNetWebViewGlue(this);
        }
        protected override void OnBrowserCreated(EventArgs e) {
            base.OnBrowserCreated(e);
            _isBrowserCreated = true;
            OnAddressChanged();
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
            if (!e.IsRightPress(this) ||
                e.KeyModifiers != KeyModifiers.Control) {
                return;
            }
            OpenDevTools();
        }



        private void W_Closed(object sender, EventArgs e) {
            if (sender is not Window w) {
                return;
            }
            w.Closed -= W_Closed;
            this.Dispose(false);
        }
#endif
        protected override void OnLoaded(global::Avalonia.Interactivity.RoutedEventArgs e) {
            base.OnLoaded(e);
            if (this is MpAvContentWebView) {
                return;
            }
#if CEFNET_WV
            if (MpAvWindowManager.GetTopLevel(this) is MpAvWindow w) {
                w.Closed += W_Closed;
            }
#endif
        }
        protected override void OnUnloaded(global::Avalonia.Interactivity.RoutedEventArgs e) {
            base.OnUnloaded(e);
#if SUGAR_WV
            if (InnerWebView == null) {
                return;
            }
            InnerWebView.WebViewCreated -= InnerWebView_WebViewCreated;
            InnerWebView.NavigationStarting -= InnerWebView_NavigationStarting;
            InnerWebView.NavigationCompleted -= InnerWebView_NavigationCompleted;
            InnerWebView.WebMessageReceived -= InnerWebView_WebMessageReceived;
#endif
        }
        #endregion

        #region Private Methods
#if CEFNET_WV
        private void MpAvWebView_Navigating(object sender, CefNet.BeforeBrowseEventArgs e) {
            IsNavigating = true;
        }

        private void MpAvWebView_Navigated(object sender, CefNet.NavigatedEventArgs e) {
            IsNavigating = false;
            if (e.Url == Address) {
                LoadErrorInfo = null;
            }
        }

        private void MpAvWebView_LoadError(object sender, CefNet.LoadErrorEventArgs e) {
            LoadErrorInfo = e.ErrorText;
        }
#elif OUTSYS_WV
        private void MpAvWebView_LoadFailed(string url, int errorCode, string frameName) {
            LoadErrorInfo = $"Error Code: '{errorCode}' Frame: '{frameName}' Url: '{url}'";
        }
        private void MpAvWebView_Navigated(string url, string frameName) {
            IsNavigating = false;

            // NOTE can't find a good spot to disable this cause of timing when outsys tries to enable so doing here
            //DragDrop.SetAllowDrop(this, false);

            if (url == Address) {
                LoadErrorInfo = null;
            }
            OnNavigated(url);
        }
#elif SUGAR_WV

        private void InnerWebView_WebViewCreated(object sender, WebViewCore.Events.WebViewCreatedEventArgs e) {
#if MAC
            if (InnerWebView.PlatformWebView is MacCatalystWebViewCore wvc &&
                    wvc.WebView is WkWebview wv) {
                //wv.SetValueForKey(NSObject.FromObject(true), new NSString("drawsTransparentBackground"));
                wv.SetValueForKey(NSObject.FromObject(false), new NSString("drawsBackground"));
            } else {

            }
#elif ANDROID
            MpAvDeviceWrapper.Instance.DeviceWebViewHelper.ConfigureWebview(InnerWebView);
#elif IOS
            MpAvDeviceWrapper.Instance.DeviceWebViewHelper.ConfigureWebview(new object[] { InnerWebView, this });
#endif
        }
        private void InnerWebView_NavigationCompleted(object sender, WebViewCore.Events.WebViewUrlLoadedEventArg e) {
            Dispatcher.UIThread.Post(() => {
                IsNavigating = false;
                OnNavigated(InnerWebView.Url.AbsoluteUri);
            });
        }

        private void InnerWebView_NavigationStarting(object sender, WebViewCore.Events.WebViewUrlLoadingEventArg e) {
            Dispatcher.UIThread.Post(() => {
                IsNavigating = true;
            });
        }

        private void InnerWebView_WebMessageReceived(object sender, WebViewCore.Events.WebViewMessageReceivedEventArgs e) {
            if (e.Message is not string jsonStr ||
                    string.IsNullOrEmpty(jsonStr)) {
                return;
            }
            Dispatcher.UIThread.Post(() => {
                var msg = jsonStr.DeserializeObject<MpQuillPostMessageResponse>();
                HandleBindingNotification(msg.msgType, msg.msgData, msg.handle);
            });
        }
#endif
        #endregion

        #region Commands
        #endregion


    }
}
