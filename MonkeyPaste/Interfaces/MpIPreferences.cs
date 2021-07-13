using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIPreferences {
        object GetPreferenceValue(string preferenceName);
        void SetPreferenceValue(string preferenceName, object preferenceValue);
    }
}
