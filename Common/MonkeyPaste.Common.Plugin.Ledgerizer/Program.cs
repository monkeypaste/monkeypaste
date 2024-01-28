using MonkeyPaste.Avalonia;
using MonkeyPaste.Common.Plugin.Localizer;
using System.Diagnostics;
using System.IO.Compression;

namespace MonkeyPaste.Common.Plugin.Ledgerizer {
    internal class Program {
        const string VERSION_PHRASE = "Im the big T pot check me out";
        static string VERSION => "1.0.7.0";

        static bool DO_LOCAL_PACKAGING = false;

        static bool DO_REMOTE_PACKAGING = true;
        static bool FORCE_REPLACE_REMOTE_TAG = true;

        static bool DO_LOCAL_VERSIONS = false;
        static bool DO_REMOTE_VERSIONS = true;

        static bool DO_LOCAL_INDEX = false;
        static bool DO_REMOTE_INDEX = true;


        const string BUILD_CONFIG =
#if DEBUG
            "Debug";
#else
            "Release";
#endif
        const string BUILD_OS =
#if WINDOWS
            "WINDOWS";
#else
            "";
#endif
        const string README_URL_FORMAT = @"https://raw.githubusercontent.com/monkeypaste/{0}/master/README.md";
        const string PROJ_URL_FORMAT = @"https://github.com/monkeypaste/{0}";
        const string ICON_URL_FORMAT = @"https://raw.githubusercontent.com/monkeypaste/{0}/master/icon.png";
        const string PUBLIC_PACKAGE_URL_FORMAT = @"https://github.com/monkeypaste/{0}/releases/download/{1}/{1}.zip";

        const string PRIVATE_PACKAGE_URL_FORMAT = @"https://www.monkeypaste.com/dat/{0}/{1}.zip";

        static string[] PluginNames = [
            "ChatGpt",
            "ComputerVision",
            "CoreAnnotator",
            "CoreOleHandler",
            "FileConverter",
            "ImageAnnotator",
            //"MinimalExample",
            "QrCoder",
            "TextToSpeech",
            "TextTranslator",
            "WebSearch"
        ];

        static string[] PrivatePackagePlugins = [
            "CoreAnnotator",
            "CoreOleHandler",
        ];

