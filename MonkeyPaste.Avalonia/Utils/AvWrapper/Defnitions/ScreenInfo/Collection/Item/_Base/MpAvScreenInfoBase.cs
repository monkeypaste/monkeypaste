using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Windows;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvScreenInfoBase : MpIPlatformScreenInfo {
        public virtual MpRect Bounds { get; set; } = MpRect.Empty;
        public virtual MpRect WorkingArea { get; set; } = MpRect.Empty;
        public virtual double Scaling { get; set; }
        public virtual bool IsPrimary { get; set; }
        public virtual void Rotate(double angle) { }

        public MpAvScreenInfoBase() : this(null) { }
        public MpAvScreenInfoBase(MpIPlatformScreenInfo psi) {
            if(psi == null) {
                return;
            }
            Bounds = psi.Bounds;
            WorkingArea = psi.WorkingArea;
            Scaling = psi.Scaling;
            IsPrimary = psi.IsPrimary;
        }
        public override string ToString() {
            return $"Bounds: '{Bounds}' WorkArea: '{WorkingArea}' Scaling: '{Scaling}' IsPrimary: '{IsPrimary}'";
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
                WorkingArea.IsEqual(other.WorkingArea) &&
                IsPrimary == other.IsPrimary &&
                Scaling == other.Scaling;
        }
    }
}
