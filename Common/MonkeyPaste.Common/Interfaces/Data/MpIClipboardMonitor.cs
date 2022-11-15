using System;

namespace MonkeyPaste.Common {
    public interface MpIClipboardMonitor {
        event EventHandler<MpPortableDataObject> OnClipboardChanged;

        void StartMonitor();
        void StopMonitor();
    }
}
