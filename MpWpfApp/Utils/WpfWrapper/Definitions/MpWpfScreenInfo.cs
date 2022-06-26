using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpWpfScreenInfo : MpIPlatformScreenInfo {
        
        public MpRect Bounds { get; }
        public MpRect WorkArea { get; }
        public bool IsPrimary { get; }
        public string Name { get; }

        public MpWpfScreenInfo(System.Windows.Forms.Screen screen) { 
            Bounds = screen.Bounds.ToPortableRect();
            WorkArea = screen.WorkingArea.ToPortableRect();
            IsPrimary = screen.Primary;
            Name = screen.DeviceName;
        }
    }
}