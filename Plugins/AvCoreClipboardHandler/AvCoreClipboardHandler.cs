using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using MonkeyPaste.Common.Wpf;

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

        async Task<MpClipboardReaderResponse> MpIClipboardReaderComponent.ReadClipboardDataAsync(MpClipboardReaderRequest request) {
            MpClipboardReaderResponse resp = await AvCoreClipboardReader.ProcessReadRequestAsync(request);
            return resp;
        }

        #endregion

        #region MpClipboardWriterComponent Implementation

        async Task<MpClipboardWriterResponse> MpIClipboardWriterComponent.WriteClipboardDataAsync(MpClipboardWriterRequest request) {
            MpClipboardWriterResponse resp = await AvCoreClipboardWriter.PerformWriteRequestAsync(request);
            return resp;
        }
        #endregion
    }
}