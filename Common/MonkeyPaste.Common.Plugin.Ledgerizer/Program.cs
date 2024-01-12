using MonkeyPaste.Avalonia;
using System.Diagnostics;
using System.IO.Compression;

namespace MonkeyPaste.Common.Plugin.Ledgerizer {
    internal class Program {
        const string VERSION_PHRASE = "Im the big T pot check me out";

        static bool DO_LOCAL_PACKAGING = true;

        static bool DO_REMOTE_PACKAGING = false;
        static bool FORCE_REPLACE_REMOTE_TAG = false;

        static bool DO_LOCAL_VERSIONS = false;
        static bool DO_REMOTE_VERSIONS = true;


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

        //ledger-local.json
        static string LocalLedgerPath =
            Path.Combine(
                MpLedgerConstants.LEDGER_PROJ_DIR,
                MpLedgerConstants.LOCAL_LEDGER_NAME);

        //ledger.json
        static string RemoteLedgerPath =
            Path.Combine(
                MpLedgerConstants.LEDGER_PROJ_DIR,
                MpLedgerConstants.REMOTE_LEDGER_NAME);


        static void Main(string[] args) {
            Console.WriteLine("Press any key to ledgerize!");
            Console.ReadKey();
            Console.WriteLine("Starting...");
            if (DO_LOCAL_PACKAGING) {
                MpFileIo.DeleteDirectory(MpLedgerConstants.PLUGIN_PACKAGES_DIR);
            }

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
                string local_package_uri = PackPlugin(plugin_proj_dir, plugin_manifest.guid);
                if (local_package_uri == null) {
                    continue;
                }
                plugin_manifest.packageUrl = local_package_uri;
                ledger.manifests.Add(plugin_manifest);
            }

