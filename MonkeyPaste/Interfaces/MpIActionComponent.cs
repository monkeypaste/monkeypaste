using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIActionComponent {
        void RegisterActionComponent(MpIActionComponentHandler mvm);
        void UnregisterActionComponent(MpIActionComponentHandler mvm);
    }

    public interface MpIActionComponentHandler {
        void OnActionTriggered(object sender, object args);
        string Label { get; }
    }
}
