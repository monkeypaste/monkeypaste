using System.Collections;
using System.Globalization;
using System.Resources.NetStandard;

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
            MpPluginFormat templated_manifest = MpJsonConverter.DeserializeObject<MpPluginFormat>(templated_manifest_json);
            var lang_codes = string.IsNullOrWhiteSpace(target_lang_code) ?
                MpLocalizationHelpers.GetAvailableCultures(
                    Path.GetDirectoryName(invariant_resource_path),
                    Path.GetFileNameWithoutExtension(invariant_resource_path))
                .Select(x => x.Name) :
                new string[] { target_lang_code };
            foreach (string lang_code in lang_codes) {
                LocalizeManifest(templated_manifest, lang_code);
            }
            Console.WriteLine($"Success");

            Environment.Exit(0);
        }

        private static string LocalizeManifest(MpPluginFormat templated_manifest, string lang_code) {
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
            foreach (var pi in typeof(MpPluginFormat).GetProperties()) {
                object localized_value = null;
                if (pi.GetValue(templated_manifest) is not string val ||
                    !val.StartsWith(RESOURCE_KEY_OPEN_TOKEN) ||
                    !val.EndsWith(RESOURCE_KEY_CLOSE_TOKEN)) {
                    // unkeyed or non-string property
                    localized_value = pi.GetValue(templated_manifest);
                } else {
                    // value is "%ResourceKeyName%"
                    string key = val.Replace(RESOURCE_KEY_OPEN_TOKEN, string.Empty).Replace(RESOURCE_KEY_CLOSE_TOKEN, string.Empty);
                    localized_value = GetResourceValue(resx_reader, key);
                }

                localized_manifest.SetPropertyValue(pi.Name, localized_value);
            }


            var output_name_parts = new string[] {
                Path.GetFileNameWithoutExtension(templated_manifest_path),
                string.IsNullOrWhiteSpace(lang_code) ? CultureInfo.CurrentCulture.Name : lang_code,
                Path.GetExtension(templated_manifest_path).Substring(1) // skip initial '.'
            };

            string output_path = Path.Combine(
                output_dir,
                string.Join(".", output_name_parts));

            MpFileIo.WriteTextToFile(output_path, localized_manifest.SerializeJsonObject().ToPrettyPrintJson());
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
