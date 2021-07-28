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
using Xamarin.Forms;

namespace MonkeyPaste.Droid {
    public class MpGlobalTouch : MpIGlobalTouch {
        public MpGlobalTouch() { }

        public void Subscribe(EventHandler handler) {
            MainActivity.Current.GlobalTouchHandler += handler;
        }

        public void Unsubscribe(EventHandler handler) {
            MainActivity.Current.GlobalTouchHandler -= handler;
        }
    }
}