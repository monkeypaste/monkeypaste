﻿using MonkeyPaste.Common;

using MonkeyPaste.Common.Plugin;
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

        #region Statics

        public static string PLUG_FOLDER_NAME => "Plugins";
        public static string MANIFEST_BACKUP_FOLDER_NAME => ".cache";
        public static string DAT_FOLDER_NAME => "dat";

        public static string CoreClipboardHandlerGuid => "cf2ec03f-9edd-45e9-a605-2a2df71e03bd";
        public static string CoreAnnotatorGuid => "ecde8e7c-30cf-47ef-a6a9-8f7f439b0a31";
        public static string CoreAnnotatorDefaultPresetGuid => "a9fa2fbf-025d-4ced-a23b-234085b5ac5f";

        #endregion

        #region Properties
        public static Dictionary<string, MpPluginFormat> Plugins { get; set; } = new Dictionary<string, MpPluginFormat>();
        public static bool IsLoaded { get; private set; }
        public static string PluginRootFolderPath =>
            Path.Combine(Mp.Services.PlatformInfo.StorageDir, PLUG_FOLDER_NAME);
        public static string PluginManifestBackupFolderPath =>
            Path.Combine(PluginRootFolderPath, MANIFEST_BACKUP_FOLDER_NAME);
        static string CoreDatDir =>
            Path.Combine(Mp.Services.PlatformInfo.ExecutingDir, DAT_FOLDER_NAME);

        static string[] CorePluginGuids => new string[] {
            CoreClipboardHandlerGuid,
            CoreAnnotatorGuid
        };


        #endregion

        #region Public Methods

        public static async Task InitAsync() {
            IsLoaded = false;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            await LoadPluginsAsync();

            await ValidateLoadedPluginsAsync();
            IsLoaded = true;
        }
        public static async Task CheckAndInstallCorePluginsAsync() {
            MpDebug.Assert(CoreDatDir.IsDirectory(), $"Dat dir error, '{CoreDatDir}' does not exist");
            if (!PluginRootFolderPath.IsDirectory()) {
                bool success = MpFileIo.CreateDirectory(PluginRootFolderPath);
                MpDebug.Assert(success, $"Error creating root plugin folder at path '{PluginRootFolderPath}'");
                if (!success) {
                    return;
                }
            }
            foreach (var core_guid in CorePluginGuids) {
                if (Plugins.ContainsKey(core_guid)) {
                    // exists
                    continue;
                }
                string core_plugin_zip_path = Path.Combine(CoreDatDir, $"{core_guid}.zip");
                MpDebug.Assert(core_plugin_zip_path.IsFile(), $"Dat zip error, core plugin not found at '{core_plugin_zip_path}'");
                _ = await InstallPluginAsync(core_plugin_zip_path.ToFileSystemUriFromPath(), true);
                MpConsole.WriteLine($"Core plugin '{core_plugin_zip_path}' installed successfully.");
            }
        }


        public static async Task<MpPluginFormat> ReloadPluginAsync(string manifestPath) {
            if (!Plugins.ContainsKey(manifestPath)) {
                throw new Exception($"No plugin loaded from manifest path: '{manifestPath}'");
            }
            var reloaded_pf = await LoadPluginAsync(manifestPath);
            Plugins[manifestPath] = reloaded_pf;
            return reloaded_pf;
        }


        public static bool ValidatePluginDependencies(MpPluginFormat plugin) {
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

                try {
                    MpFileIo.CopyDirectory(dir_to_backup, backup_path, true, true);
                }
                catch (Exception ex) {
                    Mp.Services.NotificationBuilder.ShowNotificationAsync(
                        notificationType: MpNotificationType.FileIoWarning,
                        body: $"Error backing up {dir_to_backup} to '{backup_path}. Details: '{ex.Message}'").FireAndForgetSafeAsync();
                    backup_path = null;
                }
            }
            return backup_path;
        }
        public static bool DeletePlugin(string guid, bool delete_cache = true) {
            if (Plugins.All(x => x.Value.guid != guid)) {
                // not found
                return false;
            }
            var plugin_kvp = Plugins.FirstOrDefault(x => x.Value.guid == guid);
            string manifest_path = plugin_kvp.Key;

            bool success = true;
            if (manifest_path.IsFile()) {
                // delete plugin folder
                string dir_to_remove = Path.GetDirectoryName(manifest_path);
                if (!MpFileIo.DeleteDirectory(dir_to_remove)) {
                    success = false;
                    MpConsole.WriteLine($"Error deleting plugin folder '{dir_to_remove}'");
                }
            }
            if (delete_cache) {
                string cache_path = Path.Combine(PluginManifestBackupFolderPath, GetCachedPluginFileName(plugin_kvp.Value));
                if (cache_path.IsFile()) {
                    if (!MpFileIo.DeleteFile(cache_path)) {
                        success = false;
                        MpConsole.WriteLine($"Error deleting plugin cache file '{cache_path}'");
                    }
                }
            }
            // remove from loader
            if (!Plugins.Remove(manifest_path)) {
                MpConsole.WriteLine($"Loaded plugin '{manifest_path}' not found");
            }
            return success;
        }

        public static async Task<MpPluginFormat> InstallPluginAsync(string packageUrl, bool silentInstall = false) {
            try {
                // download package (should always be a zip file of the plugins root folder or contents of root folder)
                var package_bytes = await MpFileIo.ReadBytesFromUriAsync(packageUrl, string.Empty, 30_000);

                // write zip to temp
                string temp_package_zip = MpFileIo.WriteByteArrayToFile(Path.GetTempFileName(), package_bytes, true);

                // 
                string temp_package_dir = Path.GetRandomFileName();
                string pluginName = null;
                // extract to ../Plugins
                try {
                    // extract zip to temp folder and get inner folder name
                    ZipFile.ExtractToDirectory(temp_package_zip, temp_package_dir);


                    if (!temp_package_dir.IsDirectory() ||
                        Directory.GetDirectories(temp_package_dir) is not string[] tpfl ||
                        tpfl.Length == 0) {
                        throw new Exception($"Error extracting plugin from '{temp_package_dir}'");
                    }

                    pluginName = Path.GetFileName(tpfl[0]);
                    string dest_dir = Path.Combine(PluginRootFolderPath, pluginName);
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
                        body: $"Error installing plugin: {ex.Message}").FireAndForgetSafeAsync();
                    return null;
                }

                string manifest_path = Path.Combine(PluginRootFolderPath, pluginName, "manifest.json");
                if (!manifest_path.IsFile()) {
                    Mp.Services.NotificationBuilder.ShowNotificationAsync(
                        notificationType: MpNotificationType.FileIoWarning,
                        body: $"Error installing plugin '{pluginName}' corrupt or improper directory structure. Manifest should exist at '{manifest_path}' but was not found.").FireAndForgetSafeAsync();
                    return null;
                }

                var result = await LoadPluginAsync(manifest_path);
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

        public static MpNotificationFormat CreateInvalidPluginNotification(string msg, MpPluginFormat pf) {
            return new MpNotificationFormat() {
                Title = $"{pf.title} Error",
                Body = msg,
                NotificationType = MpNotificationType.InvalidPlugin,
                FixCommand = new MpCommand(() => MpFileIo.OpenFileBrowser(Path.Combine(pf.RootDirectory, "manifest.json")))
            };
        }
        #endregion

        #region Private Methods

        private static async Task LoadPluginsAsync() {
            Plugins.Clear();

            if (Mp.Services.StartupState.StartupFlags.HasFlag(MpStartupFlags.Initial)) {
                // install core plugins
                await CheckAndInstallCorePluginsAsync();
            }
            //find plugin folder in main app folder

            if (!Directory.Exists(PluginRootFolderPath)) {
                MpConsole.WriteLine("Plugin folder missing from: " + PluginRootFolderPath);
                // if plugin folder doesn't exist then no plugins so nothing to do but it should                
                return;
            }

            var manifestPaths = FindManifestPaths(PluginRootFolderPath);
            foreach (var manifestPath in manifestPaths) {
                var plugin = await LoadPluginAsync(manifestPath);
                if (plugin == null) {
                    continue;
                }
                Plugins.Add(manifestPath, plugin);
                MpConsole.WriteLine($"Successfully loaded plugin: {plugin.title}");
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
        private static void PreparePluginForInstall(string temp_plugin_dir) {

        }
        private static IEnumerable<string> FindManifestPaths(string root) {
            try {
                return Directory.EnumerateFiles(root, "manifest.json", SearchOption.AllDirectories);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Error scanning plug-in directory: " + root, ex);
                return new List<string>();
            }
        }

        private static async Task<MpPluginFormat> LoadPluginAsync(string manifestPath) {
            bool needsFixing = false;
            Func<object, object> retryFunc = (args) => {
                needsFixing = false;
                return null;
            };

            MpPluginFormat plugin = null;

            if (!needsFixing) {
                try {
                    string manifestStr = MpFileIo.ReadTextFromFile(manifestPath);
                    plugin = JsonConvert.DeserializeObject<MpPluginFormat>(manifestStr);
                    plugin.RootDirectory = Path.GetDirectoryName(manifestPath);

                    if (!ValidatePluginDependencies(plugin)) {
                        return null;
                    }

                    bool isValid = ValidatePluginManifest(plugin, manifestPath);
                }
                catch (Exception ex) {
                    var invalid_or_malformed_json_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                            notificationType: MpNotificationType.InvalidPlugin,
                            body: $"Error parsing plugin manifest '{manifestPath}': {ex.Message}",
                            retryAction: retryFunc,
                            fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(manifestPath)));
                    if (invalid_or_malformed_json_result == MpNotificationDialogResultType.Ignore) {
                        return null;
                    }
                    needsFixing = true;
                }

                if (!needsFixing) {
                    try {

                        object[] components = GetPluginComponents(manifestPath, plugin, out Assembly component_assembly);
                        LoadAnyHeadlessFormats(plugin, component_assembly, plugin.title);

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
                while (needsFixing) {
                    await Task.Delay(100);
                }
                return await LoadPluginAsync(manifestPath);
            }

            // only once manifest is validated get manifest backup
            UpdatePluginCache(plugin);
            return plugin;
        }

        #region Compnent

        private static object[] GetPluginComponents(string manifestPath, MpPluginFormat plugin, out Assembly component_assembly) {
            plugin.manifestLastModifiedDateTime = File.GetLastWriteTime(manifestPath);
            string bundle_path = GetBundlePath(manifestPath, plugin);
            switch (plugin.packageType) {
                case MpPluginPackageType.None:
                    throw new MpUserNotifiedException($"Error, Plugin '{plugin.title}' defined in '{manifestPath}' must specify a bundle type.");
                case MpPluginPackageType.Dll:
                    return GetDllComponents(bundle_path, plugin.title, out component_assembly);
                case MpPluginPackageType.Nuget:
                    return GetNugetComponent(bundle_path, plugin.title, out component_assembly);
                case MpPluginPackageType.Python:
                    component_assembly = Assembly.GetAssembly(typeof(MpPythonAnalyzerPlugin));
                    return new object[] { new MpPythonAnalyzerPlugin(bundle_path) };
                case MpPluginPackageType.Http:
                    component_assembly = Assembly.GetAssembly(typeof(MpHttpAnalyzerPlugin));
                    return new object[] { new MpHttpAnalyzerPlugin(plugin.analyzer.http) };
                default:
                    throw new MpUserNotifiedException($"Unhandled plugin bundle type for '{plugin.title}' defined at '{manifestPath}' with type '{plugin.packageType}'");
            }

        }
        private static bool ValidatePluginComponents(MpPluginFormat plugin, object[] components, string manifestPath) {
            return components.All(x => ValidatePluginComponent(plugin, x, manifestPath));
        }

        private static bool ValidatePluginComponent(MpPluginFormat plugin, object component, string manifestPath) {
            if (component is MpIAnalyzeAsyncComponent || component is MpIAnalyzeComponent) {
                if (plugin.analyzer == null) {
                    throw new MpUserNotifiedException($"Plugin error '{manifestPath}' must define 'analyzer' definition for '{component.GetType()}' which implements analyzer interface.");
                }
            } else if (component is MpIOlePluginComponent &&
                    (plugin is not MpPluginFormat avpf ||
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

        private static string GetBundlePath(string manifestPath, MpPluginFormat plugin) {
            string bundle_ext = GetBundleExt(plugin.packageType, plugin.version);
            string bundle_dir = Path.GetDirectoryName(manifestPath);
            string bundle_file_name = Path.GetFileName(bundle_dir);
            string bundle_path = Path.Combine(bundle_dir, $"{bundle_file_name}.{bundle_ext}");

            if (plugin.packageType != MpPluginPackageType.Http && !bundle_path.IsFile()) {
                // not found
                throw new MpUserNotifiedException($"Error, Plugin '{plugin.title}' is flagged as {bundle_ext} type in '{manifestPath}' but does not have a matching '{plugin.title}.{bundle_ext}' in its folder.");
            }
            return bundle_path;
        }
        private static string GetBundleExt(MpPluginPackageType bt, string version) {
            switch (bt) {
                case MpPluginPackageType.Nuget:
                    return $".{version}.nupkg";
                case MpPluginPackageType.Dll:
                    return "dll";
                case MpPluginPackageType.Python:
                    return "py";
                case MpPluginPackageType.Javascript:
                    return "js";
                default:
                    return string.Empty;
            }
        }
        private static object[] GetDllComponents(string dllPath, string pluginName, out Assembly component_assembly) {
            try {
                //component_assembly = Assembly.LoadFrom(dllPath);
                component_assembly = Assembly.Load(File.ReadAllBytes(dllPath));
            }
            catch (Exception rtle) {
                throw new MpUserNotifiedException($"Plugin Compilation error '{pluginName}':" + Environment.NewLine + rtle);
            }
            return GetComponentsFromAssembly(component_assembly, pluginName);
        }
        private static object[] GetNugetComponent(string nupkgPath, string pluginName, out Assembly component_assembly) {
            using var archive = ZipFile.OpenRead(nupkgPath);
            var entry = archive.Entries.FirstOrDefault(x => x.Name.EndsWith($"{pluginName}.dll"));

            using var target = new MemoryStream();

            using var source = entry.Open();
            source.CopyTo(target);

            component_assembly = Assembly.Load(target.ToArray());
            return GetComponentsFromAssembly(component_assembly, pluginName);
        }

        private static object[] GetComponentsFromAssembly(Assembly pluginAssembly, string pluginName) {
            IEnumerable<object> comp_objs;
            string comp_base_interface_name = $"{typeof(MpIPluginComponentBase).Namespace}.{typeof(MpIPluginComponentBase).Name}";
            try {
                comp_objs = GetInterfaceObjsFromAssembly(pluginAssembly, comp_base_interface_name, pluginName);
            }
            catch (Exception ex) {
                if (ex is MpUserNotifiedException) {
                    throw;
                }
                throw new MpUserNotifiedException("Error loading " + pluginName + " ", ex);
            }
            return comp_objs.ToArray();
        }
        #endregion


        private static void LoadAnyHeadlessFormats(MpPluginFormat plugin, Assembly pluginAssembly, string pluginName) {
            if (pluginAssembly == null || plugin == null) {
                return;
            }
            var headless_interfaces = new Type[] {
                typeof(MpISupportHeadlessAnalyzerComponentFormat),
                typeof(MpISupportHeadlessClipboardComponentFormat)
            };
            foreach (var hi in headless_interfaces) {
                string hi_name = $"{hi.Namespace}.{hi.Name}";
                try {
                    var hi_objs = GetInterfaceObjsFromAssembly(pluginAssembly, hi_name, pluginName);
                    if (hi_objs == null) {
                        continue;
                    }
                    if (hi_objs.OfType<MpISupportHeadlessAnalyzerComponentFormat>().FirstOrDefault()
                        is MpISupportHeadlessAnalyzerComponentFormat apf) {
                        plugin.analyzer = apf.GetFormat();
                    } else if (plugin is MpPluginFormat avplugin && hi_objs.OfType<MpISupportHeadlessClipboardComponentFormat>().FirstOrDefault()
                                is MpISupportHeadlessClipboardComponentFormat hccf) {

                        avplugin.oleHandler = hccf.GetFormats();
                    }
                }
                catch (Exception ex) {
                    if (ex is MpUserNotifiedException) {
                        throw;
                    }
                    throw new MpUserNotifiedException("Error loading " + plugin.title + " ", ex);
                }
            }
        }

        private static bool ValidatePluginManifest(MpPluginFormat plugin, string manifestPath) {
            if (plugin == null) {
                throw new MpUserNotifiedException($"Plugin parsing error, at path '{manifestPath}' null, likely error parsing json. Ignoring plugin");
            }

            if (string.IsNullOrWhiteSpace(plugin.title)) {
                throw new MpUserNotifiedException($"Plugin title error, at path '{manifestPath}' must have 'title' property. Ignoring plugin");
            }
            if (!MpRegEx.RegExLookup[MpRegExType.Guid].IsMatch(plugin.guid)) {
                throw new MpUserNotifiedException($"Plugin guid error, at path '{manifestPath}' with Title '{plugin.title}' must have a 'guid' property, RFC 4122 compliant 128-bit GUID (UUID). Ignoring plugin");
            }

            //bool is_icon_valid = await MpFileIo.IsAccessibleUriAsync(plugin.iconUri, plugin.RootDirectory);
            //if (!is_icon_valid) {
            //    throw new MpUserNotifiedException($"Plugin icon error, at path '{manifestPath}' with iconUri '{plugin.iconUri}' must have an 'iconUri' property which is a relative file path or valid url to an image");
            //}
            bool are_all_components_valid = plugin.componentFormats.All(x => ValidatePluginComponentManifest(x, manifestPath));
            return are_all_components_valid;
        }

        private static async Task ValidateLoadedPluginsAsync() {
            bool needsFixing = false;
            Func<object, object> retryFunc = (args) => {
                needsFixing = false;
                return null;
            };

            var invalidGuids =
                Plugins
                .GroupBy(x => x.Value.guid)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key);

            if (invalidGuids.Any()) {

                foreach (var ig in invalidGuids) {
                    var toRemove = Plugins.Where(x => x.Value.guid == ig);

                    // Show list of duplicate plugins, fix just opens plugin folder, retry will re-initialize
                    var sb = new StringBuilder();
                    sb.AppendLine($"Duplicate plugin identifiers detected for these plugins (name | path | id):");
                    toRemove.ForEach(x => sb.Append(string.Join(Environment.NewLine, new[] { string.Empty, x.Value.title, x.Key, x.Value.guid, string.Empty })));
                    sb.AppendLine();
                    sb.AppendLine($"Fix by changing plugin guid or removing duplicates. Otherwise all will be ignored");

                    var dup_guids_detected_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                            notificationType: MpNotificationType.InvalidPlugin,
                            body: sb.ToString(),
                            retryAction: retryFunc,
                            fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(PluginRootFolderPath)));
                    if (dup_guids_detected_result == MpNotificationDialogResultType.Ignore) {
                        foreach (var tr in toRemove) {
                            Plugins.Remove(tr.Key);
                        }
                        continue;
                    }
                    needsFixing = true;
                    while (needsFixing) {
                        await Task.Delay(100);
                    }
                    await LoadPluginsAsync();
                    // block initial call until completely done 
                    await ValidateLoadedPluginsAsync();
                    return;

                }
            }
        }



        private static IEnumerable<object> GetInterfaceObjsFromAssembly(Assembly pluginAssembly, string interfaceName, string pluginName) {
            if (pluginAssembly == null) {
                return null;
            }
            IEnumerable<Type> avail_types = null;
            try {
                avail_types = pluginAssembly.ExportedTypes;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Exported types exception: ", ex);
                throw new MpUserNotifiedException($"Plugin dependency error for plugin '{pluginName}': {Environment.NewLine}{ex.Message}");
            }
            if (avail_types == null) {
                return null;
            }
            var obj_types = avail_types.Where(x => x.GetInterface(interfaceName) != null);
            if (!obj_types.Any()) {
                return null;
            }
            List<object> objs = new List<object>();
            foreach (var obj_type in obj_types) {
                try {
                    var interface_obj = Activator.CreateInstance(obj_type);
                    objs.Add(interface_obj);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Exported types exception: ", ex);
                    throw new MpUserNotifiedException($"Plugin activation error for plugin '{pluginName}': {Environment.NewLine}{ex.Message}");
                }
            }
            return objs.ToArray();
        }

        #region Backup Cache

        private static void UpdatePluginCache(MpPluginFormat plugin) {
            plugin.backupCheckPluginFormat = GetLastLoadedBackupPluginFormat(plugin, out bool isOutOfDate);
            plugin.IsManifestChangedFromBackup = isOutOfDate;

            if (plugin.backupCheckPluginFormat == null) {
                // initial backup create
                plugin.backupCheckPluginFormat = CreateLastLoadedBackupPluginFormat(plugin);
            }
        }

        private static string GetCachedPluginFileName(MpPluginFormat plugin) {
            if (plugin == null || plugin.guid == null) {
                return string.Empty;
            }
            return $"{plugin.guid}.json";
        }

        public static MpPluginFormat GetLastLoadedBackupPluginFormat(MpPluginFormat plugin, out bool isBackupOutOfDate) {
            isBackupOutOfDate = false;
            if (!PluginManifestBackupFolderPath.IsDirectory()) {
                return null;
            }
            string backup_manifest_fn = GetCachedPluginFileName(plugin);
            string backup_manifest_path = Path.Combine(PluginManifestBackupFolderPath, backup_manifest_fn);
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
                    return MpJsonConverter.DeserializeObject<MpPluginFormat>(backup_manifest_str);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error deserializing backup manifest at path: '{backup_manifest_path}' with data: '{backup_manifest_str}' ex: ", ex);
                }
            }

            // no backup or it is corrupt
            MpFileIo.WriteTextToFile(backup_manifest_path, plugin_json_str, false);

            return MpJsonConverter.DeserializeObject<MpPluginFormat>(plugin_json_str);
        }
        public static MpPluginFormat CreateLastLoadedBackupPluginFormat(MpPluginFormat plugin) {
            if (!PluginManifestBackupFolderPath.IsDirectory()) {
                try {
                    Directory.CreateDirectory(PluginManifestBackupFolderPath);
                }
                catch (Exception ex) {
                    throw new MpUserNotifiedException(ex.Message);
                }
            }
            string backup_manifest_fn = $"{plugin.guid}.json";
            string backup_manifest_path = Path.Combine(PluginManifestBackupFolderPath, backup_manifest_fn);
            // no backup or it is corrupt
            string plugin_json_str = plugin.SerializeJsonObject();
            MpFileIo.WriteTextToFile(backup_manifest_path, plugin_json_str, false);

            return MpJsonConverter.DeserializeObject<MpPluginFormat>(plugin_json_str);
        }

        #endregion
        #endregion
    }
}