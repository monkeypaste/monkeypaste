using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpWpfScreenInfo : MpIPlatformScreenInfo {
        
        public MpRect Bounds { get; set; }
        public MpRect WorkArea { get; set; }
        public bool IsPrimary { get; set; }
        public string Name { get; set; }

        public double PixelDensity { get; set; }
        public MpPoint PixelsPerInch => new MpPoint(PixelDensity * 96, PixelDensity * 96);
        public MpWpfScreenInfo(System.Windows.Forms.Screen screen) { 
            Bounds = screen.Bounds.ToPortableRect();
            WorkArea = screen.WorkingArea.ToPortableRect();
            IsPrimary = screen.Primary;
            Name = screen.DeviceName;
        }
    }
}