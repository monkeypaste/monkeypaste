using System;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIShortcutManager {
        IDisposable RegisterShortcut(MpRoutingType routingType, string keyStr, ICommand shortcutCommand, object shortcutCommandParameter);
        void UnregisterShortcut(IDisposable subscriptionDisposable);
    }

}
