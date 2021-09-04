using System;
using System.Globalization;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace MonkeyPaste {
    public static class MpExtensions {
        #region Strings
        public static bool ContainsByUserSensitivity(this string str, string ostr) {
            if(string.IsNullOrEmpty(str) || string.IsNullOrEmpty(ostr)) {
                return false;
            }
            if(MpPreferences.Instance.IsSearchCaseSensitive) {
                return str.ContainsByUserSensitivity(ostr);
            }
            return str.ToLowerInvariant().Contains(ostr.ToLowerInvariant());
        }

        public static string CheckSum(this string str) {
            return MpHelpers.Instance.GetCheckSum(str);
        }

        public static bool IsBase64String(this string str) {
            try {
                // If no exception is caught, then it is possibly a base64 encoded string
                byte[] data = Convert.FromBase64String(str);
                // The part that checks if the string was properly padded to the
                // correct length was borrowed from d@anish's solution
                return (str.Replace(" ", "").Length % 4 == 0);
            }
            catch {
                // If exception is caught, then it is not a base64 encoded string
                return false;
            }
        }

        public static bool IsStringCsv(this string text) {
            if (string.IsNullOrEmpty(text) || IsStringRichText(text)) {
                return false;
            }
            return text.Contains(",");
        }

        public static bool IsStringRichText(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"{\rtf");
        }

        public static bool IsStringXaml(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=") || text.StartsWith(@"<Span xmlns=");
        }

        public static bool IsStringSpan(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Span xmlns=");
        }

        public static bool IsStringSection(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=");
        }

        public static bool IsStringPlainText(this string text) {
            //returns true for csv
            if (text == null) {
                return false;
            }
            if (text == string.Empty) {
                return true;
            }
            if (IsStringRichText(text) || IsStringSection(text) || IsStringSpan(text) || IsStringXaml(text)) {
                return false;
            }
            return true;
        }
        #endregion

        #region Visual
        /// <summary>
        /// Gets the screen coordinates from top left corner.
        /// </summary>
        /// <returns>The screen coordinates.</returns>
        /// <param name="view">View.</param>
        public static Point GetScreenCoordinates(this VisualElement view) {
            var locationFetcher = DependencyService.Get<MpIUiLocationFetcher>();
            return locationFetcher.GetCoordinates(view);
        }

        public static Rectangle GetScreenRect(this VisualElement view) {
            var origin = view.GetScreenCoordinates();
            return new Rectangle(origin, new Size(view.Width, view.Height));
        }

        public static Point GetScreenPoint(this Point p,VisualElement view) {
            var locationFetcher = DependencyService.Get<MpIUiLocationFetcher>();
            var density = locationFetcher.GetDensity(view);
            return new Point(p.X / density, p.Y / density);
        }

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
