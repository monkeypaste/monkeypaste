using CoreFoundation;
using Foundation;
using MobileCoreServices;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using ObjCRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UIKit;

namespace iosTest.iOS.ShareExtension {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = "<Pending>")]
    public partial class ShareViewController : UIViewController {
        // from https://stackoverflow.com/a/64431680/105028
        public ShareViewController(IntPtr handle) : base(handle) {
        }

        string docPath = "";

        public override void ViewDidLoad() {
            base.ViewDidLoad();

            try {
                var containerURL = new NSFileManager().GetContainerUrl("group.com.qsiga.startbss");
                docPath = $"{containerURL.Path}/share";

                //  Create directory if not exists
                try {
                    NSFileManager.DefaultManager.CreateDirectory(docPath, true, null);
                }
                catch (Exception e) {
                    MpConsole.WriteTraceLine($"Share load error.", e);
                }

                //  removing previous stored files
                NSError contentError;
                var files = NSFileManager.DefaultManager.GetDirectoryContent(docPath, out contentError);
                foreach (var file in files) {
                    try {
                        NSError err;
                        NSFileManager.DefaultManager.Remove($"{docPath}/{file}", out err);
                    }
                    catch (Exception e) {
                        MpConsole.WriteTraceLine($"Share load error.", e);
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine("ShareViewController exception: " + e);
            }
        }

        public override void ViewDidAppear(bool animated) {
            var alertView = UIAlertController.Create("Export", " ", UIAlertControllerStyle.Alert);

            PresentViewController(alertView, true, () =>
            {

                var group = new DispatchGroup();

                foreach (var item in ExtensionContext.InputItems) {

                    var inputItem = item as NSExtensionItem;

                    foreach (var provider in inputItem.Attachments) {

                        var itemProvider = provider as NSItemProvider;
                        group.Enter();
                        //itemProvider.LoadItem(UTType.Data.ToString(), null, (data, error) =>
#pragma warning disable CA1416 // Validate platform compatibility
                        itemProvider.LoadItem(UniformTypeIdentifiers.UTType.SHCustomCatalogContentType.Identifier, null, (data, error) =>
                        {
                            if (error == null) {
                                //  Note: "data" may be another type (e.g. Data or UIImage). Casting to URL may fail. Better use switch-statement for other types.
                                //  "screenshot-tool" from iOS11 will give you an UIImage here
                                var url = data as NSUrl;
                                var path = $"{docPath}/{(url.PathComponents.LastOrDefault() ?? "")}";

                                NSError err;
                                NSFileManager.DefaultManager.Copy(url, NSUrl.CreateFileUrl(path, null), out err);
                            }
                            group.Leave();
                        });
#pragma warning restore CA1416 // Validate platform compatibility
                    }
                }

                group.Notify(DispatchQueue.MainQueue, () =>
                {
                    try {
                        var jsonData = new Dictionary<string, string>() { { "action", "incoming-files" } }.SerializeObject();
                        var jsonString = NSString.FromData(jsonData, NSStringEncoding.UTF8).CreateStringByAddingPercentEncoding(NSUrlUtilities_NSCharacterSet.UrlQueryAllowedCharacterSet);
                        var result = openURL(new NSUrl($"startbss://share?{jsonString}"));
                    }
                    catch (Exception e) {
                        alertView.Message = $"Error: {e.Message}";
                    }
                    DismissViewController(false, () =>
                    {
                        ExtensionContext?.CompleteRequest(new NSExtensionItem[] { }, null);
                    });
                });
            });
        }

        public bool openURL(NSUrl url) {
            UIResponder responder = this;
            while (responder != null) {
                var application = responder as UIApplication;
                if (application != null)
                    return CallSelector(application, url);

                responder = responder?.NextResponder;
            }
            return false;
        }

        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        static extern bool _callSelector(
            IntPtr target,
            IntPtr selector,
            IntPtr url,
            IntPtr options,
            IntPtr completionHandler
        );

        private bool CallSelector(UIApplication application, NSUrl url) {
            Selector selector = new Selector("openURL:options:completionHandler:");

            return _callSelector(
                application.Handle,
                selector.Handle,
                url.Handle,
                IntPtr.Zero,
                IntPtr.Zero
            );
        }
    }
}