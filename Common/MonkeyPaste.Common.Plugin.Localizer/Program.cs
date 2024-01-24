using System.Collections;
using System.Globalization;
using System.Resources.NetStandard;
using System.Text.RegularExpressions;

namespace MonkeyPaste.Common.Plugin.Localizer {
    internal class Program {
        const string RESOURCE_KEY_OPEN_TOKEN = "%";
        const string RESOURCE_KEY_CLOSE_TOKEN = "%";

        static string templated_manifest_path, invariant_resource_path, target_lang_code, output_dir;
        static void Main(string[] args) {
            if (args == null || args.Length < 2) {
                Console.WriteLine("Enter templated manifest path:");
                templated_manifest_path = Console.ReadLine();
                invariant_resource_path = Path.Combine(Path.GetDirectoryName(templated_manifest_path), "Resources", "Resources.resx");
                if (!invariant_resource_path.IsFile()) {
                    Console.WriteLine("Enter invariant resoure path (optional):");
                    invariant_resource_path = Console.ReadLine();
                    Console.WriteLine("Enter output language-code (optional):");
                    target_lang_code = Console.ReadLine();
                    Console.WriteLine("Enter output dir (optional):");
                    output_dir = Console.ReadLine();
                }

            } else {
                templated_manifest_path = args[0];
                invariant_resource_path = args[1];
                target_lang_code = args.Length > 2 ? args[2] : CultureInfo.CurrentCulture.Name;
                output_dir = args.Length > 3 ? args[3] : null;
            }
            output_dir = string.IsNullOrWhiteSpace(output_dir) ? Path.GetDirectoryName(templated_manifest_path) : output_dir;

            output_dir = output_dir.Replace("\"", string.Empty);
            templated_manifest_path = templated_manifest_path.Replace("\"", string.Empty);
            invariant_resource_path = invariant_resource_path.Replace("\"", string.Empty);

            string templated_manifest_json = MpFileIo.ReadTextFromFile(templated_manifest_path);

            var lang_codes = string.IsNullOrWhiteSpace(target_lang_code) ?
                MpLocalizationHelpers.GetAvailableCultures(
                    Path.GetDirectoryName(invariant_resource_path),
                    Path.GetFileNameWithoutExtension(invariant_resource_path))
                .Select(x => x.Name) :
                new string[] { target_lang_code };

            foreach (string lang_code in lang_codes) {
                //LocalizeManifest(templated_manifest, lang_code);
                LocalizeManifest(templated_manifest_json, lang_code);
            }
            Console.WriteLine($"Success");

            Environment.Exit(0);
        }

        private static string LocalizeManifest(string templated_manifest_json, string lang_code) {
            MpPluginFormat localized_manifest = new MpPluginFormat();

            var localized_name_parts = new string[] {
                Path.GetFileNameWithoutExtension(invariant_resource_path),
                lang_code,
                Path.GetExtension(invariant_resource_path).Substring(1) // skip initial '.
            };

            string localized_resource_path = Path.Combine(
                Path.GetDirectoryName(invariant_resource_path),
                string.Join(".", localized_name_parts));

            string resx_path = localized_resource_path.IsFile() ? localized_resource_path : invariant_resource_path;
            using ResXResourceReader resx_reader = new ResXResourceReader(resx_path);
            var mc = Regex.Matches(templated_manifest_json, "%.*%");
            string localized_json = templated_manifest_json;
            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {
                        string key = c.Value.Replace(RESOURCE_KEY_OPEN_TOKEN, string.Empty).Replace(RESOURCE_KEY_CLOSE_TOKEN, string.Empty);
                        string localized_value = GetResourceValue(resx_reader, key) as string;
                        localized_json = localized_json.Replace(c.Value, localized_value);
                    }
                }
            }

            var output_name_parts = new string[] {
                Path.GetFileNameWithoutExtension(templated_manifest_path),
                string.IsNullOrWhiteSpace(lang_code) ? CultureInfo.CurrentCulture.Name : lang_code,
                Path.GetExtension(templated_manifest_path).Substring(1) // skip initial '.'
            };

            string output_path = Path.Combine(
                output_dir,
                string.Join(".", output_name_parts));

            MpFileIo.WriteTextToFile(
                output_path,
                localized_json);
            Console.WriteLine(output_path);
            return output_path;
        }

        private static object GetResourceValue(ResXResourceReader reader, object key) {
            foreach (DictionaryEntry d in reader) {
                if (d.Key.Equals(key)) {
                    return d.Value;
                }
            }
            return null;
        }

    }
}
