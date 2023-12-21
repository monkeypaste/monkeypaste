using MonkeyPaste.Common;

using MonkeyPaste.Common.Plugin;
using MonoMac.CoreImage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
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
        public static bool USE_ASSEMBLY_LOAD_CONTEXT => true;
        static string MANIFEST_INVARIANT_FILE_NAME =>
            $"{MANIFEST_FILE_NAME_PREFIX}.{MANIFEST_FILE_EXT}";

        public static string PLUG_FOLDER_NAME => "Plugins";
        public static string MANIFEST_BACKUP_FOLDER_NAME => ".cache";
        public static string DAT_FOLDER_NAME => "dat";

        public static string CoreClipboardHandlerGuid => "cf2ec03f-9edd-45e9-a605-2a2df71e03bd";
        public static string CoreAnnotatorGuid => "ecde8e7c-30cf-47ef-a6a9-8f7f439b0a31";
        public static string CoreAnnotatorDefaultPresetGuid => "a9fa2fbf-025d-4ced-a23b-234085b5ac5f";

        #endregion

        #region Properties
        public static Dictionary<string, MpPluginWrapper> Plugins { get; set; } = new Dictionary<string, MpPluginWrapper>();
        public static bool IsLoaded { get; private set; }
        public static string PluginRootFolderPath =>
            Path.Combine(Mp.Services.PlatformInfo.StorageDir, PLUG_FOLDER_NAME);
        public static string PluginCacheDir =>
            Path.Combine(PluginRootFolderPath, MANIFEST_BACKUP_FOLDER_NAME);
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
            MpPluginUnloader.UnloadAll();

            await LoadPluginsAsync();

            await ValidateLoadedPluginsAsync();
            IsLoaded = true;
        }
        public static async Task<MpPluginWrapper> ReloadPluginAsync(string plugin_guid) {
            var kvp = Plugins.FirstOrDefault(x => x.Value.guid == plugin_guid);
            if (kvp.IsDefault()) {
                throw new Exception(string.Format(UiStrings.PluginErrMissingManifest, plugin_guid));
            }
            string manifestPath = kvp.Key;
            var reloaded_pf = await LoadPluginAsync(manifestPath);
            Plugins[manifestPath] = reloaded_pf;
            return reloaded_pf;
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
                backup_path = Path.Combine(MpFileIo.GetThisAppTempDir(), Path.GetFileName(dir_to_backup));
                if (backup_path.IsFileOrDirectory()) {
                    if (!MpFileIo.DeleteFileOrDirectory(backup_path)) {
                        throw new Exception($"Error deleting existing backup dir '{backup_path}'");
                    }
                }
                try {
                    MpFileIo.CopyDirectory(dir_to_backup, backup_path, true, true);
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool DeletePluginByGuid(string guid, bool delete_cache = true) {
            if (Plugins.FirstOrDefault(x => x.Value.guid != guid).IsDefault()) {
                // not found
                return false;
            }
            return DeletePlugin(Plugins.FirstOrDefault(x => x.Value.guid == guid).Value, delete_cache);
        }
        public static async Task<MpPluginWrapper> InstallPluginAsync(string plugin_guid, string packageUrl, bool silentInstall = false) {
            try {
                // download package (should always be a zip file of the plugins root folder or contents of root folder)
                var package_bytes = await MpFileIo.ReadBytesFromUriAsync(packageUrl, string.Empty, 10 * 60_000);

                // write zip to temp
                string temp_package_zip = MpFileIo.WriteByteArrayToFile(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), package_bytes, true);

                // 
                string temp_package_dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                string pluginName = null;
                // extract to ../Plugins
                try {
                    // extract zip to temp folder and get inner folder name
                    if (!temp_package_dir.IsDirectory()) {
                        if (!MpFileIo.CreateDirectory(temp_package_dir)) {
                            throw new Exception(string.Format(UiStrings.PluginErrStagingText, temp_package_dir));
                        }
                    }
                    ZipFile.ExtractToDirectory(temp_package_zip, temp_package_dir);


                    if (!temp_package_dir.IsDirectory() ||
                        Directory.GetDirectories(temp_package_dir) is not string[] tpfl ||
                        tpfl.Length == 0) {
                        throw new Exception(string.Format(UiStrings.PluginErrStagingText, temp_package_dir));
                    }

                    pluginName = Path.GetFileName(tpfl[0]);
                    string dest_dir = Path.Combine(PluginRootFolderPath, plugin_guid, pluginName);
                    if (dest_dir.IsDirectory()) {
                        // just in case remove existing dir if found
                        MpFileIo.DeleteDirectory(dest_dir);
                    }
                    // copy unzipped plugin from temp to plugin folder
                    MpFileIo.CopyDirectory(tpfl[0], dest_dir, true);

                    if (silentInstall) {
                        // install is core plugin, will be picked up in general load
                        return null;
                    }
                }
                catch (Exception ex) {
                    Mp.Services.NotificationBuilder.ShowNotificationAsync(
                        notificationType: MpNotificationType.FileIoWarning,
                        body: string.Format(UiStrings.PluginErrInstallNtfText, ex.Message)).FireAndForgetSafeAsync();
                    return null;
                }

                string manifest_path = Path.Combine(PluginRootFolderPath, plugin_guid, pluginName, "manifest.json");
                if (!manifest_path.IsFile()) {
                    Mp.Services.NotificationBuilder.ShowNotificationAsync(
                        notificationType: MpNotificationType.FileIoWarning,
                        body: string.Format(UiStrings.PluginErrInstallStructureNtfText, pluginName, manifest_path)).FireAndForgetSafeAsync();
                    return null;
                }

                var result = await LoadPluginAsync(manifest_path, true);
                if (result != null) {
                    Plugins.AddOrReplace(manifest_path, result);
                }

                return result;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error installing plugin from uri '{packageUrl}'. ", ex);
                Mp.Services.NotificationBuilder.ShowNotificationAsync(
                    notificationType: MpNotificationType.FileIoWarning,
                    body: ex.Message).FireAndForgetSafeAsync();

                return null;
            }
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

        private static async Task LoadPluginsAsync() {
            Plugins.Clear();
            if (Mp.Services.StartupState.IsInitialStartup) {
                await CheckAndInstallCorePluginsAsync();
            }

            if (!Directory.Exists(PluginRootFolderPath)) {
                MpConsole.WriteLine("Plugin folder missing from: " + PluginRootFolderPath, level: MpLogLevel.Error);
                // if plugin folder doesn't exist then no plugins so nothing to do but it should                
                return;
            }
            var inv_manifest_paths = FindInvariantManifestPaths(PluginRootFolderPath);
            foreach (var inv_manifest_path in inv_manifest_paths) {
                // attempt to localized manifest
                string localized_manifest_path = ResolveManifestPath(inv_manifest_path);
                var plugin = await LoadPluginAsync(localized_manifest_path);
                if (plugin == null) {
                    continue;
                }
                Plugins.Add(localized_manifest_path, plugin);
                MpConsole.WriteLine($"Successfully loaded plugin: {plugin.title}");
            }

        }
        private static async Task CheckAndInstallCorePluginsAsync() {
            if (!CoreDatDir.IsDirectory()) {
                // android dat dir supposed to be '/data/user/0/com.Monkey.MonkeyPaste.Avalonia/files/dat'
                MpDebug.Break($"Dat dir error, '{CoreDatDir}' does not exist");
                return;
            }
            if (!PluginRootFolderPath.IsDirectory()) {
                bool success = MpFileIo.CreateDirectory(PluginRootFolderPath);
                MpDebug.Assert(success, $"Error creating root plugin folder at path '{PluginRootFolderPath}'");
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

        private static async Task<MpPluginWrapper> LoadPluginAsync(string manifestPath, bool isLoadFromInstall = false) {
            if (manifestPath.ToLower().Contains("coreole")) {

            }
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
                        return null;
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
                        return null;
                    }
                    needsFixing = true;
                }

                if (!needsFixing) {
                    try {

                        var components = MpAvPluginAssemblyHelpers.Load(manifestPath, plugin, out Assembly component_assembly).ToArray();
                        LoadAnyHeadlessFormats(plugin, component_assembly);

                        if (ValidatePluginComponents(plugin, components, manifestPath)) {
                            plugin.Components = components;
                        }
                    }
                    catch (Exception ex) {
                        var ivalid_plugin_component_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                notificationType: MpNotificationType.InvalidPlugin,
                                body: ex.Message,
                                retryAction: retryFunc,
                                fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(manifestPath)));
                        if (ivalid_plugin_component_result == MpNotificationDialogResultType.Ignore) {
                            return null;
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
                    bool success = DeletePlugin(plugin, true);
                    MpDebug.Assert(success, $"Error deleting unaccepted plugin '{plugin}'");
                    return null;
                }
            }

            // only once manifest is validated get manifest backup
            UpdatePluginCache(plugin);
            return plugin;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool RemovePlugin(string manifest_path) {
            if (!Plugins.TryGetValue(manifest_path, out MpPluginWrapper plugin) ||
                plugin == null) {
                MpConsole.WriteLine($"Plugin not found to remove for manifest path '{manifest_path}'");
                return true;
            }
            Plugins.Remove(manifest_path);
            MpAvPluginAssemblyHelpers.Unload(ref plugin);
            return true;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool DeletePlugin(MpPluginWrapper plugin, bool delete_cache) {
            // NOTE this won't work with LoadFrom, maybe this can be used to load into sep domain then unload then delete
            // https://stackoverflow.com/a/62018508/105028
            bool success = true;
            string fix_path = null;
            string manifest_path = plugin.ManifestPath;
            string title = plugin.title;
            string root_dir = plugin.RootDirectory;
            string cache_path = plugin.CachePath;
            // clear ref to plugin
            plugin = null;
            try {
                success = RemovePlugin(manifest_path);
                if (USE_ASSEMBLY_LOAD_CONTEXT) {
                    // delete actual plugin on demand
                    if (!MpFileIo.DeleteDirectory(root_dir)) {
                        fix_path = root_dir;
                        throw new MpUserNotifiedException($"Error unloading '{title}'. Try shutting down MonkeyPaste and deleting the following folder manually: '{fix_path}'");
                    }
                    if (delete_cache && cache_path.IsFile()) {
                        if (!MpFileIo.DeleteFile(cache_path)) {
                            fix_path = cache_path;
                            throw new MpUserNotifiedException($"Error unloading cache for '{title}'. Try shutting down MonkeyPaste and deleting the following file manually: '{fix_path}'");
                        }
                    }
                }
            }
            catch (Exception ex) {
                success = false;
                Mp.Services.NotificationBuilder.ShowNotificationAsync(
                            notificationType: MpNotificationType.UnloadPluginError,
                            body: ex.ToString(),
                            fixCommand: fix_path == null ? null : new MpCommand(() => MpFileIo.OpenFileBrowser(fix_path)),
                            iconSourceObj: "ErrorImage");
            }

            return success;
        }

        #region Component
        private static bool ValidatePluginComponents(MpPluginWrapper plugin, object[] components, string manifestPath) {
            return components.All(x => ValidatePluginComponent(plugin, x, manifestPath));
        }

        private static bool ValidatePluginComponent(MpPluginWrapper plugin, object component, string manifestPath) {
            if (component is MpIAnalyzeAsyncComponent || component is MpIAnalyzeComponent) {
                if (plugin.analyzer == null) {
                    throw new MpUserNotifiedException($"Plugin error '{manifestPath}' must define 'analyzer' definition for '{component.GetType()}' which implements analyzer interface.");
                }
            } else if (component is MpIOlePluginComponent &&
                    (plugin is not MpPluginWrapper avpf ||
                    avpf.oleHandler == null)) {
                throw new MpUserNotifiedException($"Plugin error '{manifestPath}' must define 'oleHandler' definition for '{component.GetType()}' which implements analyzer interface.");
            }
            return true;
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


        private static void LoadAnyHeadlessFormats(MpPluginWrapper plugin, Assembly pluginAssembly) {
            if (pluginAssembly == null || plugin == null) {
                return;
            }
            try {
                if (pluginAssembly.FindSubTypes<MpISupportHeadlessAnalyzerComponentFormat>() is { } acl && acl.Any()) {
                    plugin.analyzer = acl.FirstOrDefault().GetFormat();
                }
                if (pluginAssembly.FindSubTypes<MpISupportHeadlessClipboardComponentFormat>() is { } cbhcl && cbhcl.Any()) {
                    plugin.oleHandler = cbhcl.FirstOrDefault().GetFormats();
                }
            }
            catch (Exception ex) {
                if (ex is MpUserNotifiedException) {
                    throw;
                }
                throw new MpUserNotifiedException("Error loading " + plugin.title + " ", ex);
            }
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
                    foreach (var tr in toRemove) {
                        RemovePlugin(tr.Key);
                    }

                    // Show list of duplicate plugins, fix just opens plugin folder, retry will re-initialize
                    var sb = new StringBuilder();
                    sb.AppendLine($"Duplicate plugin identifiers detected for these plugins (name | path | id):");
                    toRemove.ForEach(x => sb.Append(string.Join(Environment.NewLine, new[] { string.Empty, x.Value.title, x.Key, x.Value.guid, string.Empty })));
                    sb.AppendLine();
                    sb.AppendLine($"Fix by changing plugin guid or removing duplicates. Otherwise all will be ignored.");

                    var dup_guids_detected_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                            notificationType: MpNotificationType.InvalidPlugin,
                            body: sb.ToString(),
                            retryAction: retryFunc,
                            fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(PluginRootFolderPath, toRemove.Select(x => Path.GetFileName(x.Value.RootDirectory)))));
                    if (dup_guids_detected_result == MpNotificationDialogResultType.Ignore) {
                        continue;
                    }
                    //needsFixing = true;
                    //while (needsFixing) {
                    //    await Task.Delay(100);
                    //}
                    await LoadPluginsAsync();
                    // block initial call until completely done 
                    await ValidateLoadedPluginsAsync();
                    return;

                }
            }
        }



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
            MpFileIo.WriteTextToFile(backup_manifest_path, plugin_json_str, false);

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
            MpFileIo.WriteTextToFile(backup_manifest_path, plugin_json_str, false);

            return MpJsonConverter.DeserializeObject<MpPluginWrapper>(plugin_json_str);
        }

        #endregion

        #region Localization

        private static string ResolveManifestPath(string inv_manifest_path) {
            // find closest manifest culture matching users culture
            string manifest_dir = Path.GetDirectoryName(inv_manifest_path);
            string man_culture_code = MpAvLocalizationHelpers.ResolveMissingCulture(
                culture_code: MpAvCurrentCultureViewModel.Instance.CurrentCulture.Name,
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
}
