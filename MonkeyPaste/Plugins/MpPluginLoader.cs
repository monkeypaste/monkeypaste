using MonkeyPaste.Common;
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

namespace MonkeyPaste {
    public static class MpPluginLoader {
        #region Private Variables

        #endregion

        #region Constants

        public const string PLUG_FOLDER_NAME = "Plugins";
        public const string MANIFEST_BACKUP_FOLDER_NAME = "Cache";

        #endregion

        #region Properties
        public static Dictionary<string, MpPluginFormat> Plugins { get; set; } = new Dictionary<string, MpPluginFormat>();

        public static string PluginRootFolderPath =>
            Path.Combine(MpPlatform.Services.PlatformInfo.StorageDir, PLUG_FOLDER_NAME);
        public static string PluginManifestBackupFolderPath =>
            Path.Combine(MpPlatform.Services.PlatformInfo.StorageDir, MANIFEST_BACKUP_FOLDER_NAME);
        #endregion

        #region Public Methods

        public static async Task InitAsync() {

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Plugins.Clear();
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
            try {
                ValidateLoadedPlugins();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Plugin loader error, invalid plugins will be ignored: ", ex);
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
                    catch (Exception ex) {
                        var ass2 = Assembly.Load(assembly_name);
                        return ass2;
                    }
                }
            }
            return null;
        }

        public static async Task<MpPluginFormat> ReloadPluginAsync(string manifestPath) {
            if (!Plugins.ContainsKey(manifestPath)) {
                throw new Exception($"No plugin loaded from manifest path: '{manifestPath}'");
            }
            var reloaded_pf = await LoadPluginAsync(manifestPath);
            Plugins[manifestPath] = reloaded_pf;
            return reloaded_pf;
        }

        #endregion

        #region Private Methods

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
            string manifestStr = MpFileIo.ReadTextFromFile(manifestPath);
            if (string.IsNullOrEmpty(manifestStr)) {
                // Empty or io error on manifest file read
                var manifest_not_found_result = await MpNotificationBuilder.ShowNotificationAsync(
                    notificationType: MpNotificationType.InvalidPlugin,
                    body: $"Plugin manifest not found in '{manifestPath}'",
                    retryAction: retryFunc,
                    fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(Path.GetDirectoryName(manifestPath))));


