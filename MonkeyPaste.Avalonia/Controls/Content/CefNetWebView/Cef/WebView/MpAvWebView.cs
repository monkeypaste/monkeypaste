﻿using Avalonia;
using System;
using PropertyChanged;
using MonkeyPaste.Common;
using Avalonia.Data;
using Avalonia.Threading;
using System.Threading.Tasks;

#if DESKTOP

#if PLAT_WV
        using AvaloniaWebView; 
#elif CEF_WV
using CefNet.Avalonia;
using CefNet.Internal;

#endif

#endif

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public class MpAvWebView :
#if DESKTOP

#if PLAT_WV
        UserControl
#elif CEF_WV
        WebView
#endif

#else
        MpAvNativeWebViewHost
#endif
        , MpIWebView, MpIHasDataContext {

#if !DESKTOP
        public object DataContext { get; set; }
#endif

        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        void MpIWebView.ExecuteJavascript(string script) {
#if DESKTOP
            this.GetMainFrame().ExecuteJavaScript(script, this.GetMainFrame().Url, 0);
#endif
        }
        #endregion

        #region Properties

        #region Address

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


        private void OnAddressChanged() {
            if (!Uri.IsWellFormedUriString(Address, UriKind.Absolute)) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                while (true) {
                    if (AliveBrowserObject != null &&
                        !AliveBrowserObject.IsLoading) {
                        break;
                    }
                    await Task.Delay(100);
                }
                Navigate(Address);
            });
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
        public MpAvWebView() {
            this.GetObservable(MpAvWebView.AddressProperty).Subscribe(value => OnAddressChanged());
#if DESKTOP
            Navigating += MpAvWebView_Navigating;
            Navigated += MpAvWebView_Navigated;
            LoadError += MpAvWebView_LoadError;
#endif

        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods

#if DESKTOP
        protected override WebViewGlue CreateWebViewGlue() {
            return new MpAvCefNetWebViewGlue(this);
        }
#endif

        #endregion

        #region Private Methods
#if DESKTOP
        private void MpAvWebView_Navigating(object sender, CefNet.BeforeBrowseEventArgs e) {
            if (this is not MpAvContentWebView) {
                MpConsole.WriteLine($"Navigating to: '{e.Url}'");
            }
            IsNavigating = true;
        }

        private void MpAvWebView_Navigated(object sender, CefNet.NavigatedEventArgs e) {
            if (this is not MpAvContentWebView) {
                MpConsole.WriteLine($"Navigated to: '{e.Url}'");
            }
            IsNavigating = false;
            if (e.Url == Address) {
                LoadErrorInfo = null;
            }
        }

        private void MpAvWebView_LoadError(object sender, CefNet.LoadErrorEventArgs e) {
            LoadErrorInfo = e.ErrorText;
        }
#endif
        #endregion

        #region Commands
        #endregion


    }
}