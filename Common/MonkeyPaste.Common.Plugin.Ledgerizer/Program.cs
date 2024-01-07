using MonkeyPaste.Avalonia;
using System.Diagnostics;
using System.IO.Compression;

namespace MonkeyPaste.Common.Plugin.Ledgerizer {
    internal class Program {

        static bool LEDGERIZE_LOCAL = true;

        const string BUILD_CONFIG =
#if DEBUG
            "Debug";
#else
            "Release";
#endif

        const string README_URL_FORMAT = @"https://raw.githubusercontent.com/monkeypaste/{0}/master/README.md";
        const string PROJ_URL_FORMAT = @"https://github.com/monkeypaste/{0}";
        const string PACKAGE_URL_FORMAT = @"https://github.com/monkeypaste/{0}/releases/download/{1}/{1}.zip";
        const string ICON_URL_FORMAT = @"https://raw.githubusercontent.com/monkeypaste/{0}/master/icon.png";

        static string[] PluginNames = [
            "ChatGpt",
            "ComputerVision",
            "FileConverter",
            "ImageAnnotator",
            "MinimalExample",
            "QrCoder",
            "TextToSpeech",
            "TextTranslator",
            "WebSearch"
        ];


        static string LocalLedgerDir = MpLedgerConstants.LOCAL_LEDGER_DIR;

        static string LedgerPath =
            Path.Combine(
                LocalLedgerDir,
                LEDGERIZE_LOCAL ?
                    MpLedgerConstants.LOCAL_LEDGER_NAME :
                    MpLedgerConstants.REMOTE_LEDGER_NAME);

        static string LocalReleasesDir = MpLedgerConstants.LOCAL_RELEASE_DIR;

        static void Main(string[] args) {
            MpFileIo.DeleteDirectory(LocalReleasesDir);

            MpManifestLedger ledger = new MpManifestLedger();
            foreach (var plugin_name in PluginNames) {
                string plugin_manifest_path = Path.Combine(
                        MpCommonHelpers.GetSolutionDir(),
                        "Plugins",
                        plugin_name,
                        "manifest.json");
                string plugin_manifest_text = MpFileIo.ReadTextFromFile(plugin_manifest_path);
                MpManifestFormat plugin_manifest = plugin_manifest_text.DeserializeObject<MpManifestFormat>();

                string plugin_proj_dir = Path.GetDirectoryName(plugin_manifest_path);
                string local_package_uri = PublishToLocalReleases(plugin_proj_dir);
                if (local_package_uri == null) {
                    continue;
                }
                plugin_manifest.packageUrl = local_package_uri;
                ledger.manifests.Add(plugin_manifest);
            }

            string ledger_text = FinishPublish(ledger);
            MpFileIo.WriteTextToFile(LedgerPath, ledger_text);
            Console.WriteLine($"Ledger written to: ");
            Console.WriteLine(LedgerPath);
        }

        static string PublishToLocalReleases(string proj_dir) {
            if (!LocalReleasesDir.IsDirectory()) {
                MpFileIo.CreateDirectory(LocalReleasesDir);
            }
            string publish_dir = Path.Combine(LocalReleasesDir, Path.GetFileName(proj_dir));

            MpFileIo.DeleteDirectory(Path.Combine(proj_dir, "bin"));
            MpFileIo.DeleteDirectory(Path.Combine(proj_dir, "obj"));
            var proc = new Process();
            proc.StartInfo.FileName = @"C:\Program Files\dotnet\dotnet.exe";
            proc.StartInfo.Arguments = $"publish --configuration {BUILD_CONFIG} --output {publish_dir}";
            proc.StartInfo.WorkingDirectory = proj_dir;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            string proc_output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();
            int exit_code = proc.ExitCode;
            proc.Close();
            proc.Dispose();

            if (exit_code != 0) {
                Console.WriteLine("");
                Console.WriteLine($"Error from '{Path.GetFileName(proj_dir)}' exit code '{exit_code}'");
                Console.WriteLine(proc_output);
                Console.WriteLine("");
                return null;
            }

            if (!publish_dir.IsDirectory()) {
                return null;
            }
            string output_path = Path.Combine(LocalReleasesDir, $"{Path.GetFileName(proj_dir)}.zip");
            ZipFile.CreateFromDirectory(publish_dir, output_path, CompressionLevel.Fastest, true);
            MpFileIo.DeleteDirectory(publish_dir);
            Console.WriteLine(output_path + " DONE");

            return output_path.ToFileSystemUriFromPath();
        }

