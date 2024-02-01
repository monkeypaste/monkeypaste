using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MonkeyPaste.Common {
    public static class MpLocalizationHelpers {
        public static bool IsInvariant(this CultureInfo ci) {
            return ci != null && ci.Name == string.Empty;
        }
        public static IEnumerable<CultureInfo> FindCulturesInDirectory(string dir, string file_name_prefix = default, string inv_code = "en-US") {
            List<CultureInfo> cl = new List<CultureInfo>();
            var fil = new DirectoryInfo(dir).EnumerateFiles();
            foreach (var fi in fil) {
                if (file_name_prefix != default && !fi.Name.StartsWith(file_name_prefix)) {
                    continue;
                }
                CultureInfo to_add = null;
                var fn_parts = fi.Name.SplitNoEmpty(".");
                if (fn_parts.Length == 2) {
                    // default
                    to_add = CultureInfo.InvariantCulture;
                } else if (fn_parts.Length == 3) {
                    try {
                        to_add = new CultureInfo(fn_parts[1]);
                    }
                    catch (CultureNotFoundException) {
                        continue;
                    }

                } else {
                    MpDebug.Break($"Localization error, weird file name: '{fi.Name}'");
                    continue;
                }
                if (to_add != null) {
                    if (to_add == CultureInfo.InvariantCulture) {
                        to_add = new CultureInfo(inv_code);
                    }
                    if (cl.All(x => x.Name != to_add.Name)) {
                        // ensure not to dup inv
                        cl.Add(to_add);
                    }
                }
            }
            return cl;
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

        public static string FindClosestCultureCode(string target_culture_code, string dir, string file_name_prefix = default, string inv_code = "en-US") {
            CultureInfo closest_info = CultureInfo.InvariantCulture;
            var acl = FindCulturesInDirectory(dir, file_name_prefix, inv_code);
            foreach (var ac in acl) {
                if (GetSelfOrAncestorByCode(ac, target_culture_code) is CultureInfo match) {
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
