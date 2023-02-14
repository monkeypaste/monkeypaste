using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIClipboardPluginComponent : MpIPluginComponentBase { }


    public interface MpIClipboardReaderComponent : MpIClipboardPluginComponent {
        Task<MpClipboardReaderResponse> ReadClipboardDataAsync(MpClipboardReaderRequest request);
    }

    public interface MpIClipboardWriterComponent : MpIClipboardPluginComponent {
        Task<MpClipboardWriterResponse> WriteClipboardDataAsync(MpClipboardWriterRequest request);
    }
}
