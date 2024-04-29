//using Avalonia.Gtk3;
//using Gio;
//
//using GLib;
//using Gdk;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Avalonia {
    public static class X11Tools {
        const string PidName = "x11tools.so";
        [DllImport(PidName)]
        public static extern int get_exe_for_pid(int pid, [Out] byte[] exe_path_return);

        [DllImport(PidName)]
        public static extern int get_clipboard_owner(); 
    }
}
