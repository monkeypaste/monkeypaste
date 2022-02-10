using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using static Xamarin.Forms.Internals.Profile;

namespace MonkeyPaste {
    public static class MpExtensions {
        #region Private Variables

        private static List<string> _resourceNames;
        #endregion
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

        //public static IList<TSource> AddRange<TSource>(this IList<TSource> collection, IList<TSource> itemsToAdd) where TSource : class {
        //    foreach (var item in itemsToAdd) {
        //        collection.Add(item);
        //    }
        //    return collection;
        //}

        //public static IList<TSource> Add<TSource>(this IList<TSource> collection, TSource itemToAdd) where TSource : class {
        //    collection.Add(itemToAdd);
        //    return collection;
        //}

        //public static IList<TSource> InsertRange<TSource>(this IList<TSource> collection, int idx, IList<TSource> itemsToInsert) where TSource : class {
        //    itemsToInsert.Reverse();
        //    foreach (var item in itemsToInsert) {
        //        collection.Insert(idx, item);
        //    }
        //    return collection;
        //}

        //public static IList<TSource> Insert<TSource>(this IList<TSource> collection, int idx, TSource itemToInsert) where TSource : class {
        //    collection.Insert(idx, itemToInsert);
        //    return collection;
        //}

        //public static List<T> ToList<T>(this T obj) where T : class {
        //    return new List<T>() { obj };
        //}

        //public static ObservableCollection<T> ToObservableCollection<T>(this T obj) where T : class {
        //    return new ObservableCollection<T>() { obj };
        //}
        #endregion

        #region Enums

        public static string[] EnumToLabels(this Type e,  string noneText = "", bool hideFirst = false) {
            if(!e.IsEnum) {
                throw new Exception($"{e.ToString()} is not enum type");
            }
            var names = Enum.GetNames(e);
            for (int i = hideFirst ? 1:0; i < names.Length; i++) {
                names[i] = names[i].ToLabel(noneText);
            }
            return names;
        }

        public static string EnumToLabel<TValue>(this TValue value, string noneText = "")
            where TValue : Enum {
            string valStr = value.ToString();
            var valParts = valStr.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            return valParts[valParts.Length - 1].ToLabel(noneText);
        }

        public static int EnumToInt<TValue>(this TValue value) 
            where TValue : Enum => Convert.ToInt32(value);

        #endregion

        #region Strings

        public static bool IsStringResourcePath(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            if(text.StartsWith("pack:")) {
                return true;
            }
            if (_resourceNames == null) {
                //add executing resource names
                _resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames().Select(x=>x.ToLower()).ToList();
                //add shared resource names
                _resourceNames.AddRange(Assembly.GetCallingAssembly().GetManifestResourceNames().Select(x => x.ToLower()));
            }

            return _resourceNames.Contains(text.ToLower());
        }

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
            TextInfo textInfo = new CultureInfo(MpPreferences.UserCultureInfoName, false).TextInfo;
            return textInfo.ToTitleCase(str);
        }

        public static bool ContainsByCaseOrRegexSetting(this string str, string compareStr) {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(compareStr)) {
                return false;
            }
            if(MpPreferences.SearchByRegex) {
                return Regex.IsMatch(str, compareStr);
            }
            return str.ContainsByCase(compareStr, MpPreferences.SearchByIsCaseSensitive);
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
            string result = await Task<string>.Run(() => {
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
                    string hash = BitConverter.ToString(
                        md5.ComputeHash(
                            Encoding.UTF8.GetBytes(str))).Replace("-", String.Empty);
                    return hash;
                }
            });
            return result;
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

        public static bool IsUnsetValue(this object obj) {
            return obj.ToString().Contains("DependencyProperty.UnsetValue");
        }
        public static DateTime ToDateTime(this double unixTimeStamp) {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static void AssertNotNull(this object obj, string message) {
            if(obj == null) {
                MpConsole.WriteLine($"{DateTime.Now} {message} is null");
            }
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
