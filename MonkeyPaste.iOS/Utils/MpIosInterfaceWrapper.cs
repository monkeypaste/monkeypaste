using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace MonkeyPaste.iOS {
    public class MpIosInterfaceWrapper : MpINativeInterfaceWrapper {
        public MpKeyboardInteractionService KeyboardService { private get; set; }
        //public MpLocalStorage StorageService { private get; set; }

        public MpIKeyboardInteractionService GetKeyboardInteractionService() {
            return KeyboardService;
        }

        public MpILocalStorage GetLocalStorageManager() {
            return null;
        }

        public MpIPhotoGalleryManager GetPhotoGalleryManager() {
            throw new NotImplementedException();
        }
    }
}