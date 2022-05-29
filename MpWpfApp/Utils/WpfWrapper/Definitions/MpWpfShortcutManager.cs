using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpWpfShortcutManager : MpIShortcutManager {
        public IDisposable RegisterShortcut(MpRoutingType routingType, string keyStr, ICommand shortcutCommand, object shortcutCommandParameter) {
            throw new NotImplementedException();
        }

        public void UnregisterShortcut(IDisposable subscriptionDisposable) {
            throw new NotImplementedException();
        }
    }
}
