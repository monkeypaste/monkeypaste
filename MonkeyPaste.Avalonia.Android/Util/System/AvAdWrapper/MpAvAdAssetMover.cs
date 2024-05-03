using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace MonkeyPaste.Avalonia.Android {
    public static class MpAvAdAssetMover {
        static bool IGNORE_OVERWRITE =
#if DEBUG
            false;
#else
            true;
#endif
        public static void MoveDats() {
            var sw = Stopwatch.StartNew();
            MovePlugins();
            MoveResources();
            MpConsole.WriteLine($"Total asset move time: {sw.ElapsedMilliseconds}ms");
        }
        private static void MovePlugins() {
            string core_dat_dir = Path.Combine(MpDeviceWrapper.Instance.PlatformInfo.ExecutingDir, MpPluginLoader.DAT_FOLDER_NAME);
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
                string source_cpg_path = Path.Combine("dat", cpg_fn);

                using (var fileAssetStream = MainActivity.Instance.Assets.Open(source_cpg_path)) {
                    string target_cpg_path = Path.Combine(core_dat_dir, cpg_fn);
                    using (var fileStream = new FileStream(target_cpg_path, FileMode.OpenOrCreate)) {
                        CopyStream(fileAssetStream, fileStream);
                        MpConsole.WriteLine($"Core plugin moved from '{source_cpg_path}' to '{target_cpg_path}' complete");
                    }
                }

            }
        }

        private static void MoveResources() {
            var pi = MpDeviceWrapper.Instance.PlatformInfo;
            var asset_lookup = new Dictionary<string, string>() {
                { Path.Combine("dat","editor.zip"), Path.GetDirectoryName(pi.EditorPath) },
                { Path.Combine("dat","terms.zip"), Path.GetDirectoryName(pi.TermsPath) },
                { Path.Combine("dat","enums.zip"), Path.GetDirectoryName(pi.EnumsPath) },
                { Path.Combine("dat","uistrings.zip"), Path.GetDirectoryName(pi.UiStringsPath) },
            };

            foreach (var asset_kvp in asset_lookup) {
                //if (!IsAssetNewer(asset_kvp.Key, asset_kvp.Value)) {
                //    // no change
                //    continue;
                //}
                UnzipAsset(asset_kvp.Key, asset_kvp.Value);
            }
        }

        private static bool IsAssetNewer(string assetPath, string targetDir) {
            try {
                // BUG can't read file info from assets so ignoring for now
                long dat_ticks = new Java.IO.File(assetPath).LastModified();
                long dir_ticks = MpFileIo.GetDateTimeInfo(targetDir, true, false).Value.Ticks;
                bool result = dat_ticks > dir_ticks;
                MpConsole.WriteLine($"Asset '{assetPath}' mod datetime: {new DateTime(dat_ticks)}", true);
                MpConsole.WriteLine($"Dir '{targetDir}' mod datetime: {new DateTime(dir_ticks)}");
                MpConsole.WriteLine($"Asset is {(result ? "NEWER" : "OLDER")}");
                return result;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error moving asset at path '{assetPath}' to dir '{targetDir}'.", ex);
                return true;
            }
        }

        private static void UnzipAsset(string assetPath, string targetDir) {
            if (targetDir.IsDirectory()) {
                if (IGNORE_OVERWRITE) {
                    return;
                }
                MpFileIo.DeleteDirectory(targetDir);
            }
            MpFileIo.CreateDirectory(targetDir);

            using (var fileAssetStream = MainActivity.Instance.Assets.Open(assetPath)) {
                using (ZipArchive archive = new ZipArchive(fileAssetStream)) {
                    archive.ExtractToDirectory(targetDir);
                }
            }
        }
        private static void CopyStream(Stream source, Stream target) {
            int buff_len = 1024;
            byte[] buffer = new byte[buff_len];
            int length;
            while ((length = source.Read(buffer, 0, buff_len)) > 0) {
                target.Write(buffer, 0, length);
            }
            target.Flush();
            target.Close();
            source.Close();
        }
    }
}
