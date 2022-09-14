using System;

namespace MonkeyPaste.Common {
    public interface MpIClipboardMonitor {
        event EventHandler<MpPortableDataObject> OnClipboardChanged;

        bool IgnoreNextClipboardChangeEvent { get; set; }

        void StartMonitor();
        void StopMonitor();
    }
}
