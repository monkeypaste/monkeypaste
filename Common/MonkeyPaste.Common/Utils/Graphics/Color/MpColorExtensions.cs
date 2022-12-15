using System;

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
            //distance between 0 and 1 (tested by checking between black and white where distance is 1)
            long rmean = ((long)(e1.R) + (long)(e2.R)) / 2;
            long r = (long)(e1.R) - (long)(e2.R);
            long g = (long)(e1.G) - (long)(e2.G);
            long b = (long)(e1.B) - (long)(e2.B);
            double max = 764.83331517396655; // changed last digit from 3 to 5 :)
            double d = Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            return d / max;
        }

        public static MpColor ToPortableColor(this string str) {
            return new MpColor(str);
        }

        public static string ShiftHexColorHue(this string hex, double hue_delta) {
            // hue from 0 - 360
            MpColorHelpers.ColorToHsv(hex.ToPortableColor(), out double h, out double s, out double v);
            double new_hue = h + hue_delta;
            if(new_hue > 360) {
                // wrap hue
                new_hue = new_hue - 360;
            }
            return MpColorHelpers.ColorFromHsv(new_hue, s, v).ToHex();
        }

        public static string ToContrastHexColor(this string hex) {

            return ShiftHexColorHue(hex, 180);
        }
        public static string ToComplementHexColor(this string hex) {
            /*
            from https://stackoverflow.com/a/7261283/105028

            Complement Changes each component of a color to a new value 
            based on the sum of the highest and lowest RGB values in the 
            selected color. Illustrator adds the lowest and highest RGB 
            values of the current color, and then subtracts the value of 
            each component from that number to create new RGB values. 
            For example, suppose you select a color with an RGB 
            value of 102 for red, 153 for green, and 51 for blue. 
            Illustrator adds the high (153) and low (51) values, to 
            end up with a new value (204). Each of the RGB values in the 
            existing color is subtracted from the new value to create new 
            complementary RGB values: 204 – 102 (the current red value) = 102 
            for the new red value, 204 – 153 (the current green value) = 51 
            for the new green value, and 204 – 51 (the current blue value) = 153 
            for the new blue value.
            */
            var c = hex.ToPortableColor();
            // ex 102,153,51
            byte hi = Math.Max(Math.Max(c.R, c.G), c.B); //153
            byte lo = Math.Min(Math.Min(c.R, c.G), c.B); //51
            int hi_lo_sum = (int)hi + (int)lo; //204
            c.R = (byte)(hi_lo_sum - (int)c.R);
            c.G = (byte)(hi_lo_sum - (int)c.G);
            c.B = (byte)(hi_lo_sum - (int)c.B);
            return c.ToHex();
        }
    }
}
