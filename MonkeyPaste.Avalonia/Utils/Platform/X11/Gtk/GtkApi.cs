using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static unsafe class GdiApi {
        private const string GlibName = "libglib-2.0.so.0";
        private const string GObjectName = "libgobject-2.0.so.0";


    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct GSList {
        public readonly IntPtr Data;
        public readonly GSList* Next;
    }
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct GList {
        public readonly IntPtr Data;
        public readonly GSList* Next;
        public readonly GSList* Prev;
    }


    public static unsafe class GtkApi {
        private static IntPtr s_display;
        private const string GdkName = "libgdk-3.so.0";
        private const string GtkName = "libgtk-3.so.0";

        [DllImport("libwnck-1")]
        static extern string wnck_application_get_name(IntPtr app);

        [DllImport("libwnck-1")]
        static extern string wnck_application_get_icon_name(IntPtr app);
        [DllImport("libwnck-1")]
        static extern int wnck_application_get_pid(IntPtr app);
        [DllImport("libwnck-1")]
        static extern IntPtr wnck_application_get_icon(IntPtr app);
        [DllImport("libwnck-1")]
        static extern IntPtr wnck_application_get_mini_icon(IntPtr app);
        [DllImport("libwnck-1")]
        static extern bool wnck_application_get_icon_is_fallback(IntPtr app);
        [DllImport("libwnck-1")]
        static extern string wnck_application_get_startup_id(IntPtr app);
        [DllImport("libwnck-1")]
        static extern IntPtr wnck_application_get_windows(IntPtr app);
    }
 
}
