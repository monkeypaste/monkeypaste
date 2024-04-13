using Avalonia.Platform.Interop;
using Avalonia.X11;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
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
                    using (var backends = new MpUtf8Buffer("x11"))
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
                using (var utf = new MpUtf8Buffer($"avalonia.app.a{Guid.NewGuid():N}"))
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
}
