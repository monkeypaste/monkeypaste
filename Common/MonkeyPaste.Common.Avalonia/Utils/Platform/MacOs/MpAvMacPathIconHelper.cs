using MonoMac.AppKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvMacPathIconHelper {
        #region Path Icon
        public static string GetIconBase64FromMacPath(string path) {
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

            MpAvMacHelpers.EnsureInitialized();

            var icon = NSWorkspace.SharedWorkspace.IconForFile(path);
            var data = icon.AsTiff();
            var bitmap = new NSBitmapImageRep(data);

            data = bitmap.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, new MonoMac.Foundation.NSDictionary());
            using (var stream = data.AsStream()) {
                using (var memStream = new MemoryStream()) {
                    stream.CopyTo(memStream);
                    var bytes = memStream.ToArray();
                    string base64 = @"data:image/png;base64," + Convert.ToBase64String(bytes);
                    return base64;
                }
            }    
        }
        #endregion
    }
}
