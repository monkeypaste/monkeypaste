using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common {
    public interface MpIActionComponent {
        void RegisterActionComponent(MpIInvokableAction mvm);
        void UnregisterActionComponent(MpIInvokableAction mvm);
    }

    public interface MpIInvokableAction {
        void OnActionInvoked(object sender, object args);
        string Label { get; }
    }
}
