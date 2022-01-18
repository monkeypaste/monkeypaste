using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpINativeInterfaceWrapper {
        void Init();

        MpIKeyboardInteractionService GetKeyboardInteractionService();
        MpIGlobalTouch GetGlobalTouch();
        MpIUiLocationFetcher GetLocationFetcher();
        MpIDbInfo GetDbInfo();
        MpIIconBuilder GetIconBuilder();
        MpIPreferenceIO GetPreferenceIO();
        MpIQueryInfo GetQueryInfo();
    }
}
