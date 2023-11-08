using Android.App;
using Android.Content.Res;
using Avalonia.Input.Platform;
using MonkeyPaste.Common;
using System.IO;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdWrapper : MpDeviceWrapper {

        #region Interfaces

        #region MpIDeviceWrapper Implementation
        public override MpIPlatformInfo PlatformInfo { get; set; }
        public override MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }
        public override MpIIconBuilder IconBuilder { get; set; }
        public override IClipboard DeviceClipboard { get; set; }
        #endregion


        #endregion

        #region Public Methods

        public override void CreateDeviceInstance(object args) {
            if (args is not Activity activity) {
                return;
            }
            PlatformInfo = new MpAvAdPlatformInfo();
            ScreenInfoCollection = new MpAvAdScreenInfoCollection(new[] { new MpAvAdScreenInfo(activity) });
            IconBuilder = new MpAvAdIconBuilder();
            DeviceClipboard = new MpAvAdClipboard();
            MoveDats(activity.Assets);

            _instance = this;
        }

        private void MoveDats(AssetManager am) {
            string core_dat_dir = Path.Combine(PlatformInfo.ExecutingDir, MpPluginLoader.DAT_FOLDER_NAME);
            if (core_dat_dir.IsDirectory()) {
                // already moved
                return;
            }
            // create dat dir in plugin dir
            if (!MpFileIo.CreateDirectory(core_dat_dir)) {
                MpDebug.Break($"Error could not create CoreDatDir at '{core_dat_dir}'");
                return;
            }
            foreach (var core_plugin_guid in MpPluginLoader.CorePluginGuids) {
                string cpg_fn = $"{core_plugin_guid}.zip";
                string source_cpg_path = Path.Combine("dat", cpg_fn);

                using (var fileAssetStream = am.Open(source_cpg_path)) {
                    string target_cpg_path = Path.Combine(core_dat_dir, cpg_fn);
                    using (var fileStream = new FileStream(target_cpg_path, FileMode.OpenOrCreate)) {
                        copyInToLocationAsync(fileAssetStream, fileStream);
                        MpConsole.WriteLine($"Core plugin moved from '{source_cpg_path}' to '{target_cpg_path}' complete");
                    }
                }

            }
        }
        private void copyInToLocationAsync(Stream fileAssetStream, Stream fileStream) {
            int buff_len = 1024;
            byte[] buffer = new byte[buff_len];
            int length;
            while ((length = fileAssetStream.Read(buffer, 0, buff_len)) > 0) {
                fileStream.Write(buffer, 0, length);
            }
            fileStream.Flush();
            fileStream.Close();
            fileAssetStream.Close();
        }
        #endregion
    }
}
