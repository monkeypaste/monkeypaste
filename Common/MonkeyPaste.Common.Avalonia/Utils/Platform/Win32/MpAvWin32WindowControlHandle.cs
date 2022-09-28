using System;

using Avalonia.Controls.Platform;
using Avalonia.Platform;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvWin32WindowControlHandle : PlatformHandle, INativeControlHostDestroyableControlHandle {
        public MpAvWin32WindowControlHandle(IntPtr handle, string descriptor) : base(handle, descriptor) {  }

        public void Destroy() {
            _ = WinApi.DestroyWindow(Handle);
        }
    }
}