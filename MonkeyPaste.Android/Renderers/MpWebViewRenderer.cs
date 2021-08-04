using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using MonkeyPaste;
using Xamarin.Forms.Platform.Android;
using System.Threading;
using Android.Service.Controls;
using Android.Webkit;
using System.Threading.Tasks;
using MonkeyPaste.Droid;

[assembly: ExportRenderer(typeof(MpWebView), typeof(MpWebViewRenderer))]
namespace MonkeyPaste.Droid {
    //from https://www.xamarinhelp.com/xamarin-forms-webview-executing-javascript/
    // TODO Add renderers to other platforms
    public class MpWebViewRenderer : WebViewRenderer {
        public MpWebViewRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e) {
            base.OnElementChanged(e);

            var webView = e.NewElement as MpWebView;
            if (webView != null) {
                webView.EvaluateJavascript = async (js) => {
                    var reset = new ManualResetEvent(false);
                    var response = string.Empty;
                    Device.BeginInvokeOnMainThread(() => {
                        Control?.EvaluateJavascript(js, new JavascriptCallback((r) => { response = r; reset.Set(); }));
                    });
                    await Task.Run(() => { 
                        reset.WaitOne(); 
                    });
                    return response;
                };
            }
        }
    }

    internal class JavascriptCallback : Java.Lang.Object, IValueCallback {
        public JavascriptCallback(Action<string> callback) {
            _callback = callback;
        }

        private Action<string> _callback;
        public void OnReceiveValue(Java.Lang.Object value) {
            _callback?.Invoke(Convert.ToString(value));
        }
    }
}