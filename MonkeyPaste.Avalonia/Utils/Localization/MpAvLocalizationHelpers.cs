using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public static class MpAvLocalizationHelpers {
        public static IEnumerable<CultureInfo> GetAvailableCultures(string dir, string file_name_prefix = default) {
            List<CultureInfo> cl = new List<CultureInfo>();
            var fil = new DirectoryInfo(dir).EnumerateFiles();
            foreach (var fi in fil) {
                if (file_name_prefix != default && !fi.Name.StartsWith(file_name_prefix)) {
                    continue;
                }
                var fn_parts = fi.Name.SplitNoEmpty(".");
                if (fn_parts.Length == 2) {
                    // default
                    cl.Add(CultureInfo.InvariantCulture);
                } else if (fn_parts.Length == 3) {
                    cl.Add(new CultureInfo(fn_parts[1]));
                } else {
                    MpDebug.Break($"Localization error, weird file name: '{fi.Name}'");
                    continue;
                }
            }
            return cl;
        }

        public static string ResolveMissingCulture(string culture_code, string dir, string file_name_prefix = default) {
            CultureInfo closest_info = CultureInfo.InvariantCulture;
            var acl = GetAvailableCultures(dir, file_name_prefix);
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
    }
}
