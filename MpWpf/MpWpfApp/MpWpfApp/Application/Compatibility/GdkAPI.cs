using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//using Gdk;

namespace MpWpfApp {
    public static class GdkApi {       

        [DllImport("libgtk-x11-2.0")]
        public static extern IntPtr gdk_x11_drawable_get_xid(IntPtr gdkWindow);

        [DllImport("libgtk-x11-2.0")]
        public static extern IntPtr gdk_x11_drawable_get_xdisplay(IntPtr gdkDrawable);

        [DllImport("libgtk-x11-2.0")]
        public static extern IntPtr gdk_x11_window_get_drawable_impl(IntPtr gdkWindow);

        [DllImport("libX11")]
        public static extern int XKeysymToKeycode(IntPtr display,int key);

        [DllImport("libX11")]
        public static extern int XGrabKey(
            IntPtr display,
            int keycode,
            uint modifiers,
            IntPtr grab_window,
            bool owner_events,
            int pointer_mode,
            int keyboard_mode);

        [DllImport("libX11")]
        public static extern int XUngrabKey(
            IntPtr display,
            int keycode,
            uint modifiers,
            IntPtr grab_window);
    }
}
