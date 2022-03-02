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
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    public static class MpPluginManager {
        #region Properties

        public static Dictionary<string,MpPluginFormat> Plugins { get; set; } = new Dictionary<string,MpPluginFormat>();
        
        #endregion

        #region Public Methods

        public static async Task Init() {
            Plugins.Clear();
            //find plugin folder in main app folder
            string pluginRootFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

            if (!Directory.Exists(pluginRootFolderPath)) {
                //if plugin folder doesn't exist then no plugins so nothing to do
                return;
            }

            var manifestPaths = FindManifestPaths(pluginRootFolderPath);

            foreach(var manifestPath in manifestPaths) {
                var plugin = await LoadPlugin(manifestPath);
                if (plugin == null) {
                    continue;
                }
                Plugins.Add(manifestPath,plugin);
                MpConsole.WriteLine($"Successfully loaded plugin: {plugin.title}");
            }
        }

        public static async Task<MpPluginFormat> ReloadPlugin(MpPluginFormat plugin) {
            if(plugin == null || string.IsNullOrEmpty(plugin.guid)) {
                var userAction = await MpNotificationBalloonViewModel.Instance.ShowUserActions(
                    notificationType: MpNotificationDialogType.InvalidPlugin,
                    exceptionType: MpNotificationExceptionSeverityType.Error,
                    msg: "Error reloading plugin or guid null: " + plugin.title);
                return plugin;
            }
            var pkvp = Plugins.FirstOrDefault(x => x.Value.guid == plugin.guid);
            if(string.IsNullOrEmpty(pkvp.Key)) {                
                var userAction = await MpNotificationBalloonViewModel.Instance.ShowUserActions(
                    notificationType: MpNotificationDialogType.InvalidPlugin,
                    exceptionType: MpNotificationExceptionSeverityType.Error,
                    msg: $"Error reloading plugin '{plugin.title}' with guid '{plugin.guid}', manifest.json found.");

                if(userAction == MpDialogResultType.Retry) {
                    await Init();
                    return await ReloadPlugin(plugin);
                }
                return plugin;
            }
            plugin = await LoadPlugin(pkvp.Key);
            return plugin;
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
            string manifestStr = MpFileIoHelpers.ReadTextFromFile(manifestPath);
            if(string.IsNullOrEmpty(manifestStr)) {
                var userAction = await MpNotificationBalloonViewModel.Instance.ShowUserActions(
                    notificationType: MpNotificationDialogType.InvalidPlugin,
                    exceptionType: MpNotificationExceptionSeverityType.WarningWithOption,
                    msg: $"Plugin manifest not found in '{manifestPath}'");

                
                if(userAction == MpDialogResultType.Retry) {
                    var retryPlugin = await LoadPlugin(manifestPath);
                    return retryPlugin;
                }
                return null;
            }
            MpPluginFormat plugin = null;
            try {
                plugin = JsonConvert.DeserializeObject<MpPluginFormat>(manifestStr);                
            } catch(Exception ex) {                
                var userAction = await MpNotificationBalloonViewModel.Instance.ShowUserActions(
                        notificationType: MpNotificationDialogType.InvalidPlugin,
                        exceptionType: MpNotificationExceptionSeverityType.WarningWithOption,
                        msg: $"Error parsing plugin manifest '{manifestPath}': {ex.Message}");

                if (userAction == MpDialogResultType.Retry) {
                    var retryPlugin = await LoadPlugin(manifestPath);
                    return retryPlugin;
                }
                return null;
            }
            if(plugin != null) {
                try {
                    plugin.Component = GetPluginComponent(manifestPath, plugin);
                } catch(Exception ex) {
                    var userAction = await MpNotificationBalloonViewModel.Instance.ShowUserActions(
                            notificationType: MpNotificationDialogType.InvalidPlugin,
                            exceptionType: MpNotificationExceptionSeverityType.WarningWithOption,
                            msg: ex.Message);

                    if (userAction == MpDialogResultType.Retry) {
                        var retryPlugin = await LoadPlugin(manifestPath);
                        return retryPlugin;
                    }
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
                Assembly pluginAssembly = Assembly.LoadFrom(dllPath);
                for (int i = 0; i < pluginAssembly.GetTypes().Length; i++) {
                    var curType = pluginAssembly.GetTypes()[i];
                    if (curType.GetInterface("MonkeyPaste.Plugin.MpIPlugin") != null) {
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
