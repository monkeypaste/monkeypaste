using System;

namespace MonkeyPaste.Common {
    public interface MpIClipboardMonitor : MpIActionComponent {
        event EventHandler<MpPortableDataObject> OnClipboardChanged;

        void StartMonitor();
        void StopMonitor();
    }
}
