using MonoMac.AppKit;
using System;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public static class MacHelper {
        private static bool _isInitialized;

        public static void EnsureInitialized() {
            if (_isInitialized) {
                return;
            }
            _isInitialized = true;
            NSApplication.Init();
        }

        public static string GetIconBase64FromPath(string path) {
            if(string.IsNullOrWhiteSpace(path)) {
                return string.Empty;
            }

            EnsureInitialized();

            var icon = NSWorkspace.SharedWorkspace.IconForFile(path);
            var data = icon.AsTiff();
            var bitmap = new NSBitmapImageRep(data);

            data = bitmap.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, new MonoMac.Foundation.NSDictionary());
            using(var stream = data.AsStream()) {
                using(var memStream = new MemoryStream()) {
                    stream.CopyTo(memStream);
                    var bytes = memStream.ToArray();
                    string base64 = @"data:image/png;base64," + Convert.ToBase64String(bytes);
                    return base64;
                }                
            }
            

            /*
             var path: NSString = NSWorkspace.sharedWorkspace().absolutePathForAppBundleWithIdentifier("com.apple.dt.xcode")!
var icon: NSImage = NSWorkspace.sharedWorkspace().iconForFile(path)

var data: NSData = icon.TIFFRepresentation!
var bitmap: NSBitmapImageRep = NSBitmapImageRep(data: data)!
data = bitmap.representationUsingType(NSBitmapImageFileType.NSPNGFileType, properties: [:])!
var base64: NSString = "data:image/png;base64," + data.base64EncodedStringWithOptions(NSDataBase64EncodingOptions.allZeros)
            */

        }
    }
}

