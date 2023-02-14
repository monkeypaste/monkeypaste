using Avalonia.Controls.Platform;
using MonoMac.AppKit;
using System;

namespace MonkeyPaste.Common.Avalonia {
    internal class MpAvMacViewHandle : INativeControlHostDestroyableControlHandle {
        private NSView _view;

        public MpAvMacViewHandle(NSView view) {
            _view = view;
        }

        public IntPtr Handle => _view?.Handle ?? IntPtr.Zero;
        public string HandleDescriptor => "NSView";

        public void Destroy() {
            _view.Dispose();
            _view = null;
        }
    }
}
