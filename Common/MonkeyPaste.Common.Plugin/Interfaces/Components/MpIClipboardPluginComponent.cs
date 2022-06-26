using MonkeyPaste.Common;
using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIClipboardPluginComponent : MpIPluginComponentBase { }

    public interface MpIClipboardReaderComponent : MpIClipboardPluginComponent {
        MpClipboardReaderResponse ReadClipboardData(MpClipboardReaderRequest request);
    }

    public interface MpIClipboardWriterComponent : MpIClipboardPluginComponent {
        MpClipboardWriterResponse WriteClipboardData(MpClipboardWriterRequest request);
    }
}
