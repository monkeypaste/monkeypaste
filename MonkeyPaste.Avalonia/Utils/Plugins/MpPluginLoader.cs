using Avalonia.Threading;
using McMaster.NETCore.Plugins;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public static class MpPluginLoader {
        #region Private Variables

        #endregion

        #region Constants

        const int MAX_TITLE_LENGTH = 32;
        const int MAX_DESCRIPTION_LENGTH = 1024;
        const int MAX_TAGS_LENGTH = 1024;

        const string MANIFEST_FILE_NAME_PREFIX = "manifest";
        public const string MANIFEST_FILE_EXT = "json";

        #endregion

        #region Properties
        private static Type[] _sharedTypes;
        //private static Type[] _sharedTypes = [
        //    typeof(MpIPluginComponentBase),
        //    typeof(MpISupportHeadlessAnalyzerFormat),
        //    typeof(MpISupportHeadlessClipboardComponentFormat),
        //    typeof(MpIAnalyzeComponent),
        //    typeof(MpIAnalyzeComponentAsync),
        //    typeof(MpISupportDeferredValue),
        //    typeof(MpISupportDeferredValueAsync),
        //    typeof(MpISupportDeferredParameterCommand),
        //    typeof(MpIUnloadPluginComponent),
        //    typeof(MpIOlePluginComponent),
        //    typeof(MpIOleReaderComponent),
        //    typeof(MpIOleWriterComponent),
        //    typeof(MpIParamterValueProvider),
        //    typeof(MpIParamterValueProvider),
        //    typeof(MpOlePluginRequest),
        //    typeof(MpOlePluginResponse),
        //    typeof(MpHeadlessComponentFormatRequest),
        //    typeof(MpParameterFormat),
        //    typeof(MpParameterValueFormat),
        //    ];
        private static Type[] sharedTypes {
            get {
                if (_sharedTypes == null) {
                    _sharedTypes =
                        typeof(MpIPluginComponentBase).Assembly.ExportedTypes
                        //.Union(typeof(MpCommonTools).Assembly.ExportedTypes)
                        // .Union(typeof(MpAvCommonTools).Assembly.ExportedTypes)
#if WINDOWS
                        //.Union(typeof(MpWpfHtmlToRtfConverter).Assembly.ExportedTypes)
#endif
                        .OrderBy(x => x.Name)
                        .ToArray();
                }
                return _sharedTypes;
            }
        }

        public static bool USE_LOADERS => false;

        private static Dictionary<string, PluginLoader> _loaders = [];
        static string PLUGIN_INFO_URL =>
            $"{MpServerConstants.PLUGINS_BASE_URL}/plugin-info-check.php";
        static string MANIFEST_INVARIANT_FILE_NAME =>
            $"{MANIFEST_FILE_NAME_PREFIX}.{MANIFEST_FILE_EXT}";

        public static string PLUG_FOLDER_NAME => "Plugins";
        public static string PLUGIN_CACHE_FOLDER_NAME => ".cache";
        public static string PLUGIN_BACKUP_FOLDER_NAME => ".backup";
        public static string PLUGIN_UPDATES_FOLDER_NAME => ".updates";
        public static string DAT_FOLDER_NAME => "dat";
        public static string MIN_APP_VERSION => "0.0.0";

        public static string CoreClipboardHandlerGuid => "cf2ec03f-9edd-45e9-a605-2a2df71e03bd";
        public static string CoreAnnotatorGuid => "ecde8e7c-30cf-47ef-a6a9-8f7f439b0a31";
        public static string CoreAnnotatorDefaultPresetGuid => "a9fa2fbf-025d-4ced-a23b-234085b5ac5f";

        public static List<string> UpdatedPluginGuids { get; set; } = [];
        public static ObservableCollection<string> UninstalledPluginGuids { get; set; } = [];
        public static ObservableCollection<MpRuntimePlugin> Plugins { get; } = new();
        public static IReadOnlyDictionary<string, MpRuntimePlugin> PluginManifestLookup =>
            Plugins.ToDictionary(x => x.ManifestPath, x => x);
        public static IReadOnlyDictionary<string, MpRuntimePlugin> PluginGuidLookup =>
            Plugins.ToDictionary(x => x.guid, x => x);
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

        static string SharedAssemblyQualifiedName =>
            typeof(MpIPluginComponentBase).AssemblyQualifiedName;

        #endregion

        #region Public Methods

        public static async Task InitAsync() {
            IsLoaded = false;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            MpStartupCleaner.UnloadAll();
            FinishPluginUpdates();

            await LoadAllPluginsAsync();

            await ValidateLoadedPluginsAsync();
            IsLoaded = true;
        }

        public static async Task<bool> ReloadPluginAsync(string plugin_guid) {
            var kvp = PluginManifestLookup.FirstOrDefault(x => x.Value.guid == plugin_guid);
            if (kvp.IsDefault()) {
                throw new Exception(string.Format(UiStrings.PluginErrMissingManifest, plugin_guid));
            }
            string manifestPath = kvp.Key;
            bool success = await LoadPluginAsync(manifestPath);
            return success;
        }

        public static async Task<bool> InstallPluginAsync(string plugin_guid, string packageUrl, bool silentInstall, MpICancelableProgressIndicatorViewModel cpivm) {
            try {
                string install_dir = await DownloadAndExtractPluginToDirAsync(plugin_guid, packageUrl, PluginRootDir, cpivm);
                if (install_dir == null) {
                    throw new MpUserNotifiedException(UiStrings.PluginInstallEx1);
                }
                if (FindInvariantManifestPaths(install_dir) is not { } inv_manifests ||
                    ResolveLocalizedManifestPath(inv_manifests.FirstOrDefault()) is not string manifest_path) {
                    return false;
                }
                bool success = await LoadPluginAsync(manifest_path, silentInstall);
                if (success && !silentInstall) {
                    await ValidateLoadedPluginsAsync();
                    // ensure plugin wasn't removed by validation
                    success = PluginGuidLookup.ContainsKey(plugin_guid);
                    if (success) {
                        await GetOrUpdatePluginStatsAsync(plugin_guid, true);
                    }
                }
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
            // for browser update/uninstall
            if (!PluginGuidLookup.TryGetValue(plugin_guid, out var plugin)) {
                return true;
            }
            bool success = await DeletePluginAsync_internal(plugin);
            return success;
        }
        private static async Task<bool> DeletePluginAsync_internal(MpRuntimePlugin plugin) {
            // returns true if plugin folder DELETED
            // when declined terms no restart
            bool needs_restart_on_unload_fail = PluginGuidLookup.ContainsKey(plugin.guid);
            bool success = await DetachPluginAsync(plugin);
            AddPluginToDeleteList(plugin);
            if (success) {
                // ATTEMPT to delete plugin dir
                success = MpStartupCleaner.UnloadAll(clearFailures: false);
            }

            if (!success && needs_restart_on_unload_fail) {
                // NOTE this won't return if they choose restart
                await Mp.Services.PlatformMessageBox.ShowRestartNowOrLaterMessageBoxAsync(
                    title: UiStrings.NtfPluginUninstallPendingTitle,
                    message: UiStrings.NtfPluginUninstallPendingText,
                    iconResourceObj: "ResetImage");
            }
            return success;
        }

        public static async Task<bool> BeginUpdatePluginAsync(string plugin_guid, string packageUrl, MpICancelableProgressIndicatorViewModel cpivm) {

            string plugin_update_dir = await DownloadAndExtractPluginToDirAsync(plugin_guid, packageUrl, PluginUpdatesDir, cpivm);
            if (!plugin_update_dir.IsDirectory()) {
                // update failed, only returns if no restart
                await Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                    title: UiStrings.NtfPluginUpdateFailedTitle,
                    message: UiStrings.NtfPluginUpdateFailedText,
                    iconResourceObj: "ErrorImage");
                return false;
            }
            // plugin downloaded to update dir, 

            // create backup of current so if anything fails it can be merged back in and coll vm can always reload item
            string backup_dir = CreatePluginBackup(plugin_guid);
            //ATTEMPT to delete current plugin
            bool success = await DeletePluginByGuidAsync(plugin_guid);
            if (success) {
                // install plugin from update dir
                success = await InstallPluginAsync(plugin_guid, plugin_update_dir.ToFileSystemUriFromPath(), false, cpivm);
                if (success) {
                    // clean up everything
                    MpFileIo.DeleteDirectory(backup_dir);
                    MpFileIo.DeleteDirectory(plugin_update_dir);
                    return true;
                }
            }
            // at this point either active dir couldn't be deleted (or SOMETHING couldn't be deleted) or install failed
            // so try to restore plugin dir and reload
            bool was_reloaded = false;
            try {
                // copy any missing files over
                string root_plugin_dir = Path.Combine(PluginRootDir, plugin_guid);
                MpFileIo.CreateDirectory(root_plugin_dir);
                MpFileIo.CopyContents(backup_dir, root_plugin_dir, overwrite: false);
                // find manifest
                if (FindInvariantManifestPaths(root_plugin_dir) is { } inv_mf_paths &&
                    inv_mf_paths.FirstOrDefault() is { } inv_mf_path &&
                    ResolveLocalizedManifestPath(inv_mf_path) is { } mf_path) {
                    // load and validate
                    was_reloaded = await LoadPluginAsync(mf_path);
                    await ValidateLoadedPluginsAsync();
                    // confirm it was loaded
                    was_reloaded = PluginGuidLookup.ContainsKey(plugin_guid);
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error restoring plugin backup for plugin '{plugin_guid}'.", ex);
            }
            // remove backup but keep update for next startup
            MpFileIo.DeleteDirectory(backup_dir);

            // NOTE this won't return if they choose restart
            await Mp.Services.PlatformMessageBox.ShowRestartNowOrLaterMessageBoxAsync(
                title: UiStrings.NtfPluginUpdateReadyTitle,
                message: UiStrings.NtfPluginUpdateReadyText,
                iconResourceObj: "ResetImage");
            return was_reloaded;
        }
        public static MpNotificationFormat CreateInvalidPluginNotification(string msg, MpRuntimePlugin pf) {
            return new MpNotificationFormat() {
                Title = string.Format(UiStrings.PluginErrNtfTitle, msg),
                Body = msg,
                NotificationType = MpNotificationType.InvalidPlugin,
                FixCommand = new MpCommand(() => MpFileIo.OpenFileBrowser(pf.ManifestPath.LocalStoragePathToPackagePath()))
            };
        }

        public static async Task<(int, DateTime?)> GetOrUpdatePluginStatsAsync(string guid, bool is_install) {
            var req_args = new Dictionary<string, string>() {
                {"plugin_guid", guid },
                {"is_install", is_install ?"1":"0" }
            };
            var resp = await MpHttpRequester.SubmitPostDataToUrlAsync(PLUGIN_INFO_URL, req_args);
            bool success = MpHttpRequester.ProcessServerResponse(resp, out var resp_args);
            int count = 0;
            DateTime? pub_dt = null;
            if (success) {
                if (int.TryParse(resp_args["install_count"], out int install_count)) {
                    count = install_count;
                }
                if (DateTime.TryParse(resp_args["publish_dt"], out DateTime resp_pub_dt)) {
                    pub_dt = resp_pub_dt;
                }
            }
            return (count, pub_dt);
        }

        public static bool ValidatePluginDependencies(MpManifestFormat plugin) {
            if (plugin == null) {
                return false;
            }
            if (plugin.dependencies == null) {
                return true;
            }
            if (plugin.publishedAppVersion != null && plugin.publishedAppVersion.ToVersion() < MIN_APP_VERSION.ToVersion()) {
                // hopefully never needed but good to have
                return false;
            }
            var this_os_dep = new MpPluginDependency() {
                type = MpPluginDependencyType.os,
                name = Mp.Services.PlatformInfo.OsType.ToString(),
                version = Mp.Services.PlatformInfo.OsVersion
            };

            foreach (var dep in plugin.dependencies) {
                switch (dep.type) {
                    case MpPluginDependencyType.os:
                        MpUserDeviceType dep_os = dep.name.ToEnum<MpUserDeviceType>();
                        MpUserDeviceType actual_os = this_os_dep.name.ToEnum<MpUserDeviceType>();
                        if (actual_os != dep_os) {
                            MpConsole.WriteLine($"Cannot load plugin '{plugin.title}'. OS not supported Requires {dep}. Actual {this_os_dep}");
                            return false;
                        }
                        // TODO compare version here
                        // see https://learn.microsoft.com/en-us/nuget/concepts/package-versioning#version-ranges
                        if (!string.IsNullOrWhiteSpace(dep.version) && dep.version.ToVersion() < Mp.Services.PlatformInfo.OsVersion.ToVersion()) {
                            MpConsole.WriteLine($"Cannot load plugin '{plugin.title}'. Version not supported Requires {dep}. Actual {this_os_dep}");
                            return false;
                        }
                        break;
                }
            }
            return true;
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
            //if (Mp.Services.StartupState.IsInitialStartup) {
            await CheckAndInstallCorePluginsAsync();
            //}

            if (!Directory.Exists(PluginRootDir)) {
                MpConsole.WriteLine("Plugin folder missing from: " + PluginRootDir, level: MpLogLevel.Error);
                // if plugin folder doesn't exist then no plugins so nothing to do but it should                
                return;
            }
            var inv_manifest_paths = FindInvariantManifestPaths(PluginRootDir);
            foreach (var inv_manifest_path in inv_manifest_paths) {
                // attempt to localized manifest
                string localized_manifest_path = ResolveLocalizedManifestPath(inv_manifest_path);
                bool success = await LoadPluginAsync(localized_manifest_path);
                MpConsole.WriteLine($"Load:{success.ToTestResultLabel()} from manifest: '{localized_manifest_path}' ");
            }
        }
        private static async Task CheckAndInstallCorePluginsAsync() {
            //if (!CoreDatDir.IsDirectory()) {
            //    // android dat dir supposed to be '/data/user/0/com.Monkey.MonkeyPaste.Avalonia/files/dat'
            //    MpDebug.Break($"Dat dir error, '{CoreDatDir}' does not exist");
            //    return;
            //}
            if (!PluginRootDir.IsDirectory()) {
                bool success = MpFileIo.CreateDirectory(PluginRootDir);
                MpDebug.Assert(success, $"Error creating root plugin folder at path '{PluginRootDir}'");
                if (!success) {
                    return;
                }
            }
            var missing_core_plugin_guids = CorePluginGuids.Where(x => !Path.Combine(PluginRootDir, x).IsDirectory());
            if (!missing_core_plugin_guids.Any()) {
                // all installed
                return;
            }

            foreach (var core_guid in missing_core_plugin_guids) {
                //string core_plugin_zip_path = Path.Combine(CoreDatDir, $"{core_guid}.zip");
                //string core_plugin_uri = core_plugin_zip_path.ToFileSystemUriFromPath();
                //if (!core_plugin_zip_path.IsFile()) {
                //    core_plugin_uri = $"https://www.monkeypaste.com/dat/{core_guid}/latest.zip";
                //}
                //MpDebug.Assert(core_plugin_zip_path.IsFile(), $"Dat zip error, core plugin not found at '{core_plugin_zip_path}'");
                string core_plugin_uri = $"avares://MonkeyPaste.Avalonia/Assets/dat/{core_guid}.zip";
                _ = await InstallPluginAsync(core_guid, core_plugin_uri, true, null);
                MpConsole.WriteLine($"Core plugin '{core_guid}' installed.");
            }
        }
        private static async Task<bool> LoadPluginAsync(string manifestPath, bool isLoadFromInstall = false) {
            bool needsFixing = false;
            Func<object, object> retryFunc = (args) => {
                //needsFixing = false;
                return null;
            };

            MpRuntimePlugin plugin = null;

            if (!needsFixing) {
                try {
                    string manifestStr = MpFileIo.ReadTextFromFile(manifestPath);
                    plugin = JsonConvert.DeserializeObject<MpRuntimePlugin>(manifestStr);
                    plugin.ManifestPath = ResolveInvManifestPath(manifestPath);


                    if (!ValidatePluginDependencies(plugin)) {
                        await DetachPluginByManifestPathAsync(plugin.ManifestPath);
                        return false;
                    }

                    bool isValid = ValidatePluginManifest(plugin, plugin.ManifestPath);
                }
                catch (Exception ex) {
                    var invalid_or_malformed_json_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                            notificationType: MpNotificationType.InvalidPlugin,
                            body: string.Format(UiStrings.PluginErrManifestText, plugin.ManifestPath, ex.Message),
                            retryAction: retryFunc,
                            fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(plugin.ManifestPath.LocalStoragePathToPackagePath())));
                    if (invalid_or_malformed_json_result == MpNotificationDialogResultType.Ignore) {
                        await DetachPluginByManifestPathAsync(plugin.ManifestPath);
                        return false;
                    }
                    needsFixing = true;
                }

                if (!needsFixing) {
                    try {
                        MpConsole.WriteLine($"Plugin {plugin} debug mode: {plugin.debugMode} isAttached: {Debugger.IsAttached}");

                        if (plugin.debugMode != MpPluginDebugMode.None && !Debugger.IsAttached) {
                            var cts = new CancellationTokenSource();
                            bool? cancel_debug_result = null;
                            Dispatcher.UIThread.Post(async () => {
                                cancel_debug_result = await Mp.Services.PlatformMessageBox.ShowBusyMessageBoxAsync(
                                    title: UiStrings.NtfWaitForDebuggerTitle,
                                    message: string.Format(UiStrings.NtfWaitForDebuggerText, Path.GetFileName(Mp.Services.PlatformInfo.ExecutingPath)),
                                    iconResourceObj: "BugImage",
                                    cancel_token_arg: cts.Token,
                                    can_user_cancel: true);
                            });

                            while (true) {
                                if (Debugger.IsAttached) {
                                    MpConsole.WriteLine("plugin debugger now attached.");
                                    cts.Cancel();
                                    break;
                                }
                                if (cancel_debug_result.HasValue) {
                                    //user canceled
                                    MpConsole.WriteLine("canceled. result: " + cancel_debug_result);
                                    break;
                                }
                                await Task.Delay(100);
                            }
                            if (Debugger.IsAttached && plugin.debugMode == MpPluginDebugMode.DebugLocalInputOnly) {
                                MpAvShortcutCollectionViewModel.Instance.ToggleGlobalHooksCommand.Execute(false);
                            }
                        }
                        await LoadComponentsAsync(plugin.ManifestPath, plugin);
                        ValidatePluginComponents(plugin);
                    }
                    catch (Exception ex) {
                        var ivalid_plugin_component_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                notificationType: MpNotificationType.InvalidPlugin,
                                body: ex.Message,
                                retryAction: retryFunc,
                                fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(plugin.ManifestPath.LocalStoragePathToPackagePath())));
                        if (ivalid_plugin_component_result == MpNotificationDialogResultType.Ignore) {
                            await DetachPluginByManifestPathAsync(plugin.ManifestPath);
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
                bool fix_result = await LoadPluginAsync(manifestPath);
                return fix_result;
            }

            // valid after here

            if (isLoadFromInstall &&
                plugin.licenseUrl.IsValidUrl() &&
                plugin.requireLicenseAcceptance.IsTrue()) {
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
                    bool success = await DeletePluginAsync_internal(plugin);
                    MpDebug.Assert(success, $"Error deleting unaccepted plugin '{plugin}'");
                    return false;
                }
            }

            // only once manifest is validated get manifest backup
            UpdatePluginCache(plugin);
            if (PluginGuidLookup.TryGetValue(plugin.guid, out var existing_plugin)) {
                Plugins.Remove(existing_plugin);
            }
            Plugins.Add(plugin);
            return true;
        }
        public static async Task<bool> DetachPluginByGuidAsync(string plugin_guid) {
            if (PluginGuidLookup.TryGetValue(plugin_guid, out var to_remove)) {
                return await DetachPluginAsync(to_remove);
            }
            return false;
        }
        private static async Task<bool> DetachPluginByManifestPathAsync(string manifest_path) {
            if (PluginManifestLookup.TryGetValue(manifest_path, out var to_remove)) {
                return await DetachPluginAsync(to_remove);
            }
            return false;
        }
        private static async Task<bool> DetachPluginAsync(MpRuntimePlugin plugin) {
            if (plugin == null) {
                // not found
                return true;
            }
            bool success = Plugins.Remove(plugin);
            if (success) {
                success = await UnloadPluginAsync(plugin);
            }

            // NOTE always returning true for now since unload/update happens passively
            return success;
        }
        private static bool ValidatePluginManifest(MpRuntimePlugin plugin, string manifestPath) {
            if (plugin == null) {
                //throw new MpUserNotifiedException($"Plugin parsing error, at path '{manifestPath}' null, likely error parsing json. Ignoring plugin");
                throw new MpUserNotifiedException(string.Format(UiStrings.InvalidPluginEx1, manifestPath));
            }
            // validate TITLE
            if (string.IsNullOrWhiteSpace(plugin.title) || plugin.title.Length > MAX_TITLE_LENGTH) {
                //throw new MpUserNotifiedException($"Plugin title error, at path '{manifestPath}' must have 'title' property with no more than {MAX_TITLE_LENGTH} characters. Ignoring plugin");
                throw new MpUserNotifiedException(string.Format(UiStrings.InvalidPluginEx2, manifestPath, MAX_TITLE_LENGTH));
            }
            // validate GUID
            if (!MpRegEx.RegExLookup[MpRegExType.Guid].IsMatch(plugin.guid.ToStringOrEmpty())) {
                //throw new MpUserNotifiedException($"Plugin guid error, at path '{manifestPath}' with Title '{plugin.title}' must have a 'guid' property, RFC 4122 compliant 128-bit GUID (UUID) with only letters, numbers and hyphens.. Ignoring plugin");
                throw new MpUserNotifiedException(string.Format(UiStrings.InvalidPluginEx3, manifestPath, plugin.title));
            }
            // validate DESCRIPTION
            if (!string.IsNullOrEmpty(plugin.description) && plugin.description.Length > MAX_DESCRIPTION_LENGTH) {
                //throw new MpUserNotifiedException($"Plugin description error, at path '{manifestPath}' description must be no more than {MAX_DESCRIPTION_LENGTH} characters. Ignoring plugin");
                throw new MpUserNotifiedException(string.Format(UiStrings.InvalidPluginEx4, MAX_DESCRIPTION_LENGTH));
            }
            // validate TAGS
            if (!string.IsNullOrEmpty(plugin.tags) && plugin.tags.Length > MAX_TAGS_LENGTH) {
                //throw new MpUserNotifiedException($"Plugin tags error, at path '{manifestPath}' tags must be no more than {MAX_TAGS_LENGTH} characters. Ignoring plugin");
                throw new MpUserNotifiedException(string.Format(UiStrings.InvalidPluginEx5, manifestPath, MAX_TAGS_LENGTH));
            }
            return true;
        }
        private static async Task ValidateLoadedPluginsAsync() {
            //bool needsFixing = false;
            Func<object, object> retryFunc = (args) => {
                //needsFixing = false;
                return null;
            };

            async Task<bool> ShowInterPluginInvalidationAsync(List<(string guid, string RootDirectory)> to_remove_min_refs, string msg) {
                // returns true to detach them and ignore false if they fixed
                foreach (var tr in to_remove_min_refs) {
                    await DetachPluginByGuidAsync(tr.guid);
                }
                var dup_guids_detected_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                        notificationType: MpNotificationType.InvalidPlugin,
                        body: msg,
                        retryAction: retryFunc,
                        fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(PluginRootDir, to_remove_min_refs.Select(x => Path.GetFileName(x.RootDirectory).LocalStoragePathToPackagePath()))));
                if (dup_guids_detected_result == MpNotificationDialogResultType.Ignore) {
                    return true;
                }

                await LoadAllPluginsAsync();
                // block initial call until completely done 
                await ValidateLoadedPluginsAsync();
                return false;
            }

            // DUP PLUGIN GUIDS

            var invalidGuids =
                PluginManifestLookup
                .GroupBy(x => x.Value.guid)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (invalidGuids.Any()) {
                foreach (var ig in invalidGuids) {
                    var toRemove = PluginManifestLookup.Where(x => x.Value.guid == ig).ToList();

                    // Show list of duplicate plugins, fix just opens plugin folder, retry will re-initialize
                    var sb = new StringBuilder();
                    //sb.AppendLine($"Duplicate plugin guid '{ig}' detected for these plugins:");
                    sb.AppendLine(string.Format(UiStrings.DupPluginGuid1, ig));
                    toRemove.ForEach(x => sb.AppendLine(x.Value.title));
                    sb.AppendLine();
                    //sb.AppendLine($"Fix by changing plugin guid or removing duplicates. Otherwise all associated plugins will be ignored.");
                    sb.AppendLine(UiStrings.DupPluginGuid2);
                    var to_remove_min_refs = toRemove.Select(x => (x.Value.guid, x.Value.RootDirectory)).ToList();
                    toRemove = null;
                    bool can_continue = await ShowInterPluginInvalidationAsync(to_remove_min_refs, sb.ToString());
                    if (can_continue) {
                        continue;
                    }
                    // reload triggered
                    return;
                }
            }

            // DUP PRESET GUIDS

            foreach (var plugin in Plugins.ToList()) {
                foreach (var presetHost in plugin.PresetHosts.ToList()) {
                    if (presetHost.presets == null) {
                        continue;
                    }
                    foreach (var preset in presetHost.presets) {
                        if (string.IsNullOrEmpty(preset.guid)) {
                            continue;
                        }
                        var dup_preset_guid_plugins = Plugins.Where(x => x != plugin && x.PresetHosts.Any(y => y.presets != null && y.presets.Any(z => z.guid == preset.guid)));
                        if (!dup_preset_guid_plugins.Any()) {
                            continue;
                        }
                        // Show list of duplicate plugins, fix just opens plugin folder, retry will re-initialize
                        var sb = new StringBuilder();
                        //sb.AppendLine($"Duplicate plugin preset guid '{preset.guid}' detected for these plugins and presets:");
                        sb.AppendLine(string.Format(UiStrings.DupPresetGuid1, preset.guid));
                        //sb.AppendLine($"Plugin: {plugin.title} {Environment.NewLine}Preset(s): {(string.IsNullOrEmpty(preset.label) ? $"Preset #{presetHost.presets.IndexOf(preset)}" : preset.label)}");
                        if (string.IsNullOrEmpty(preset.label)) {
                            sb.AppendLine(string.Format(UiStrings.DupPresetGuid21, plugin.title, Environment.NewLine, presetHost.presets.IndexOf(preset)));
                        } else {
                            sb.AppendLine(string.Format(UiStrings.DupPresetGuid22, plugin.title, Environment.NewLine, preset.label));
                        }
                        foreach (var dup_preset_plugin in dup_preset_guid_plugins) {
                            sb.AppendLine();
                            var dup_presets = dup_preset_plugin.PresetHosts.Where(x => x.presets != null).SelectMany(x => x.presets).Where(x => x.guid == preset.guid);
                            sb.AppendLine($"{UiStrings.CommonPluginLabel} {dup_preset_plugin.title} {Environment.NewLine}{UiStrings.CommonPresetLabel} {string.Join(", ", dup_presets.Select((x, idx) => string.IsNullOrEmpty(x.label) ? $"{UiStrings.CommonPresetLabel} #{idx}" : x.label))}");
                        }

                        sb.AppendLine();
                        sb.AppendLine(UiStrings.DupPresetGuid3);
                        List<(string guid, string RootDirectory)> to_remove_min_refs = [(plugin.guid, plugin.RootDirectory)];
                        to_remove_min_refs.AddRange(dup_preset_guid_plugins.Select(x => (x.guid, x.RootDirectory)));

                        bool can_continue = await ShowInterPluginInvalidationAsync(to_remove_min_refs, sb.ToString());
                        if (can_continue) {
                            continue;
                        }
                        // reload triggered
                        return;
                    }
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
            // increment install stats for all updates 
            Task.WhenAll(UpdatedPluginGuids.Select(x => GetOrUpdatePluginStatsAsync(x, true))).FireAndForgetSafeAsync();
        }
        private static async Task<string> DownloadAndExtractPluginToDirAsync(string plugin_guid, string packageUrl, string targetBaseDir, MpICancelableProgressIndicatorViewModel cpivm) {
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
            string temp_package_zip_path = Path.Combine(target_dir, Path.GetRandomFileName());
            await MpHttpRequester.DownloadAsync(packageUrl, temp_package_zip_path, cpivm == null ? null : (a, b, c) => { return cpivm.UpdateProgress(a.Value, b, c.Value); });

            if (!temp_package_zip_path.IsFile() ||
                    MpFileIo.ReadBytesFromFile(temp_package_zip_path) is not { } package_bytes) {
                // download error, remove broken plugin dir
                MpFileIo.DeleteDirectory(target_dir);
                return null;
            }


            try {
                // extract zip to target dir
                ZipFile.ExtractToDirectory(temp_package_zip_path, target_dir);
                // delete zip
                if (!MpFileIo.DeleteFile(temp_package_zip_path)) {
                    MpConsole.WriteLine($"Warning! couldn't delete plugin zip from path '{temp_package_zip_path}'");
                } else {
                    MoveExtraRuntimeModules(target_dir);
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
        private static void MoveExtraRuntimeModules(string plugin_outer_dir) {
            // some assemblies end up in a sub folder /runtimes and won't load right, this moves those where 
            // they need to be so plugins (mine at least) can just be 1 package for all platforms

            if (!plugin_outer_dir.IsDirectory() ||
                Directory.GetDirectories(plugin_outer_dir) is not { } plugin_dirs ||
                plugin_dirs.FirstOrDefault() is not string plugin_dir ||
                Mp.Services.PlatformInfo.RuntimeShortName is not string runtime_name) {
                return;
            }

            try {
                string runtime_dir = Path.Combine(plugin_dir, "runtimes", runtime_name, "native");
                if (!runtime_dir.IsDirectory()) {
                    return;
                }
                MpFileIo.CopyContents(runtime_dir, plugin_dir, true, false);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"error moving extra runtimes.", ex);
            }

        }
        private static string CreatePluginBackup(string guid) {
            if (!PluginGuidLookup.TryGetValue(guid, out var plugin) ||
                !plugin.RootDirectory.IsDirectory()) {
                // not found
                return null;
            }
            string original_dir = plugin.RootDirectory;
            string dir_to_backup = Path.GetDirectoryName(original_dir);
            string backup_path = Path.Combine(PluginBackupDir, Path.GetFileName(dir_to_backup));
            if (backup_path.IsFileOrDirectory()) {
                if (!MpFileIo.DeleteFileOrDirectory(backup_path)) {
                    MpDebug.Break($"Error deleting existing backup dir '{backup_path}'");
                    return null;
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
            return backup_path;
        }
        #region Assembly

        [MethodImpl(MethodImplOptions.NoInlining)]
        static async Task<bool> UnloadPluginAsync(MpRuntimePlugin plugin) {
            await plugin.UnloadComponentsAsync();

            if (USE_LOADERS) {
                if (!_loaders.TryGetValue(plugin.guid, out var pl)) {
                    // not found
                    return true;
                }
                if (!pl.IsUnloadable) {
                    return false;
                }
                pl.Dispose();
                _loaders.Remove(plugin.guid);
            }

            return false;
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        static async Task LoadComponentsAsync(string manifestPath, MpRuntimePlugin plugin) {
            AssemblyLoadContext alc = null;
            plugin.manifestLastModifiedDateTime = File.GetLastWriteTime(manifestPath);
            string bundle_path = GetBundlePath(manifestPath, plugin);
            switch (plugin.packageType) {
                default:
                case MpPluginPackageType.Dll:
                    plugin.Components = LoadDll<MpIPluginComponentBase>(bundle_path, plugin.guid, out alc);
                    break;
                case MpPluginPackageType.Nuget:
                    if (LoadNuget(bundle_path, out alc) is { } nuget_assembly) {
                        plugin.Components = nuget_assembly.FindSubTypes<MpIPluginComponentBase>().ToArray();

                    }
                    break;
                case MpPluginPackageType.Python:
                    //component_assembly = Assembly.GetAssembly(typeof(MpPythonAnalyzerPlugin));
                    plugin.Components = new MpIPluginComponentBase[] { new MpPythonAnalyzerPlugin(bundle_path) };
                    break;
                    //case MpPluginPackageType.Http:
                    //    component_assembly = Assembly.GetAssembly(typeof(MpHttpAnalyzerPlugin));
                    //    components = new MpIPluginComponentBase[] { new MpHttpAnalyzerPlugin(plugin.analyzer.http) };
                    //    break;
            }

            plugin.LoadContext = alc;

            if (plugin.SharedAssemblyQualifiedName != SharedAssemblyQualifiedName) {
                MpConsole.WriteLine($"Warning! Plugin '{plugin.title}' has shared assembly mismatch.");
                MpConsole.WriteLine($"Expected: '{SharedAssemblyQualifiedName}'");
                MpConsole.WriteLine($"Actual: '{plugin.SharedAssemblyQualifiedName}'");
            }
            switch (plugin.pluginType) {
                case MpPluginType.Analyzer:
                    if (plugin.analyzer != null) {
                        break;
                    }
                    plugin.analyzer =
                        await plugin.IssueRequestAsync<MpAnalyzerComponent>(
                            nameof(MpISupportHeadlessAnalyzerFormat.GetFormat),
                            typeof(MpISupportHeadlessAnalyzerFormat).FullName,
                            new MpHeadlessComponentFormatRequest(), sync_only: true);
                    break;
                case MpPluginType.Clipboard:
                    if (plugin.oleHandler != null) {
                        break;
                    }
                    plugin.oleHandler =
                        await plugin.IssueRequestAsync<MpClipboardComponent>(
                            nameof(MpISupportHeadlessClipboardComponentFormat.GetFormats),
                            typeof(MpISupportHeadlessClipboardComponentFormat).FullName,
                            new MpHeadlessComponentFormatRequest(), sync_only: true);
                    break;
            }
        }
        private static IEnumerable<T> LoadDll<T>(string targetDllPath, string guid, out AssemblyLoadContext alc) {
            alc = null;
            try {
                if (USE_LOADERS) {
                    if (PluginLoader.CreateFromAssemblyFile(
                        assemblyFile: targetDllPath,
                        sharedTypes: sharedTypes,
                        isUnloadable: true) is { } pl) {
                        var objs = new List<T>();
                        var plugin_types = pl.LoadDefaultAssembly().GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract);
                        foreach (var pluginType in plugin_types) {
                            if (Activator.CreateInstance(pluginType) is T pcb) {
                                objs.Add(pcb);
                            }
                        }
                        _loaders.AddOrReplace(guid, pl);
                        return objs;
                    }
                }
                alc = new MpPluginAssemblyLoadContext(targetDllPath);
                var assembly = alc.LoadFromAssemblyPath(targetDllPath);
                var result = assembly.FindSubTypes<T>().ToArray();
                return result;
                //return Assembly.LoadFrom(targetDllPath);

                //return Assembly.Load(MpFileIo.ReadBytesFromFile(targetDllPath));

                //Assembly result = null;
                //var dir_dlls = Directory.GetFiles(Path.GetDirectoryName(targetDllPath)).Where(x => x.ToLower().EndsWith("dll"));
                //foreach (var dll_path in dir_dlls) {
                //    var assembly = Assembly.Load(MpFileIo.ReadBytesFromFile(dll_path));
                //    MpConsole.WriteLine($"{Path.GetFileNameWithoutExtension(targetDllPath)} loaded: {assembly.FullName}");
                //    if (dll_path == targetDllPath) {
                //        result = assembly;
                //    }
                //}
                //return result;

            }
            catch (Exception ex) {
                throw new MpUserNotifiedException($"Plugin Linking error '{targetDllPath}':{Environment.NewLine}{ex}");
            }
        }
        public static IDisposable GetPluginContext(string plugin_guid) {
            if (!_loaders.TryGetValue(plugin_guid, out var pl)) {
                return null;
            }
            return pl.EnterContextualReflection();
        }
        private static Assembly LoadNuget(string nupkgPath, out AssemblyLoadContext alc) {
            alc = null;

            try {
                using var archive = ZipFile.OpenRead(nupkgPath);
                string entryName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(nupkgPath)) + ".dll";
                var entry = archive.Entries.FirstOrDefault(x => x.Name.EndsWith(entryName));

                using var target = new MemoryStream();
                using var source = entry.Open();
                source.CopyTo(target);
                return Assembly.Load(target.ToArray());
            }
            catch (Exception ex) {
                throw new MpUserNotifiedException($"Plugin Linking error '{nupkgPath}':{Environment.NewLine}{ex}");
            }
        }

        private static string GetBundlePath(string manifestPath, MpRuntimePlugin plugin) {
            string bundle_ext = GetBundleExt(plugin.packageType, plugin.version);
            string bundle_dir = Path.GetDirectoryName(manifestPath);
            string bundle_file_name = Path.GetFileName(bundle_dir);
            string bundle_path = Path.Combine(bundle_dir, $"{bundle_file_name}.{bundle_ext}");

            if (plugin.packageType != MpPluginPackageType.Http && !bundle_path.IsFile()) {
                // not found
                throw new MpUserNotifiedException($"Error, Plugin '{plugin.title}' is flagged as {bundle_ext} type in '{manifestPath}' but does not have a matching '{bundle_file_name}.{bundle_ext}' in its folder.");
            }
            return bundle_path;
        }
        private static string GetBundleExt(MpPluginPackageType bt, string version) {
            switch (bt) {
                case MpPluginPackageType.Nuget:
                    return $".{version}.nupkg";
                case MpPluginPackageType.Python:
                    return "py";
                case MpPluginPackageType.Javascript:
                    return "js";
                default:
                case MpPluginPackageType.Dll:
                    return "dll";
            }
        }

        #region Extensions

        private static IEnumerable<T> FindSubTypes<T>(this Assembly pluginAssembly) {
            if (pluginAssembly == null) {
                return null;
            }

            IEnumerable<Type> avail_types = null;
            try {
                avail_types = pluginAssembly.ExportedTypes;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Exported types exception: ", ex);
                throw new MpUserNotifiedException($"Plugin dependency error for plugin '{pluginAssembly.FullName}': {Environment.NewLine}{ex.Message}");
            }
            if (avail_types == null) {
                return null;
            }
            IEnumerable<Type> obj_types = null;
            if (typeof(T).IsInterface) {
                string interfaceName = $"{typeof(T).Namespace}.{typeof(T).Name}";
                obj_types = avail_types.Where(x => x.GetInterface(interfaceName) != null);
            } else {
                obj_types = avail_types.Where(x => x is T);
            }
            if (!obj_types.Any()) {
                return null;
            }
            var objs = new List<T>();
            foreach (var obj_type in obj_types) {
                try {
                    if (Activator.CreateInstance(obj_type) is T pcb) {
                        objs.Add(pcb);
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Exported types exception: ", ex);
                    throw new MpUserNotifiedException($"Plugin activation error for plugin '{pluginAssembly.FullName}': {Environment.NewLine}{ex}");
                }
            }
            return objs;
        }
        #endregion
        #endregion

        #region Component
        private static bool ValidateParametersAndPresets(MpPresetParamaterHostBase host, string plugin_label) {
            if (host == null) {
                // undefined, ignore
                return true;
            }

            bool has_params = host.parameters != null && host.parameters.Count > 0;
            bool has_presets = host.presets != null && host.presets.Count > 0;
            if (!has_params && !has_presets) {
                return true;
            }
            if (has_presets && !has_params) {
                //throw new MpUserNotifiedException($"Plugin '{plugin_label}' cannot have presets without at least 1 parameter provided");
                throw new MpUserNotifiedException(string.Format(UiStrings.InvalidParamEx1, plugin_label));
            }

            // MISSING PARAMID's
            var missing_paramids = host.parameters.Where(x => string.IsNullOrEmpty(x.paramId));
            if (missing_paramids.Any()) {
                //string missing_param_labels = string.Join(",", missing_paramids.Select(x => string.IsNullOrEmpty(x.label) ? $"Unlabeled param #{host.parameters.IndexOf(x)}" : x.label));
                string missing_param_labels = string.Join(",", missing_paramids.Select(x => string.IsNullOrEmpty(x.label) ? $"{UiStrings.InvalidParamEx21.Format()}" : x.label));
                //string missing_param_ids_msg = $"Plugin parameter ids (paramId) must be defined. Plugin '{plugin_label}' has the following parameters with missing paramId's: {missing_param_labels}";
                string missing_param_ids_msg = UiStrings.InvalidParamEx3.Format(UiStrings.CommonParamIdLabel, plugin_label, missing_param_labels);
                throw new MpUserNotifiedException(missing_param_ids_msg);
            }

            // DUP PARAMID
            var dup_paramid_groups = host.parameters.GroupBy(x => x.paramId).Where(x => x.Count() > 1);
            foreach (var dup_param_id_grp in dup_paramid_groups) {
                //string dup_param_ids_msg = $"Duplicate paramId '{dup_param_id_grp.Key}' detected (all must be unique).{Environment.NewLine}Labels:{Environment.NewLine}{string.Join(Environment.NewLine, dup_param_id_grp.Select(x => x.label))}";
                string dup_param_ids_msg = UiStrings.InvalidParamEx4.Format(dup_param_id_grp.Key, Environment.NewLine, string.Join(Environment.NewLine, dup_param_id_grp.Select(x => x.label)));
                throw new MpUserNotifiedException(dup_param_ids_msg);
            }

            if (!has_presets) {
                return true;
            }
            foreach (var preset in host.presets) {
                string preset_label = string.IsNullOrWhiteSpace(preset.label) ? $"{UiStrings.CommonPresetLabel} #{host.presets.IndexOf(preset)}" : preset.label;

                // MISSING PRESET GUID
                if (!MpRegEx.RegExLookup[MpRegExType.Guid].IsMatch(preset.guid.ToStringOrEmpty())) {
                    //throw new MpUserNotifiedException($"Preset guid is required. It was not found or wrong format for '{preset_label}' in Plugin {plugin_label}");
                    throw new MpUserNotifiedException(UiStrings.InvalidParamEx5.Format(preset_label, plugin_label));
                }
                // NO PARAMID MATCHES
                var preset_param_vals_with_no_param_match =
                    preset.values.Where(x => host.parameters.All(y => y.paramId != x.paramId));
                foreach (var preset_param_val_with_no_param_match in preset_param_vals_with_no_param_match) {
                    //throw new MpUserNotifiedException($"Cannot find parameter with paramId '{preset_param_val_with_no_param_match.paramId}' referenced by Preset '{preset_label}' for Plugin '{plugin_label}'. Parameter may have changed or was removed, update preset paramValue or remove it.");
                    throw new MpUserNotifiedException(UiStrings.InvalidParamEx6.Format(preset_param_val_with_no_param_match.paramId, preset_label, plugin_label));
                }

                // SHARED VALUE PARAM IN PRESET
                var preset_vals_for_persistent_params =
                    preset.values.Where(x => host.parameters.FirstOrDefault(y => x.paramId == y.paramId).isSharedValue);
                foreach (var preset_val_for_persistent_params in preset_vals_for_persistent_params) {
                    //throw new MpUserNotifiedException($"Cannot set shared value parameters in Presets. ParamId '{preset_val_for_persistent_params.paramId}' in Preset '{preset_label}' for Plugin '{plugin_label}' needs to be removed or paramValue can be specified in the parameter definition section.");
                    throw new MpUserNotifiedException(UiStrings.InvalidParamEx7.Format(preset_val_for_persistent_params.paramId, preset_label, plugin_label));
                }
            }
            // DUPLICATE PRESET GUIDS
            var dup_preset_groups = host.presets.GroupBy(x => x.guid).Where(x => x.Count() > 1);
            foreach (var dup_preset_group in dup_preset_groups) {
                //throw new MpUserNotifiedException($"Presets must have unique guids. Duplicate preset guid '{dup_preset_group}' was detected in plugin '{plugin_label}' for presets: {string.Join(", ", dup_preset_group.Select((x, idx) => string.IsNullOrEmpty(x.label) ? $"Preset #{idx}" : x.label))}");
                throw new MpUserNotifiedException(UiStrings.InvalidParamEx8.Format(dup_preset_group, plugin_label, string.Join(", ", dup_preset_group.Select((x, idx) => string.IsNullOrEmpty(x.label) ? $"{UiStrings.CommonPresetLabel} #{idx}" : x.label))));
            }

            return true;
        }

        private static bool ValidatePluginComponents(MpRuntimePlugin plugin) {
            if ((plugin.pluginType == MpPluginType.Analyzer && plugin.analyzer == null) ||
                (plugin.pluginType == MpPluginType.Clipboard && plugin.oleHandler == null)) {
                //throw new MpUserNotifiedException($"Plugin error. Plugin '{plugin.title}' is '{plugin.pluginType}' type but no '{plugin.pluginType}' format found");
                throw new MpUserNotifiedException(UiStrings.InvalidCompEx1.Format(plugin.title, plugin.pluginType));
            }
            bool are_all_components_valid = plugin.PresetHosts.All(x => ValidateParametersAndPresets(x, plugin.title));
            return are_all_components_valid;
        }
        private static void AddPluginToDeleteList(MpRuntimePlugin plugin) {
            UninstalledPluginGuids.Add(plugin.guid);
            MpStartupCleaner.AddPathToDelete(plugin.RootDirectory);
            MpStartupCleaner.AddPathToDelete(plugin.CachePath);
            MpStartupCleaner.AddPathToDelete(plugin.UpdateDir);
        }
        private static void RemovePluginToDeleteList(MpRuntimePlugin plugin) {
            MpStartupCleaner.RemovePathToDelete(plugin.RootDirectory);
            MpStartupCleaner.RemovePathToDelete(plugin.CachePath);
            MpStartupCleaner.RemovePathToDelete(plugin.UpdateDir);
        }
        #endregion

        #region Backup Cache

        private static void UpdatePluginCache(MpRuntimePlugin plugin) {
            plugin.backupCheckPluginFormat = GetLastLoadedBackupPluginFormat(plugin, out bool isOutOfDate);
            plugin.IsManifestChangedFromBackup = isOutOfDate;

            if (plugin.backupCheckPluginFormat == null) {
                // initial backup create
                plugin.backupCheckPluginFormat = CreateLastLoadedBackupPluginFormat(plugin);
            }
        }

        private static string GetCachedPluginFileName(MpRuntimePlugin plugin) {
            if (plugin == null || plugin.guid == null) {
                return string.Empty;
            }
            return $"{plugin.guid}.json";
        }

        public static MpRuntimePlugin GetLastLoadedBackupPluginFormat(MpRuntimePlugin plugin, out bool isBackupOutOfDate) {
            isBackupOutOfDate = false;
            if (!PluginCacheDir.IsDirectory()) {
                return null;
            }
            string backup_manifest_fn = GetCachedPluginFileName(plugin);
            string backup_manifest_path = Path.Combine(PluginCacheDir, backup_manifest_fn);
            if (!backup_manifest_path.IsFile()) {
                return null;
            }

            string plugin_json_str = plugin.SerializeObject();
            string backup_manifest_str = MpFileIo.ReadTextFromFile(backup_manifest_path);
            // NOTE when backup doesn't match current manifest mark as out-of-date for reset in locators
            isBackupOutOfDate = plugin_json_str != backup_manifest_str;
            if (isBackupOutOfDate) {

            }
            if (!string.IsNullOrWhiteSpace(backup_manifest_str)) {
                try {
                    return MpJsonExtensions.DeserializeObject<MpRuntimePlugin>(backup_manifest_str);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error deserializing backup manifest at path: '{backup_manifest_path}' with data: '{backup_manifest_str}' ex: ", ex);
                }
            }

            // no backup or it is corrupt
            MpFileIo.WriteTextToFile(backup_manifest_path, plugin_json_str);

            return MpJsonExtensions.DeserializeObject<MpRuntimePlugin>(plugin_json_str);
        }
        public static MpRuntimePlugin CreateLastLoadedBackupPluginFormat(MpRuntimePlugin plugin) {
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
            string plugin_json_str = plugin.SerializeObject();
            MpFileIo.WriteTextToFile(backup_manifest_path, plugin_json_str);

            return MpJsonExtensions.DeserializeObject<MpRuntimePlugin>(plugin_json_str);
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
        private static string ResolveInvManifestPath(string some_manifest_path) {
            if (Path.GetDirectoryName(some_manifest_path).EndsWith("Resources")) {
                // cultured manifest path
                string inv_manifest_path = Path.Combine(
                    Path.GetDirectoryName(Path.GetDirectoryName(some_manifest_path)),
                    MANIFEST_INVARIANT_FILE_NAME);
                if (inv_manifest_path.IsFile()) {
                    return inv_manifest_path;
                }
                throw new FileNotFoundException(UiStrings.InvalidManifestEx1);
            }
            return some_manifest_path;
        }
        private static string ResolveLocalizedManifestPath(string inv_manifest_path) {
            // find closest manifest culture matching users culture
            if (!inv_manifest_path.IsFile()) {
                return null;
            }
            string localized_manifest_path = inv_manifest_path;
            string manifest_dir = Path.GetDirectoryName(inv_manifest_path);
            string res_dir = Path.Combine(manifest_dir, "Resources");
            if (res_dir.IsDirectory()) {
                string man_culture_code = MpLocalizationHelpers.FindClosestCultureCode(
                    target_culture_code: Mp.Services.UserCultureInfo.CultureCode,
                    dir: res_dir,
                    file_name_filter: MANIFEST_FILE_NAME_PREFIX);

                string localized_manifest_file_name =
                    $"{MANIFEST_FILE_NAME_PREFIX}.{man_culture_code}.{MANIFEST_FILE_EXT}".Replace("..", ".");

                localized_manifest_path =
                    Path.Combine(
                        res_dir,
                        localized_manifest_file_name);
            }


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