        static string FinishPublish(MpManifestLedger ledger) {
            if (LEDGERIZE_LOCAL) {
                return ledger.SerializeObjectOmitNulls().ToPrettyPrintJson();
            }

            foreach (var manifest in ledger.manifests) {
                string proj_dir = Path.Combine(
                                MpCommonHelpers.GetSolutionDir(),
                                "Plugins",
                                Path.GetFileNameWithoutExtension(manifest.packageUrl.ToPathFromUri()));
                manifest.packageUrl = PushReleaseToGitHub(manifest, proj_dir);
                if (manifest.packageUrl == null) {
                    // didn't upload
                    continue;
                }
                string plugin_name = Path.GetFileName(proj_dir);
                manifest.readmeUrl = string.Format(README_URL_FORMAT, plugin_name);
                manifest.projectUrl = string.Format(PROJ_URL_FORMAT, plugin_name);
                manifest.iconUri = string.Format(ICON_URL_FORMAT, plugin_name);
            }
            return ledger.SerializeObjectOmitNulls().ToPrettyPrintJson();
        }
        static string PushReleaseToGitHub(MpManifestFormat manifest, string proj_dir, string initial_failed_ver = null) {
            string local_package_uri = manifest.packageUrl;
            string version = manifest.version;
            // see this about gh release https://cli.github.com/manual/gh_release_create
            string source_package_path = local_package_uri.ToPathFromUri();
            string target_package_name = $"v{version}";
            string target_package_file_name = $"{target_package_name}.zip";
            string target_package_path = Path.Combine(proj_dir, target_package_file_name);

            MpFileIo.CopyFileOrDirectory(source_package_path, target_package_path, forceOverwrite: true);
            var proc = new Process();
            proc.StartInfo.FileName = @"C:\Program Files\GitHub CLI\gh.exe";
            proc.StartInfo.WorkingDirectory = proj_dir;
            proc.StartInfo.Arguments = $"release create {target_package_name} --latest --generate-notes {target_package_file_name}";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            string proc_output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();
            int exit_code = proc.ExitCode;
            proc.Close();
            proc.Dispose();
            MpFileIo.DeleteFile(target_package_path);

            if (exit_code == 1) {
                // version exist, increment
                if (version.SplitNoEmpty(".") is not { } verParts ||
                    !int.TryParse(verParts.Last(), out int minor_rev)) {
                    Console.WriteLine($"Error bad version for plugin at '{proj_dir}'");
                    return null;
                }
                manifest.version = $"{verParts[0]}.{verParts[1]}.{minor_rev + 1}";

                var new_ver_result = PushReleaseToGitHub(manifest, proj_dir, initial_failed_ver ?? version);
                return new_ver_result;
            } else if (exit_code == 0 && initial_failed_ver != null) {
                // new rev works, update local manifest to match

                // NOTE avoiding full re-write since manifest can be subclass, just replacing version...
                string manifest_json = MpFileIo.ReadTextFromFile(Path.Combine(proj_dir, "manifest.json"));
                string old_ver_json = $"\"version\": \"{initial_failed_ver}\"";
                string new_ver_json = $"\"version\": \"{version}\"";
                if (manifest_json.Contains(old_ver_json)) {
                    manifest_json = manifest_json.Replace(old_ver_json, new_ver_json);
                    MpFileIo.WriteTextToFile(Path.Combine(proj_dir, "manifest.json"), manifest_json);
                } else {
                    Console.WriteLine($"Error! Could not find old ver string '{old_ver_json}' trying to replace with '{new_ver_json}' in plugin '{proj_dir}'");
                }
            }

            if (exit_code != 0) {
                Console.WriteLine("");
                Console.WriteLine($"Error from '{Path.GetFileName(proj_dir)}' exit code '{exit_code}'");
                Console.WriteLine(proc_output);
                Console.WriteLine("");
                return null;
            }

            string github_release_uri = string.Format(PACKAGE_URL_FORMAT, Path.GetFileName(proj_dir), target_package_name);
            Console.WriteLine(github_release_uri + " DONE");
            return github_release_uri;
        }
    }
}
