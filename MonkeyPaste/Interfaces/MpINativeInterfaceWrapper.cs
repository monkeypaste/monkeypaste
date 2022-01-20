using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpINativeInterfaceWrapper {
        Task Init();

        MpIKeyboardInteractionService GetKeyboardInteractionService();
        MpIGlobalTouch GetGlobalTouch();
        MpIUiLocationFetcher GetLocationFetcher();
        MpIDbInfo GetDbInfo();
        MpIconBuilder GetIconBuilder();
        MpIPreferenceIO GetPreferenceIO();
        MpIQueryInfo GetQueryInfo();
    }
}
