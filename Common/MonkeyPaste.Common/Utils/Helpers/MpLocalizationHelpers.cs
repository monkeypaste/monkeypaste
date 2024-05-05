using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MonkeyPaste.Common {
    public static class MpLocalizationHelpers {
        public static string GetFlagEmoji(string culture_code) {
            /*
            function getFlagEmoji(countryCode) {
  const codePoints = countryCode
    .toUpperCase()
    .split('')
    .map(char =>  127397 + char.charCodeAt());
  return String.fromCodePoint(...codePoints);
}
            */
            // from https://dev.to/jorik/country-code-to-flag-emoji-a21
            return string.Join(string.Empty,
                culture_code
                .SplitNoEmpty("-")
                .FirstOrDefault()
                .ToUpperInvariant()
                .ToCharArray()
                .Select(x => char.ConvertFromUtf32(0x1f1a5 + (int)x)));
        }
        public static bool IsInvariant(this CultureInfo ci) {
            return ci != null && ci.Name == string.Empty;
        }
        public static IEnumerable<CultureInfo> FindCulturesInDirectory(string dir, string file_name_filter = default, string file_ext_filter = default) {
            List<CultureInfo> cl = new List<CultureInfo>();
            if (!dir.IsDirectory()) {
                return cl;
            }
            var fil = new DirectoryInfo(dir).EnumerateFiles();

            foreach (var fi in fil) {
                if (fi.Name.EndsWith(".Designer.cs")) {
                    continue;
                }
                if (file_name_filter != default && !fi.Name.StartsWith(file_name_filter)) {
                    continue;
                }
                if (file_ext_filter != default && !fi.Extension.EndsWith(file_ext_filter)) {
                    continue;
                }
                if (fi.Name.SplitNoEmpty(".") is not { } fn_parts ||
                    fn_parts.Length <= 2) {
                    // ignore neutral
                    continue;
                }

                try {
                    CultureInfo to_add = new(fn_parts[1]);
                    cl.Add(to_add);
                }
                catch (CultureNotFoundException) {
                    MpConsole.WriteTraceLine($"Error parsing culture from file '{fi.FullName}'");
                    continue;
                }
            }
            return cl;
        }

        public static string FindClosestCultureCode(string target_culture_code, string dir, string file_name_filter = default, string file_ext_filter = default) {
            CultureInfo closest_info = CultureInfo.InvariantCulture;
            var acl = FindCulturesInDirectory(dir, file_name_filter, file_ext_filter);
            foreach (var ac in acl) {
                if (GetSelfOrAncestorByCode(ac, target_culture_code) is CultureInfo match) {
                    closest_info = match;
                    break;
                }
            }
            return closest_info.Name;
        }
        public static string FindClosestCultureCode(string culture_code, string[] cultures) {
            CultureInfo closest_info = CultureInfo.InvariantCulture;
            var acl = cultures.Select(x => new CultureInfo(x));
            foreach (var ac in acl) {
                if (GetSelfOrAncestorByCode(ac, culture_code) is CultureInfo match) {
                    closest_info = match;
                    break;
                }
            }
            return closest_info.Name;
        }

        private static CultureInfo GetSelfOrAncestorByCode(CultureInfo ci, string culture_code) {
            if (ci == null ||
                string.IsNullOrEmpty(culture_code) ||
                string.IsNullOrEmpty(ci.Name)) {
                return null;
            }
            if (ci.Name == culture_code) {
                return ci;
            }
            return GetSelfOrAncestorByCode(ci.Parent, culture_code);
        }
        public static IEnumerable<CultureInfo> GetAncestors(this CultureInfo self) {
            var item = self.Parent;

            while (!string.IsNullOrEmpty(item.Name)) {
                yield return item;
                item = item.Parent;
            }
        }

        /// <summary>
        /// Returns an enumeration of elements that contain this element, and the ancestors of this element.
        /// </summary>
        /// <param name="self">The starting element.</param>
        /// <returns>The ancestor list.</returns>
        public static IEnumerable<CultureInfo> GetAncestorsAndSelf(this CultureInfo self) {
            var item = self;

            while (!string.IsNullOrEmpty(item.Name)) {
                yield return item;
                item = item.Parent;
            }
        }

        /// <summary>
        /// Enumerates the immediate children of the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The immediate children of the specified item.</returns>
        public static ICollection<CultureInfo> GetChildren(this CultureInfo item) {
            return CultureInfo.GetCultures(CultureTypes.AllCultures).Where(child => child?.Parent.Equals(item) == true).ToArray();
        }


        /// <summary>
        /// Enumerates all descendants of the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The descendants of the item.</returns>
        public static IEnumerable<CultureInfo> GetDescendants(this CultureInfo item) {
            foreach (var child in item.GetChildren()) {
                yield return child;

                foreach (var d in child.GetDescendants()) {
                    yield return d;
                }
            }
        }
    }
}
