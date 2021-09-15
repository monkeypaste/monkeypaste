using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpINativeInterfaceWrapper {
        MpIKeyboardInteractionService GetKeyboardInteractionService();
        MpIGlobalTouch GetGlobalTouch();
        MpIUiLocationFetcher GetLocationFetcher();
        MpIDbInfo GetDbInfo();
        MpIIconBuilder GetIconBuilder();
    }
}
