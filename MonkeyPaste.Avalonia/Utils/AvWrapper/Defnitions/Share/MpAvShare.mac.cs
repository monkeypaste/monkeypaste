using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using System.Linq;
using System.Threading.Tasks;
using NSRectEdge = MonoMac.AppKit.NSRectEdge;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvShare {

        async Task PlatformRequestAsync(MpAvShareTextRequest request) {
            await Task.Delay(1);
            // from https://medium.com/@hawkfalcon/creating-a-custom-macos-sharing-service-in-swift-e7e0e46cbdd3

            NSString text = new NSString(request.Text);
            ShowShareUi(request, new[] { text });
        }

        async Task PlatformRequestAsync(MpAvShareMultipleFilesRequest request) {
            await Task.Delay(1);
            ShowShareUi(request, request.Files.Select(x => new NSUrl(x.FullPath)).ToArray());

        }

        private void ShowShareUi(MpAvShareRequestBase request, NSObject[] data) {
            NSSharingServicePicker ssp = new NSSharingServicePicker(data);
            NSWindow mw = GetMainWindow();
            MpDebug.Assert(mw != null, $"Error getting NSWindow");
            ssp.Delegate = new NSSharingServicePickerDelegate(mw.ContentView.Handle);

            if (request.PresentationSourceBounds == null ||
                request.PresentationSourceBounds == MpRect.Empty) {
                request.PresentationSourceBounds = new MpRect(
                    MpAvWindowManager.MainWindow.Bounds.ToPortableRect().Centroid(),
                    new MpSize(300, 300));
            }

            CGRect rect = new CGRect(
                request.PresentationSourceBounds.X,
                request.PresentationSourceBounds.Y,
                request.PresentationSourceBounds.Width,
                request.PresentationSourceBounds.Height);
            ssp.ShowRelativeToRect(rect, mw.ContentView, NSRectEdge.MinYEdge);
        }

        private NSWindow GetMainWindow() {
            if (MpAvWindowManager.MainWindow is not { } mw ||
                mw.TryGetPlatformHandle() is not IPlatformHandle ph ||
                MpAvMacProcessWatcher.GetCGWindowByHandle(ph.Handle) is not { } cg_win_obj ||
                cg_win_obj.ValueForKey(new NSString("kCGWindowNumber")) is not NSNumber win_num_obj ||
                !long.TryParse(win_num_obj.StringValue, out long win_num) ||
                new NSApplication(ph.Handle) is not { } app ||
                app.WindowWithWindowNumber(win_num) is not NSWindow window) {
                return null;
            }
            return window;
        }
    }
}
