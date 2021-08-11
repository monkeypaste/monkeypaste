using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpINativeInterfaceWrapper {
        MpIPhotoGalleryManager GetPhotoGalleryManager();
        MpIKeyboardInteractionService GetKeyboardInteractionService();
        MpILocalStorage GetLocalStorageManager();
        MpIGlobalTouch GetGlobalTouch();
        MpIUiLocationFetcher GetLocationFetcher();
        MpIScreenshot GetScreenshot();

        //void Register<T>(object so) where T : class;

        //T Get<T>() where T : class;
    }
}
