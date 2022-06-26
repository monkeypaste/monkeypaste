using System.Collections.Generic;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste {
    public interface MpIClipboardFormatDataHandlers {
        IEnumerable<MpIClipboardPluginComponent> Handlers { get; }
    }
}
