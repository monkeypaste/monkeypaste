using ICSharpCode.SharpZipLib.BZip2;
using MonkeyPaste.Common;
using Org.BouncyCastle.Apache.Bzip2;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MonkeyPaste.Avalonia {
    public static class MpAvCefUpdater {
        #region Private Variables
        #endregion

        #region Constants
        public const string CEF_BUILD_BASE_URL = @"https://cef-builds.spotifycdn.com/";
        public const string CEF_BUILD_INDEX_URL = CEF_BUILD_BASE_URL + @"index.json";
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region Model

        public static MpCefVersion ThisPlatformMostRecentVersion {
            get {
                if (ThisPlatformBuild == null ||
                    ThisPlatformBuild.versions == null) {
                    return null;
                }
                return
                    ThisPlatformBuild
                    .versions
                    .Where(x => x.channel == "stable")
                    .OrderByDescending(x => Version.Parse(x.chromium_version))
                    .FirstOrDefault();

            }
        }
        public static MpCefBuildVersion ThisPlatformBuild {
            get {
                if (BuildRoot == null) {
                    return null;
                }
                switch (Mp.Services.PlatformInfo.OsType) {
                    case MpUserDeviceType.Windows:
                        if (Environment.Is64BitProcess) {
                            return BuildRoot.windows64;
                        }
                        return BuildRoot.windows32;
                    default:
                        return null;

                }
            }
        }
        public static MpCefBuildRoot BuildRoot { get; set; }
        #endregion

        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        public static bool IsUpdatePendingInstall() {
            return MpAvCefNetApplication.CefTempUpdateFolderPath.IsFileOrDirectory();
        }

        public static bool CanRestoreBackup() {
            return MpAvCefNetApplication.CefTempUpdateBackupFrameworkPath.IsFileOrDirectory();
        }

        public static bool RestoreBackup() {
            if (!CanRestoreBackup()) {
                return false;
            }

            MpFileIo.DeleteDirectory(MpAvCefNetApplication.CefFrameworkPath);
            try {
                // restore backup 
                MpFileIo.CopyDirectory(
                    MpAvCefNetApplication.CefTempUpdateBackupFrameworkPath,
                    MpAvCefNetApplication.CefFrameworkPath, true, false);
                return true;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Cef install failed. ", ex);
            }
            return false;
        }
        public static bool PerformInstall() {
            if (!IsUpdatePendingInstall()) {
                return false;
            }
            if (MpAvCefNetApplication.CefFrameworkPath.IsDirectory()) {
                // cef_<os> folder exists, backup and delete it
                try {
                    // backup existing cef
                    MpFileIo.CopyDirectory(
                        MpAvCefNetApplication.CefFrameworkPath,
                        MpAvCefNetApplication.CefTempUpdateBackupFolderPath, true, false);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Cef install failed. ", ex);
                    return false;
                }
                MpFileIo.DeleteDirectory(MpAvCefNetApplication.CefFrameworkPath);
            }
            string source_path =
                Path.Combine(
                    MpAvCefNetApplication.CefTempUpdateFolderPath,
                    MpAvCefNetApplication.CefFrameworkFolderName);
            string dest_path = MpAvCefNetApplication.CefRootPath;

            try {
                MpFileIo.CopyDirectory(source_path, dest_path, true, false);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Cef install failed. ", ex);
                return false;
            }
            MpFileIo.DeleteDirectory(MpAvCefNetApplication.CefTempUpdateFolderPath);
            return true;
        }

        public static async Task CheckForCefUpdateAsync() {
            try {

                string cef_build_json = await MpFileIo.ReadTextFromUriAsync(CEF_BUILD_INDEX_URL);
                BuildRoot = MpJsonConverter.DeserializeObject<MpCefBuildRoot>(cef_build_json);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error getting cef info: ", ex);
                return;
            }
            if (ThisPlatformMostRecentVersion == null) {
                return;
            }
            string cur_cef_version = MpAvCefNetApplication.GetCurrentCefVersion();

            MpConsole.WriteLine($"Current Cef: {cur_cef_version}");
            MpConsole.WriteLine($"Available Cef: {ThisPlatformMostRecentVersion.cef_version}");

            if (cur_cef_version.ToLower() == ThisPlatformMostRecentVersion.cef_version.ToLower()) {
                return;
            }

            var msg_sb = new StringBuilder();
            msg_sb.AppendLine($"There is an update to the content viewer available.");
            msg_sb.AppendLine($"This may create or solve problems (a backup will be created). Would you like to update? (requires restart)");
            var confirm_update_result = await Mp.Services.PlatformMessageBox.ShowYesNoMessageBoxAsync(
                title: $"Update Available",
                message: msg_sb.ToString(),
                iconResourceObj: "CefImage");

            if (!confirm_update_result) {
                // no update wanted
                return;
            }


            string min_install_file_name = ThisPlatformMostRecentVersion.files.FirstOrDefault(x => x.type == "minimal").name;
            string min_install_uri = $"{CEF_BUILD_BASE_URL}{HttpUtility.UrlEncode(min_install_file_name)}";

            string dl_dir = MpAvCefNetApplication.CefTempUpdateFolderPath;
            if (!dl_dir.IsDirectory()) {
                MpFileIo.CreateDirectory(dl_dir);
            }

            string min_install_dl_path = Path.Combine(dl_dir, min_install_file_name);
            if (min_install_dl_path.IsFile()) {
                MpConsole.WriteLine($"Warning! overwriting cef file '{min_install_dl_path}'");
                MpFileIo.DeleteFile(min_install_dl_path);
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            var prog_vm = new MpAvProgressViewModel();
            MpFileIo.WriteUrlToFileAsync(
                min_install_uri,
                min_install_dl_path,
                timeout: TimeSpan.FromHours(1),
                progress: prog_vm,
                cancellationToken: cts.Token)
                .FireAndForgetSafeAsync();

            bool cancel_dl_result = await Mp.Services.PlatformMessageBox.ShowCancelableProgressMessageBoxAsync(
                title: "Please wait...",
                message: "Updating content view",
                iconResourceObj: "AppImage",
                anchor: MpAvWindowManager.ActiveWindow,
                iprog_and_or_cancel_token_arg: new object[] { prog_vm, cts.Token });

            if (!cancel_dl_result) {
                // didn't complete
                if (!cts.Token.IsCancellationRequested) {
                    // user canceled, set token to terminate dl
                    cts.Cancel();
                } else {
                    // network error?
                    // TODO should probably show some info here...
                }
                // delete dl folder and file
                MpFileIo.DeleteDirectory(dl_dir);
                return;
            }

            // decompress and write folder to temp
            string min_install_temp_folder_path = Path.Combine(dl_dir, MpAvCefNetApplication.CefFrameworkFolderName);
            if (min_install_temp_folder_path.IsFileOrDirectory()) {
                // remove last decompressed update (shouldn't be there..)
                MpFileIo.DeleteFileOrDirectory(min_install_temp_folder_path);
            }
            using (var fs = new FileStream(min_install_dl_path, FileMode.Open)) {
                using (FileStream decompressedStream = File.Create(min_install_temp_folder_path)) {
                    try {
                        BZip2.Decompress(fs, decompressedStream, true);
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            bool restart_confirm_result = await Mp.Services.PlatformMessageBox.ShowYesNoMessageBoxAsync(
                title: $"Download complete",
                message: "Would you like to shutdown for changes to affect?",
                iconResourceObj: "QuestionMarkImage");
            if (!restart_confirm_result) {
                // no restart, ignore 
                return;
            }
            Mp.Services.ShutdownHelper.ShutdownApp("cef update");
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion


    }
    public class MpAvProgressViewModel : MpViewModelBase, IProgress<double> {
        public double Progress { get; set; }
        public MpAvProgressViewModel() : base(null) {

        }

        public void Report(double value) {
            Progress = value;
        }
    }
}
