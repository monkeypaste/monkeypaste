using Avalonia.Platform.Storage;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvPathDialog : MpINativePathDialog {
        public async Task<string> ShowFileDialogAsync(string title = "", string initDir = "", object filters = null, bool resolveShortcutPath = false) {
            var result = await ShowFileOrFolderDialogAsync(false, title, initDir, filters, resolveShortcutPath);
            return result;
        }

        public async Task<string> ShowFolderDialogAsync(string title = "", string initDir = "") {
            var result = await ShowFileOrFolderDialogAsync(false, title, initDir, null);
            return result;
        }

        private static async Task<string> ShowFileOrFolderDialogAsync(bool isFolder, string title, string initDir, object filtersObj, bool resolveShortcutPath = false) {
            title ??= $"Select {(isFolder ? "Folder" : "File")}";
            IReadOnlyList<FilePickerFileType> filters = filtersObj as IReadOnlyList<FilePickerFileType> ?? (new[] { FilePickerFileTypes.All });
            var storage_provider = GetStorageProvider();
            if (storage_provider == null) {
                return null;
            }
            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
            IStorageFolder start_location = await GetInitFolderAsync(initDir);
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
            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;

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
        private static async Task<IStorageFolder?> GetInitFolderAsync(string initDir) {
            if (string.IsNullOrEmpty(initDir)) {
                initDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            } else if (initDir.IsFile()) {
                initDir = Path.GetDirectoryName(initDir);
            }
            IStorageFolder? start_location = null;
            if (!string.IsNullOrEmpty(initDir)) {
                start_location = await GetStorageProvider().TryGetFolderFromPathAsync(initDir);
            }
            return start_location;
        }

        private static IStorageProvider GetStorageProvider() {
            return App.Current.GetMainWindow().StorageProvider;
        }
    }
}
