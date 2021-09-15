using System;

namespace MonkeyPaste.UWP {
    public class MpGlobalTouch : MpIGlobalTouch {
        public MpGlobalTouch() { }

        public void Subscribe(EventHandler handler) {
           // MainActivity.Current.GlobalTouchHandler += handler;
        }

        public void Unsubscribe(EventHandler handler) {
            //MainActivity.Current.GlobalTouchHandler -= handler;
        }
    }
}