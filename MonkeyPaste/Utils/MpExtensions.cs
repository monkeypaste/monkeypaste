using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using static Xamarin.Forms.Internals.Profile;

namespace MonkeyPaste {
    public static class MpExtensions {
        #region Collections

        public static bool AddOrReplace<TKey, TValue>(this Dictionary<TKey,TValue> d,TKey key, TValue value) {
            //returns true if kvp was added
            //returns false if kvp was replaced
            if(d.ContainsKey(key)) {
                d[key] = value;
                return false;
            }
            d.Add(key, value);
            return true;
        }

        public static List<T> GetRange<T>(this ObservableCollection<T> collection, int startIdx, int count) {
            if (count == 0 && startIdx + count > 0) {
                throw new Exception("Collection empty");
            }
            if (startIdx < 0 || startIdx >= collection.Count()) { 
                throw new Exception($"startIdx {startIdx} is greater than collection {collection.Count()}");
            }
            if (startIdx + count >= collection.Count()) {
                count = collection.Count() - startIdx;
            }
            int i = 0;
            var outList = new List<T>();
            while (count > 0) {
                outList.Add(collection.ElementAt(startIdx + i));
                i++;
                count--;
            }
            return outList;
        }

        public static IOrderedEnumerable<TSource> OrderByDynamic<TSource, TKey>(this IEnumerable<TSource> source, bool isDescending, Func<TSource, TKey> keySelector) {
            if (isDescending) {
                return source.OrderByDescending<TSource, TKey>(keySelector);
            } else {
                return source.OrderBy<TSource, TKey>(keySelector);
            }
        }

        public static IOrderedEnumerable<TSource> OrderByDynamic<TSource, TKey>(this IEnumerable<TSource> source, bool isDescending, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (isDescending) {
                return source.OrderByDescending<TSource, TKey>(keySelector,comparer);
            } else {
                return source.OrderBy<TSource, TKey>(keySelector,comparer);
            }
        }

        public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> source, IEnumerable<TKey> keys, Func<TSource, TKey> keySelector) => source.Join(keys, keySelector, id => id, (o, id) => o);
        
        public static void RemoveRange<TSource>(this IList<TSource> collection, int startIdx, int count) where TSource: class {
            if(count == 0) {
                return;
            }
            if(startIdx < 0 || startIdx >= collection.Count()) {
                return;
            }
            if(startIdx + count >= collection.Count()) {
                count = collection.Count() - startIdx;
            }
            while(count > 0) {
                collection.RemoveAt(startIdx);
                count--;
            }
        }

        public static void AddRange<TSource>(this IList<TSource> collection, int idx, IList<TSource> itemsToAdd) where TSource : class {
            if (idx >= collection.Count()) {
                throw new IndexOutOfRangeException($"Idx: {idx} out of range {collection.Count()}");
            }
            if(idx < 0) {
                idx = Math.Max(0,collection.Count - 1);
            }
            itemsToAdd.Reverse();
            foreach(var item in itemsToAdd) {
                collection.Insert(idx, item);
            }
        }
        #endregion

        #region Enums

        public static string[] EnumToLabels(this Type e, string noneText = "") {
            if(!e.IsEnum) {
                throw new Exception($"{e.ToString()} is not enum type");
            }
            var names = Enum.GetNames(e);
            for (int i = 0; i < names.Length; i++) {
                names[i] = names[i].ToLabel(noneText);
            }
            return names;
        }

        public static string EnumToLabel<TValue>(this TValue value)
            where TValue : Enum {
            string valStr = nameof(value);
            var valParts = valStr.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            return valParts[valParts.Length - 1].ToLabel();
        }

        public static int EnumToInt<TValue>(this TValue value) 
            where TValue : Enum => Convert.ToInt32(value);

        #endregion

        #region Strings

        public static string ToLabel(this string titleCaseStr, string noneText = "") {
            // TODO when automating UI language need to parameterize low vs up case logic
            //Converts 'ThisIsALabel" to 'This Is A Label'
            string outStr = string.Empty;
            for (int i = 0; i < titleCaseStr.Length; i++) {                
                if(i > 0 && titleCaseStr[i-1] > 'Z' && titleCaseStr[i] <= 'Z') {
                    outStr += " ";
                }
                outStr += titleCaseStr[i];
            }
            if(outStr.ToLower() == "none") {
                return noneText;
            }
            return outStr;
        }

        public static string ToReadableTimeSpan(this DateTime dt) {
            int totalYears, totalMonths, totalWeeks, totalDays, totalMinutes;

            var ts = DateTime.Now - dt;
            string outStr = string.Empty;
            totalYears = (int)(ts.TotalDays / 365);
            totalMonths = DateTime.Now.MonthDifference(dt);
            totalWeeks = DateTime.Now.WeekDifference(dt);
            totalDays = (int)ts.TotalDays;
            totalMinutes = (int)ts.TotalMinutes;

            if(totalYears > 1) {
                return string.Format($"{totalYears} years ago");
            }
            if(totalMonths >= 1) {
                return string.Format($"{totalMonths} month{(totalMonths == 1 ? string.Empty:"s")} ago");
            }
            if(totalWeeks >= 1) {
                return string.Format($"{totalWeeks} week{(totalWeeks == 1 ? string.Empty : "s")} ago");
            }
            if (totalDays >= 1) {
                return string.Format($"{totalDays} day{(totalDays == 1 ? string.Empty : "s")} ago");
            }
            if (totalMinutes >= 1) {
                return string.Format($"{totalMinutes} minute{(totalMinutes == 1 ? string.Empty : "s")} ago");
            }
            return "Less than a minute ago";
        }

