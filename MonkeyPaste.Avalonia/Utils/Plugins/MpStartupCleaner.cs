﻿using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpStartupCleaner {
        public static void AddPathToDelete(string path) {
            if (!path.IsFileOrDirectory()) {
                return;
            }
            MpAvPrefViewModel.Instance.PluginDirsToUnloadCsvStr =
                    MpAvPrefViewModel.Instance.PluginDirsToUnloadCsvStr.AddCsvItem(
                        item: path,
                        allowDup: false,
                        csvProps: MpAvPrefViewModel.Instance.CsvFormat);
            MpConsole.WriteLine($"StartupCleaner path added: '{path}'");
        }

        public static void RemovePathToDelete(string path) {
            if (!path.IsFileOrDirectory()) {
                return;
            }
            MpAvPrefViewModel.Instance.PluginDirsToUnloadCsvStr =
                    MpAvPrefViewModel.Instance.PluginDirsToUnloadCsvStr.RemoveCsvItem(
                        item: path,
                        csvProps: MpAvPrefViewModel.Instance.CsvFormat);
            MpConsole.WriteLine($"StartupCleaner path removed: '{path}'");
        }
        public static bool UnloadAll(bool clearFailures = true) {
            // delete all existing paths to unload, noting ones it can't delete
            List<string> to_unload = MpAvPrefViewModel.Instance.PluginDirsToUnloadCsvStr.ToListFromCsv(MpAvPrefViewModel.Instance.CsvFormat);
            List<string> errors = new List<string>();
            foreach (string path in to_unload) {
                if (!path.IsFileOrDirectory()) {
                    MpConsole.WriteLine($"Startup Cleaner ignoring missing path: '{path}'");
                    continue;
                }
                bool success = MpFileIo.DeleteFileOrDirectory(path);
                MpConsole.WriteLine($"Startup Cleaner removing '{path}': {success.ToTestResultLabel()}");
                if (success) {
                    continue;
                }
                errors.Add(path);
            }
            MpAvPrefViewModel.Instance.PluginDirsToUnloadCsvStr = string.Empty;
            if (!errors.Any()) {
                return true;
            }
            if (clearFailures) {
                MpDebug.Assert(!errors.Any(), $"Startup cleaner error removing:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            } else {
                // called testing alc
                foreach (var path in errors) {
                    // restore failed deletes
                    AddPathToDelete(path);
                }
            }
            return false;
        }

    }
}