            if (DO_LOCAL_PACKAGING) {
                // write ledger-local.js
                MpConsole.WriteLine($"Local ledger written to: {MpFileIo.WriteTextToFile(
                    MpLedgerConstants.LOCAL_LEDGER_URI.ToPathFromUri(),
                    ledger.SerializeObject(true).ToPrettyPrintJson())}", true);
            }
            if (DO_REMOTE_PACKAGING) {
                MpConsole.WriteLine($"Remote ledger written to: {MpFileIo.WriteTextToFile(
                    MpLedgerConstants.REMOTE_LEDGER_URI.ToPathFromUri(),
                    PublishRemote(ledger))}", true);
            }
            if (DO_LOCAL_VERSIONS) {
                UpdateVersions(ledger, false);
            }
            if (DO_REMOTE_VERSIONS) {
                UpdateVersions(ledger, true);
            }
            MpConsole.WriteLine("Done.. press key to finish", true);
            Console.ReadLine();
        }

        static string PackPlugin(string proj_dir, string guid) {
            string root_pack_dir = MpLedgerConstants.PLUGIN_PACKAGES_DIR;
            string plugin_name = Path.GetFileName(proj_dir);
            string output_path = Path.Combine(root_pack_dir, $"{plugin_name}.zip");


            if (!root_pack_dir.IsDirectory()) {
                // create packages dir if first pack
                MpFileIo.CreateDirectory(root_pack_dir);
            }
            string publish_dir = Path.Combine(root_pack_dir, plugin_name);

            // delete build stuff
            MpFileIo.DeleteDirectory(Path.Combine(proj_dir, "bin"));
            MpFileIo.DeleteDirectory(Path.Combine(proj_dir, "obj"));

            // perform publish and output to ledger proj/packages_* dir
            (int exit_code, string proc_output) =
                RunProcess(
                    file: "dotnet",
                    dir: proj_dir,
                    args: $"publish --configuration {BUILD_CONFIG} --output {publish_dir}");

            if (exit_code != 0) {
                MpConsole.WriteLine("");
                MpConsole.WriteLine($"Error from '{plugin_name}' exit code '{exit_code}'");
                MpConsole.WriteLine(proc_output);
                MpConsole.WriteLine("");
                return null;
            }

            if (!publish_dir.IsDirectory()) {
                return null;
            }
            // zip publish output 
            ZipFile.CreateFromDirectory(publish_dir, output_path, CompressionLevel.Fastest, true);

            // get plugin install dir
            string plugin_install_dir =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
#if DEBUG
                    "MonkeyPaste_DEBUG",
#else
                    "MonkeyPaste",
#endif
                    "Plugins",
                    guid);
            string install_update_suffix = string.Empty;
            if (plugin_install_dir.IsDirectory()) {
                // if plugin is installed we need to use this build output 
                // at least for debugging but probably in general too
                string inner_install_dir = Path.Combine(plugin_install_dir, plugin_name);
                MpFileIo.DeleteDirectory(inner_install_dir);
                // duplicate just published dir to plugin container dir
                MpFileIo.CreateDirectory(inner_install_dir);
                MpFileIo.CopyDirectory(publish_dir, inner_install_dir);
                install_update_suffix = " install UPDATED";
            }
            // cleanup published output
            MpFileIo.DeleteDirectory(publish_dir);
            MpConsole.WriteLine($"{plugin_name} local DONE" + install_update_suffix);

            // return zip uri to use for local packageUrl
            return output_path.ToFileSystemUriFromPath();
        }

        static string PublishRemote(MpManifestLedger ledger) {
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
            return ledger.SerializeObject(true).ToPrettyPrintJson();
        }
        static string PushReleaseToGitHub(MpManifestFormat manifest, string proj_dir, string initial_failed_ver = null) {
            string plugin_name = Path.GetFileName(proj_dir);
            string local_package_uri = manifest.packageUrl;
            string version = manifest.version;
            // see this about gh release https://cli.github.com/manual/gh_release_create
            string source_package_path = local_package_uri.ToPathFromUri();
            string target_tag_name = $"v{version}";
            string target_package_file_name = $"{target_tag_name}.zip";
            string target_package_path = Path.Combine(proj_dir, target_package_file_name);

            MpFileIo.CopyFileOrDirectory(source_package_path, target_package_path, forceOverwrite: true);
            (int exit_code, string proc_output) = RunProcess(
                file: "gh.exe",
                dir: proj_dir,
                args: $"release create {target_tag_name} --latest --generate-notes {target_package_file_name}");

            MpFileIo.DeleteFile(target_package_path);

            if (exit_code == 1) {
                // version exist
                if (FORCE_REPLACE_REMOTE_TAG) {
                    // delete version, call again
                    if (initial_failed_ver != null) {
                        // should only occur once 
                        MpConsole.WriteLine($"Uncaught error after delete for '{proj_dir}' skipping upload");
                        return null;
                    }
                    (int del_exit_code, string del_proc_output) =
                        RunProcess(
                            file: "gh.exe",
                            dir: proj_dir,
                            args: $"release delete {target_tag_name} -yes --cleanup-tag");
                    if (exit_code != 0) {
                        MpConsole.WriteLine($"Error delete failed exit code {del_exit_code}");
                        return null;
                    }
                } else {
                    // increment, call again
                    if (version.SplitNoEmpty(".") is not { } verParts ||
                        !int.TryParse(verParts.Last(), out int minor_rev)) {
                        MpConsole.WriteLine($"Error bad version for plugin at '{proj_dir}'");
                        return null;
                    }
                    manifest.version = $"{verParts[0]}.{verParts[1]}.{minor_rev + 1}";
                }
                // if first fail use failed version
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
                    MpConsole.WriteLine($"Error! Could not find old ver string '{old_ver_json}' trying to replace with '{new_ver_json}' in plugin '{proj_dir}'");
                }
            }

            if (exit_code != 0) {
                MpConsole.WriteLine($"Error from '{plugin_name}' exit code '{exit_code}'", true);
                MpConsole.WriteLine(proc_output, false, true);
                return null;
            }

            string github_release_uri = string.Format(PACKAGE_URL_FORMAT, plugin_name, target_tag_name);
            MpConsole.WriteLine($"{plugin_name} remote DONE");
            return github_release_uri;
        }

        static void UpdateVersions(MpManifestLedger ledger, bool is_remote) {
            bool is_done = false;

            _ = Task.Run(async () => {
                foreach (var mf in ledger.manifests) {
                    var req_args = new Dictionary<string, string>() {
                        {"plugin_guid", mf.guid },
                        {"version", mf.version},
                        {"is_install", "0" },
                        {"add_phrase", "Im the big T pot check me out" }
                    };
                    string url = is_remote ?
                        $"{MpServerConstants.REMOTE_SERVER_URL}/plugins/plugin-info-check.php" :
                        $"{MpServerConstants.LOCAL_SERVER_URL}/plugins/plugin-info-check.php";

                    var resp = await MpHttpRequester.SubmitPostDataToUrlAsync(url, req_args);
                    bool success = MpHttpRequester.ProcessServerResponse(resp, out var resp_args);
                    MpConsole.WriteLine($"{mf} {success.ToTestResultLabel()} info check resp: {resp}");
                }
                is_done = true;
            });

            while (!is_done) {
                Thread.Sleep(100);
            }

        }
        static (int, string) RunProcess(string file, string dir, string args) {
            var proc = new Process();
            proc.StartInfo.FileName = file;
            proc.StartInfo.WorkingDirectory = dir;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            string proc_output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();
            int exit_code = proc.ExitCode;
            proc.Close();
            proc.Dispose();
            return (exit_code, proc_output);
        }
    }
}