        public static int WeekDifference(this DateTime lValue, DateTime rValue) {
            double weeks = (lValue - rValue).TotalDays / 7;
            return (int)weeks;
        }

        public static int MonthDifference(this DateTime lValue, DateTime rValue) {
            return (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
        }

        public static string ToTitleCase(this string str) {
            TextInfo textInfo = new CultureInfo(MpPreferences.Instance.UserCultureInfoName, false).TextInfo;
            return textInfo.ToTitleCase(str);
        }

        public static List<int> IndexListOfAll(this string str, string compareStr) {
            return MpHelpers.Instance.IndexListOfAll(str, compareStr);
        }


        public static bool ContainsByCaseOrRegexSetting(this string str, string compareStr) {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(compareStr)) {
                return false;
            }
            if(MpPreferences.Instance.SearchByRegex) {
                return Regex.IsMatch(str, compareStr);
            }
            return str.ContainsByCase(compareStr, MpPreferences.Instance.SearchByIsCaseSensitive);
        }

        public static bool ContainsByCase(this string str, string compareStr, bool isCaseSensitive) {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(compareStr)) {
                return false;
            }
            if (isCaseSensitive) {
                return str.Contains(compareStr);
            }
            return str.ToLowerInvariant().Contains(compareStr.ToLowerInvariant());
        }


        public static async Task<string> CheckSum(this string str) {
            string result = await MpHelpers.Instance.GetCheckSum(str);
            return result;
        }

        public static bool IsStringHexColor(this string text) {
            //if (!text.StartsWith("#")) {
            // enforce that hex colors start with '#'
            //    text = "#" + text;
            //}
            return MpRegEx.Instance.IsMatch(MpSubTextTokenType.HexColor6, text) ||
                   MpRegEx.Instance.IsMatch(MpSubTextTokenType.HexColor8, text);
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

        public static bool IsStringQuillText(this string text) {
            return MpHelpers.Instance.IsStringQuillText(text);
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

        public static SKColor ToGrayScale(this SKColor c) {
            // from https://stackoverflow.com/a/3968341/105028
            byte intensity = (byte)((double)c.Blue * 0.11 + (double)c.Green * 0.59 + (double)c.Red * 0.3);
            return new SKColor(intensity, intensity, intensity);
        }

        public static int ColorDistance(this SKColor a, SKColor b) {
            // from https://stackoverflow.com/a/3968341/105028

            byte a_intensity = a.ToGrayScale().Red;
            byte b_intensity = b.ToGrayScale().Red;
            return (int)(((a_intensity - b_intensity) * 100) / 255);
        }

        public static SKColor ToSkColor(this string hexColor) {
            return Color.FromHex(hexColor).ToSKColor();
        }

        public static SKColor ToSKColor(this Color c) {
            return new SKColor((byte)(c.R * 255), (byte)(c.G * 255), (byte)(c.B * 255),(byte)(c.A * 255));
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

        #region Data

        public static DateTime ToDateTime(this double unixTimeStamp) {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        #endregion

        #region Reflection

        public static async Task<T> InvokeAsync<T>(this MethodInfo @this, object obj, params object[] parameters) {
            dynamic awaitable = @this.Invoke(obj, parameters);
            await awaitable;
            return (T)awaitable.GetAwaiter().GetResult();
        }

        public static async Task InvokeAsync(this MethodInfo @this, object obj, params object[] parameters) {
            dynamic awaitable = @this.Invoke(obj, parameters);
            await awaitable;
        }

        public static object GetPropertyValue(this object obj, string propertyPath, object[] index = null) {
            object propObj = obj;
            PropertyInfo propInfo = null;
            var propPathParts = propertyPath.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < propPathParts.Length; i++) {
                string propPathPart = propPathParts[i];

                if (propObj == null) {
                    throw new Exception($"Child Object {propPathPart} on path {propertyPath} not found on object: {obj.GetType()}");
                }
                Type objType = propObj.GetType();
                propInfo = objType.GetProperty(propPathPart);
                if (propObj == null) {
                    throw new Exception($"Property {propPathPart} not found on object: {propObj.GetType()}");
                }
                propInfo.GetValue(propObj);

                if (i < propPathParts.Length - 1) {
                    propObj = propInfo.GetValue(propObj);
                }
            }
            return propInfo.GetValue(propObj, index);
        }

        public static T GetPropertyValue<T>(this object obj, string propertyPath, object[] index = null) 
            where T : class {
            return obj.GetPropertyValue(propertyPath, index) as T;
        }

        #endregion
    }
}
