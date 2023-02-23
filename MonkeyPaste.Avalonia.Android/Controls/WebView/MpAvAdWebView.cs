using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Webkit;
using Com.Xamarin.Formsviewgroup;
using Java.Nio;
using MonkeyPaste.Common;
using SkiaSharp;
using SkiaSharp.Views.Android;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdWebView : WebView, MpIWebViewNavigator, MpIOffscreenRenderSource {
        #region Private Variable
        private Canvas offscreen = new Canvas();

        private string _navUrl;
        private bool _isPageFinished;
        private bool _isProgressDone;

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
                BufferChanged?.Invoke(this, null);
            }
        }
        public event EventHandler BufferChanged;
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

        public MpAvAdWebView(Context context) : base(context) {
            SetWebViewClient(_webViewClient);
            SetWebChromeClient(_webChromeClient);
            _webViewClient.PageFinished += WebViewClient_PageFinished;
            _webChromeClient.ProgressDone += WebChromeClient_ProgressDone;
        }

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
                }
                //var skbmp = bitmap.ToSKBitmap();
                //Buffer = skbmp.Bytes;
                //Buffer = ConvertBitmapToByteArray(bitmap);

                //var stream = new FileStream(System.IO.Path.Combine(MpPlatform.Services.PlatformInfo.StorageDir, "ss.png"), FileMode.Create);
                //bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
                //stream.Close();

                //MpFileIo.WriteByteArrayToFile(
                //    System.IO.Path.Combine(MpPlatform.Services.PlatformInfo.StorageDir, "ss.png"),
                //    Buffer, false);
                ////Create paint to draw effect
                //Paint p = new Paint();
                //p.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.Darken));
                ////Draw on the canvas. Fortunately, this class uses relative coordinates so that we don't have to worry about where this View is actually positioned.
                //canvas.DrawBitmap(bitmap, 0, 0, p);
            }
        }

        #endregion

        #region Private Methods
        private byte[] ConvertBitmapToByteArray(Bitmap bitmap) {
            ByteBuffer byteBuffer = ByteBuffer.Allocate(bitmap.ByteCount);
            bitmap.CopyPixelsToBuffer(byteBuffer);
            byteBuffer.Rewind();
            byteBuffer.Order(ByteOrder.LittleEndian);
            var bytes = new byte[bitmap.ByteCount];
            byteBuffer.Get(bytes);
            return bytes;
        }
        #endregion

        #region Commands
        #endregion
    }
}
