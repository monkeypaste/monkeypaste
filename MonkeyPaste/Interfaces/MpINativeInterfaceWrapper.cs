using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpINativeInterfaceWrapper {
        MpIPhotoGalleryManager GetPhotoGalleryManager();
        MpIKeyboardInteractionService GetKeyboardInteractionService();
        MpILocalStorage GetLocalStorageManager();
    }
}
