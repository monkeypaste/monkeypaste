using Avalonia.Platform.Interop;
using System;
using System.Runtime.InteropServices;
namespace MonkeyPaste.Common.Avalonia {
    public static unsafe class GdiApi {
        private const string GlibName = "libglib-2.0.so.0";
        private const string GObjectName = "libgobject-2.0.so.0";

    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GSList {
        public readonly IntPtr Data;
        public readonly GSList* Next;
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GList {
        public readonly IntPtr Data;
        public readonly GSList* Next;
        public readonly GSList* Prev;
    }

    public enum GtkFileChooserAction {
        Open,
        Save,
        SelectFolder,
    }

    // ReSharper disable UnusedMember.Global
    public enum GtkResponseType {
        Help = -11,
        Apply = -10,
        No = -9,
        Yes = -8,
        Close = -7,
        Cancel = -6,
        Ok = -5,
        DeleteEvent = -4,
        Accept = -3,
        Reject = -2,
        None = -1,
    }
    // ReSharper restore UnusedMember.Global

    public static unsafe class GtkApi {

        private static IntPtr s_display = IntPtr.Zero;
        private const string GdkName = "libgdk-3.so.0";
        private const string GtkName = "libgtk-3.so.0";

        #region Wnck

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

        #endregion

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

        #region File Chooser


        [DllImport(GtkName)]
        public static extern void gtk_window_set_modal(IntPtr window, bool modal);

        [DllImport(GtkName)]
        public static extern void gtk_window_present(IntPtr gtkWindow);


        public delegate bool signal_generic(IntPtr gtkWidget, IntPtr userData);

        public delegate bool signal_dialog_response(IntPtr gtkWidget, GtkResponseType response, IntPtr userData);

        [DllImport(GtkName)]
        public static extern IntPtr gtk_file_chooser_dialog_new(Utf8Buffer title, IntPtr parent,
            GtkFileChooserAction action, IntPtr ignore);

        [DllImport(GtkName)]
        public static extern void gtk_file_chooser_set_select_multiple(IntPtr chooser, bool allow);

        [DllImport(GtkName)]
        public static extern void gtk_file_chooser_set_do_overwrite_confirmation(IntPtr chooser, bool do_overwrite_confirmation);

        [DllImport(GtkName)]
        public static extern void
            gtk_dialog_add_button(IntPtr raw, Utf8Buffer button_text, GtkResponseType response_id);

        [DllImport(GtkName)]
        public static extern GSList* gtk_file_chooser_get_filenames(IntPtr chooser);

        [DllImport(GtkName)]
        public static extern void gtk_file_chooser_set_filename(IntPtr chooser, Utf8Buffer file);

        [DllImport(GtkName)]
        public static extern void gtk_file_chooser_set_current_name(IntPtr chooser, Utf8Buffer file);

        [DllImport(GtkName)]
        public static extern void gtk_file_chooser_set_current_folder(IntPtr chooser, Utf8Buffer file);

        [DllImport(GtkName)]
        public static extern IntPtr gtk_file_filter_new();

        [DllImport(GtkName)]
        public static extern IntPtr gtk_file_filter_set_name(IntPtr filter, Utf8Buffer name);

        [DllImport(GtkName)]
        public static extern IntPtr gtk_file_filter_add_pattern(IntPtr filter, Utf8Buffer pattern);

        [DllImport(GtkName)]
        public static extern IntPtr gtk_file_chooser_add_filter(IntPtr chooser, IntPtr filter);

        [DllImport(GtkName)]
        public static extern IntPtr gtk_file_chooser_get_filter(IntPtr chooser);

        [DllImport(GtkName)]
        public static extern void gtk_widget_realize(IntPtr gtkWidget);

        [DllImport(GtkName)]
        public static extern void gtk_widget_destroy(IntPtr gtkWidget);

        [DllImport(GtkName)]
        public static extern IntPtr gtk_widget_get_window(IntPtr gtkWidget);

        [DllImport(GtkName)]
        public static extern void gtk_widget_hide(IntPtr gtkWidget);

        [DllImport(GdkName)]
        static extern IntPtr gdk_x11_window_foreign_new_for_display(IntPtr display, IntPtr xid);

        [DllImport(GdkName)]
        public static extern IntPtr gdk_x11_window_get_xid(IntPtr window);


        [DllImport(GtkName)]
        public static extern IntPtr gtk_container_add(IntPtr container, IntPtr widget);

        [DllImport(GdkName)]
        public static extern void gdk_window_set_transient_for(IntPtr window, IntPtr parent);

        public static IntPtr GetForeignWindow(IntPtr xid) => gdk_x11_window_foreign_new_for_display(s_display, xid);

        #endregion
    }
}
