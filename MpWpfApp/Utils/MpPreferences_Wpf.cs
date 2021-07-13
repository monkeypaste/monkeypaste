using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpPrefernces_Wpf : MonkeyPaste.MpIPreferences { 

        public object GetPreferenceValue(string preferenceName) {
            var prefs = Properties.Settings.Default;
            return prefs.GetType().GetProperty(preferenceName).GetValue(this);
        }

        public void SetPreferenceValue(string preferenceName, object preferenceValue) {
            var prefs = Properties.Settings.Default;
            prefs.GetType().GetProperty(preferenceName).SetValue(this, preferenceValue);
        }
    }
}
