using Avalonia.Platform.Interop;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

        #region StartGtk

        [DllImport(GdkName)]
        public static extern IntPtr gdk_set_allowed_backends(Utf8Buffer backends);
        
        [DllImport(GtkName)]
        public static extern bool gtk_init_check(int argc, IntPtr argv);

        [DllImport(GtkName)]
        public static extern IntPtr gtk_application_new(Utf8Buffer appId, int flags);

        [DllImport(GdkName)]
        public static extern IntPtr gdk_display_get_default();

        [DllImport(GtkName)]
        public static extern void gtk_main_iteration();
        #endregion

        
    }

    public static class GtkHelper {
        private static IntPtr s_display;
        private static Task<bool> _initialized;
        public static async Task EnsureInitialized() {
            if (_initialized == null) _initialized = StartGtk();

            if (!(await _initialized))
                throw new Exception("Unable to initialize GTK on separate thread");
        }

        static Task<bool> StartGtk() {
            var tcs = new TaskCompletionSource<bool>();
            new Thread(() => {
                try {
                    using (var backends = new Utf8Buffer("x11"))
                        GtkApi.gdk_set_allowed_backends(backends);
                }
                catch {
                    //Ignore
                }

                Environment.SetEnvironmentVariable("WAYLAND_DISPLAY",
                    "/proc/fake-display-to-prevent-wayland-initialization-by-gtk3");

                if (!GtkApi.gtk_init_check(0, IntPtr.Zero)) {
                    tcs.SetResult(false);
                    return;
                }

                IntPtr app;
                using (var utf = new Utf8Buffer($"avalonia.app.a{Guid.NewGuid():N}"))
                    app = GtkApi.gtk_application_new(utf, 0);
                if (app == IntPtr.Zero) {
                    tcs.SetResult(false);
                    return;
                }

                s_display = GtkApi.gdk_display_get_default();
                tcs.SetResult(true);
                while (true)
                    GtkApi.gtk_main_iteration();
            }) { Name = "GTK3THREAD", IsBackground = true }.Start();
            return tcs.Task;
        }
    }

    public static class GtkTest {
        public static void Test() {
            Dispatcher.UIThread.Post(async () => {
                await GtkHelper.EnsureInitialized();

            });
        }
    }
 
}
