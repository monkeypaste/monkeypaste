using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using MonkeyPaste.Common.Wpf;
using Avalonia.Threading;

namespace AvCoreClipboardHandler {
    public class AvCoreClipboardHandler :
        MpIClipboardReaderComponent,
        MpIClipboardWriterComponent {
        static AvCoreClipboardHandler() {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        

        #region Private Variables
        #endregion

        #region MpIClipboardReaderComponentAsync Implementation

        public async Task<MpClipboardReaderResponse> ReadClipboardDataAsync(MpClipboardReaderRequest request) {
            if(!Dispatcher.UIThread.CheckAccess()) {
                return await Dispatcher.UIThread.InvokeAsync(()=>ReadClipboardDataAsync(request));
            }
            MpClipboardReaderResponse resp = await AvCoreClipboardReader.ProcessReadRequestAsync(request);
            return resp;
        }

        #endregion

        #region MpClipboardWriterComponent Implementation

        public async Task<MpClipboardWriterResponse> WriteClipboardDataAsync(MpClipboardWriterRequest request) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                return await Dispatcher.UIThread.InvokeAsync(() => WriteClipboardDataAsync(request));
            }
            MpClipboardWriterResponse resp = await AvCoreClipboardWriter.PerformWriteRequestAsync(request);
            return resp;
        }
        #endregion
    }
}