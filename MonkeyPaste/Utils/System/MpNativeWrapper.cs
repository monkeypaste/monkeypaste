using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpNativeWrapper : MpINativeInterfaceWrapper {
        #region Singleton Definition
        private static readonly Lazy<MpNativeWrapper> _Lazy = new Lazy<MpNativeWrapper>(() => new MpNativeWrapper());
        public static MpNativeWrapper Instance { get { return _Lazy.Value; } }

        public void Init(MpINativeInterfaceWrapper niw) {
            _niw = niw;
        }
        #endregion

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
        #endregion
    }
}
