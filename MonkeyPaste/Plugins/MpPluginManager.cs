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
        public static ObservableCollection<MpPluginFormat> Plugins { get; set; } = new ObservableCollection<MpPluginFormat>();
        #endregion

        #region Public Methods

        public static void Init() {
            //find plugin folder in main app folder
            string pluginRootFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

            if (!Directory.Exists(pluginRootFolderPath)) {
                //if plugin folder doesn't exist then no plugins so nothing to do
                return;
            }

            var manifestPaths = FindManifestPaths(pluginRootFolderPath);

            foreach(var manifestPath in manifestPaths) {
                var plugin = LoadPlugin(manifestPath);
                if (plugin == null) {
                    continue;
                }
                Plugins.Add(plugin);
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

        private static MpPluginFormat LoadPlugin(string manifestPath) {
            string manifestStr = MpFileIo.ReadTextFromFile(manifestPath);
            MpPluginFormat plugin = JsonConvert.DeserializeObject<MpPluginFormat>(manifestStr);
            plugin.Component = GetPluginComponent(manifestPath, plugin);
            return plugin;
        }

        private static object GetPluginComponent(string manifestPath, MpPluginFormat plugin) {
            try {
                plugin.manifestLastModifiedDateTime = File.GetLastWriteTime(manifestPath);
                string pluginDir = Path.GetDirectoryName(manifestPath);
                string pluginName = Path.GetFileName(pluginDir);
                if (plugin.ioType.isDll) {
                    string dllPath = Path.Combine(pluginDir, string.Format(@"{0}.dll", pluginName));
                    if (!File.Exists(dllPath)) {
                        MpConsole.WriteTraceLine(@"Warning! Plugin flagged as dll type does not have a dll matching folder w/ manifest.json in it, ignoring");
                        return null;
                    }
                    Assembly pluginAssembly = null;
                    try {
                        pluginAssembly = Assembly.LoadFrom(dllPath);
                    } catch(Exception ex) {
                        MpConsole.WriteTraceLine(@"Error loading dll: " + dllPath);
                        MpConsole.WriteLine(ex);
                        return null;
                    }
                    if (pluginAssembly != null) {
                        for (int i = 0; i < pluginAssembly.GetTypes().Length; i++) {
                            var curType = pluginAssembly.GetTypes()[i];
                            if (curType.GetInterface("MonkeyPaste.Plugin.MpIPlugin") != null) {
                                var pluginObj = Activator.CreateInstance(curType);
                                if (pluginObj != null) {
                                    return pluginObj;
                                }
                            }
                        }
                    }
                } else if(plugin.ioType.isCommandLine) {
                    string exePath = Path.Combine(pluginDir, string.Format(@"{0}.exe", pluginName));
                    if (!File.Exists(exePath)) {
                        MpConsole.WriteTraceLine(@"Warning! Plugin flagged as exe type does not have a exe matching folder w/ manifest.json in it, ignoring");
                        return null;
                    }
                    return new MpCommandLinePlugin() { Endpoint = exePath };
                } else if(plugin.ioType.isHttp) {
                    return new MpHttpPlugin(plugin.analyzer.http);
                } else {
                    throw new Exception(@"Unknown or undefined plugin type: " + JsonConvert.SerializeObject(plugin.ioType));
                }
            } catch(Exception ex) {
                MpConsole.WriteTraceLine("Error loading plugin w/ manifest " + manifestPath);
                MpConsole.WriteLine(ex);
                return null;
            }
            return null;
        }
        #endregion
    }
}
