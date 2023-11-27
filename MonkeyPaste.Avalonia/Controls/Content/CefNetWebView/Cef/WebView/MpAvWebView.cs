using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Threading.Tasks;


#if CEFNET_WV
using CefNet.Avalonia;
using CefNet.Internal;
#elif OUTSYS_WV
using WebViewControl;
#endif

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public class MpAvWebView :
#if CEFNET_WV
        WebView
#elif OUTSYS_WV
        WebView, MpIWebViewNavigator
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
#endif
        }
        #endregion

        #region Interfaces

        public void OpenDevTools() =>
#if OUTSYS_WV
            ShowDeveloperTools();
#else
        ShowDevTools();
#endif

        #region MpAvIWebViewBindingResponseHandler Implemention
#if CEFNET_WV
#elif OUTSYS_WV
        public MpAvIWebViewBindingResponseHandler BindingHandler =>
            this;
#else
        public override MpAvIWebViewBindingResponseHandler BindingHandler =>
            this;
#endif

        public virtual void HandleBindingNotification(MpEditorBindingFunctionType notificationType, string msgJsonBase64Str, string contentHandle) {

#if DEBUG
            MpJsonObject ntf = null;
            switch (notificationType) {
                case MpEditorBindingFunctionType.notifyShowDebugger:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillShowDebuggerNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillShowDebuggerNotification showDebugNtf) {
                        MpConsole.WriteLine($"WebView ShowDebugger Request Received [{DataContext}] {showDebugNtf.reason}");
                        OpenDevTools();
                    }
                    break;
                case MpEditorBindingFunctionType.notifyException:
                    OpenDevTools();
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillExceptionMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillExceptionMessage exceptionMsgObj) {
                        MpConsole.WriteLine($"WebView Exception ntf Received [{DataContext}] {exceptionMsgObj}");

                    }

                    break;
            }
#endif
        }
        #endregion

        #region MpIWebViewNavigator 
#if OUTSYS_WV
        public void Navigate(string url) {
            // NOTE outsys has an address prop, this does nothing but is called when address changes
            // so treating it like Navigating cefnet event
            IsNavigating = true;
        }
#endif
        #endregion

        #region MpICanExecuteJavascript 
        void MpICanExecuteJavascript.ExecuteJavascript(string script) {
#if CEFNET_WV
            this.GetMainFrame().ExecuteJavaScript(script, this.GetMainFrame().Url, 0);
#endif
        }
        #endregion
        #endregion

        #region Properties

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
            this.GetObservable(MpAvWebView.AddressProperty).Subscribe(value => OnAddressChanged());
#if CEFNET_WV
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

#if OUTSYS_WV
        public virtual void OnNavigated(string url) { }
#endif
        #endregion

        #region Protected Methods

#if CEFNET_WV
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
            if (e.IsMiddleDown(this)) {
                this.ShowDevTools();
            }
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
            if (TopLevel.GetTopLevel(this) is MpAvWindow w) {
                w.Closed += W_Closed;
            } 
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
#endif
        #endregion

        #region Commands
        #endregion


    }
}
