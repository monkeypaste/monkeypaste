//using Avalonia.Gtk3;
//using Gio;
//
//using GLib;
//using Gdk;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Avalonia {
    public static class XdoLib {
        const string XdoName = "libxdo.so";
        [DllImport(XdoName)]
        public static extern nint xdo_new(string displayName);

        [DllImport(XdoName)]
        public static extern int xdo_get_pid_window(nint xdo, int window);

        [DllImport(XdoName)]
        public static extern int xdo_get_active_window(nint xdo, ref int window);

        [DllImport(XdoName)]
        public static extern int xdo_activate_window(nint xdo, int window);

        [DllImport(XdoName)]
        public static extern int xdo_focus_window(nint xdo, int window);

        [DllImport(XdoName)]
        public static extern int xdo_get_window_name(nint xdo, int window,
                        ref string name_ret, ref int name_len_ret,
                        ref int name_type);

        [DllImport(XdoName)]
        public static extern int xdo_get_window_classname(nint xdo, int window, ref string class_ret);
    }
    
    public static class PidTools {
        const string PidName = "pid.so";
        [DllImport(PidName)]
        public static extern int get_exe_for_pid(int pid, [Out] byte[] exe_path_return);
    }
}
