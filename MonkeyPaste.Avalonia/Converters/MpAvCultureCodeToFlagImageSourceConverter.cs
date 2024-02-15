using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvCultureCodeToFlagImageSourceConverter : IValueConverter {
        public static readonly MpAvCultureCodeToFlagImageSourceConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not string cultureStr) {
                return null;
            }
            if (GetFlagKey(new CultureInfo(cultureStr), 0) is not string key ||
                string.IsNullOrEmpty(key)) {
                return null;
            }

            string flags_base_uri = Mp.Services.PlatformResource.GetResource<string>("FlagsBase");
            string source = $"{flags_base_uri}/{key.ToLowerInvariant()}.png";
            MpConsole.WriteLine($"{source}");
            return MpAvIconSourceObjToBitmapConverter.Instance.Convert(source, typeof(Bitmap), parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private static readonly string[] _existingFlags =
    {
        "ad", "ae", "af", "ag", "ai", "al", "am", "an", "ao", "ar", "as", "at", "au", "aw", "ax", "az", "ba", "bb", "bd", "be", "bf",
        "bg", "bh", "bi", "bj", "bm", "bn", "bo", "br", "bs", "bt", "bv", "bw", "by", "bz", "ca", "cc", "cd", "cf", "cg", "ch", "ci",
        "ck", "cl", "cm", "cn", "co", "cr", "cs", "cu", "cv", "cx", "cy", "cz", "de", "dj", "dk", "dm", "do", "dz", "ec", "ee", "eg",
        "eh", "er", "es", "et", "fi", "fj", "fk", "fm", "fo", "fr", "fy", "ga", "gb", "gd", "ge", "gf", "gh", "gi", "gl", "gm",
        "gn", "gp", "gq", "gr", "gs", "gt", "gu", "gw", "gy", "hk", "hm", "hn", "hr", "ht", "hu", "id", "ie", "il", "in", "io", "iq",
        "ir", "is", "it", "jm", "jo", "jp", "ke", "kg", "kh", "ki", "km", "kn", "kp", "kr", "kw", "ky", "kz", "la", "lb", "lc", "li",
        "lk", "lr", "ls", "lt", "lu", "lv", "ly", "ma", "mc", "md", "me", "mg", "mh", "mk", "ml", "mm", "mn", "mo", "mp", "mq", "mr",
        "ms", "mt", "mu", "mv", "mw", "mx", "my", "mz", "na", "nc", "ne", "nf", "ng", "ni", "nl", "no", "np", "nr", "nu", "nz", "om",
        "pa", "pe", "pf", "pg", "ph", "pk", "pl", "pm", "pn", "pr", "ps", "pt", "pw", "py", "qa", "re", "ro", "rs", "ru", "rw", "sa",
        "sb", "sc", "sd", "se", "sg", "sh", "si", "sj", "sk", "sl", "sm", "sn", "so", "sr", "st", "sv", "sy", "sz", "tc", "td", "tf",
        "tg", "th", "tj", "tk", "tl", "tm", "tn", "to", "tr", "tt", "tv", "tw", "tz", "ua", "ug", "um", "us", "uy", "uz", "va", "vc",
        "ve", "vg", "vi", "vn", "vu", "wf", "ws", "ye", "yt", "za", "zm", "zw"
    };
        private static Dictionary<CultureInfo, CultureInfo> _cultureCountryOverrides;
        private static Dictionary<CultureInfo, CultureInfo> cultureCountryOverrides {
            get {
                if (_cultureCountryOverrides == null) {
                    _cultureCountryOverrides =
                        "en=en-US,zh=zh-CN,zh-CHT=zh-CN,zh-HANT=zh-CN,fy=fy,"
            .SplitNoEmpty(",")
            .ToDictionary(x => new CultureInfo(x.SplitNoEmpty("=").First()), x => new CultureInfo(x.SplitNoEmpty("=").Last()));
                }
                return _cultureCountryOverrides;
            }
        }

        private static string GetFlagKey(CultureInfo culture, int recursionCounter) {
            var cultureName = culture.Name;

            if (cultureCountryOverrides.TryGetValue(culture, out var countryOverride)) {
                culture = countryOverride;
                cultureName = culture.Name;
            }

            var cultureParts = cultureName.Split('-');
            if (!cultureParts.Any())
                return null;

            var key = cultureParts.Last();

            if (Array.BinarySearch(_existingFlags, key, StringComparer.OrdinalIgnoreCase) < 0) {
                var bestMatch = culture.GetDescendants()
                    .Select(item => GetFlagKey(item, recursionCounter))
                    .FirstOrDefault(item => item != null);

                if (bestMatch is null && recursionCounter < 3 && !culture.IsNeutralCulture) {
                    return GetFlagKey(culture.Parent, recursionCounter + 1);
                }

                return bestMatch;
            }
            return key;
        }
    }
}