        static string[] LedgerIgnoredPlugins = [
            "MinimalExample"
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

        static string ManifestPrefix = "manifest";
        static string ManifestExt = "json";
        static string ManifestFileName = ManifestPrefix + "." + ManifestExt;

        static List<string> CulturesFound { get; set; } = [];
        static void Main(string[] args) {
            Console.WriteLine("Press any key to ledgerize!");
            Console.ReadKey();
            Console.WriteLine("Starting...");

            ProcessAll();

            MpConsole.WriteLine("Done.. press key to finish", true);
            Console.ReadLine();
        }
        static void ProcessAll() {
            if (DO_LOCAL_PACKAGING) {
                PublishLocal();
            }
            if (DO_REMOTE_PACKAGING) {
                PublishRemote();
            }
            if (DO_LOCAL_VERSIONS) {
                UpdateVersions(false);
            }
            if (DO_REMOTE_VERSIONS) {
                UpdateVersions(true);
            }
            if (DO_LOCAL_INDEX) {
                CreateIndex(false);
            }
            if (DO_REMOTE_INDEX) {
                CreateIndex(true);
            }
        }

        #region Localizing
        static void CreateIndex(bool is_remote) {
            MpConsole.WriteLine($"Creating {(is_remote ? "REMOTE" : "LOCAL")} Cultures...", true);
            string inv_code = "";

            List<string> found_cultures = [];
            // find all distinct cultures
            foreach (var plugin_name in PluginNames) {
                string plugin_proj_dir = GetPluginProjDir(plugin_name);
                if (MpLocalizationHelpers.GetAvailableCultures(plugin_proj_dir, file_name_prefix: ManifestPrefix, inv_code: inv_code) is { } cil) {
                    var to_add = cil.Where(x => !found_cultures.Contains(x.Name) && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name);
                    found_cultures.AddRange(to_add);
                }
            }

            // recreate invariant ledger
            var ledger = GetInvLedger(is_remote);

            // create localized ledger for each distinct culture in /Cultures dir
            foreach (string cc in found_cultures) {
                var culture_manifests = new List<MpManifestFormat>();
                foreach (string plugin_name in PluginNames) {
                    // find closest culture for each plugin and create that manifest
                    var culture_manifest = GetLocalizedManifest(plugin_name, cc, inv_code);
                    if (ledger.manifests.FirstOrDefault(x => x.guid == culture_manifest.guid) is { } ledger_manifest) {
                        // use inv ledger packageUrl
                        culture_manifest.publishedAppVersion = VERSION;
                        culture_manifest.packageUrl = ledger_manifest.packageUrl;
                    }

                    culture_manifests.Add(culture_manifest);
                }
                var culture_ledger = new MpManifestLedger() {
                    manifests = culture_manifests
                };
                // save ledger to /Cultures dir
                string culture_ledger_file_name =
                    $"{MpLedgerConstants.LEDGER_PREFIX}{(is_remote ? string.Empty : MpLedgerConstants.LOCAL_SUFFIX)}.{cc}.{MpLedgerConstants.LEDGER_EXT}";
                string culture_ledger_path = Path.Combine(
                    MpLedgerConstants.LOCAL_CULTURES_DIR_URI.ToPathFromUri(),
                    culture_ledger_file_name);
                MpFileIo.WriteTextToFile(culture_ledger_path, culture_ledger.SerializeObject(omitNulls: true).ToPrettyPrintJson());
                MpConsole.WriteLine(culture_ledger_path);
            }

            MpConsole.WriteLine($"Creating {(is_remote ? "REMOTE" : "LOCAL")} index...", true);
            // create index of all written cultures
            string ledger_index_file_name = is_remote ?
                MpLedgerConstants.REMOTE_LEDGER_INDEX_NAME :
                MpLedgerConstants.LOCAL_LEDGER_INDEX_NAME;
            string ledger_index_path = Path.Combine(
                MpLedgerConstants.LEDGER_PROJ_DIR,
                ledger_index_file_name);
            MpFileIo.WriteTextToFile(ledger_index_path, found_cultures.SerializeObject().ToPrettyPrintJson());
            MpConsole.WriteLine(ledger_index_path);
        }

        static MpManifestFormat GetLocalizedManifest(string plugin_name, string culture, string inv_code) {
            string plugin_proj_dir = GetPluginProjDir(plugin_name);
            string resolved_cultre = MpLocalizationHelpers.FindClosestCultureCode(culture, plugin_proj_dir, file_name_prefix: ManifestPrefix, inv_code: inv_code);
            string localized_manifest_path = Path.Combine(
                plugin_proj_dir,
                $"{ManifestPrefix}.{resolved_cultre}.{ManifestExt}").Replace("..", ".");
            MpDebug.Assert(localized_manifest_path.IsFile(), $"ERror can't find manifest {localized_manifest_path}");
            return MpFileIo.ReadTextFromFile(localized_manifest_path).DeserializeObject<MpManifestFormat>();
        }


        #endregion

        #region Packaging
        static void WriteLedger(MpManifestLedger ledger, bool is_remote) {
            // filter any ledger ignored plugins (minimal example)
            //var output_ledger = new MpManifestLedger() {
            //    manifests =
            //}
            string output_path = is_remote ?
                MpLedgerConstants.REMOTE_INV_LEDGER_PATH :
                MpLedgerConstants.LOCAL_INV_LEDGER_PATH;

            MpFileIo.WriteTextToFile(
                    output_path,
                    ledger.SerializeObject(true).ToPrettyPrintJson());
            MpConsole.WriteLine($"{(is_remote ? "REMOTE" : "LOCAL")} ledger written to: {output_path}", true);
        }
        static string PackPlugin(string proj_dir, string guid) {
            // returns zip uri to use for local packageUrl
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
            //string args = $"publish --configuration {BUILD_CONFIG} --output {publish_dir}";
            string args = $"msbuild /p:OutDir={publish_dir} -target:Publish /property:Configuration={BUILD_CONFIG} /property:DefineConstants=AUX%3B{BUILD_OS} -restore";

            (int exit_code, string proc_output) =
                RunProcess(
                    file: "dotnet",
                    dir: proj_dir,
                    args: args);

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

        static void PublishLocal() {
            MpFileIo.DeleteDirectory(MpLedgerConstants.PLUGIN_PACKAGES_DIR);

            MpManifestLedger ledger = new MpManifestLedger();
            foreach (var plugin_name in PluginNames) {
                string plugin_proj_dir = GetPluginProjDir(plugin_name);
                string plugin_manifest_path = Path.Combine(
                        plugin_proj_dir,
                        ManifestFileName);

                string plugin_manifest_text = MpFileIo.ReadTextFromFile(plugin_manifest_path);
                MpManifestFormat plugin_manifest = plugin_manifest_text.DeserializeObject<MpManifestFormat>();

                string local_package_uri = PackPlugin(plugin_proj_dir, plugin_manifest.guid);
                if (local_package_uri == null) {
                    continue;
                }
                plugin_manifest.publishedAppVersion = VERSION;
                plugin_manifest.packageUrl = local_package_uri;
                ledger.manifests.Add(plugin_manifest);
            }
            // write ledger-local.js
            WriteLedger(ledger, false);
        }
        static void PublishRemote() {
            // returns the complete remote ledger

            var ledger = GetInvLedger(true);
            foreach (var manifest in ledger.manifests) {
                string proj_dir = Path.Combine(
                                MpCommonHelpers.GetSolutionDir(),
                                "Plugins",
                                Path.GetFileNameWithoutExtension(manifest.packageUrl.ToPathFromUri()));
                manifest.packageUrl = PushReleaseToRemote(manifest, proj_dir);
                if (manifest.packageUrl == null) {
                    // didn't upload
                    continue;
                }
                string plugin_name = Path.GetFileName(proj_dir);
                manifest.readmeUrl = string.Format(README_URL_FORMAT, plugin_name);
                manifest.projectUrl = string.Format(PROJ_URL_FORMAT, plugin_name);
                manifest.iconUri = string.Format(ICON_URL_FORMAT, plugin_name);
            }

            WriteLedger(ledger, true);
        }
        static string PushReleaseToRemote(MpManifestFormat manifest, string proj_dir, string initial_failed_ver = null) {
            string plugin_name = Path.GetFileName(proj_dir);
            string local_package_uri = manifest.packageUrl;
            string version = manifest.version;
            // see this about gh release https://cli.github.com/manual/gh_release_create
            string source_package_path = local_package_uri.ToPathFromUri();
            string target_tag_name = $"v{version}";
            string target_package_file_name = $"{target_tag_name}.zip";
            string target_package_path = Path.Combine(proj_dir, target_package_file_name);

            if (PrivatePackagePlugins.Contains(plugin_name)) {
                // TODO would be nice to be able to ssh onto server and push core plugins
                // but for now must be handled manually
                return GetRemotePackageUrl(plugin_name, manifest.guid, target_tag_name);
            }

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
                var new_ver_result = PushReleaseToRemote(manifest, proj_dir, initial_failed_ver ?? version);
                return new_ver_result;

            } else if (exit_code == 0 && initial_failed_ver != null) {
                // new rev works, update local manifest to match

                // NOTE avoiding full re-write since manifest can be subclass, just replacing version...
                string manifest_json = MpFileIo.ReadTextFromFile(Path.Combine(proj_dir, ManifestFileName));
                string old_ver_json = $"\"version\": \"{initial_failed_ver}\"";
                string new_ver_json = $"\"version\": \"{version}\"";
                if (manifest_json.Contains(old_ver_json)) {
                    manifest_json = manifest_json.Replace(old_ver_json, new_ver_json);
                    MpFileIo.WriteTextToFile(Path.Combine(proj_dir, ManifestFileName), manifest_json);
                } else {
                    MpConsole.WriteLine($"Error! Could not find old ver string '{old_ver_json}' trying to replace with '{new_ver_json}' in plugin '{proj_dir}'");
                }
            }

            if (exit_code != 0) {
                MpConsole.WriteLine($"Error from '{plugin_name}' exit code '{exit_code}'", true);
                MpConsole.WriteLine(proc_output, false, true);
                return null;
            }

            string github_release_uri = string.Format(PUBLIC_PACKAGE_URL_FORMAT, plugin_name, target_tag_name);
            MpConsole.WriteLine($"{plugin_name} remote DONE");
            return github_release_uri;
        }

        static string GetRemotePackageUrl(string plugin_name, string plugin_guid, string target_tag_name) {
            if (PrivatePackagePlugins.Contains(plugin_name)) {
                return string.Format(PRIVATE_PACKAGE_URL_FORMAT, plugin_guid, target_tag_name);
            }
            return string.Format(PUBLIC_PACKAGE_URL_FORMAT, plugin_name, target_tag_name);
        }
        #endregion

        #region Version

        static void UpdateVersions(bool is_remote) {
            MpManifestLedger ledger = GetInvLedger(is_remote);
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
        #endregion

        #region Helpers
        static MpManifestLedger GetInvLedger(bool is_remote) {
            string inv_ledger_path = Path.Combine(
                MpLedgerConstants.LEDGER_PROJ_DIR,
                is_remote ?
                MpLedgerConstants.REMOTE_LEDGER_NAME :
                MpLedgerConstants.LOCAL_LEDGER_NAME);
            return MpFileIo.ReadTextFromFile(inv_ledger_path).DeserializeObject<MpManifestLedger>();
        }
        static string GetPluginProjDir(string plugin_name) {
            return Path.Combine(
                        MpCommonHelpers.GetSolutionDir(),
                        "Plugins",
                        plugin_name);
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
        #endregion
    }
}
