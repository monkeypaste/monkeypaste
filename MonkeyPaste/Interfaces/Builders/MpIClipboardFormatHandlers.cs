using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;

namespace MonkeyPaste {
    public interface MpIClipboardFormatDataHandlers {
        IEnumerable<MpIClipboardPluginComponent> Handlers { get; }
    }
}
