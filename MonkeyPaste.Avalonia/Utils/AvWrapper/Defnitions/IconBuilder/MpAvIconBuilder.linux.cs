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

        public string GetPathIconBase64(string path, nint handle, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            return GetIcon_internal(path, iconSize, true);
        }
        public string GetPathIconBase64(string path, MpIconSize iconSize = MpIconSize.LargeIcon48) {
            return GetIcon_internal(path, iconSize, false);
        }

        private string GetIcon_internal(string path, MpIconSize iconSize, bool isApp) {
            if (string.IsNullOrEmpty(path)) {
                return null;
            }
            string iconBase64 = GetIconBase64FromX11Path(path, GetSize(iconSize), isApp);
            //MpConsole.WriteLine("Icon for path: " + path);
            //MpConsole.WriteLine(iconBase64);
            if (string.IsNullOrEmpty(iconBase64)) {
                return MpBase64Images.QuestionMark;
            }
            return iconBase64;
        }
        private string GetIconBase64FromX11Path(string path, MpSize iconSize, bool isApp) {
            byte[] bytes;
            if (isApp) {
                bytes = GetAppIcon(path, (int)iconSize.Width);
            } else {
                bytes = GetFileIcon(path, (int)iconSize.Width);
            }
            if (bytes == null ||
                bytes.Length == 0) {
                return string.Empty;
            }
            string base64 = bytes.ToAvBitmap().ResizeKeepAspect(iconSize, true).ToBase64String();
            return base64;
        }

        private byte[] GetAppIcon(string path, int size) {
            string exe_name =
                path
                .SplitNoEmpty("\\")
                .LastOrDefault()
                .SplitNoEmpty(" ")
                .FirstOrDefault();
            string exec_match_val = $"{Environment.NewLine}Exec={exe_name}";

            string desktop_dir = "/usr/share/applications";
            var dpl = new DirectoryInfo(desktop_dir)
                .GetFiles("*.desktop").Select(x => x.FullName);

            string icon_name = null;
            foreach (string dp in dpl) {
                // read desktop file
                string dp_pt = MpFileIo.ReadTextFromFile(dp);
                if (dp_pt.Contains(exec_match_val)) {
                    // desktop file contains exec match
                    if (dp_pt.SplitNoEmpty("Icon=") is { } iconParts &&
                        iconParts.Length > 1 &&
                        iconParts.Skip(1).FirstOrDefault() is { } iconNamePart1 &&
                        iconNamePart1.SplitByLineBreak().FirstOrDefault() is { } iconNamePart2 &&
                        !iconNamePart2.IsNullOrEmpty()) {
                        // found "Icon=<icon name>" 
                        icon_name = iconNamePart2;
                        break;
                    }
                }
            }
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

            return MpFileIo.ReadBytesFromFile(icon_path);
        }

        private byte[] GetFileIcon(string path, int iconSize) {
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
                return bytes;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Error reading icon for path: " + path, ex);
                return null;
            }
        }
    }
}
#endif
