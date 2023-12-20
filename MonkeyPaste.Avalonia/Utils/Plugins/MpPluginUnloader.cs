using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpPluginUnloader {
        public static void AddPluginToUnload(MpPluginWrapper plugin) {
            if (plugin == null) {
                return;
            }
            var paths = new string[]{
                plugin.RootDirectory,
                plugin.CachePath
            }.Where(x => x.IsFileOrDirectory());
            foreach (string p in paths) {
                MpAvPrefViewModel.Instance.PluginDirsToUnloadCsvStr =
                    MpAvPrefViewModel.Instance.PluginDirsToUnloadCsvStr.AddCsvItem(
                        item: p,
                        allowDup: false,
                        csvProps: MpAvPrefViewModel.Instance.CsvFormat);
            }
            MpConsole.WriteLine($"Plugin paths marked for unload: ");
            paths.ForEach(x => MpConsole.WriteLine(x));

        }
        public static void UnloadAll() {
            // delete all existing paths to unload, noting ones it can't delete
            List<string> to_unload = MpAvPrefViewModel.Instance.PluginDirsToUnloadCsvStr.ToListFromCsv(MpAvPrefViewModel.Instance.CsvFormat);
            List<string> errors = new List<string>();
            foreach (string path in to_unload) {
                bool success = MpFileIo.DeleteDirectory(path);
                MpConsole.WriteLine($"Plugin unloader removing '{path}': {success.ToTestResultLabel()}");
                if (success) {
                    continue;
                }
                errors.Add(path);
            }
            // update pref retaining failures
            MpAvPrefViewModel.Instance.PluginDirsToUnloadCsvStr = errors.ToCsv(MpAvPrefViewModel.Instance.CsvFormat);

            MpConsole.WriteLine("Plugin paths successfully unloaded: ");
            to_unload.Where(x => !errors.Contains(x)).ForEach(x => MpConsole.WriteLine(x));
            if (errors.Any()) {
                MpConsole.WriteLine($"Plugin unloader ERROR dirs:");
                errors.ForEach(x => MpConsole.WriteLine(x));
            }
        }

    }
}
