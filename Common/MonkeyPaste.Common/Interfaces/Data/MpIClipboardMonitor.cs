using System;

namespace MonkeyPaste.Common {

    public interface MpIClipboardMonitor : MpIActionComponent {
        event EventHandler<MpPortableDataObject> OnClipboardChanged;
        MpPortableDataObject LastClipboardDataObject { get; }

        void StartMonitor();
        void StopMonitor();
    }
}
