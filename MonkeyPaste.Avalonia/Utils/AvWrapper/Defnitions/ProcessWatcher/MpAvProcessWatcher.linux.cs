#if LINUX

using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using X11;


namespace MonkeyPaste.Avalonia {

    public partial class MpAvProcessWatcher {
        #region Private Variables
        private bool _hasError;
        private nint _displayPtr;
        private nint displayPtr {
            get {
                if(_displayPtr == nint.Zero) {
                    _displayPtr = Xlib.XOpenDisplay(null);
                }
                return _displayPtr;
            }
        }
        private Window _rootWindow;
        #endregion
        int HandleError(nint display, ref XErrorEvent error) {
            MpConsole.WriteLine($"Xll error: '{error}'");
            _hasError = true;
            return 1;
        }
        event XErrorHandlerDelegate HandleErrorDelegate;
        protected void Init() {
            return;
            HandleErrorDelegate += HandleError;
            Xlib.XSetErrorHandler(HandleErrorDelegate);

            if (displayPtr == nint.Zero) {
                MpConsole.WriteLine("Unable to open the default X display");
                return;
            }

            _rootWindow = Xlib.XDefaultRootWindow(displayPtr);

            if (_rootWindow == default) {
                MpConsole.WriteLine("Unable to open root window");
                return;
            }
        }
        public IEnumerable<MpPortableProcessInfo> AllWindowProcessInfos =>
            new List<MpPortableProcessInfo>();
        public nint SetActiveProcess(MpPortableProcessInfo p) {
            return nint.Zero;
        }
        protected nint GetThisAppHandle() {
            if (MpAvWindowManager.MainWindow is not { } mw ||
                mw.TryGetPlatformHandle() is not IPlatformHandle ph) {
                return nint.Zero;
            }
            return ph.Handle;
        }
        protected nint GetActiveProcessHandle() {
            return nint.Zero;
            nint handle = GetFocusWindowHandle();
            nint test = GetTopWindowHandle(handle);
            return handle;
        }

        private nint GetFocusWindowHandle() {
            // from https://gist.github.com/kui/2622504
            Window w = default;
            RevertFocus revert_to = default;
            Status result = Xlib.XGetInputFocus(displayPtr, ref w, ref revert_to);
            nint handle = (nint)((int)w);

            if(handle != nint.Zero) {
                string prop_return = default;

                var a = Xlib.XFetchName(displayPtr, w, ref prop_return);
                //nint actual_type_return = default;
                //int actual_format_return = default; 
                //ulong nitems_return = default;
                //ulong bytes_after_return = default;
                //string prop_return = default;
                //int a =
                //XGetWindowProperty(
                //    displayPtr,
                //    w,
                //    Atom.WmName,
                //    0L,
                //    (~0L),
                //    false,
                //    Atom.None,
                //    ref actual_type_return,
                //    ref actual_format_return,
                //    ref nitems_return,
                //    ref bytes_after_return,
                //    ref prop_return);
            }

            return handle;
        }
        private nint GetTopWindowHandle(nint start) {
            // from https://gist.github.com/kui/2622504
            Window w = (Window)((int)start);
            Window parent = (Window)((int)start);
            Window root = Window.None;

            while(parent != root) {
                w = parent;
                int result = Xlib.XQueryTree(displayPtr, w, ref root, ref parent, out _);
                if(result < 0 || _hasError) {
                    MpConsole.WriteLine($"Find top window failed");
                    break;
                }
            }
            //MpConsole.WriteLine($"Found top window: {w}");
            return (nint)((int)w);
        }
        protected bool IsHandleWindowProcess(nint handle) {
            return handle != nint.Zero;
        }
        protected string GetProcessPath(nint handle) {
            return string.Empty;
        }
        protected string GetProcessTitle(nint handle) {
            // from https://gist.github.com/kui/2622504
            //Xlib.XGet
            return "<process title here>";
        }
        protected string GetAppNameByProessPath(string path) {
            return string.Empty;
        }
        protected nint GetParentHandleAtPoint(MpPoint p) {
            return nint.Zero;
        }

        public Dictionary<string, List<string>> GetRunningApps() {
            nint ev = Marshal.AllocHGlobal(24 * sizeof(long));
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

            var windows = new Dictionary<string, nint>();
            return null;
        }

        [DllImport("libX11.so.6")]
        private static extern X11.Atom XInternAtom(nint display, string name, bool only_if_exists);

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
            nint display,
            Window window,
            Atom atom,
            long long_offset,
            long long_length,
            bool delete,
            Atom req_type,
            ref nint actual_type_return, //atom
            ref int actual_format_return, //int
            ref ulong nitems_return, //ulong
            ref ulong bytes_after_return, //ulong
            ref string prop_return);

    }


}
#endif