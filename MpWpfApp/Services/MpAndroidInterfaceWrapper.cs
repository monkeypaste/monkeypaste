using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpWpfInterfaceWrapper : MpINativeInterfaceWrapper {
        //public MpKeyboardInteractionService KeyboardService { private get; set; }
        //public MpLocalStorage_Android StorageService { private get; set; }
        //public MpGlobalTouch TouchService { private get; set; }
        //public MpUiLocationFetcher UiLocationFetcher { private get; set; }
        //public MpScreenshot Screenshot { private get; set; }
        //public MpDbFilePath_Android DbInfo { private get; set; }
        public MpIconBuilder IconBuilder { private get; set; }

        public MpIDbInfo GetDbInfo() {
            throw new NotImplementedException();
        }

        public MpIGlobalTouch GetGlobalTouch() {
            throw new NotImplementedException();
        }

        public MpIIconBuilder GetIconBuilder() {
            return IconBuilder;
        }

        public MpIKeyboardInteractionService GetKeyboardInteractionService() {
            throw new NotImplementedException();
        }

        public MpILocalStorage GetLocalStorageManager() {
            throw new NotImplementedException();
        }

        public MpIUiLocationFetcher GetLocationFetcher() {
            throw new NotImplementedException();
        }

        public MpIPhotoGalleryManager GetPhotoGalleryManager() {
            throw new NotImplementedException();
        }

        public MpIScreenshot GetScreenshot() {
            throw new NotImplementedException();
        }
    }
}