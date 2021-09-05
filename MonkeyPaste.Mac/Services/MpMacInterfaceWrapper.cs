using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppKit;
using FFImageLoading.Forms.Platform;
using Foundation;
using Xamarin.Forms;
using MonkeyPaste;

namespace MonkeyPaste.Mac {
    public class MpMacInterfaceWrapper : MpINativeInterfaceWrapper {
        //private Dictionary<Type, object> _services { get; set; } = new Dictionary<Type, object>();


        //public void Register<T>(object so) where T: class { {
        //    var so = Activator.CreateInstance(typeof(T));
        //    _services.Add(so.GetType(), so);
        //}

        //public T Get<T>() where T : class {
        //    return _services.Where(x => x.GetType() == typeof(T)).FirstOrDefault() as T;
        //}
        //public MpKeyboardInteractionService KeyboardService { private get; set; }
        //public MpLocalStorage_Android StorageService { private get; set; }
        public MpGlobalTouch TouchService { private get; set; }
        public MpUiLocationFetcher UiLocationFetcher { private get; set; }
        //public MpScreenshot Screenshot { private get; set; }
        public MpDbFilePath_Mac DbInfo { private get; set; }

        public MpIDbInfo GetDbInfo() {
            return DbInfo;
        }

        public MpIGlobalTouch GetGlobalTouch() {
            return TouchService;
        }

        public MpIKeyboardInteractionService GetKeyboardInteractionService() {
            return null;
        }

        public MpILocalStorage GetLocalStorageManager() {
            return null;
        }

        public MpIUiLocationFetcher GetLocationFetcher() {
            return UiLocationFetcher;
        }

        public MpIPhotoGalleryManager GetPhotoGalleryManager() {
            throw new NotImplementedException();
        }

        public MpIScreenshot GetScreenshot() {
            return null;
        }
    }
}