using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIActionComponent {
        void Register(MpIActionComponentHandler mvm);
        void Unregister(MpIActionComponentHandler mvm);
    }

    public interface MpIActionComponentHandler {
        void OnActionTriggered(object sender, object args);
        string Label { get; }
    }
}
