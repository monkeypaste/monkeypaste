#if LINUX

using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using X11;

namespace MonkeyPaste.Avalonia {

    public static class MpAvX11ProcessWatcher_xlib {
        #region Private Variables

        private static IntPtr _displayPtr;
        private static Window _rootWindow;

        #endregion

        public static void Init() {
            _displayPtr = Xlib.XOpenDisplay(null);

            if (_displayPtr == IntPtr.Zero) {
                MpConsole.WriteTraceLine("Unable to open the default X display");
                return;
            }

            _rootWindow = Xlib.XDefaultRootWindow(_displayPtr);

            if (_rootWindow == default) {
                MpConsole.WriteTraceLine("Unable to open root window");
                return;
            }

        }

        public static bool IsXDisplayAvailable() {
            return _rootWindow != default;
        }

        public static Dictionary<string, List<string>> GetRunningApps() {
            IntPtr ev = Marshal.AllocHGlobal(24 * sizeof(long));
            Window ReturnedParent = 0, ReturnedRoot = 0;
            Xlib.XGrabServer(_displayPtr);

            int status = Xlib.XQueryTree(
                _displayPtr,
                _rootWindow, ref ReturnedRoot, ref ReturnedParent,
                out var ChildWindows);

            if (status == 0) {
                // could not query tree
                return null;
            }
            foreach (var child in ChildWindows) {
                // var a = XInternAtom(_displayPtr, "_NET_CLIENT_LIST", true);
                // Atom actualType;
                // int format;
                // ulong numItems, bytesAfter;
                // unsigned char *data =0;
                // int status = XGetWindowProperty(m_pDisplay,
                //                             rootWindow,
                //                             a,
                //                             0L,
                //                             (~0L),
                //                             false,
                //                             AnyPropertyType,
                //                             &actualType,
                //                             &format,
                //                             &numItems,
                //                             &bytesAfter,
                //                             &data);

                var window_name = String.Empty;
                Xlib.XFetchName(_displayPtr, child, ref window_name);
                MpConsole.WriteLine("Child Window Name: " + window_name);
            }
            Xlib.XUngrabServer(_displayPtr); // Release the lock on the server.

            var windows = new Dictionary<string, IntPtr>();
            return null;
        }

        [DllImport("libX11.so.6")]
        private static extern X11.Atom XInternAtom(IntPtr display, string name, bool only_if_exists);

        /*
        int XGetWindowProperty(display, w, property, long_offset, long_length, delete, req_type, 
                        actual_type_return, actual_format_return, nitems_return, bytes_after_return, 
                        prop_return)
      Display *display;
      Window w;
      Atom property;
      long long_offset, long_length;
      Bool delete;
      Atom req_type; 
      Atom *actual_type_return;
      int *actual_format_return;
      unsigned long *nitems_return;
      unsigned long *bytes_after_return;
      unsigned char **prop_return;
      */
        [DllImport("libX11.so.6")]
        private static extern int XGetWindowProperty(
            IntPtr display,
            Window window,
            Atom atom,
            long long_offset,
            long long_length,
            bool delete,
            Atom req_type,
            IntPtr actual_type_return, //atom
            IntPtr actual_format_return, //int
            IntPtr nitems_return, //ulong
            IntPtr bytes_after_return, //ulong
            byte prop_return);

    }


}
#endif