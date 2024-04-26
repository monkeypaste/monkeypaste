#if LINUX

using Avalonia.Platform;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using X11;


namespace MonkeyPaste.Avalonia {

    public partial class MpAvProcessWatcher {
        #region Private Variables
        bool use_shell = true;
        bool force_disabled = false;
        bool IS_DISABLED {
            get {
                if(force_disabled ||
                    Mp.Services == null ||
                    Mp.Services.StartupState == null ||
                    !Mp.Services.StartupState.IsReady) {
                    return true;
                }
                return false;
            }
        }
        private bool _hasError;
        
        private nint _xdoCtx;
        private nint xdoCtx {
            get {
                if(_xdoCtx == 0) {
                    _xdoCtx = XdoLib.xdo_new(null);
                }
                return _xdoCtx;
            }
        }

        private nint _displayPtr;
        public nint displayPtr {
            get {
                if(_displayPtr == 0) {
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
            return 0;
        }
        event XErrorHandlerDelegate HandleErrorDelegate;
        protected void Init() {
            HandleErrorDelegate += HandleError;
            try {
                _ = xdoCtx;
                Xlib.XSetErrorHandler(HandleErrorDelegate);

                if (displayPtr == 0) {
                    MpConsole.WriteLine("Unable to open the default X display");
                    return;
                }

                _rootWindow = Xlib.XDefaultRootWindow(displayPtr);

                if (_rootWindow == default) {
                    MpConsole.WriteLine("Unable to open root window");
                    return;
                }
            }
            catch(Exception ex) { MpConsole.WriteTraceLine($"proc err.",ex); }
        }
        public IEnumerable<MpPortableProcessInfo> AllWindowProcessInfos =>
            new List<MpPortableProcessInfo>();
        public nint SetActiveProcess(MpPortableProcessInfo p) {
            if(IS_DISABLED) {
                return 0;
            }
            try {
                if (p == null || p.Handle == 0) {
                    return 0;
                }
                if(use_shell) {
                    $"xdotool windowactivate {(int)p.Handle}".ShellExec().Trim();
                    $"xdotool windowfocus {(int)p.Handle}".ShellExec().Trim();
                } else {
                    XdoLib.xdo_activate_window(xdoCtx, (int)p.Handle);
                    XdoLib.xdo_focus_window(xdoCtx, (int)p.Handle);
                }
                
                
                return p.Handle;
            }
            catch(Exception ex) { MpConsole.WriteTraceLine($"proc err.",ex); }
            return 0;
        }
        protected nint GetThisAppHandle() {
            if(_thisAppHandle > 0) {
                return _thisAppHandle;
            }
            if (MpAvWindowManager.MainWindow is not { } mw ||
                mw.TryGetPlatformHandle() is not IPlatformHandle ph) {
                return 0;
            }
            if(MpAvWindowManager.MainWindow is MpAvMainWindow) {
                // make sure to use the actual mw handle
                _thisAppHandle = ph.Handle;
            }
            return ph.Handle;
        }
        protected nint GetActiveProcessHandle() {
            if (IS_DISABLED) { 
                return 0;
            }
            //if (MpAvWindowManager.IsAnyActive) {
            //    // BUG i maybe wrong but i think avalonia is one of the
            //    // kinds of windows that doesn't report a pid from its handle
            //    // when thisApp is active i get this error:
            //    // XGetWindowProperty[_NET_ACTIVE_WINDOW] failed (code=1)
            //    return ThisAppHandle;
            //}
            //nint handle = GetFocusWindowHandle();
            //nint test = GetTopWindowHandle(handle);
            //return handle;
            int handle = default;
            _hasError = false;
            
            try {
                if(use_shell) {
                    string result = $"xdotool getwindowfocus".ShellExec().Trim();
                    if(int.TryParse(result,out int intResult)) {
                        handle = intResult;
                    }
                } else {
                    XdoLib.xdo_get_focused_window_sane(xdoCtx, ref handle);
                }
                
                if (_hasError) {
                    // only w/ non shell
                    MpConsole.WriteLine($"Sane failed for handle '{handle}', attempting fallback");
                    // handle becomes 1 when there's an error and will screw other things up
                    _hasError = false;
                    XdoLib.xdo_get_focused_window(xdoCtx, ref handle);
                    if (_hasError) {
                        MpConsole.WriteLine($"backup failed for handle '{handle}', attempting fallback");
                        _hasError = false;
                    } else if (handle == 1) {
                        handle = default;
                    }
                }
                return (nint)handle;
            } catch(Exception ex) {
                MpConsole.WriteTraceLine($"Error getting active window.", ex);
            }
            return 0;
        }

        private nint GetFocusWindowHandle() {
            if (IS_DISABLED) {
                return 0;
            }
            try {
                // from https://gist.github.com/kui/2622504
                Window w = default;
                RevertFocus revert_to = default;
                Status result = Xlib.XGetInputFocus(displayPtr, ref w, ref revert_to);
                nint handle = (nint)((int)w);

                if (handle != 0) {
                    //string prop_return = default;

                    //var a = Xlib.XFetchName(displayPtr, w, ref prop_return);
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
            catch(Exception ex) { MpConsole.WriteTraceLine($"proc err.",ex); }
            return 0;
        }
        public nint GetTopWindowHandle(nint start) {
            if (IS_DISABLED) {
                return 0;
            }
            if (start == 0) {
                return 0;
            }
            // from https://gist.github.com/kui/2622504
            Window w = (Window)((int)start);
            Window parent = (Window)((int)start);
            Window root = Window.None;

            while (parent != root) {
                w = parent;
                int result = -1;
                try {
                    result = Xlib.XQueryTree(displayPtr, w, ref root, ref parent, out _);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"proc err.", ex);
                    return 0;
                }
                if (result < 0 || _hasError) {
                    MpConsole.WriteLine($"Find top window failed");
                    break;
                }
            }
            //MpConsole.WriteLine($"Found top window: {w}");
            return (nint)((int)w);
        }
        protected bool IsHandleWindowProcess(nint handle) {
            if (IS_DISABLED) {
                return false;
            }
            if (handle == 0) {
                return false;
            }
            if(handle == ThisAppHandle) {
                return true;
            }
            try {
                if(use_shell) {
                    return GetWindowPid(handle) > 0;
                } else {
                    return XdoLib.xdo_get_pid_window(xdoCtx, (int)handle) > 0;
                }
            } catch(Exception ex) {
                MpConsole.WriteTraceLine($"Error checking if handle '{handle}' is window process.", ex);
            }
            return false;
        }
        protected string GetProcessPath(nint handle) {
            if (IS_DISABLED) {
                return string.Empty;
            }
            if (handle == ThisAppHandle) {
                return Dispatcher.UIThread.Invoke(() => {
                    return Mp.Services.PlatformInfo.ExecutingPath;
                });
            }
            try {
                if (handle == 0) {
                    return string.Empty;
                }
                int pid = 0;
                if(use_shell) {
                    pid = GetWindowPid(handle);
                } else {
                    pid = XdoLib.xdo_get_pid_window(xdoCtx, (int)handle);
                }
                if (pid == 0) {
                    //var top_handle = GetTopWindowHandle(handle);
                    //if(top_handle != handle && top_handle != 0) {
                    //    return GetProcessPath(top_handle);
                    //}
                    return string.Empty;
                }
                var path_bytes = new byte[256];
                X11Tools.get_exe_for_pid(pid, path_bytes);
                if(path_bytes == null) {
                    return string.Empty;
                }
                string path = Encoding.Default.GetString(path_bytes).Replace("\0", string.Empty);
                // TODO need to test/finish parsing path/args here
                //var exe = path.ParseCmdPath();
                return path;
            } catch(Exception ex) { MpConsole.WriteTraceLine($"proc err.",ex); }
            return string.Empty;
        }
        private int GetWindowPid(nint handle) {
            if(handle == 0) {
                return 0;
            }
            string result = $"xdotool getwindowpid {(int)handle}".ShellExec().Trim();
            if (!result.StartsWith("X Error") &&
                int.TryParse(result, out int intResult)) {
                return intResult;
            }
            return 0;
        }
        protected string GetProcessTitle(nint handle) {
            if (IS_DISABLED) {
                return string.Empty;
            }
            if(handle == ThisAppHandle) {
                return Dispatcher.UIThread.Invoke(() => {
                    return MpAvWindowManager.MainWindow.Title;
                }); 
            }
            try {
                if (handle == 0 || handle == 1) {
                    // NOTE handle==1 throws exception getting title
                    return string.Empty;
                }
                string title = default;
                if (use_shell) {
                    title = $"xdotool getwindowname {(int)handle}".ShellExec();
                } else {
                // from https://gist.github.com/kui/2622504
                    int len = default;
                    int name_type = default;
                    XdoLib.xdo_get_window_name(xdoCtx, (int)handle, ref title, ref len, ref name_type);
                }
                //if(title.IsNullOrEmpty()) {
                //    var top_handle = GetTopWindowHandle(handle);
                //    if (top_handle != handle && top_handle != 0) {
                //        return GetProcessTitle(top_handle);
                //    }
                //}
                return title;
            }
            catch(Exception ex) { MpConsole.WriteTraceLine($"proc err.",ex); }
            return string.Empty;
        }
        protected string GetAppNameByProessPath(string path) {
            if(MpX11ShellHelpers.GetLauncherProperty(path,"Name") is string appName &&
                !appName.IsNullOrEmpty()) {
                return appName;
            }
            if(!path.IsNullOrEmpty()) {
                try {
                    return Path.GetFileNameWithoutExtension(path);
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine($"Error reading app file name from '{path.ToStringOrEmpty()}'.", ex);
                }
            }
            return string.Empty;
        }
        protected nint GetParentHandleAtPoint(MpPoint p) {
            return 0;
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