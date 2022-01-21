using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpINativeInterfaceWrapper {
        MpIKeyboardInteractionService GetKeyboardInteractionService();
        MpIGlobalTouch GetGlobalTouch();
        MpIUiLocationFetcher GetLocationFetcher();
        MpIDbInfo GetDbInfo();
        MpIconBuilderBase GetIconBuilder();
        MpIPreferenceIO GetPreferenceIO();
        MpIQueryInfo GetQueryInfo();
    }


    public static  class MpNativeWrapper {
        public static void Init(MpINativeInterfaceWrapper niw) {
            _niw = niw;
        }


        #region Private Variables
        private static MpINativeInterfaceWrapper _niw;
        #endregion

        #region Interface Implementation

        public static MpIKeyboardInteractionService GetKeyboardInteractionService() {
            return _niw.GetKeyboardInteractionService();
        }

        public static MpIGlobalTouch GetGlobalTouch() {
            return _niw.GetGlobalTouch();
        }

        public static MpIUiLocationFetcher GetLocationFetcher() {
            return _niw.GetLocationFetcher();
        }

        public static MpIDbInfo GetDbInfo() {
            return _niw.GetDbInfo();
        }

        public static MpIconBuilderBase GetIconBuilder() {
            return _niw?.GetIconBuilder();
        }

        public static MpIPreferenceIO GetPreferenceIO() {
            return _niw?.GetPreferenceIO();
        }

        public static MpIQueryInfo GetQueryInfo() {
            return _niw?.GetQueryInfo();
        }
        #endregion
    }
}
