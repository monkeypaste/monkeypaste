﻿using System;
using System.Globalization;
using Xamarin.Forms;
namespace MonkeyPaste {
    public static class MpExtensions {
        #region Visual
        public static string GetHexString(this Xamarin.Forms.Color color) {
            var red = (int)(color.R * 255);
            var green = (int)(color.G * 255);
            var blue = (int)(color.B * 255);
            var alpha = (int)(color.A * 255);
            var myHexString =  $"#{red:X2}{green:X2}{blue:X2}{alpha:X2}";
            var hexString = color.ToHex(); 
            return hexString;
        }

        public static Color GetColor(this string hexString) {
            //if(string.IsNullOrEmpty(str)) {
            //    return Color.Red;
            //}
            //str = str.StartsWith(@"#") ? str.Substring(1, str.Length - 1) : str;

            //double r = (double)((int)Convert.ToInt32(str.Substring(0, 2)) / 255);
            //double g = (double)((int)Convert.ToByte(str.Substring(2, 2)) / 255);
            //double b = (double)((int)Convert.ToByte(str.Substring(4, 2)) / 255);

            //replace # occurences
            if (hexString.IndexOf('#') != -1) {
                hexString = hexString.Replace("#", "");
            }


            int r = int.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            int g = int.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            int b = int.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);

            //return Color.FromArgb(r, g, b);
            return Color.FromRgba(r, g, b, 255);
        }
        #endregion
    }
}