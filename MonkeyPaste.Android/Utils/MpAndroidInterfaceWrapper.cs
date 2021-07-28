using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Droid {
    public class MpAndroidInterfaceWrapper : MpINativeInterfaceWrapper {
        public MpKeyboardInteractionService KeyboardService { private get; set; }
        public MpLocalStorage_Android StorageService { private get; set; }
        public MpGlobalTouch TouchService { private get; set; }
        public MpUiLocationFetcher UiLocationFetcher { private get; set; }

        public MpIGlobalTouch GetGlobalTouch() {
            return TouchService;
        }

        public MpIKeyboardInteractionService GetKeyboardInteractionService() {
            return KeyboardService;
        }

        public MpILocalStorage GetLocalStorageManager() {
            return StorageService;
        }

        public MpIUiLocationFetcher GetLocationFetcher() {
            return UiLocationFetcher;
        }

        public MpIPhotoGalleryManager GetPhotoGalleryManager() {
            throw new NotImplementedException();
        }
    }
}