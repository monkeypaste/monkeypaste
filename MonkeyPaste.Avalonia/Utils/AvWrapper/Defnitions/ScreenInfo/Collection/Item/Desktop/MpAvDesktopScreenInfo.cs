
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvDesktopScreenInfo : MpAvScreenInfoBase {

        public MpAvDesktopScreenInfo() : base() { }

        public MpAvDesktopScreenInfo(Screen s) : this(s, 0) { }
        public MpAvDesktopScreenInfo(Screen s, int idx) {
            Name = $"Monitor {idx}";
            Scaling = s.Scaling;
            Bounds = s.Bounds.ToPortableRect(Scaling);
            WorkArea = s.WorkingArea.ToPortableRect(Scaling);
            IsPrimary = s.IsPrimary;
        }
    }
}
