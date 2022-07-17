using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common {
    public static class MpColorExtensions {
        public static string ToHex(this byte[] bytes) {
            if (bytes == null) {
                throw new Exception("Bytes are null");
            }
            return "#" + BitConverter.ToString(bytes).Replace("-", string.Empty);
        }


        public static string AdjustAlpha(this string hexStr, double opacity) {
            // opacity is 0-1
            if (!hexStr.IsStringHexColor()) {
                throw new Exception("Not a hex color");
            }
            var c = new MpColor(hexStr);
            c.A = (byte)(255.0 * opacity);
            return c.ToHex();
        }

        public static double ColorDistance(this MpColor e1, MpColor e2) {
            //max between 0 and 764.83331517396653 (found by checking distance from white to black)
            long rmean = ((long)(e1.R * 255) + (long)(e2.R * 255)) / 2;
            long r = (long)(e1.R * 255) - (long)(e2.R * 255);
            long g = (long)(e1.G * 255) - (long)(e2.G * 255);
            long b = (long)(e1.B * 255) - (long)(e2.B * 255);
            double max = 764.83331517396653;
            double d = Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            return d / max;
        }

        public static MpColor ToPortableColor(this string str) {
            return new MpColor(str);
        }
    }
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

        public MpColor(string hexStr) {
            Channels = MpColorHelpers.GetHexColorBytes(hexStr);
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

        public string ToHex() {
            if (Channels == null) {
                return string.Empty;
            }
            return Channels.ToHex();
        }

        public override string ToString() {
            return ToHex();
        }

        public string ToReadableString() {
            return string.Format(@"A:{0} R:{1} G:{2} B:{3}", A, R, G, B);
        }
    }
}
