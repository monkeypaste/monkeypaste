using System.Drawing;
using System.Linq;

namespace MonkeyPaste.Common {
    public class MpColor {
        public byte[] Channels { get; set; }

        public byte A {
            get => Channels[0];
            set => Channels[0] = value;
        }

        public byte R {
            get => Channels[1];
            set => Channels[1] = value;
        }

        public byte G {
            get => Channels[2];
            set => Channels[2] = value;
        }

        public byte B {
            get => Channels[3];
            set => Channels[3] = value;
        }

        public MpColor(string hexOrNamedColorStr) {
            string hex = MpSystemColors.ConvertFromString(hexOrNamedColorStr, "#FF000000");
            Channels = MpColorHelpers.GetHexColorBytes(hex);
        }
        public MpColor(byte[] channels) {
            Channels = channels;
        }
        public MpColor(byte r, byte g, byte b) {
            Channels = new byte[] { 255, r, g, b };
        }
        public MpColor(byte a, byte r, byte g, byte b) {
            Channels = new byte[] { a, r, g, b };
        }

        public string ToHex(bool removeAlpha = false) {
            if (Channels == null) {
                return string.Empty;
            }
            if (removeAlpha) {
                return Channels.Skip(1).ToArray().ToHex();
            }
            return Channels.ToHex();
        }

        public double GetHue() {
            Color c = Color.FromArgb(A, R, G, B);
            return (double)c.GetHue();
        }

        public override string ToString() {
            return ToHex();
        }

        public string ToReadableString() {
            return string.Format(@"A:{0} R:{1} G:{2} B:{3}", A, R, G, B);
        }
    }
}
