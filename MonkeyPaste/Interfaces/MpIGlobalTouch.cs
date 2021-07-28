using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIGlobalTouch {
        void Subscribe(EventHandler handler);
        void Unsubscribe(EventHandler handler);
    }
}
