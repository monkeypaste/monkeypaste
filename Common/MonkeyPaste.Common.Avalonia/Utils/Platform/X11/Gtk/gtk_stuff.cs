using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.PlatformConfiguration;

namespace MonkeyPaste.Common.Avalonia.Utils.Platform.X11.Gtk {
    public class WApplication {
        private IntPtr WnckApplication;

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

        public string Name {
            get { return wnck_application_get_name(Handle); }
        }

        public string IconName {
            get { return wnck_application_get_icon_name(Handle); }
}

        public int PID {
get { return wnck_application_get_pid(Handle); }
}
        public Gdk.Pixbuf Icon {
            get { return new Gdk.Pixbuf(wnck_application_get_icon(Handle)); }
}
public Gdk.Pixbuf MiniIcon {
            get { return new Gdk.Pixbuf(wnck_application_get_mini_icon(Handle)); }
        }

        public WWindow[] Windows {
            get {
                //GLib.List wlist = new GLib.List(wnck_application_get_windows(Handle));
                GList wlist = new GList(wnck_application_get_windows(Handle));

                ArrayList windows = new ArrayList(wlist.Count);

                for (int i = 0; i < wlist.Count; i++) {
                    GLib.Object wobj = (GLib.Object)wlist[i];
                    WWindow window = new WWindow(wobj.Handle);
                    windows.Add(window);
                }

                return (Wnck.WWindow[])windows.ToArray(System.Type.GetType("Wnck.WWindow"));
            }
        }

        public IntPtr Handle {
            get { return WnckApplication; }
        }

        public WApplication(IntPtr application) {
            WnckApplication = application;
        }
    }

    public class WWindow {
        private IntPtr WnckWindow;

        [DllImport("libwnck-1")]
        static extern IntPtr wnck_window_get_application(IntPtr window);
        [DllImport("libwnck-1")]
        static extern string wnck_application_get_name(IntPtr app);

        [DllImport("libwnck-1")]
        static extern string wnck_window_get_name(IntPtr window);
        [DllImport("libwnck-1")]
        static extern IntPtr wnck_window_get_workspace(IntPtr window);
        [DllImport("libwnck-1")]
        static extern IntPtr wnck_window_get_screen(IntPtr window);

