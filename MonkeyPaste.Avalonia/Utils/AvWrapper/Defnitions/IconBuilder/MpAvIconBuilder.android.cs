
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvIconBuilder {

        public string GetPathIconBase64(string path, MpIconSize iconSize = MpIconSize.MediumIcon32) =>
            GetPathIconBase64(path, IntPtr.Zero, iconSize);

        public string GetPathIconBase64(string path, nint handle, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            return MpAvDeviceWrapper.Instance.IconBuilder.GetPathIconBase64(path, handle, iconSize);
        }
    }
}