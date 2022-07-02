using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvScreenInfo : MpIPlatformScreenInfo {
        public MpRect Bounds { get; set; } = new MpRect();
        public MpRect WorkArea { get; set; } = new MpRect();
        public bool IsPrimary { get; set; }
        public string Name { get; set; } = string.Empty;
        public double PixelDensity { get; set; }
    }
}
