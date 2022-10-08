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

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvX11PathIconHelper {
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

        public static string GetIconBase64FromX11Path(string path) {
            return null;
            // MpConsole.WriteLine("Getting x11 icon for path: " + path);
            // try {
            //     var file = FileFactory.NewForPath(path);
            //     var fileInfo = file.QueryInfo("standard::icon", 0, Cancellable.Current);
            //     var fileIcon = fileInfo.Icon;
            //     var iconTheme = IconTheme.GetForScreen(Screen.Default);

            //     //IconInfo iconInfo = null;
            //     //foreach (var contextName in iconTheme.ListContexts()) {
            //     //    foreach(var iconName in iconTheme.ListIcons(contextName)) {
            //     //        for (int i = 0; i < 1024; i++) {
            //     //            iconInfo = iconTheme.LookupIcon(fileIcon, i, IconLookupFlags.ForceSize | IconLookupFlags.UseBuiltin);
            //     //            if (iconInfo != null) {
            //     //                break;
            //     //            }
            //     //        }
            //     //    }
            //     //}
            //     var iconInfo = iconTheme.LookupIcon(fileIcon, 128, IconLookupFlags.ForceSize | IconLookupFlags.UseBuiltin);


            //     var pixBuf = iconInfo.LoadIcon();
            //     string base64 = pixBuf.PixelBytes.Data.ToBase64String();
            //     return base64;
            // }catch(Exception ex) {
            //     MpConsole.WriteTraceLine("Error reading icon for path: " + path, ex);
            //     return null;
            // }
        }
        #endregion
    }
}
