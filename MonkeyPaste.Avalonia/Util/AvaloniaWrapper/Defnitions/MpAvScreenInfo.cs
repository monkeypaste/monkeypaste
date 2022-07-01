using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvScreenInfo : MpIPlatformScreenInfo {
        public MpRect Bounds { get; set; }
        public MpRect WorkArea { get; set; }
        public bool IsPrimary { get; set; }
        public string Name { get; set; }
    }
}
