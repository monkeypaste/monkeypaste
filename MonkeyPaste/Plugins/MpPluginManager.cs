using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using HtmlAgilityPack;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {
    public static class MpPluginManager {
        #region Properties

        public static Dictionary<string, MpPluginFormat> Plugins { get; set; } = new Dictionary<string, MpPluginFormat>();

        public static string PluginRootFolderPath => Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
        #endregion

        #region Public Methods

        public static async Task Init() {
            Plugins.Clear();
            //find plugin folder in main app folder

            if (!Directory.Exists(PluginRootFolderPath)) {
                //if plugin folder doesn't exist then no plugins so nothing to do
                return;
            }

            var manifestPaths = FindManifestPaths(PluginRootFolderPath);

            foreach (var manifestPath in manifestPaths) {
                var plugin = await LoadPlugin(manifestPath);
                if (plugin == null) {
                    continue;
                }
                Plugins.Add(manifestPath, plugin);
                MpConsole.WriteLine($"Successfully loaded plugin: {plugin.title}");
            }
        }

        #endregion

        #region Private Methods

        private static IEnumerable<string> FindManifestPaths(string root) {
            try {
                return Directory.EnumerateFiles(root, "manifest.json", SearchOption.AllDirectories);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Error scanning plug-in directory: " + root);
                MpConsole.WriteLine(ex);
                return new List<string>();
            }
        }

        private static async Task<MpPluginFormat> LoadPlugin(string manifestPath) {
            string manifestStr = MpFileIo.ReadTextFromFile(manifestPath);
            if (string.IsNullOrEmpty(manifestStr)) {
                var userAction = await MpNotificationCollectionViewModel.Instance.ShowNotification(
                    dialogType: MpNotificationDialogType.InvalidPlugin,
                    msg: $"Plugin manifest not found in '{manifestPath}'", 
                    retryAction: async (args) => { await LoadPlugin(manifestPath); },
                    fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(Path.GetDirectoryName(manifestPath))));


                //if (userAction == MpDialogResultType.Retry) {
                //    var retryPlugin = await LoadPlugin(manifestPath);
                //    return retryPlugin;
                //}
                return null;
            }
            MpPluginFormat plugin;
            try {
                plugin = JsonConvert.DeserializeObject<MpPluginFormat>(manifestStr);
            }
            catch (Exception ex) {
                var userAction = await MpNotificationCollectionViewModel.Instance.ShowNotification(
                        dialogType: MpNotificationDialogType.InvalidPlugin,
                        msg: $"Error parsing plugin manifest '{manifestPath}': {ex.Message}",
                        retryAction: async (args) => { await LoadPlugin(manifestPath); },
                        fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(Path.GetDirectoryName(manifestPath))));

                //if (userAction == MpDialogResultType.Retry) {
                //    var retryPlugin = await LoadPlugin(manifestPath);
                //    return retryPlugin;
                //}
                return null;
            }
            if (plugin != null) {
                try {
                    plugin.Component = GetPluginComponent(manifestPath, plugin);
                    plugin.RootDirectory = Path.GetDirectoryName(manifestPath);
                }
                catch (Exception ex) {
                    var userAction = await MpNotificationCollectionViewModel.Instance.ShowNotification(
                            dialogType: MpNotificationDialogType.InvalidPlugin,
                            msg: ex.Message,
                            retryAction: async (args) => { await LoadPlugin(manifestPath); },
                            fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(Path.GetDirectoryName(manifestPath))));


                    //if (userAction == MpDialogResultType.Retry) {
                    //    var retryPlugin = await LoadPlugin(manifestPath);
                    //    return retryPlugin;
                    //}
                    return null;
                }

            }
            return plugin;
        }

        private static object GetPluginComponent(string manifestPath, MpPluginFormat plugin) {
            plugin.manifestLastModifiedDateTime = File.GetLastWriteTime(manifestPath);
            string pluginDir = Path.GetDirectoryName(manifestPath);
            string pluginName = Path.GetFileName(pluginDir);
            if (plugin.ioType.isDll) {
                string dllPath = Path.Combine(pluginDir, string.Format(@"{0}.dll", pluginName));
                if (!File.Exists(dllPath)) {
                    throw new MpUserNotifiedException($"Error, Plugin '{pluginName}' is flagged as dll type in '{manifestPath}' but does not have a matching '{pluginName}.dll' in its folder.");
                }
                Assembly pluginAssembly = null;
                try {
                    pluginAssembly = Assembly.LoadFrom(dllPath);
                } catch(Exception rtle) {
                    throw new MpUserNotifiedException($"Plugin Compilation error '{pluginName}':" + Environment.NewLine + rtle);
                }
                int typeCount = 0;
                try {
                    typeCount = pluginAssembly.GetTypes().Length;
                }catch(ReflectionTypeLoadException rtle) {
                    throw new MpUserNotifiedException("Error loading "+pluginName+" ",rtle);
                }                


                for (int i = 0; i < typeCount; i++) {
                    var curType = pluginAssembly.GetTypes()[i];
                    if (curType.GetInterface("MonkeyPaste.Common.Plugin."+nameof(MpIPluginComponentBase)) != null) {
                        var pluginObj = Activator.CreateInstance(curType);
                        if (pluginObj != null) {
                            return pluginObj;
                        }
                    }
                }
            } else if (plugin.ioType.isCli) {
                string exePath = Path.Combine(pluginDir, string.Format(@"{0}.exe", pluginName));
                if (!File.Exists(exePath)) {
                    throw new MpUserNotifiedException($"Error, Plugin '{pluginName}' is flagged as a CLI type in '{manifestPath}' but does not have a matching '{pluginName}.exe' in its folder.");
                }
                return new MpCommandLinePlugin() { Endpoint = exePath };
            } else if (plugin.ioType.isHttp) {
                return new MpHttpPlugin(plugin.analyzer.http);
            }
            throw new MpUserNotifiedException(@"Unknown or undefined plugin type: " + JsonConvert.SerializeObject(plugin.ioType));
        }
        #endregion
    }
}
