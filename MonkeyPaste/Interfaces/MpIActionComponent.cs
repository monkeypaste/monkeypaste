using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIActionComponent {
        void RegisterActionComponent(MpIActionTrigger mvm);
        void UnregisterActionComponent(MpIActionTrigger mvm);
    }

    public interface MpIActionTrigger {
        void OnActionTriggered(object sender, object args);
        string Label { get; }
    }
}
