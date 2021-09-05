using AppKit;
using FFImageLoading.Forms.Platform;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace MonkeyPaste.Mac
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public MpINativeInterfaceWrapper MacInterfaceWrapper { get; set; }
        NSWindow window;
        public AppDelegate() {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
            window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            window.Title = "Xamarin.Forms on Mac!"; // choose your own Title here
            window.TitleVisibility = NSWindowTitleVisibility.Hidden;
        }

        public override NSWindow MainWindow {
            get { return window; }
        }

        public override void DidFinishLaunching(NSNotification notification) {
            Rg.Plugins.Popup.Popup.Init();

            global::Xamarin.Forms.Forms.Init();

            CachedImageRenderer.Init();
            CachedImageRenderer.InitImageSourceHandler();

            Forms.Init();

            MacInterfaceWrapper = new MpMacInterfaceWrapper() {
                DbInfo = new MpDbFilePath_Mac(),
                UiLocationFetcher = new MpUiLocationFetcher(),
                TouchService = new MpGlobalTouch()
            };
            LoadApplication(new App(MacInterfaceWrapper));
            base.DidFinishLaunching(notification);
        }
    }
}
