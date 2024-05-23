using Avalonia.Platform;
using MonkeyPaste.Avalonia;
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
            MoveAssets();
            Console.WriteLine($"Total asset move time: {sw.ElapsedMilliseconds}ms");
        }
        private static void MoveAssets() {
            var pi = MpAvDeviceWrapper.Instance.PlatformInfo;
            var asset_lookup = new Dictionary<string, string>() {
                { "editor.zip", Path.GetDirectoryName(pi.EditorPath) },
                { "legal.zip", Path.GetDirectoryName(pi.TermsPath) },
                { "enums.zip", Path.GetDirectoryName(pi.EnumsPath) },
                { "uistrings.zip", Path.GetDirectoryName(pi.UiStringsPath) },
            };
            string core_dat_dir = Path.Combine(MpAvDeviceWrapper.Instance.PlatformInfo.ExecutingDir, MpPluginLoader.DAT_FOLDER_NAME);
            MpPluginLoader.CorePluginGuids.ForEach(x => asset_lookup.Add($"{x}.zip", core_dat_dir));

            foreach (var asset_kvp in asset_lookup) {
                UnzipDatAsset(asset_kvp.Key, asset_kvp.Value);
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
