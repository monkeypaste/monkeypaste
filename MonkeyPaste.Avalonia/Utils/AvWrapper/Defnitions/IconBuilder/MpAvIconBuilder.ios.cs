using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvIconBuilder {

        public string GetPathIconBase64(string path, MpIconSize iconSize) =>
            GetPathIconBase64(path, IntPtr.Zero, iconSize);

        public string GetPathIconBase64(string path, nint handle, MpIconSize iconSize) {
            return MpAvDeviceWrapper.Instance.IconBuilder.GetPathIconBase64(path, handle, iconSize);
        }
    }
}
