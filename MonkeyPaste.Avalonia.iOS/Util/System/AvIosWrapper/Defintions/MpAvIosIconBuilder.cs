using Foundation;
using MonkeyPaste;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.IO;
using System.Linq;
using UIKit;

namespace MonkeyPaste.Avalonia.iOS {
    public class MpAvIosIconBuilder : MpIPathToPlatformIcon {
        public virtual string GetPathIconBase64(string path, nint handle, MpIconSize iconSize = MpIconSize.ExtraLargeIcon128) =>
            GetPathIconBase64(path, iconSize);
        public string GetPathIconBase64(string path, MpIconSize iconSize = MpIconSize.ExtraLargeIcon128) {
            // from https://indiestack.com/2018/05/icon-for-file-with-uikit/
            if (string.IsNullOrWhiteSpace(path) ||
                iconSize == MpIconSize.None) {
                return string.Empty;
            }
            //if (MpAvMacHelpers.IsPathExecutableUnderAppBundle(path)) {
            //    // ONLY when path has no extension and is under a .app bundle get .app icon not generic exe icon
            //    path = MpAvMacHelpers.GetAppBundlePathOrDefault(path);
            //} 

            if(NSUrl.FromString(path.ToFileSystemUriFromPath()) is not { } url ||
                UIDocumentInteractionController.FromUrl(url) is not { } dic ||
                dic.Icons is not { } il) {
                return null;
            }
            var s = double.Parse(iconSize.ToString().Split("Icon").Last());
            // find closest icon by min size diff
            UIImage img =
                il.OrderBy(x => Math.Abs(x.PreferredPresentationSizeForItemProvider.Width.Value - s)).FirstOrDefault();

            return img.AsPNG().GetBase64EncodedString(NSDataBase64EncodingOptions.None);
        }
    }
}
