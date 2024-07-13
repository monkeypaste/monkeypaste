using Avalonia.Platform;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace MonkeyPaste.Avalonia {
    public static class MpAvAssetMover {
        static bool IGNORE_OVERWRITE =
#if DEBUG
            true;
#else
            true;
#endif

        public static bool IsLoaded { get; private set; }

        public static void MoveAssets() {
            var sw = Stopwatch.StartNew();
            // NOTE this presumes that the outer folder is the root contents of the zip
            string resources_dir = new MpAvPlatformInfoBase().ResourcesDir;
            var asset_lookup = new Dictionary<string, string>() {
                { "Editor", resources_dir },
                { "Legal", resources_dir },
                { "Enums", resources_dir },
                { "UiStrings",resources_dir },
            };
            string plugin_root_dir = MpPluginLoader.PluginRootDir;
            MpPluginLoader.CorePluginGuids.ForEach(x => asset_lookup.Add(x, plugin_root_dir));

            foreach (var asset_kvp in asset_lookup) {
                UnzipDatAsset(asset_kvp.Key, asset_kvp.Value);
            }
            Console.WriteLine($"Total asset move time: {sw.ElapsedMilliseconds}ms");

            IsLoaded = true;
        }

        private static bool UnzipDatAsset(string assetDirName, string targetDir) {
            try {
                string targetAssetDir = Path.Combine(targetDir, assetDirName);
                if (Directory.Exists(targetAssetDir)) {
                    if (IGNORE_OVERWRITE) {
                        return true;
                    }
                    Directory.Delete(targetAssetDir);
                }
                Directory.CreateDirectory(targetAssetDir);

                string asset_uri = $"avares://{typeof(MpAvAssetMover).Namespace}/Assets/dat/{assetDirName}.zip";
                using (var fileAssetStream = AssetLoader.Open(new Uri(asset_uri))) {
                    fileAssetStream.Seek(0, SeekOrigin.Begin);
                    using (var ms = new MemoryStream()) {
                        fileAssetStream.CopyTo(ms);
                        using (ZipArchive archive = new ZipArchive(ms)) {
                            archive.ExtractToDirectory(targetDir);
                            return true;
                        }
                    }
                }
            } catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }
    }
}
