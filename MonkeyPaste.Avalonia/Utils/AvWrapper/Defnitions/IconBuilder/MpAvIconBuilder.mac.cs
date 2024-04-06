using Avalonia.Platform;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.WebKit;
using System;
using System.IO;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvIconBuilder {
        public virtual string GetPathIconBase64(string path, nint handle, MpIconSize iconSize = MpIconSize.MediumIcon32) =>
            GetPathIconBase64(path, iconSize);
        public string GetPathIconBase64(string path, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            /*
                var path: NSString = NSWorkspace.sharedWorkspace().absolutePathForAppBundleWithIdentifier("com.apple.dt.xcode")!
                var icon: NSImage = NSWorkspace.sharedWorkspace().iconForFile(path)

                var data: NSData = icon.TIFFRepresentation!
                var bitmap: NSBitmapImageRep = NSBitmapImageRep(data: data)!
                data = bitmap.representationUsingType(NSBitmapImageFileType.NSPNGFileType, properties: [:])!
                var base64: NSString = "data:image/png;base64," + data.base64EncodedStringWithOptions(NSDataBase64EncodingOptions.allZeros)
            */

            // from https://gist.github.com/hinzundcode/2ca9b9a425b8ed0d9ec4
            if (string.IsNullOrWhiteSpace(path)) {
                return string.Empty;
            }

            var icon = NSWorkspace.SharedWorkspace.IconForFile(path);
            var data = icon.AsTiff();
            var bitmap = new NSBitmapImageRep(data);
            data = bitmap.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, new NSDictionary());
            using (var stream = data.AsStream()) {
                using (var memStream = new MemoryStream()) {
                    stream.CopyTo(memStream);
                    var bytes = memStream.ToArray();
                    string base64 = bytes.ToAvBitmap().ResizeKeepAspect(GetSize(iconSize), true).ToBase64String();
                    return base64;
                }
            }
        }
        private MpSize GetSize(MpIconSize ics) {
            if (ics.ToString().Split("Icon").Last() is { } len_str &&
                int.Parse(len_str) is int len) {
                return new MpSize(len, len);
            }
            return new MpSize(32, 32);
        }
    }
}