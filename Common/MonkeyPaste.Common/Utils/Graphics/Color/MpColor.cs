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

        public MpColor(string hexRgbaOrNamedColorStr, string fallback = default) {
            if (hexRgbaOrNamedColorStr == "#FFA020F0") {

            }
            fallback = fallback == default ? "#00000000" : fallback;
            string hex = MpColorHelpers.ParseHexFromString(hexRgbaOrNamedColorStr, fallback);
            Channels = MpColorHelpers.GetHexColorBytes(hex);
        }
        public MpColor(byte[] argb_channels) {
            Channels =
                argb_channels == null ?
                    new byte[] { 0, 0, 0, 0 } :
                    argb_channels.Length == 3 ?
                        new byte[] { 255, argb_channels[0], argb_channels[1], argb_channels[2] } :
                        argb_channels;
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


        public override string ToString() {
            return ToHex();
        }

        public PixelColor ToPixelColor() {
            return new PixelColor() { Alpha = A, Red = R, Green = G, Blue = B };
        }

        public string ToReadableString() {
            return string.Format(@"A:{0} R:{1} G:{2} B:{3}", A, R, G, B);
        }
    }
}
