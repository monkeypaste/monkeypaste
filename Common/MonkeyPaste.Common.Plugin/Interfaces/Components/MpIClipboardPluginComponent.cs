using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIClipboardPluginComponent : MpIPluginComponentBase { }

    public interface MpIClipboardReaderComponent : MpIClipboardPluginComponent {
        MpClipboardReaderResponse ReadClipboardData(MpClipboardReaderRequest request);
    }

    public interface MpIClipboardWriterComponent : MpIClipboardPluginComponent {
        MpClipboardWriterResponse WriteClipboardData(MpClipboardWriterRequest request);
    }

    public interface MpIClipboardReaderComponentAsync : MpIClipboardPluginComponent {
        Task<MpClipboardReaderResponse> ReadClipboardDataAsync(MpClipboardReaderRequest request);
    }

    public interface MpIClipboardWriterComponentAsync : MpIClipboardPluginComponent {
        Task<MpClipboardWriterResponse> WriteClipboardDataAsync(MpClipboardWriterRequest request);
    }
}
