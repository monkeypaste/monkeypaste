using System;

namespace MonkeyPaste.Common {

    public interface MpIClipboardMonitor : MpIActionComponent {
        event EventHandler<MpPortableDataObject> OnClipboardChanged;
        MpPortableDataObject LastClipboardDataObject { get; }

        bool IsMonitoring { get; }
        void StartMonitor(bool ignoreCurrentState);
        void StopMonitor();
    }
}
