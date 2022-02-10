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
        public static ObservableCollection<MpPlugin> Plugins { get; set; } = new ObservableCollection<MpPlugin>();
        #endregion

        #region Public Methods

        public static void Init() {
            //find plugin folder in main app folder
            var pluginRootFolderPath = Directory.GetCurrentDirectory();

            pluginRootFolderPath = pluginRootFolderPath.Replace("mp.db", string.Empty);
            pluginRootFolderPath = Path.Combine(pluginRootFolderPath, "Plugins");

            if (!Directory.Exists(pluginRootFolderPath)) {
                //if plugin folder doesn't exist then no plugins so nothing to do
                return;
            }

            try {
                foreach (var pdp in Directory.GetDirectories(pluginRootFolderPath)) {


                    

                    //loop through each plugin subdirectory which needs a .dll or .exe with the same name as the folder 
                    //name which should be the plugin name as well;                    
                    Assembly pluginAssembly = null;
                    string pluginExePath = null;
                    foreach (var pdf in Directory.GetFiles(pdp)) {
                        var pluginName = Path.GetFileName(Path.GetDirectoryName(pdf));
                        if (Path.GetFileNameWithoutExtension(pdf).ToLower() == pluginName.ToLower() &&
                            Path.GetExtension(pdf).ToLower() == ".dll") {
                            try {
                                //found dll that should contain MpPluginHook
                                pluginAssembly = Assembly.LoadFrom(pdf);
                            } 
                            catch(Exception ex) {
                                //cannot load plugin so log and break to ignore and continue
                                MonkeyPaste.MpConsole.WriteTraceLine(ex);
                                pluginAssembly = null;
                                break;
                            }                            
                        } else if (Path.GetFileNameWithoutExtension(pdf).ToLower() == pluginName.ToLower() &&
                            Path.GetExtension(pdf).ToLower() == ".exe") {
                            try {
                                //found dll that should contain MpPluginHook
                                pluginExePath = pdf;
                            }
                            catch (Exception ex) {
                                //cannot load plugin so log and break to ignore and continue
                                MonkeyPaste.MpConsole.WriteTraceLine(ex);
                                pluginAssembly = null;
                                break;
                            }
                        }
                    }
                    if (pluginAssembly == null && pluginExePath == null) {
                        //malformed plugin directory, log, ignore and continue
                        MonkeyPaste.MpConsole.WriteLine(string.Format(@"Plugin directory {0} does not contain {0}.dll or {0}.dll cannot be loaded, ignoring plugin",pdp));
                        continue;
                    }


                    string manifestPath = Path.Combine(pdp, "manifest.json");
                    if (!File.Exists(manifestPath)) {
                        MonkeyPaste.MpConsole.WriteLine($"Plugin directory {pdp} does not contain manifest.json cannot be loaded, ignoring plugin");
                        continue;
                    }
                    string manifestJson = MpFileIo.ReadTextFromFile(manifestPath);

                    var plugin = JsonConvert.DeserializeObject<MpPlugin>(manifestJson);
                    if (pluginAssembly != null) {
                        for (int i = 0; i < pluginAssembly.GetTypes().Length; i++) {
                            var curType = pluginAssembly.GetTypes()[i];
                            if (curType.GetInterface("MonkeyPaste.Plugin.MpIPlugin") != null) {
                                var pluginObj = Activator.CreateInstance(curType);

                                if (pluginObj != null) {
                                    plugin.Components.Add(pluginObj);
                                }
                            }
                        }
                    }

                    if(!string.IsNullOrWhiteSpace(pluginExePath)) {
                        plugin.Components.Add(new MpCommandLinePlugin() { Endpoint = pluginExePath });

                        //var af = plugin.types
                        //            .Where(x => x.analyzers != null && x.analyzers.Count > 0)
                        //            .SelectMany(x => x.analyzers)
                        //            .FirstOrDefault(x => x.endpoint.ToLower() == Path.GetFileName(pluginExePath.ToLower()));
                        
                        //if (af != null) {
                        //    af.parametersResourcePath = manifestPath;
                        //}
                    }

                    Plugins.Add(plugin);
                    MpConsole.WriteLine($"Successfully loaded plugin: {plugin.title}");
                }
            }
            catch(Exception ex) {
                MonkeyPaste.MpConsole.WriteTraceLine(ex);
            }
            return;
        }


        #endregion

        #region Private Methods
        #endregion
    }
}
