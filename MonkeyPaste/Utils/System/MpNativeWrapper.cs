using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpNativeWrapper : MpISingleton<MpNativeWrapper>, MpINativeInterfaceWrapper {

        private static MpNativeWrapper _instance;
        public static MpNativeWrapper Instance => _instance ?? (_instance = new MpNativeWrapper());

        public MpNativeWrapper() {
            throw new Exception("Must be init'd with args");
        }

        public async Task Init() {
            await _niw.Init();
        }

        public MpNativeWrapper(MpINativeInterfaceWrapper niw) {
            _niw = niw;
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

        public MpIconBuilder GetIconBuilder() {
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
