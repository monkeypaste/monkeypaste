using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpNativeWrapper : MpSingleton2<MpNativeWrapper>, MpINativeInterfaceWrapper {
        public MpNativeWrapper(MpINativeInterfaceWrapper niw) {
            _niw = niw;
        }

        public void Init() {
            _niw.Init();
        }

        #region Private Variables
        private MpINativeInterfaceWrapper _niw;
        #endregion

        #region Interface Implementation

        public MpIKeyboardInteractionService GetKeyboardInteractionService() {
            return _niw.GetKeyboardInteractionService();
        }

        public MpIGlobalTouch GetGlobalTouch() {
            return _niw.GetGlobalTouch();
        }

        public MpIUiLocationFetcher GetLocationFetcher() {
            return _niw.GetLocationFetcher();
        }

        public MpIDbInfo GetDbInfo() {
            return _niw.GetDbInfo();
        }

        public MpIIconBuilder GetIconBuilder() {
            return _niw?.GetIconBuilder();
        }

        public MpIPreferenceIO GetPreferenceIO() {
            return _niw?.GetPreferenceIO();
        }

        public MpIQueryInfo GetQueryInfo() {
            return _niw?.GetQueryInfo();
        }
        #endregion
    }
}
