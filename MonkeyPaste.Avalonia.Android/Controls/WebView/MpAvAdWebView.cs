using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Com.Xamarin.Formsviewgroup;
using Java.Nio;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SkiaSharp;
using SkiaSharp.Views.Android;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdWebView :
        WebView,
        MpIWebViewNavigator,
        MpIOffscreenRenderSource {

        #region Private Variable
        private Canvas offscreen = new Canvas();

        private string _navUrl;
        private bool _isPageFinished;
        private bool _isProgressDone;
        private MpIWebViewHost _host;

        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIOffscreenRenderSource Implementation
        private byte[] _buffer;
        public byte[] Buffer {
            get => _buffer;
            private set {
                _buffer = value;
                //BufferChanged?.Invoke(this, null);
            }
        }

        #endregion
        #region MpIWebViewNavigator Implementation

        public void Navigate(string url) {
            _navUrl = url;
            _isPageFinished = false;
            _isProgressDone = false;
            this.LoadUrl(url);
        }

        public event EventHandler<string> Navigated;
        #endregion

        #endregion

        #region Properties
        private MpAvAdWebViewClient _webViewClient = new MpAvAdWebViewClient();
        //public new MpAvAdWebViewClient WebViewClient =>
        //    _webViewClient;

        private MpAvAdWebChromeClient _webChromeClient = new MpAvAdWebChromeClient();
        //public new MpAvAdWebChromeClient WebChromeClient =>
        //    _webChromeClient;

        #endregion

        #region Events
        #endregion

        #region Constructors

        public MpAvAdWebView(Context context, MpIWebViewHost host) : base(context) {
            _host = host;

            Settings.JavaScriptEnabled = true;
            Settings.AllowFileAccess = true;
            Settings.AllowFileAccessFromFileURLs = true;
            Settings.AllowUniversalAccessFromFileURLs = true;
            AddJavascriptInterface(new MpAvAdJsInterface(context, host), "CSharp");
            SetWebViewClient(_webViewClient);
            SetWebChromeClient(_webChromeClient);

            _webViewClient.PageFinished += WebViewClient_PageFinished;
            _webChromeClient.ProgressDone += WebChromeClient_ProgressDone;
        }


        public MpAvAdWebView(Context context, IAttributeSet attrs) : base(context, attrs) {
        }

        public MpAvAdWebView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) {
        }

        [System.Obsolete]
        public MpAvAdWebView(Context context, IAttributeSet attrs, int defStyleAttr, bool privateBrowsing) : base(context, attrs, defStyleAttr, privateBrowsing) {
        }

        public MpAvAdWebView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes) {
        }

        protected MpAvAdWebView(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        public override bool OnTouchEvent(MotionEvent e) {
            if (_host == null) {
                return true;
            }

            _host.SendPointerEvent(e.RawX, e.RawY, e.Action.ToPointerEventType());
            return false;
        }
        protected override void OnDraw(Canvas canvas) {
            //We want the superclass to draw directly to the offscreen canvas so that we don't get an infinitely deep recursive call
            if (canvas == offscreen) {
                base.OnDraw(canvas);
            } else {
                //Our offscreen image uses the dimensions of the view rather than the canvas
                Bitmap bitmap = Bitmap.CreateBitmap(Width, Height, Bitmap.Config.Argb8888); //Config.ARGB_8888);
                //offscreen = new Canvas(bitmap);
                //offscreen.SetViewport(Width, Height);
                offscreen.SetBitmap(bitmap);
                base.Draw(offscreen);
                using (var ms = new MemoryStream()) {
                    bitmap.Compress(Bitmap.CompressFormat.Png, 100, ms);
                    if (_buffer == null ||
                        _buffer.Length != ms.Length) {
                        _buffer = new byte[ms.Length];
                    }
                    ms.Position = 0;
                    ms.Read(Buffer, 0, (int)ms.Length);
                    if (_host != null) {
                        _host.Render();
                    }
                }
            }
        }

        #endregion

        #region Private Methods


        private void WebChromeClient_ProgressDone(object sender, EventArgs e) {
            _isProgressDone = true;
            NavCheck();
        }

        private void WebViewClient_PageFinished(object sender, string e) {
            MpConsole.WriteLine("PageFinished url: " + e);
            if (e == _navUrl) {
                _isPageFinished = true;
            }
            NavCheck();
        }

        private void NavCheck() {
            if (_isProgressDone && _isPageFinished) {
                Navigated?.Invoke(this, _navUrl);
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
