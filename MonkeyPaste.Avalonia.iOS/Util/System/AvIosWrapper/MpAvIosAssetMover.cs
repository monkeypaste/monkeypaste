using Avalonia.Platform;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace MonkeyPaste.Avalonia.iOS {
    public static class MpAvIosAssetMover {
        static bool IGNORE_OVERWRITE =
#if DEBUG
            true;
#else
            true;
#endif
        public static void MoveDats() {
            var sw = Stopwatch.StartNew();
            //MovePlugins();
            MoveResources();
            Console.WriteLine($"Total asset move time: {sw.ElapsedMilliseconds}ms");
        }
        private static void MoveResources() {
            //string editor_zip_path = "avares://iosTest/Assets/dat/Editor.zip";
            //string dest_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Resources");
            //UnzipAsset(editor_zip_path, dest_dir);

            var pi = MpAvDeviceWrapper.Instance.PlatformInfo;
            var asset_lookup = new Dictionary<string, string>() {
                { "editor.zip", Path.GetDirectoryName(pi.EditorPath) },
                { "terms.zip", Path.GetDirectoryName(pi.TermsPath) },
                { "enums.zip", Path.GetDirectoryName(pi.EnumsPath) },
                { "uistrings.zip", Path.GetDirectoryName(pi.UiStringsPath) },
            };

            foreach (var asset_kvp in asset_lookup) {
                UnzipDatAsset(asset_kvp.Key, asset_kvp.Value);
            }
        }

        private static void MovePlugins() {
            string core_dat_dir = Path.Combine(Path.GetDirectoryName(typeof(MpAvIosAssetMover).Assembly.Location), "Assets", "dat");
            if (core_dat_dir.IsDirectory()) {
                // already moved
                if (IGNORE_OVERWRITE) {
                    // release 
                    return;
                }
                // remove it
                MpFileIo.DeleteDirectory(core_dat_dir);
            }
            // create dat dir in <local storage>/files/dat
            if (!MpFileIo.CreateDirectory(core_dat_dir)) {
                MpDebug.Break($"Error could not create CoreDatDir at '{core_dat_dir}'");
                return;
            }

            // move all plugin zips to newly created dat dir
            foreach (var core_plugin_guid in MpPluginLoader.CorePluginGuids) {
                string cpg_fn = $"{core_plugin_guid}.zip";
                string target_cpg_path = Path.Combine(core_dat_dir, cpg_fn);
                UnzipDatAsset(cpg_fn, target_cpg_path);
            }
        }


        private static void UnzipDatAsset(string assetName, string targetDir) {
            if (Directory.Exists(targetDir)) {
                if (IGNORE_OVERWRITE) {
                    return;
                }
                Directory.Delete(targetDir);
            }
            Directory.CreateDirectory(targetDir);

            string asset_uri = $"avares://MonkeyPaste.Avalonia.iOS/Assets/dat/{assetName}";
            using (var fileAssetStream = AssetLoader.Open(new Uri(asset_uri))) {
                fileAssetStream.Seek(0, SeekOrigin.Begin);
                using (var ms = new MemoryStream()) {
                    fileAssetStream.CopyTo(ms);
                    using (ZipArchive archive = new ZipArchive(ms)) {
                        archive.ExtractToDirectory(targetDir);
                    }
                }                
            }
        }
    }
}
