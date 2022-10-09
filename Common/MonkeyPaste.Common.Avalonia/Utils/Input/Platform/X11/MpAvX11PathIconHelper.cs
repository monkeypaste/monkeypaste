using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using Gio;
using Gtk;
using GLib;
using Gdk;
using X11;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvX11PathIconHelper {
        #region Private Variables

        private static IntPtr _displayPtr;
        private static X11.Window _rootWindow;

        #endregion

        #region Path Icon
        /* --from handle---
            #define WNCK_I_KNOW_THIS_IS_UNSTABLE = 1

            #include <libwnck/libwnck.h>

            int main(int argc,
             char **argv) {
            WnckScreen *screen;

            gdk_init(&argc, &argv);

            screen = wnck_screen_get_default();

            wnck_screen_force_update(screen);

            auto win = wnck_window_get(127926341); // 127926341 is window id, it can be get by linux cli xdotool or wmctrl or other many clis
            GdkPixbuf *r = wnck_window_get_icon(win);
            GError *err = NULL;
            gdk_pixbuf_save(r, "/tmp/prpr.png", "png", &err, "quality", "100", NULL);
            return 0;
            }

            -- from file path (icon file path)

            import gi
            gi.require_version('Gtk', '3.0')
            from gi.repository import Gio as gio
            from gi.repository import Gtk as gtk
            import os

            def get_icon_filename(filename,size):
                #final_filename = "default_icon.png"
                final_filename = ""
                if os.path.isfile(filename):
                    # Get the icon name
                    file = gio.File.new_for_path(filename)
                    file_info = file.query_info('standard::icon',0)
                    file_icon = file_info.get_icon().get_names()[0]
                    # Get the icon file path
                    icon_theme = gtk.IconTheme.get_default()
                    icon_filename = icon_theme.lookup_icon(file_icon, size, 0)
                    if icon_filename != None:
                        final_filename = icon_filename.get_filename()

                return final_filename


            print(get_icon_filename("/home/newtron/Desktop/counter.desktop",48))\
            

            c++

            Glib::RefPtr<Gdk::Pixbuf> Info::getPixbuf(File *f) {
            //File is a custom class
            static Glib::RefPtr<Gtk::IconTheme> iconTheme = Gtk::IconTheme::get_default();
            Glib::ustring sPath = Glib::build_filename(f->getDirPath(), f->getName());
            Glib::RefPtr<Gio::File> gioFile = Gio::File::create_for_path(sPath);
            Glib::RefPtr<Gio::FileInfo> info = gioFile->query_info();
            Glib::RefPtr<Gio::Icon> icon = info->get_icon();
            //getIconSize() a custom function returning the desired size
            Gtk::IconInfo iconInfo = iconTheme->lookup_icon(icon, getIconSize(), Gtk::ICON_LOOKUP_USE_BUILTIN);
            return iconInfo.load_icon();
        */

        public static string GetIconBase64FromX11Path(string path, string pathType, int iconSize = 48) {
            //return null;
            MpConsole.WriteLine("Getting x11 icon for path: " + path);
            // if(pathType == "EXECUTABLE") {
            //     string app_icon = GetAppIcon(path, iconSize);
            //     if(!string.IsNullOrEmpty(app_icon)) {
            //         return app_icon;
            //     }
            // }
            return GetFileIcon(path, iconSize);
        }

        private static string GetAppIcon(string path, int iconSize) {
            if(_displayPtr == IntPtr.Zero) {
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
            }
            Xlib.XGrabServer(_displayPtr);

            // var icon_prop = XInternAtom(_displayPtr, "_NET_CLIENT_LIST", true);

            // int result = XGetWindowProperty(
            //     _displayPtr,
            //     _rootWindow,
            //     icon_prop,
            //     0,
            //     long.MaxValue,
            //     false,
            //     X11.Atom.)
            // Xlib.XUngrabServer(_displayPtr); 
            return null;
        }

        private static string GetFileIcon(string path, int iconSize) {
            try {
                var file = FileFactory.NewForPath(path);
                
                var fileInfo = file.QueryInfo("standard::icon", 0, Cancellable.Current);
                var fileIcon = fileInfo.Icon;
                var iconTheme = Gtk.IconTheme.Default;
                if(iconTheme == null) {
                    MpConsole.WriteLine("IconTheme not found");
                } else {
                    MpConsole.WriteLine("IconTheme exists");
                }
                
                var iconInfo = iconTheme.LookupIcon(
                    fileIcon, iconSize, IconLookupFlags.ForceSize | IconLookupFlags.UseBuiltin);


                var pixBuf = iconInfo.LoadIcon();
                //string base64 = pixBuf.PixelBytes.Data.ToBase64String();
                var bytes = pixBuf.SaveToBuffer("png");
                return bytes.ToBase64String();
            }catch(Exception ex) {
                MpConsole.WriteTraceLine("Error reading icon for path: " + path, ex);
                return null;
            }
        }

        [DllImport("libX11.so.6")]
        private static extern X11.Atom XInternAtom(IntPtr display, string name, bool only_if_exists);

        /*
        int XGetWindowProperty(display, w, property, long_offset, long_length, delete, req_type, 
                        actual_type_return, actual_format_return, nitems_return, bytes_after_return, 
                        prop_return)
      Display *display;
      Window w;
      Atom property;
      long long_offset, long_length;
      Bool delete;
      Atom req_type; 
      Atom *actual_type_return;
      int *actual_format_return;
      unsigned long *nitems_return;
      unsigned long *bytes_after_return;
      unsigned char **prop_return;
      */
        [DllImport("libX11.so.6")]
        private static extern int XGetWindowProperty(
            IntPtr display,
            X11.Window window,
            X11.Atom atom,
            long long_offset,
            long long_length,
            bool delete,
            X11.Atom req_type,
            IntPtr actual_type_return, //atom
            IntPtr actual_format_return, //int
            IntPtr nitems_return, //ulong
            IntPtr bytes_after_return, //ulong
            byte prop_return);
        #endregion
    }
}
