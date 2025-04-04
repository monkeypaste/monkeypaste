﻿using System;

namespace MonkeyPaste.Common {

    public interface MpIClipboardMonitor : MpIActionComponent {
        event EventHandler<MpPortableDataObject> OnClipboardChanged;
        MpPortableDataObject LastClipboardDataObject { get; }

        bool IsMonitoring { get; }
        bool IsStartupClipboard { get; }
        void StartMonitor(bool ignoreCurrentState);
        void StopMonitor();
        void ForceChange(MpPortableDataObject mpdo);
    }
}
