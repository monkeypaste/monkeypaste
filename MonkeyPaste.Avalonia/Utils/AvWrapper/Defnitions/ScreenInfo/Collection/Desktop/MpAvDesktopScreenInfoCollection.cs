using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvDesktopScreenInfoCollection : MpAvScreenInfoCollectionBase {

        public MpAvDesktopScreenInfoCollection(Window w) {
#if MOBILE_OR_WINDOWED
            var b = new MpRect(0, 0, w.Width, w.Height); // this should match windowed-mode class;
            Screens.Add(new MpAvDesktopScreenInfo() {
                Bounds = b,
                WorkingArea = b,
                Scaling = 1.0,
                IsPrimary = true
            }); 
#else
            Screens = new(w.Screens.All.Select(x => new MpAvDesktopScreenInfo(x)));
#endif
        }
    }
}