        [DllImport("libwnck-1")]
        static extern void wnck_window_activate(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_close(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_minimize(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_unminimize(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_maximize(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_unmaximize(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_shade(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_unshade(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_stick(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_unstick(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_keyboard_move(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_keyboard_size(IntPtr window);

        [DllImport("libwnck-1")]
        static extern bool wnck_window_is_minimized(IntPtr window);
        [DllImport("libwnck-1")]
        static extern bool wnck_window_is_maximized_horizontally(IntPtr window);
        [DllImport("libwnck-1")]
        static extern bool wnck_window_is_maximized_vertically(IntPtr window);
        [DllImport("libwnck-1")]
        static extern bool wnck_window_is_maximized(IntPtr window);
        [DllImport("libwnck-1")]
        static extern bool wnck_window_is_shaded(IntPtr window);
        [DllImport("libwnck-1")]
        static extern bool wnck_window_is_skip_pager(IntPtr window);
        [DllImport("libwnck-1")]
        static extern bool wnck_window_is_skip_tasklist(IntPtr window);
        [DllImport("libwnck-1")]
        static extern bool wnck_window_is_sticky(IntPtr window);

        [DllImport("libwnck-1")]
        static extern bool wnck_window_is_pinned(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_pin(IntPtr window);
        [DllImport("libwnck-1")]
        static extern void wnck_window_unpin(IntPtr window);

        [DllImport("libwnck-1")]
        static extern void wnck_window_set_skip_pager(IntPtr window, bool skip);
        [DllImport("libwnck-1")]
        static extern void wnck_window_set_skip_tasklist(IntPtr window, bool skip);

        [DllImport("libwnck-1")]
        static extern IntPtr wnck_window_get_icon(IntPtr window);
        [DllImport("libwnck-1")]
        static extern IntPtr wnck_window_get_mini_icon(IntPtr window);

        public string Name {
            get {
                return wnck_window_get_name(WnckWindow);
            }
        }

        public WApplication Application {
            get {
                return new WApplication(wnck_window_get_application(Handle));
            }
        }

        public WScreen Screen {
            get {
                return new WScreen(wnck_window_get_screen(Handle));
            }
        }

        public WWorkspace Workspace {
            get {
                return new WWorkspace(this.Screen, wnck_window_get_workspace(Handle));
            }
        }

        public bool InTaskList {
            get {
                return !wnck_window_is_skip_tasklist(WnckWindow);
            }
            set {
                wnck_window_set_skip_tasklist(Handle, !value);
            }
        }

        public bool InPager {
            get {
                return !wnck_window_is_skip_pager(Handle);
            }
            set {
                wnck_window_set_skip_pager(Handle, !value);
            }
        }

        public bool Maximized {
            get {
                return wnck_window_is_maximized(Handle);
            }
            set {
                if (value) wnck_window_maximize(Handle);
                else wnck_window_unmaximize(Handle);
            }
        }

        public bool Minimized {
            get {
                return wnck_window_is_minimized(Handle);
            }
            set {
                if (value) wnck_window_minimize(Handle);
                else wnck_window_unminimize(Handle);
            }
        }

        public bool Shaded {
            get {
                return wnck_window_is_shaded(Handle);
            }
            set {
                if (value) wnck_window_shade(Handle);
                else wnck_window_unshade(Handle);
            }
        }

        public bool Sticky {
            get {
                return wnck_window_is_sticky(Handle);
            }
            set {
                if (value) wnck_window_stick(Handle);
                else wnck_window_unstick(Handle);
            }
        }

        public bool Pinned {
            get {
                return wnck_window_is_pinned(Handle);
            }
            set {
                if (value) wnck_window_pin(Handle);
                else wnck_window_unpin(Handle);
            }
}
public Gdk.Pixbuf Icon {
get {
                return new Gdk.Pixbuf(wnck_window_get_icon(Handle));
            }
        }

        public Gdk.Pixbuf MiniIcon {
get {
                return new Gdk.Pixbuf(wnck_window_get_mini_icon(Handle));
            }
        }

        public IntPtr Handle {
            get { return WnckWindow; }
        }

        public WWindow(IntPtr window) {
            WnckWindow = window;
        }

        public void Activate() {
            wnck_window_activate(Handle);
        }

        public void Close() {
            wnck_window_close(Handle);
        }
    }

    public class WWorkspace {
        private IntPtr WnckWorkspace;
        private WScreen screen;

        [DllImport("libwnck-1")]
        static extern bool wnck_window_is_on_workspace(IntPtr window, IntPtr workspace);

        public bool Contains(WWindow window) {
            return wnck_window_is_on_workspace(window.Handle, Handle);
        }

        public IntPtr Handle {
            get { return WnckWorkspace; }
        }

        public WScreen Screen {
            get { return screen; }
        }

        public WWorkspace(WScreen screen, IntPtr workspace) {
            this.screen = screen;
            WnckWorkspace = workspace;
        }

        public WWindow[] Tasks {
            get {
                WWindow[] alltasks = this.Screen.Tasks;

                ArrayList windows = new ArrayList(alltasks.Length);

                foreach (WWindow w in alltasks) {
                    if (Contains(w))
                        windows.Add(w);
                }

                return (WWindow[])windows.ToArray(System.Type.GetType("Wnck.WWindow"));
            }
        }
    }

    public class WScreen {
        private IntPtr WnckScreen;

        [DllImport("libwnck-1")]
        static extern IntPtr wnck_screen_get_default();

        [DllImport("libwnck-1")]
        static extern IntPtr wnck_screen_get_windows(IntPtr screen);

        [DllImport("libwnck-1")]
        static extern void wnck_screen_force_update(IntPtr screen);

        [DllImport("libwnck-1")]
        static extern IntPtr wnck_screen_get_active_workspace(IntPtr window);

        public WWorkspace Active {
            get {
                wnck_screen_force_update(Handle);
                return new WWorkspace(this, wnck_screen_get_active_workspace(Handle));
            }
        }

        public WWindow[] Tasks {
            get {
                wnck_screen_force_update(Handle);
                IntPtr raw_list = wnck_screen_get_windows(Handle);

                GLib.List wlist = new GLib.List(raw_list);
                ArrayList windows = new ArrayList(wlist.Count);

                for (int i = 0; i < wlist.Count; i++) {
                    GLib.Object wobj = (GLib.Object)wlist[i];
                    WWindow window = new WWindow(wobj.Handle);

                    if (window.InTaskList) windows.Add(window);
                }

                return (WWindow[])windows.ToArray(System.Type.GetType("Wnck.WWindow"));
            }
        }

        public IntPtr Handle {
            get { return WnckScreen; }
        }

        public WScreen(IntPtr screen) {
            WnckScreen = screen;
        }
        public WScreen() {
            WnckScreen = wnck_screen_get_default();
        }
    }
}
