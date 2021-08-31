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
        //private Dictionary<Type, object> _services { get; set; } = new Dictionary<Type, object>();


        //public void Register<T>(object so) where T: class { {
        //    var so = Activator.CreateInstance(typeof(T));
        //    _services.Add(so.GetType(), so);
        //}

        //public T Get<T>() where T : class {
        //    return _services.Where(x => x.GetType() == typeof(T)).FirstOrDefault() as T;
        //}
        public MpKeyboardInteractionService KeyboardService { private get; set; }
        public MpLocalStorage_Android StorageService { private get; set; }
        public MpGlobalTouch TouchService { private get; set; }
        public MpUiLocationFetcher UiLocationFetcher { private get; set; }
        public MpScreenshot Screenshot { private get; set; }
        public MpDbFilePath_Android DbInfo { private get; set; }

        public MpIDbInfo GetDbInfo() {
            return DbInfo;
        }

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

        public MpIScreenshot GetScreenshot() {
            return Screenshot;
        }
    }
}