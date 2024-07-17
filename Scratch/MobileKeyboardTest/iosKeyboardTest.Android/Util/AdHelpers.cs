using Android.Graphics;
using Avalonia.Media;
using Color = Android.Graphics.Color;

namespace iosKeyboardTest.Android {
    public static class AdHelpers {
        public static Color ToAdColor(this SolidColorBrush scb) {
            return new(scb.Color.R, scb.Color.G, scb.Color.B, scb.Color.A);
        }
        public static float UnscaledF(this double d) {
            return (float)(d * AndroidDisplayInfo.Scaling);
        }
        public static int UnscaledI(this double d) {
            return (int)(d * AndroidDisplayInfo.Scaling);
        }
    }
}
