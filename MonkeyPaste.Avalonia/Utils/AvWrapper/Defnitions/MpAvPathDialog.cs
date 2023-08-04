using Avalonia.Controls;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Platform.Storage;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvPathDialog : MpIPlatformPathDialog {
        public async Task<string> ShowFileDialogAsync(
            string title = "",
            string initDir = "",
            object filters = null,
            bool resolveShortcutPath = false,
            object owner = null) {
            var result = await ShowFileOrFolderDialogAsync(false, title, initDir, filters, resolveShortcutPath, owner);
            return result;
        }

        public async Task<string> ShowFolderDialogAsync(
            string title = "",
            string initDir = "",
            object owner = null) {
            var result = await ShowFileOrFolderDialogAsync(true, title, initDir, null, false, owner);
            return result;
        }

        private static async Task<string> ShowFileOrFolderDialogAsync(
            bool isFolder,
            string title,
            string initDir,
            object filtersObj,
            bool resolveShortcutPath = false,
            object owner = null) {
            owner ??= MpAvWindowManager.ActiveWindow;
            if (owner is not MpAvWindow) {
                owner = MpAvWindowManager.MainWindow;
            }
            if (owner is MpAvMainWindow) {
                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
            }
            if (owner is not TopLevel tl) {
                return string.Empty;
            }
            var storage_provider = GetStorageProvider(tl);
            if (storage_provider == null) {
                return string.Empty;
            }
            title ??= $"Select {(isFolder ? "Folder" : "File")}";
            IReadOnlyList<FilePickerFileType> filters = filtersObj as IReadOnlyList<FilePickerFileType> ?? (new[] { FilePickerFileTypes.All });


            IStorageFolder start_location = await GetInitFolderAsync(initDir, tl);
            IReadOnlyList<IStorageItem> result;
            if (isFolder) {
                result = await
                    storage_provider.OpenFolderPickerAsync(
                        new FolderPickerOpenOptions() {
                            AllowMultiple = false,
                            Title = title,
                            SuggestedStartLocation = start_location
                        });
            } else {
                result = await
                   storage_provider.OpenFilePickerAsync(
                       new FilePickerOpenOptions() {
                           AllowMultiple = false,
                           Title = title,
                           FileTypeFilter = filters,
                           SuggestedStartLocation = start_location
                       });
            }
            if (owner is MpAvMainWindow) {
                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
            }
            if (result.FirstOrDefault() is IStorageItem si &&
                si.TryGetLocalPath() is string path) {
                if (resolveShortcutPath &&
                    path.IsShortcutPath()) {
                    path = MpFileIo.GetLnkTargetPath(path);
                }
                return path;
            }
            return null;
        }
        private static async Task<IStorageFolder?> GetInitFolderAsync(string initDir, TopLevel tl) {
            if (string.IsNullOrEmpty(initDir)) {
                initDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            } else if (initDir.IsFile()) {
                initDir = Path.GetDirectoryName(initDir);
            }
            IStorageFolder? start_location = null;
            if (!string.IsNullOrEmpty(initDir)) {
                start_location = await GetStorageProvider(tl).TryGetFolderFromPathAsync(initDir);
            }
            return start_location;
        }

        private static IStorageProvider GetStorageProvider(TopLevel tl) {
            return tl.StorageProvider;
        }
    }
}
