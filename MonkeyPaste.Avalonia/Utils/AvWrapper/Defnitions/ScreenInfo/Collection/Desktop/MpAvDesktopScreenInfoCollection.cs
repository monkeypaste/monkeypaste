using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvDesktopScreenInfoCollection : MpAvScreenInfoCollectionBase {

        public MpAvDesktopScreenInfoCollection(Window w) {
            var b = 
                w.Bounds.Size.ToPortableSize().IsValueEqual(MpSize.Empty) ?
                new MpRect(0, 0, w.Width,w.Height) : // this should match windowed-mode class
                w.Bounds.ToPortableRect();
            Screens.Add(new MpAvDesktopScreenInfo() {
                Bounds = b,
                WorkingArea = b,
                Scaling = 1.0,
                IsPrimary = true
            });
        }
    }
}
