using System;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;
//using Gio;
//using Gtk;
//using GLib;
//using Gdk;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
//using Avalonia.Gtk3;
using X11;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Avalonia {
    
    public static class MpAvX11ProcessWatcher_xlib  {
        #region Private Variables

        private static IntPtr _displayPtr;
        private static Window _rootWindow;

        #endregion

        public static IDictionary<string, IntPtr> GetOpenWindows() { 
            var displayNamePtr = Xlib.XDisplayName(null);
            var displayName = Marshal.PtrToStringAnsi(displayNamePtr);
            if (displayName == String.Empty) {
                MpConsole.WriteTraceLine("No display configured for X11; check the value of the DISPLAY variable is set correctly");
                return null;
            }

            MpConsole.WriteLine($"Connecting to X11 Display {displayName}");
            _displayPtr = Xlib.XOpenDisplay(null);

            if (_displayPtr == IntPtr.Zero) {
                MpConsole.WriteTraceLine("Unable to open the default X display");
                return null;
            }

            _rootWindow = Xlib.XDefaultRootWindow(_displayPtr);
            
            if (_rootWindow == default) {
                MpConsole.WriteTraceLine("Unable to open root window");
                return null;
            }

            IntPtr ev = Marshal.AllocHGlobal(24 * sizeof(long));
            Window ReturnedParent = 0, ReturnedRoot = 0;
            Xlib.XGrabServer(_displayPtr);

            var r = Xlib.XQueryTree(
                _displayPtr, 
                _rootWindow, ref ReturnedRoot, ref ReturnedParent,
                out var ChildWindows);
            
            foreach(var child in ChildWindows) {
                var Name = String.Empty;
                Xlib.XFetchName(_displayPtr, child, ref Name);
                MpConsole.WriteLine("Child Window Name: " + Name);
            }
            Xlib.XUngrabServer(_displayPtr); // Release the lock on the server.

            var windows = new Dictionary<string, IntPtr>();
            return null;
        }
        
    }
}
