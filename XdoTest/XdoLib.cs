using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XdoTest {
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
    }
}
