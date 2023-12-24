
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Avalonia {
    public class MpAvDesktopScreenInfo : MpAvScreenInfoBase {

#if WINDOWS
        public override MpRect WorkArea {
            get => base.WorkArea;
            set => base.WorkArea = value;
        }
#endif
        public MpAvDesktopScreenInfo() : base() { }

        public MpAvDesktopScreenInfo(Screen s) : this(s, 0) { }
        public MpAvDesktopScreenInfo(Screen s, int idx) {
            Scaling = s.Scaling;
            Bounds = s.Bounds.ToPortableRect(Scaling);
            WorkArea = s.WorkingArea.ToPortableRect(Scaling);
            IsPrimary = s.IsPrimary;
        }


    }
}
