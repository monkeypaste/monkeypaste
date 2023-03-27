using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvScreenInfoBase : MpIPlatformScreenInfo {
        public MpRect Bounds { get; set; }
        public MpRect WorkArea { get; set; }
        public double Scaling { get; set; }
        public virtual bool IsPrimary { get; set; }
        public MpPoint PixelsPerInch =>
            new MpPoint(Scaling * 96, Scaling * 96);
        public string Name { get; set; }

        public virtual void Rotate(double angle) {
        }

        public override string ToString() {
            return $"Bounds: '{Bounds}' WorkArea: '{WorkArea}' Scaling: '{Scaling}' IsPrimary: '{IsPrimary}'";
        }
    }
}
