using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Text;

namespace AvCoreClipboardHandler {
    public class AvCoreClipboardHandler :
        MpIClipboardReaderComponent,
        MpIClipboardWriterComponent {
        static AvCoreClipboardHandler() {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static IClipboard ClipboardRef { get; private set; }
        #region Private Variables
        #endregion

        #region MpIClipboardReaderComponentAsync Implementation

        public async Task<MpClipboardReaderResponse> ReadClipboardDataAsync(MpClipboardReaderRequest request) {
            if (ClipboardRef == null) {
                var mw = Application.Current.GetMainWindow();
                if (mw is Control c && TopLevel.GetTopLevel(c) is TopLevel tl &&
                    tl.Clipboard is IClipboard cb) {
                    ClipboardRef = cb;
                }
                if (ClipboardRef == null) {
                    return null;
                }
            }
            MpClipboardReaderResponse resp = null;
            if (Dispatcher.UIThread.CheckAccess()) {
                resp = await AvCoreClipboardReader.ProcessReadRequestAsync(request);
            } else {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    resp = await ReadClipboardDataAsync(request);
                });
            }

            return resp;
        }

        #endregion

        #region MpClipboardWriterComponent Implementation

        public async Task<MpClipboardWriterResponse> WriteClipboardDataAsync(MpClipboardWriterRequest request) {
            if (ClipboardRef == null) {
                var mw = Application.Current.GetMainWindow();
                if (mw is Control c && TopLevel.GetTopLevel(c) is TopLevel tl &&
                    tl.Clipboard is IClipboard cb) {
                    ClipboardRef = cb;
                }
                if (ClipboardRef == null) {
                    return null;
                }
            }
            MpClipboardWriterResponse resp = null;
            if (Dispatcher.UIThread.CheckAccess()) {
                resp = await AvCoreClipboardWriter.PerformWriteRequestAsync(request);
            } else {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    resp = await WriteClipboardDataAsync(request);
                });
            }

            return resp;
        }
        #endregion
    }
}