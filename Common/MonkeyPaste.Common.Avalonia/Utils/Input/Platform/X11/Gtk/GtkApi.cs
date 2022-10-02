using Avalonia.Controls.Platform;
using Avalonia.Controls;
using Avalonia.Platform;
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
using System.IO;
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

        public static INativeControlHostDestroyableControlHandle CreateGtkFileChooser(IntPtr parentXid) {
            //if (s_gtkTask == null)
            //    s_gtkTask = StartGtk();
            //if (!s_gtkTask.Result)
            //    return null;
            EnsureInitialized().RunSynchronously();

            return Glib.RunOnGlibThreadAsync(() => {
                using (var title = new Utf8Buffer("Embedded")) {
                    var widget = GtkApi.gtk_file_chooser_dialog_new(title, IntPtr.Zero, GtkFileChooserAction.SelectFolder,
                        IntPtr.Zero);
                    GtkApi.gtk_widget_realize(widget);
                    var xid = GtkApi.gdk_x11_window_get_xid(GtkApi.gtk_widget_get_window(widget));
                    GtkApi.gtk_window_present(widget);
                    return new GtkFileChooser(widget, xid);
                }
            }).Result;
        }
    }

    public static class GtkTest {
        public static void Test() {
            Dispatcher.UIThread.Post(async () => {
                await GtkHelper.EnsureInitialized();

            });
        }
    }

    public class GtkSystemDialog : ISystemDialogImpl {
        //private Task<bool> _initialized;

        private unsafe Task<string[]> ShowDialog(string title, IWindowImpl parent, GtkFileChooserAction action,
            bool multiSelect, string initialDirectory, string initialFileName, IEnumerable<FileDialogFilter> filters, string defaultExtension, bool overwritePrompt) {
            IntPtr dlg;
            using (var name = new Utf8Buffer(title))
                dlg = GtkApi.gtk_file_chooser_dialog_new(name, IntPtr.Zero, action, IntPtr.Zero);
            UpdateParent(dlg, parent);
            if (multiSelect)
                GtkApi.gtk_file_chooser_set_select_multiple(dlg, true);

            GtkApi.gtk_window_set_modal(dlg, true);
            var tcs = new TaskCompletionSource<string[]>();
            List<IDisposable> disposables = null;

            void Dispose() {
                // ReSharper disable once PossibleNullReferenceException
                foreach (var d in disposables) d.Dispose();
                disposables.Clear();
            }

            var filtersDic = new Dictionary<IntPtr, FileDialogFilter>();
            if (filters != null)
                foreach (var f in filters) {
                    var filter = GtkApi.gtk_file_filter_new();
                    filtersDic[filter] = f;
                    using (var b = new Utf8Buffer(f.Name))
                        GtkApi.gtk_file_filter_set_name(filter, b);

                    foreach (var e in f.Extensions)
                        using (var b = new Utf8Buffer("*." + e))
                            GtkApi.gtk_file_filter_add_pattern(filter, b);

                    GtkApi.gtk_file_chooser_add_filter(dlg, filter);
                }

            disposables = new List<IDisposable>
            {
                Glib.ConnectSignal<GtkApi.signal_generic>(dlg, "close", delegate
                {
                    tcs.TrySetResult(null);
                    Dispose();
                    return false;
                }),
                Glib.ConnectSignal<GtkApi.signal_dialog_response>(dlg, "response", (_, resp, __) =>
                {
                    string[] result = null;
                    if (resp == GtkResponseType.Accept)
                    {
                        var resultList = new List<string>();
                        var gs = GtkApi.gtk_file_chooser_get_filenames(dlg);
                        var cgs = gs;
                        while (cgs != null)
                        {
                            if (cgs->Data != IntPtr.Zero)
                                resultList.Add(Utf8Buffer.StringFromPtr(cgs->Data));
                            cgs = cgs->Next;
                        }
                        Glib.g_slist_free(gs);
                        result = resultList.ToArray();
                        
                        // GTK doesn't auto-append the extension, so we need to do that manually
                        if (action == GtkFileChooserAction.Save)
                        {
                            var currentFilter = GtkApi.gtk_file_chooser_get_filter(dlg);
                            filtersDic.TryGetValue(currentFilter, out var selectedFilter);
                            for (var c = 0; c < result.Length; c++)
                                result[c] = NameWithExtension(result[c], defaultExtension, selectedFilter);
                        }
                    }

                    GtkApi.gtk_widget_hide(dlg);
                    Dispose();
                    tcs.TrySetResult(result);
                    return false;
                })
            };
            using (var open = new Utf8Buffer(
                action == GtkFileChooserAction.Save ? "Save"
                : action == GtkFileChooserAction.SelectFolder ? "Select"
                : "Open"))
                GtkApi.gtk_dialog_add_button(dlg, open, GtkResponseType.Accept);
            using (var open = new Utf8Buffer("Cancel"))
                GtkApi.gtk_dialog_add_button(dlg, open, GtkResponseType.Cancel);

            if (initialDirectory != null) {
                using var dir = new Utf8Buffer(initialDirectory);
                GtkApi.gtk_file_chooser_set_current_folder(dlg, dir);
            }

            if (initialFileName != null) {
                // gtk_file_chooser_set_filename() expects full path
                using var fn = action == GtkFileChooserAction.Open
                    ? new Utf8Buffer(Path.Combine(initialDirectory ?? "", initialFileName))
                    : new Utf8Buffer(initialFileName);

                if (action == GtkFileChooserAction.Save) {
                    GtkApi.gtk_file_chooser_set_current_name(dlg, fn);
                } else {
                    GtkApi.gtk_file_chooser_set_filename(dlg, fn);
                }
            }

            GtkApi.gtk_file_chooser_set_do_overwrite_confirmation(dlg, overwritePrompt);

            GtkApi.gtk_window_present(dlg);
            return tcs.Task;
        }

        string NameWithExtension(string path, string defaultExtension, FileDialogFilter filter) {
            var name = Path.GetFileName(path);
            if (name != null && !name.Contains(".")) {
                if (filter?.Extensions?.Count > 0) {
                    if (defaultExtension != null
                        && filter.Extensions.Contains(defaultExtension))
                        return path + "." + defaultExtension.TrimStart('.');

                    var ext = filter.Extensions.FirstOrDefault(x => x != "*");
                    if (ext != null)
                        return path + "." + ext.TrimStart('.');
                }

                if (defaultExtension != null)
                    path += "." + defaultExtension.TrimStart('.');
            }

            return path;
        }

        public async Task<string[]> ShowFileDialogAsync(FileDialog dialog, Window parent) {
            await GtkHelper.EnsureInitialized();

            var platformImpl = parent?.PlatformImpl;

            return await await Glib.RunOnGlibThreadAsync(() => ShowDialog(
                dialog.Title, platformImpl,
                dialog is OpenFileDialog ? GtkFileChooserAction.Open : GtkFileChooserAction.Save,
                (dialog as OpenFileDialog)?.AllowMultiple ?? false,
                dialog.Directory,
                dialog.InitialFileName,
                dialog.Filters,
                (dialog as SaveFileDialog)?.DefaultExtension,
                true));
        }

        public async Task<string> ShowFolderDialogAsync(OpenFolderDialog dialog, Window parent) {
            await GtkHelper.EnsureInitialized();

            var platformImpl = parent?.PlatformImpl;

            return await await Glib.RunOnGlibThreadAsync(async () => {
                var res = await ShowDialog(
                    dialog.Title,
                    platformImpl, GtkFileChooserAction.SelectFolder,
                    false,
                    dialog.Directory,
                    null,
                    null,
                    null,
                    false);

                return res?.FirstOrDefault();
            });
        }

        void UpdateParent(IntPtr chooser, IWindowImpl parentWindow) {
            var xid = parentWindow.Handle.Handle;
            GtkApi.gtk_widget_realize(chooser);
            var window = GtkApi.gtk_widget_get_window(chooser);
            var parent = GtkApi.GetForeignWindow(xid);
            if (window != IntPtr.Zero && parent != IntPtr.Zero)
                GtkApi.gdk_window_set_transient_for(window, parent);
        }
    }

    public class GtkFileChooser : INativeControlHostDestroyableControlHandle {
        private readonly IntPtr _widget;

        public GtkFileChooser(IntPtr widget, IntPtr xid) {
            _widget = widget;
            Handle = xid;
        }

        public IntPtr Handle { get; }
        public string HandleDescriptor => "XID";

        public void Destroy() {
            Glib.RunOnGlibThreadAsync(() =>
            {
                GtkApi.gtk_widget_destroy(_widget);
                return 0;
            }).Wait();
        }
    }
}
