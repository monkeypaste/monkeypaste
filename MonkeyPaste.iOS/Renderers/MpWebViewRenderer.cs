using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using MonkeyPaste;
using Xamarin.Forms;
using MonkeyPaste.iOS;
using System.Threading.Tasks;

[assembly: ExportRenderer(typeof(MpWebView), typeof(MpWebViewRenderer))]
namespace MonkeyPaste.iOS {
    public class MpWebViewRenderer : WkWebViewRenderer {
        protected override void OnElementChanged(VisualElementChangedEventArgs e) {
            base.OnElementChanged(e);
            var webView = e.NewElement as MpWebView;
            if (webView != null)
                webView.EvaluateJavascript = async (js) => {
                    var x = await webView.EvaluateJavaScriptAsync(js);
                    return x;
                };
        }
        
        public override bool CanPerform(ObjCRuntime.Selector action, NSObject withSender) {
            if (MonkeyPaste.MpCopyItemTileCollectionPageViewModel.IsAnyItemExpanded) {
                NSOperationQueue.MainQueue.AddOperation(() => {
                    UIMenuController.SharedMenuController.SetMenuVisible(false, false); 
                });
            }
            return base.CanPerform(action, withSender);
        }
    }
}