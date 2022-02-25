using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpINativeInterfaceWrapper {
        MpICursor Cursor { get; }
        MpIDbInfo DbInfo { get; }
        MpIPreferenceIO PreferenceIO { get; }
        MpIQueryInfo QueryInfo { get; }
        MpIconBuilderBase IconBuilder { get; }
        MpICustomColorChooserMenu CustomColorChooserMenu { get; }
        MpIKeyboardInteractionService KeyboardInteractionService { get; }
        MpIGlobalTouch GlobalTouch { get; }
        MpIUiLocationFetcher LocationFetcher { get; }
    }


    public static class MpNativeWrapper {

        #region Private Variables
        public static MpINativeInterfaceWrapper Services { get; private set; }
        #endregion

        public static void Init(MpINativeInterfaceWrapper niw) {
            Services = niw;
        }


    }
}
