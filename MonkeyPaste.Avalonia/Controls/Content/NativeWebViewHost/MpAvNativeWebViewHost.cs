using Avalonia.Controls;
using Avalonia.Platform;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public interface MpAvINativeControl {
        IPlatformHandle CreateControl(IPlatformHandle parent, Func<IPlatformHandle> createDefault, object args);
    }
    [DoNotNotify]
    public class MpAvNativeWebViewHost : NativeControlHost {
        public static MpAvINativeControl Implementation { get; set; }

        static MpAvNativeWebViewHost() {

        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent) {

            return Implementation?.CreateControl(parent, () => base.CreateNativeControlCore(parent), MpAvCefNetApplication.GetEditorPath().ToFileSystemUriFromPath())
                ?? base.CreateNativeControlCore(parent);
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control) {
            base.DestroyNativeControlCore(control);
        }
    }
}
