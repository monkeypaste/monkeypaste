using System.Linq;
using MonkeyPaste;
using System.Collections.Generic;

namespace MpWpfApp {
    public class MpWpfScreenInfoCollection : MpIPlatformScreenInfoCollection {
        public IEnumerable<MpIPlatformScreenInfo> Screens { get; set; }

        public MpWpfScreenInfoCollection() {
            Screens = System.Windows.Forms.Screen.AllScreens.Select(x => new MpWpfScreenInfo(x));
        }

        public double PixelScaling => 1.75;
    }
}