                if (manifest_not_found_result == MpNotificationDialogResultType.Ignore) {
                    return null;
                }
                needsFixing = true;
            }
            if (!needsFixing) {
                try {
                    plugin = JsonConvert.DeserializeObject<MpPluginFormat>(manifestStr);

                    bool isValid = ValidatePluginManifest(plugin, manifestPath);
                }
                catch (Exception ex) {
                    var invalid_or_malformed_json_result = await MpNotificationBuilder.ShowNotificationAsync(
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
                        plugin.Component = GetPluginComponent(manifestPath, plugin);
                        plugin.RootDirectory = Path.GetDirectoryName(manifestPath);
                    }
                    catch (Exception ex) {
                        var ivalid_plugin_component_result = await MpNotificationBuilder.ShowNotificationAsync(
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
            plugin.backupCheckPluginFormat = GetLastLoadedBackupPluginFormat(plugin);
            if (plugin.backupCheckPluginFormat == null) {
                // initial backup create
                plugin.backupCheckPluginFormat = CreateLastLoadedBackupPluginFormat(plugin);
            }
            return plugin;
        }

        private static object GetPluginComponent(string manifestPath, MpPluginFormat plugin) {
            plugin.manifestLastModifiedDateTime = File.GetLastWriteTime(manifestPath);

            if (plugin.ioType.isHttp) {
                return new MpHttpAnalyzerPlugin(plugin.analyzer.http);
            }

            string pluginDir = Path.GetDirectoryName(manifestPath);
            string pluginName = Path.GetFileName(pluginDir);
            string pluginPathName = pluginName;
            string pluginExt = plugin.ioType.ToFileExt();
            if (plugin.ioType.isNuget) {
                pluginPathName += "." + plugin.version;
            }
            string pluginPath = Path.Combine(pluginDir, $"{pluginPathName}.{plugin.ioType.ToFileExt()}");

            if (!File.Exists(pluginPath)) {
                throw new MpUserNotifiedException($"Error, Plugin '{pluginName}' is flagged as {pluginExt} type in '{manifestPath}' but does not have a matching '{pluginName}.{pluginExt}' in its folder.");
            }
            if (plugin.ioType.isNuget) {
                return GetNugetComponent(pluginPath, pluginName);
            }
            if (plugin.ioType.isDll) {
                return GetDllComponent(pluginPath, pluginName);
            }
            if (plugin.ioType.isCli) {
                return new MpCommandLinePlugin() { Endpoint = pluginPath };
            }

            throw new MpUserNotifiedException(@"Unknown or undefined plugin type: " + JsonConvert.SerializeObject(plugin.ioType));
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
            if (string.IsNullOrWhiteSpace(plugin.iconUri)) {
                throw new MpUserNotifiedException($"Plugin icon error, at path '{manifestPath}' with title '{plugin.title}' must have an 'iconUri' property which is a relative file path or valid url to an image");
            }
            return true;
        }
        private static string ToFileExt(this MpPluginIoTypeFormat ioType) {
            if (ioType.isNuget) {
                return "nupkg";
            }
            if (ioType.isDll) {
                return "dll";
            }
            if (ioType.isCli) {

                switch (MpPlatform.Services.PlatformInfo.OsType) {
                    case MpUserDeviceType.Windows:
                        return "exe";
                    default:
                        throw new NotSupportedException("needs to be implemented");
                }
            }
            return string.Empty;
        }
        private static bool ValidateLoadedPlugins() {
            var invalidGuida = Plugins.GroupBy(x => x.Value.guid).Where(x => x.Count() > 1).Select(x => x.Key);

            if (invalidGuida.Count() > 0) {
                var sb = new StringBuilder();
                foreach (var ig in invalidGuida) {
                    var toRemove = Plugins.Where(x => x.Value.guid == ig);
                    toRemove.Select(x => sb.AppendLine($"Duplicate guids detected for plugin at path '{x.Key}' with guid '{x.Value.guid}'. Plugin will be ignored"));
                    foreach (var tr in toRemove) {
                        Plugins.Remove(tr.Key);
                    }
                }
                throw new MpUserNotifiedException(sb.ToString());
            }
            return true;
        }

        private static object GetDllComponent(string dllPath, string pluginName) {
            Assembly pluginAssembly;
            try {
                pluginAssembly = Assembly.LoadFrom(dllPath);
            }
            catch (Exception rtle) {
                throw new MpUserNotifiedException($"Plugin Compilation error '{pluginName}':" + Environment.NewLine + rtle);
            }
            return GetComponentFromAssembly(pluginAssembly, pluginName);
        }
        private static object GetNugetComponent(string nupkgPath, string pluginName) {
            using var archive = ZipFile.OpenRead(nupkgPath);
            var entry = archive.Entries.FirstOrDefault(x => x.Name.EndsWith($"{pluginName}.dll"));

            using var target = new MemoryStream();

            using var source = entry.Open();
            source.CopyTo(target);

            var assembly = Assembly.Load(target.ToArray());
            return GetComponentFromAssembly(assembly, pluginName);
        }

        private static object GetComponentFromAssembly(Assembly pluginAssembly, string pluginName) {
            if (pluginAssembly == null) {
                return null;
            }
            int typeCount = 0;
            try {
                typeCount = pluginAssembly.GetTypes().Length;
            }
            catch (ReflectionTypeLoadException rtle) {
                throw new MpUserNotifiedException("Error loading " + pluginName + " ", rtle);
            }
            for (int i = 0; i < typeCount; i++) {
                var curType = pluginAssembly.GetTypes()[i];
                if (curType.GetInterface("MonkeyPaste.Common.Plugin." + nameof(MpIPluginComponentBase)) != null) {
                    var pluginObj = Activator.CreateInstance(curType);
                    if (pluginObj != null) {
                        return pluginObj;
                    }
                }
            }
            return null;
        }

        public static MpPluginFormat GetLastLoadedBackupPluginFormat(MpPluginFormat plugin) {
            if (!PluginManifestBackupFolderPath.IsDirectory()) {
                return null;
            }
            string backup_manifest_fn = $"{plugin.guid}.json";
            string backup_manifest_path = Path.Combine(PluginManifestBackupFolderPath, backup_manifest_fn);
            if (!backup_manifest_path.IsFile()) {
                return null;
            }

            string backup_manifest_data = MpFileIo.ReadTextFromFile(backup_manifest_path);
            if (!string.IsNullOrWhiteSpace(backup_manifest_data)) {
                try {
                    return MpJsonConverter.DeserializeObject<MpPluginFormat>(backup_manifest_data);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error deserializing backup manifest at path: '{backup_manifest_path}' with data: '{backup_manifest_data}' ex: ", ex);
                }
            }

            // no backup or it is corrupt
            string plugin_json_str = plugin.SerializeJsonObject();
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
    }
}
