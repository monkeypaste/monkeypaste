using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvScreenInfoBase : MpIPlatformScreenInfo {
        public virtual MpRect Bounds { get; set; } = MpRect.Empty;
        public virtual MpRect WorkArea { get; set; } = MpRect.Empty;
        public virtual double Scaling { get; set; }
        public virtual bool IsPrimary { get; set; }
        public virtual void Rotate(double angle) { }

        public override string ToString() {
            return $"Bounds: '{Bounds}' WorkArea: '{WorkArea}' Scaling: '{Scaling}' IsPrimary: '{IsPrimary}'";
        }

        public bool IsEqual(Screen s) {
            if (s == null) {
                return false;
            }
            return Bounds.IsEqual(s.Bounds.ToPortableRect(s.Scaling), 1);
        }
        public bool IsEqual(MpIPlatformScreenInfo other) {
            if (this == other) {
                return true;
            }
            if (other == null) {
                return false;
            }
            if (this.Bounds == null || other.Bounds == null) {
                return false;
            }
            return
                Bounds.IsEqual(other.Bounds) &&
                WorkArea.IsEqual(other.WorkArea) &&
                IsPrimary == other.IsPrimary &&
                Scaling == other.Scaling;
        }
    }
}
