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
