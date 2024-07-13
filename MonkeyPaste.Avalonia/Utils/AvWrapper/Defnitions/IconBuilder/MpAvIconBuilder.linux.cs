#if LINUX

using GLib;
using Gtk;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.IO;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvIconBuilder {
        const int ICON_SIZE = 256;
        public string GetPathIconBase64(string path, nint handle, MpIconSize iconSize) {
            return GetIcon_internal(path, iconSize, true);
        }
        public string GetPathIconBase64(string path, MpIconSize iconSize) {
            return GetIcon_internal(path, iconSize, false);
        }

        private string GetIcon_internal(string path, MpIconSize iconSize, bool isApp) {
            if (string.IsNullOrEmpty(path)) {
                return null;
            }
            string iconBase64 = null;
            if(isApp) {
                iconBase64 = GetAppIcon(path, ICON_SIZE);
            }
            if(!isApp || iconBase64 == null) {
                iconBase64 = GetFileIcon(path, ICON_SIZE);
            }

            if (string.IsNullOrEmpty(iconBase64)) {
                return MpBase64Images.QuestionMark;
            }
            return iconBase64;
        }

        private string GetAppIcon(string path, int size) {
            if(Mp.Services != null && Mp.Services.PlatformInfo != null && Mp.Services.PlatformInfo.ExecutingPath == path) {
                return MpBase64Images.AppIcon;
            }
            string icon_name = MpX11ShellHelpers.GetLauncherProperty(path, "Icon");
            if (icon_name == null) {
                return null;
            }

            string icon_path = null;
            // NOTE only looks for pngs since no simple svg rendering
            // find all known icons w/ icon name
            var find_results =
                $"find /usr/share/icons/ -name \"{icon_name}.png\"".ShellExec()
                .SplitByLineBreak();

            if (find_results.FirstOrDefault(x => x.Contains($"{size}x{size}")) is { } size_match_icon_path) {
                // found icon matching dimensions
                icon_path = size_match_icon_path;
            } else {
                // pick first (if any)
                icon_path = find_results.FirstOrDefault();
            }
            if (icon_path.IsNullOrEmpty()) {
                return null;
            } 

            // use shell instead of dotnet cause of permission errors
            string base64 = $"base64 --wrap=0 {icon_path}".ShellExec();
            //MpConsole.WriteLine($"Icon for '{path}':");
            //MpConsole.WriteLine(base64);
            return base64;
        }

        private string GetFileIcon(string path, int iconSize) {
            try {
                var file = FileFactory.NewForPath(path);

                var fileInfo = file.QueryInfo("standard::icon", 0, Cancellable.Current);
                var fileIcon = fileInfo.Icon;
                var iconTheme = Gtk.IconTheme.Default;

                var iconInfo = iconTheme.LookupIcon(
                    fileIcon, iconSize, IconLookupFlags.ForceSize | IconLookupFlags.UseBuiltin);


                var pixBuf = iconInfo.LoadIcon();
                //string base64 = pixBuf.PixelBytes.Data.ToBase64String();
                var bytes = pixBuf.SaveToBuffer("png");
                return bytes.ToBase64String();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Error reading icon for path: " + path, ex);
                return null;
            }
        }
    }
}
#endif
