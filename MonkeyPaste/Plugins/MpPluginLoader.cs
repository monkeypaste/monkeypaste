﻿using System;
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
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
using System.ComponentModel;
using System.Collections;

namespace MonkeyPaste {
    public static class MpPluginLoader {
        #region Private Variables

        #endregion

        #region Constants

        public const string PLUG_FOLDER_NAME = "Plugins";

        #endregion


        #region Properties

        //#region INotifyDataErrorInfo Implementation

        //private readonly Dictionary<string, List<string>> _errorsByPropertyName = new Dictionary<string, List<string>>();

        //public bool HasErrors => _errorsByPropertyName.Any();

        //public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        //public IEnumerable GetErrors(string propertyName) {
        //    return _errorsByPropertyName.ContainsKey(propertyName) ?
        //        _errorsByPropertyName[propertyName] : null;
        //}

        //private void OnErrorsChanged(string propertyName) {
        //    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        //}

        //#endregion

        public static Dictionary<string, MpPluginFormat> Plugins { get; set; } = new Dictionary<string, MpPluginFormat>();

        public static string PluginRootFolderPath => Path.Combine(Directory.GetCurrentDirectory(), PLUG_FOLDER_NAME);
        #endregion

        #region Public Methods

        public static async Task InitAsync() {
            Plugins.Clear();
            //find plugin folder in main app folder

            if (!Directory.Exists(PluginRootFolderPath)) {
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
            }catch(Exception ex) {
                MpConsole.WriteLine("Plugin loader error, invalid plugins will be ignored: ", ex); 
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

        private static async Task<MpPluginFormat> LoadPluginAsync(string manifestPath) {
            string manifestStr = MpFileIo.ReadTextFromFile(manifestPath);
            if (string.IsNullOrEmpty(manifestStr)) {
                var userAction = await MpNotificationCollectionViewModel.Instance.ShowNotification(
                    dialogType: MpNotificationDialogType.InvalidPlugin,
                    msg: $"Plugin manifest not found in '{manifestPath}'", 
                    retryAction: async (args) => { await LoadPluginAsync(manifestPath); },
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
                bool isValid = ValidatePluginManifest(plugin,manifestPath);
            }
            catch (Exception ex) {
                var userAction = await MpNotificationCollectionViewModel.Instance.ShowNotification(
                        dialogType: MpNotificationDialogType.InvalidPlugin,
                        msg: $"Error parsing plugin manifest '{manifestPath}': {ex.Message}",
                        retryAction: async (args) => { await LoadPluginAsync(manifestPath); },
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
                            retryAction: async (args) => { await LoadPluginAsync(manifestPath); },
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

        private static bool ValidatePluginManifest(MpPluginFormat plugin,string manifestPath) {
            if(plugin == null) {
                throw new MpUserNotifiedException($"Plugin parsing error, at path '{manifestPath}' null, likely error parsing json. Ignoring plugin");
            }
            if(string.IsNullOrWhiteSpace(plugin.title)) {
                throw new MpUserNotifiedException($"Plugin title error, at path '{manifestPath}' must have 'title' property. Ignoring plugin");
            }
            if (!MpRegEx.RegExLookup[MpRegExType.Guid].IsMatch(plugin.guid)) {
                throw new MpUserNotifiedException($"Plugin guid error, at path '{manifestPath}' with Title '{plugin.title}' must have a 'guid' property, RFC 4122 compliant 128-bit GUID (UUID). Ignoring plugin");
            }
            if(string.IsNullOrWhiteSpace(plugin.iconUri)) {
                throw new MpUserNotifiedException($"Plugin icon error, at path '{manifestPath}' with title '{plugin.title}' must have an 'iconUri' property which is a relative file path or valid url to an image");
            }
            return true;
        }

        private static bool ValidateLoadedPlugins() {
            var invalidGuida = Plugins.GroupBy(x => x.Value.guid).Where(x => x.Count() > 1).Select(x => x.Key);

            if (invalidGuida.Count() > 0) {
                var sb = new StringBuilder();
                foreach(var ig in invalidGuida) {
                    var toRemove = Plugins.Where(x => x.Value.guid == ig);
                    toRemove.Select(x => sb.AppendLine($"Duplicate guids detected for plugin at path '{x.Key}' with guid '{x.Value.guid}'. Plugin will be ignored"));
                    foreach(var tr in toRemove) {
                        Plugins.Remove(tr.Key);
                    }                    
                }
                throw new MpUserNotifiedException(sb.ToString());
            }
            return true;
        }
        #endregion
    }
}