using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common.Plugin.Localizer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public static class MpPluginLoader {
        #region Private Variables

        #endregion

        #region Constants

        const string MANIFEST_FILE_NAME_PREFIX = "manifest";
        public const string MANIFEST_FILE_EXT = "json";

        #endregion

        #region Statics
        static string MANIFEST_INVARIANT_FILE_NAME =>
            $"{MANIFEST_FILE_NAME_PREFIX}.{MANIFEST_FILE_EXT}";

        public static string PLUG_FOLDER_NAME => "Plugins";
        public static string PLUGIN_CACHE_FOLDER_NAME => ".cache";
        public static string PLUGIN_BACKUP_FOLDER_NAME => ".backup";
        public static string PLUGIN_UPDATES_FOLDER_NAME => ".updates";
        public static string DAT_FOLDER_NAME => "dat";

        public static string CoreClipboardHandlerGuid => "cf2ec03f-9edd-45e9-a605-2a2df71e03bd";
        public static string CoreAnnotatorGuid => "ecde8e7c-30cf-47ef-a6a9-8f7f439b0a31";
        public static string CoreAnnotatorDefaultPresetGuid => "a9fa2fbf-025d-4ced-a23b-234085b5ac5f";


        #endregion

        #region Properties
        public static List<string> UpdatedPluginGuids { get; set; } = [];
        public static Dictionary<string, MpPluginWrapper> Plugins { get; set; } = new Dictionary<string, MpPluginWrapper>();
        public static Dictionary<string, MpPluginWrapper> PluginLookup =>
            Plugins.ToDictionary(x => x.Value.guid, x => x.Value);
        public static bool IsLoaded { get; private set; }
        public static string PluginRootDir =>
            Path.Combine(Mp.Services.PlatformInfo.StorageDir, PLUG_FOLDER_NAME);
        public static string PluginCacheDir =>
            Path.Combine(PluginRootDir, PLUGIN_CACHE_FOLDER_NAME);
        public static string PluginBackupDir =>
            Path.Combine(PluginCacheDir, PLUGIN_BACKUP_FOLDER_NAME);
        public static string PluginUpdatesDir =>
            Path.Combine(PluginCacheDir, PLUGIN_UPDATES_FOLDER_NAME);
        public static string CoreDatDir =>
            Path.Combine(Mp.Services.PlatformInfo.ExecutingDir, DAT_FOLDER_NAME);

        public static string[] CorePluginGuids => new string[] {
            CoreClipboardHandlerGuid,
            CoreAnnotatorGuid
        };

        #endregion

        #region Public Methods

        public static async Task InitAsync() {
            IsLoaded = false;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            MpStartupCleaner.UnloadAll();
            FinishPluginUpdates();

            await LoadAllPluginsAsync();

            await ValidateLoadedPluginsAsync();
            IsLoaded = true;
        }
        public static async Task<bool> ReloadPluginAsync(string plugin_guid) {
            var kvp = Plugins.FirstOrDefault(x => x.Value.guid == plugin_guid);
            if (kvp.IsDefault()) {
                throw new Exception(string.Format(UiStrings.PluginErrMissingManifest, plugin_guid));
            }
            string manifestPath = kvp.Key;
            bool success = await LoadPluginAsync(manifestPath);
            return success;
        }
        public static bool ValidatePluginDependencies(MpPluginWrapper plugin) {
            if (plugin == null) {
                return false;
            }
            if (plugin.dependencies == null) {
                return true;
            }

            foreach (var dep in plugin.dependencies) {
                switch (dep.type) {
                    case MpPluginDependencyType.os:
                        MpUserDeviceType dep_os = dep.name.ToEnum<MpUserDeviceType>();
                        MpUserDeviceType actual_os = Mp.Services.PlatformInfo.OsType;
                        if (actual_os != dep_os) {
                            MpConsole.WriteLine($"Cannot load plugin '{plugin.title}'. Requires {dep.type}:{dep.name}. Actual {dep.type}:{actual_os}");
                            return false;
                        }
                        // TODO compare version here
                        // see https://learn.microsoft.com/en-us/nuget/concepts/package-versioning#version-ranges
                        break;
                }
            }
            return true;
        }
        public static string CreatePluginBackup(string guid, out string original_dir) {
            original_dir = null;
            if (Plugins.All(x => x.Value.guid != guid)) {
                // not found
                return null;
            }
            var plugin_kvp = Plugins.FirstOrDefault(x => x.Value.guid == guid);
            original_dir = plugin_kvp.Key;

            string backup_path = null;
            if (original_dir.IsFile()) {
                // delete plugin folder
                string dir_to_backup = Path.GetDirectoryName(original_dir);
                backup_path = Path.Combine(PluginBackupDir, Path.GetFileName(dir_to_backup));
                if (backup_path.IsFileOrDirectory()) {
                    if (!MpFileIo.DeleteFileOrDirectory(backup_path)) {
                        throw new Exception($"Error deleting existing backup dir '{backup_path}'");
                    }
                }
                try {
                    MpFileIo.CopyDirectory(dir_to_backup, backup_path);
                }
                catch (Exception ex) {
                    Mp.Services.NotificationBuilder.ShowNotificationAsync(
                        notificationType: MpNotificationType.FileIoWarning,
                        body: string.Format(UiStrings.PluginLoaderBackupErrorText, dir_to_backup, backup_path, ex.Message)).FireAndForgetSafeAsync();
                    backup_path = null;
                }
            }
            return backup_path;
        }

        public static async Task<bool> InstallPluginAsync(string plugin_guid, string packageUrl, bool silentInstall = false) {
            try {
                string install_dir = await DownloadAndExtractPluginToDirAsync(plugin_guid, packageUrl, PluginRootDir);
                if (FindInvariantManifestPaths(install_dir) is not { } inv_manifests ||
                    ResolveManifestPath(inv_manifests.FirstOrDefault()) is not string manifest_path) {
                    return false;
                }
                bool success = await LoadPluginAsync(manifest_path, true);
                return success;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error installing plugin from uri '{packageUrl}'. ", ex);
                Mp.Services.NotificationBuilder.ShowNotificationAsync(
                    notificationType: MpNotificationType.FileIoWarning,
                    body: ex.Message).FireAndForgetSafeAsync();

                return false;
            }
        }
        public static async Task<bool> DeletePluginByGuidAsync(string plugin_guid) {
            // NOTE this won't work with LoadFrom, maybe this can be used to load into sep domain then unload then delete
            // https://stackoverflow.com/a/62018508/105028
            if (!Plugins.Any(x => x.Value.guid == plugin_guid)) {
                return true;
            }
            MpPluginWrapper plugin = Plugins.FirstOrDefault(x => x.Value.guid == plugin_guid).Value;
            bool success = true;

            string manifest_path = plugin.ManifestPath;
            string root_dir = plugin.RootDirectory;
            string cache_path = plugin.CachePath;
            string update_dir = plugin.UpdateDir;

            // clear ref to plugin
            plugin = null;
            MpStartupCleaner.AddPathToDelete(root_dir);
            MpStartupCleaner.AddPathToDelete(cache_path);
            MpStartupCleaner.AddPathToDelete(update_dir);
            success = await UnloadAndRemovePluginAsync(manifest_path);
            return success;
        }

        public static async Task<bool> BeginUpdatePluginAsync(string plugin_guid, string packageUrl) {
            string plugin_update_dir = await DownloadAndExtractPluginToDirAsync(plugin_guid, packageUrl, PluginUpdatesDir);
            if (!plugin_update_dir.IsDirectory()) {
                // update failed, only returns if no restart
                await Mp.Services.PlatformMessageBox.ShowRestartNowOrLaterMessageBoxAsync(
                    title: UiStrings.NtfPluginUpdateFailedTitle,
                    message: UiStrings.NtfPluginUpdateFailedText,
                    iconResourceObj: "ErrorImage");
                return false;
            }
            // NOTE this won't return if they choose restart
            await Mp.Services.PlatformMessageBox.ShowRestartNowOrLaterMessageBoxAsync(
                title: UiStrings.NtfPluginUpdateReadyTitle,
                message: UiStrings.NtfPluginUpdateReadyText,
                iconResourceObj: "ResetImage");
            return true;
        }
        public static MpNotificationFormat CreateInvalidPluginNotification(string msg, MpPluginWrapper pf) {
            return new MpNotificationFormat() {
                Title = string.Format(UiStrings.PluginErrNtfTitle, msg),
                Body = msg,
                NotificationType = MpNotificationType.InvalidPlugin,
                FixCommand = new MpCommand(() => MpFileIo.OpenFileBrowser(pf.ManifestPath))
            };
        }

        #endregion

        #region Private Methods
        private static void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowLoadComplete:
                    UpdatedPluginGuids.Clear();
                    break;
            }
        }
        private static async Task LoadAllPluginsAsync() {
            Plugins.Clear();
            if (Mp.Services.StartupState.IsInitialStartup) {
                await CheckAndInstallCorePluginsAsync();
            }

            if (!Directory.Exists(PluginRootDir)) {
                MpConsole.WriteLine("Plugin folder missing from: " + PluginRootDir, level: MpLogLevel.Error);
                // if plugin folder doesn't exist then no plugins so nothing to do but it should                
                return;
            }
            var inv_manifest_paths = FindInvariantManifestPaths(PluginRootDir);
            foreach (var inv_manifest_path in inv_manifest_paths) {
                // attempt to localized manifest
                string localized_manifest_path = ResolveManifestPath(inv_manifest_path);
                bool success = await LoadPluginAsync(localized_manifest_path);
                MpConsole.WriteLine($"Plugin manifest at path: '{localized_manifest_path}' Load:{success.ToTestResultLabel()}");
            }
        }
        private static async Task CheckAndInstallCorePluginsAsync() {
            if (!CoreDatDir.IsDirectory()) {
                // android dat dir supposed to be '/data/user/0/com.Monkey.MonkeyPaste.Avalonia/files/dat'
                MpDebug.Break($"Dat dir error, '{CoreDatDir}' does not exist");
                return;
            }
            if (!PluginRootDir.IsDirectory()) {
                bool success = MpFileIo.CreateDirectory(PluginRootDir);
                MpDebug.Assert(success, $"Error creating root plugin folder at path '{PluginRootDir}'");
                if (!success) {
                    return;
                }
            }

            foreach (var core_guid in CorePluginGuids) {
                string core_plugin_zip_path = Path.Combine(CoreDatDir, $"{core_guid}.zip");
                MpDebug.Assert(core_plugin_zip_path.IsFile(), $"Dat zip error, core plugin not found at '{core_plugin_zip_path}'");
                _ = await InstallPluginAsync(core_guid, core_plugin_zip_path.ToFileSystemUriFromPath(), true);
                MpConsole.WriteLine($"Core plugin '{core_plugin_zip_path}' installed.");
            }
        }
        private static async Task<bool> LoadPluginAsync(string manifestPath, bool isLoadFromInstall = false) {
            bool needsFixing = false;
            Func<object, object> retryFunc = (args) => {
                //needsFixing = false;
                return null;
            };

            MpPluginWrapper plugin = null;

            if (!needsFixing) {
                try {
                    string manifestStr = MpFileIo.ReadTextFromFile(manifestPath);
                    plugin = JsonConvert.DeserializeObject<MpPluginWrapper>(manifestStr);
                    plugin.ManifestPath = manifestPath;

                    if (!ValidatePluginDependencies(plugin)) {
                        Plugins.Remove(manifestPath);
                        return false;
                    }

                    bool isValid = ValidatePluginManifest(plugin, manifestPath);
                }
                catch (Exception ex) {
                    var invalid_or_malformed_json_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                            notificationType: MpNotificationType.InvalidPlugin,
                            body: string.Format(UiStrings.PluginErrManifestText, manifestPath, ex.Message),
                            retryAction: retryFunc,
                            fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(manifestPath)));
                    if (invalid_or_malformed_json_result == MpNotificationDialogResultType.Ignore) {
                        Plugins.Remove(manifestPath);
                        return false;
                    }
                    needsFixing = true;
                }

                if (!needsFixing) {
                    try {

                        MpAvPluginAssemblyHelpers.Load(manifestPath, plugin, out Assembly component_assembly);
                        LoadAnyHeadlessFormats(plugin, component_assembly);
                        plugin.ValidatePluginComponents();
                    }
                    catch (Exception ex) {
                        var ivalid_plugin_component_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                notificationType: MpNotificationType.InvalidPlugin,
                                body: ex.Message,
                                retryAction: retryFunc,
                                fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(manifestPath)));
                        if (ivalid_plugin_component_result == MpNotificationDialogResultType.Ignore) {
                            Plugins.Remove(manifestPath);
                            return false;
                        }
                        needsFixing = true;
                    }
                }
            }

            if (needsFixing) {
                //while (needsFixing) {
                //    await Task.Delay(100);
                //}
                return await LoadPluginAsync(manifestPath);
            }

            // valid after here

            if (isLoadFromInstall &&
                Uri.IsWellFormedUriString(plugin.licenseUrl, UriKind.Absolute) &&
                plugin.requireLicenseAcceptance) {
                // show terms
                bool agreed = await MpAvTermsView.ShowTermsAgreementWindowAsync(
                    new MpAvTermsAgreementCollectionViewModel() {
                        IntroText = UiStrings.TermsIntroAppText,
                        OutroText = UiStrings.TermsOutroAppText,
                        Items = new[] {
                            new MpAvTermsAgreementViewModel() {
                                Author = string.IsNullOrEmpty(plugin.author) ? UiStrings.PluginAnonymousAuthor : plugin.author,
                                PackageName =plugin.title,
                                LicenseUri = plugin.licenseUrl
                            }
                        }.ToList()
                    });

                if (!agreed) {
                    // uninstall here
                    string plugin_guid = plugin.guid;
                    plugin = null;
                    bool success = await DeletePluginByGuidAsync(plugin_guid);
                    MpDebug.Assert(success, $"Error deleting unaccepted plugin '{plugin}'");
                    return false;
                }
            }

            // only once manifest is validated get manifest backup
            UpdatePluginCache(plugin);
            Plugins.AddOrReplace(manifestPath, plugin);
            return true;
        }
        private static async Task<bool> UnloadAndRemovePluginAsync(string manifest_path) {
            if (!Plugins.TryGetValue(manifest_path, out MpPluginWrapper plugin) ||
                plugin == null) {
                MpConsole.WriteLine($"Plugin not found to remove for manifest path '{manifest_path}'");
                return true;
            }
            bool success = Plugins.Remove(manifest_path);
            await plugin.UnloadAsync();
            plugin = null;

            // NOTE always returning true for now since unload/update happens passively
            return true;
        }
        private static bool ValidatePluginManifest(MpPluginWrapper plugin, string manifestPath) {
            if (plugin == null) {
                throw new MpUserNotifiedException($"Plugin parsing error, at path '{manifestPath}' null, likely error parsing json. Ignoring plugin");
            }

            if (string.IsNullOrWhiteSpace(plugin.title)) {
                throw new MpUserNotifiedException($"Plugin title error, at path '{manifestPath}' must have 'title' property. Ignoring plugin");
            }
            if (!MpRegEx.RegExLookup[MpRegExType.Guid].IsMatch(plugin.guid)) {
                throw new MpUserNotifiedException($"Plugin guid error, at path '{manifestPath}' with Title '{plugin.title}' must have a 'guid' property, RFC 4122 compliant 128-bit GUID (UUID) with only alphanumeric characters ie. no hyphens curly braces or quotes etc.. Ignoring plugin");
            }

            //bool is_icon_valid = await MpFileIo.IsAccessibleUriAsync(plugin.iconUri, plugin.RootDirectory);
            //if (!is_icon_valid) {
            //    throw new MpUserNotifiedException($"Plugin icon error, at path '{inv_manifest_path}' with iconUri '{plugin.iconUri}' must have an 'iconUri' property which is a relative file path or valid url to an image");
            //}
            bool are_all_components_valid = plugin.componentFormats.All(x => ValidatePluginComponentManifest(x, manifestPath));
            return are_all_components_valid;
        }
        private static async Task ValidateLoadedPluginsAsync() {
            //bool needsFixing = false;
            Func<object, object> retryFunc = (args) => {
                //needsFixing = false;
                return null;
            };

            var invalidGuids =
                Plugins
                .GroupBy(x => x.Value.guid)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key);

            if (invalidGuids.Any()) {

                foreach (var ig in invalidGuids) {
                    var toRemove = Plugins.Where(x => x.Value.guid == ig).ToList();

                    // Show list of duplicate plugins, fix just opens plugin folder, retry will re-initialize
                    var sb = new StringBuilder();
                    sb.AppendLine($"Duplicate plugin identifiers detected for these plugins (name | path | id):");
                    toRemove.ForEach(x => sb.Append(string.Join(Environment.NewLine, new[] { string.Empty, x.Value.title, x.Key, x.Value.guid, string.Empty })));
                    sb.AppendLine();
                    sb.AppendLine($"Fix by changing plugin guid or removing duplicates. Otherwise all will be ignored.");

                    var to_remove_min_refs = toRemove.Select(x => (x.Key, x.Value.RootDirectory)).ToList();
                    toRemove = null;

                    foreach (var tr in to_remove_min_refs) {
                        await UnloadAndRemovePluginAsync(tr.Key);
                    }
                    var dup_guids_detected_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                            notificationType: MpNotificationType.InvalidPlugin,
                            body: sb.ToString(),
                            retryAction: retryFunc,
                            fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(PluginRootDir, to_remove_min_refs.Select(x => Path.GetFileName(x.RootDirectory)))));
                    if (dup_guids_detected_result == MpNotificationDialogResultType.Ignore) {
                        continue;
                    }
                    //needsFixing = true;
                    //while (needsFixing) {
                    //    await Task.Delay(100);
                    //}
                    await LoadAllPluginsAsync();
                    // block initial call until completely done 
                    await ValidateLoadedPluginsAsync();
                    return;

                }
            }
        }

        private static void FinishPluginUpdates() {
            MpDebug.Assert(!IsLoaded, $"Plugin loader error! Plugin updates should only be called BEFORE plugins are loaded");
            UpdatedPluginGuids.Clear();
            if (!PluginUpdatesDir.IsDirectory() ||
                Directory.GetDirectories(PluginUpdatesDir) is not { } update_dirs ||
                !update_dirs.Any()) {
                // no updates found
                return;
            }
            // move updates to root plugin dir, replacing existing
            foreach (var update_dir in update_dirs) {
                string plugin_guid = Path.GetFileName(update_dir);
                string target_dir = Path.Combine(
                    PluginRootDir,
                    plugin_guid);
                bool can_move_update = true;
                if (target_dir.IsDirectory()) {
                    // try to remove existing plugin dir
                    can_move_update = MpFileIo.DeleteDirectory(target_dir);
                }
                if (can_move_update) {
                    // duplicate update dir in root plugin dir
                    MpFileIo.CopyDirectory(update_dir, PluginRootDir);
                    UpdatedPluginGuids.Add(plugin_guid);
                }
                // remove update dir
                MpFileIo.DeleteDirectory(update_dir);
            }
        }

        private static async Task<string> DownloadAndExtractPluginToDirAsync(string plugin_guid, string packageUrl, string targetBaseDir) {
            // returns packagr CONTAINING dir ie the guid wrapper dir
            var package_bytes = await Task.Run(async () => {
                // do download in background. Not sure if this still blocks ui thread though...
                var bytes = await MpFileIo.ReadBytesFromUriAsync(packageUrl, string.Empty, 10 * 60_000);
                return bytes;
            });

            if (package_bytes == null || package_bytes.Length == 0) {
                // download error
                return null;
            }
            string target_dir = Path.Combine(targetBaseDir, plugin_guid);
            if (target_dir.IsDirectory()) {
                if (!MpFileIo.DeleteDirectory(target_dir)) {
                    // can't remove where its supposed to go
                    return null;
                }
            }
            if (!MpFileIo.CreateDirectory(target_dir)) {
                // shouldn't really happen
                return null;
            }
            // write zip to target dir
            string temp_package_zip_path = MpFileIo.WriteByteArrayToFile(Path.Combine(target_dir, Path.GetRandomFileName()), package_bytes);

            try {
                // extract zip to target dir
                ZipFile.ExtractToDirectory(temp_package_zip_path, target_dir);

                // delete zip
                if (!MpFileIo.DeleteFile(temp_package_zip_path)) {
                    MpConsole.WriteLine($"Warning! couldn't delete plugin zip from path '{temp_package_zip_path}'");
                }
                return target_dir;
            }
            catch (Exception ex) {
                // when download/extract fails cleanup and be SURE not to delete
                // either .updates or rootplugin dir
                if (target_dir != targetBaseDir &&
                    target_dir != PluginRootDir &&
                    target_dir != PluginUpdatesDir &&
                    target_dir != PluginCacheDir &&
                    target_dir.IsDirectory()) {
                    MpFileIo.DeleteDirectory(target_dir);
                }
                if (temp_package_zip_path.IsFile()) {
                    MpFileIo.DeleteFile(temp_package_zip_path);
                }
                Mp.Services.NotificationBuilder.ShowNotificationAsync(
                    notificationType: MpNotificationType.FileIoWarning,
                    body: string.Format(UiStrings.PluginErrInstallNtfText, ex.Message)).FireAndForgetSafeAsync();
                return null;
            }
        }

        #region Component
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            string assembly_name = args.Name.SplitNoEmpty(",").FirstOrDefault();
            if (string.IsNullOrEmpty(assembly_name)) {
                return null;
            }
            //assembly_name += ".dll";
            foreach (var plugin in Plugins) {
                string plugin_dir = Path.GetDirectoryName(plugin.Key);
                string assembly_test_Path = Path.Combine(plugin_dir, assembly_name + ".dll");
                if (assembly_test_Path.IsFile()) {
                    try {

                        var ass = Assembly.LoadFrom(assembly_test_Path);
                        return ass;
                    }
                    catch (Exception) {
                        var ass2 = Assembly.Load(assembly_name);
                        return ass2;
                    }
                }
            }
            return null;
        }
        private static void LoadAnyHeadlessFormats(MpPluginWrapper plugin, Assembly pluginAssembly) {
            if (pluginAssembly == null || plugin == null) {
                return;
            }
            try {
                if (pluginAssembly.FindSubTypes<MpISupportHeadlessAnalyzerComponentFormat>() is { } acl && acl.Any()) {
                    plugin.analyzer = acl.FirstOrDefault().GetFormat(new MpHeadlessAnalyzerComponentFormatRequest());
                }
                if (pluginAssembly.FindSubTypes<MpISupportHeadlessClipboardComponentFormat>() is { } cbhcl && cbhcl.Any()) {
                    plugin.oleHandler = cbhcl.FirstOrDefault().GetFormats(new MpHeadlessClipboardComponentFormatRequest());
                }
            }
            catch (Exception ex) {
                if (ex is MpUserNotifiedException) {
                    throw;
                }
                throw new MpUserNotifiedException("Error loading " + plugin.title + " ", ex);
            }
        }
        private static bool ValidatePluginComponentManifest(MpParameterHostBaseFormat cbf, string plugin_label) {
            if (cbf == null) {

                // undefined, ignore
                return true;
            }

            bool has_params = cbf.parameters != null && cbf.parameters.Count > 0;
            bool has_presets = cbf.presets != null && cbf.presets.Count > 0;
            if (!has_params && !has_presets) {
                return true;
            }
            if (has_presets && !has_params) {
                throw new MpUserNotifiedException($"Plugin '{plugin_label}' cannot have presets without at least 1 parameter provided");
            }

            var missing_paramids = cbf.parameters.Where(x => string.IsNullOrEmpty(x.paramId));
            if (missing_paramids.Any()) {
                string missing_param_labels = string.Join(",", missing_paramids.Select(x => string.IsNullOrEmpty(x.label) ? $"Unlabeled param #{cbf.parameters.IndexOf(x)}" : x.label));
                string missing_param_ids_msg = $"Plugin parameter ids (paramId) must be defined. Plugin '{plugin_label}' has the following parameters with missing paramId's: {missing_param_labels}";
                throw new MpUserNotifiedException(missing_param_ids_msg);
            }

            var dup_paramid_groups = cbf.parameters.GroupBy(x => x.paramId).Where(x => x.Count() > 1);
            foreach (var dup_paramid_group in dup_paramid_groups) {
                string dup_param_labels = string.Join(",", dup_paramid_group.Select(x => string.IsNullOrEmpty(x.label) ? $"Unlabeled param #{cbf.parameters.IndexOf(x)}" : x.label));
                string dup_param_ids_msg = $"Plugin parameter ids (paramId) must be unique. Plugin '{plugin_label}' has duplicate paramId values of '{dup_paramid_group.Key}' for parameters: {dup_param_labels}";
                throw new MpUserNotifiedException(dup_param_ids_msg);
            }

            if (!has_presets) {
                return true;
            }
            foreach (var preset in cbf.presets) {
                var preset_param_vals_with_no_param_match =
                    preset.values.Where(x => cbf.parameters.All(y => y.paramId != x.paramId));
                foreach (var preset_param_val_with_no_param_match in preset_param_vals_with_no_param_match) {
                    throw new MpUserNotifiedException($"Cannot find parameter with paramId '{preset_param_val_with_no_param_match.paramId}' referenced by Preset '{preset.label}' for Plugin '{plugin_label}'. Parameter may have changed or was removed, update preset value or remove it.");
                }
                var preset_vals_for_persistent_params =
                    preset.values.Where(x => cbf.parameters.FirstOrDefault(y => x.paramId == y.paramId).isSharedValue);
                foreach (var preset_val_for_persistent_params in preset_vals_for_persistent_params) {
                    throw new MpUserNotifiedException($"Cannot set persistent parameters in Presets. Param value w/ id '{preset_val_for_persistent_params.paramId}' in Preset '{preset.label}' for Plugin '{plugin_label}' needs to be removed or value can be specified in the parameter definition section.");
                }
            }

            return true;
        }
        #endregion

        #region Backup Cache

        private static void UpdatePluginCache(MpPluginWrapper plugin) {
            plugin.backupCheckPluginFormat = GetLastLoadedBackupPluginFormat(plugin, out bool isOutOfDate);
            plugin.IsManifestChangedFromBackup = isOutOfDate;

            if (plugin.backupCheckPluginFormat == null) {
                // initial backup create
                plugin.backupCheckPluginFormat = CreateLastLoadedBackupPluginFormat(plugin);
            }
        }

        private static string GetCachedPluginFileName(MpPluginWrapper plugin) {
            if (plugin == null || plugin.guid == null) {
                return string.Empty;
            }
            return $"{plugin.guid}.json";
        }

        public static MpPluginWrapper GetLastLoadedBackupPluginFormat(MpPluginWrapper plugin, out bool isBackupOutOfDate) {
            isBackupOutOfDate = false;
            if (!PluginCacheDir.IsDirectory()) {
                return null;
            }
            string backup_manifest_fn = GetCachedPluginFileName(plugin);
            string backup_manifest_path = Path.Combine(PluginCacheDir, backup_manifest_fn);
            if (!backup_manifest_path.IsFile()) {
                return null;
            }

            string plugin_json_str = plugin.SerializeJsonObject();
            string backup_manifest_str = MpFileIo.ReadTextFromFile(backup_manifest_path);
            // NOTE when backup doesn't match current manifest mark as out-of-date for reset in locators
            isBackupOutOfDate = plugin_json_str != backup_manifest_str;
            if (isBackupOutOfDate) {

            }
            if (!string.IsNullOrWhiteSpace(backup_manifest_str)) {
                try {
                    return MpJsonConverter.DeserializeObject<MpPluginWrapper>(backup_manifest_str);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error deserializing backup manifest at path: '{backup_manifest_path}' with data: '{backup_manifest_str}' ex: ", ex);
                }
            }

            // no backup or it is corrupt
            MpFileIo.WriteTextToFile(backup_manifest_path, plugin_json_str);

            return MpJsonConverter.DeserializeObject<MpPluginWrapper>(plugin_json_str);
        }
        public static MpPluginWrapper CreateLastLoadedBackupPluginFormat(MpPluginWrapper plugin) {
            if (!PluginCacheDir.IsDirectory()) {
                try {
                    Directory.CreateDirectory(PluginCacheDir);
                }
                catch (Exception ex) {
                    throw new MpUserNotifiedException(ex.Message);
                }
            }
            string backup_manifest_fn = $"{plugin.guid}.json";
            string backup_manifest_path = Path.Combine(PluginCacheDir, backup_manifest_fn);
            // no backup or it is corrupt
            string plugin_json_str = plugin.SerializeJsonObject();
            MpFileIo.WriteTextToFile(backup_manifest_path, plugin_json_str);

            return MpJsonConverter.DeserializeObject<MpPluginWrapper>(plugin_json_str);
        }

        #endregion

        #region Localization

        private static IEnumerable<string> FindInvariantManifestPaths(string root) {
            if (!root.IsDirectory()) {
                return new List<string>();
            }
            try {
                return Directory.EnumerateFiles(root, MANIFEST_INVARIANT_FILE_NAME, SearchOption.AllDirectories);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Error scanning plug-in directory: " + root, ex);
                return new List<string>();
            }
        }
        private static string ResolveManifestPath(string inv_manifest_path) {
            // find closest manifest culture matching users culture
            if (!inv_manifest_path.IsFile()) {
                return null;
            }
            string manifest_dir = Path.GetDirectoryName(inv_manifest_path);
            string man_culture_code = MpLocalizationHelpers.ResolveMissingCulture(
                culture_code: Mp.Services.UserCultureInfo.CultureCode,
                dir: manifest_dir,
                file_name_prefix: MANIFEST_FILE_NAME_PREFIX);

            string localized_manifest_file_name =
                $"{MANIFEST_FILE_NAME_PREFIX}.{man_culture_code}.{MANIFEST_FILE_EXT}";

            string localized_manifest_path =
                Path.Combine(
                    manifest_dir,
                    localized_manifest_file_name);

            if (localized_manifest_path.IsFile()) {
                return localized_manifest_path;
            }
            // fallback to invariant
            return inv_manifest_path;
        }
        #endregion

        #endregion
    }

    public static class MpPluginExecutor {

        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static void ExecuteAndUnload(string assemblyPath, out WeakReference alcWeakRef) {
            var alc = new MpPluginAssemblyLoadContext();
            Assembly a = alc.LoadFromAssemblyPath(assemblyPath);

            alcWeakRef = new WeakReference(alc, trackResurrection: true);

            var args = new object[1] { new string[] { "Hello" } };
            _ = a.EntryPoint?.Invoke(null, args);

            alc.Unload();
        }
    }
}
