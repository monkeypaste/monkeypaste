using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using HtmlAgilityPack;
using System.Reflection;

namespace MonkeyPaste {
    public class MpPluginManager {
        #region Singleton
        private static readonly Lazy<MpPluginManager> _Lazy = new Lazy<MpPluginManager>(() => new MpPluginManager());
        public static MpPluginManager Instance { get { return _Lazy.Value; } }

        private MpPluginManager() { }
        #endregion

        #region Properties
        public ObservableCollection<MpPlugin> Plugins = new ObservableCollection<MpPlugin>();
        #endregion

        #region Public Methods
        public void Init() {
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
                    //loop through each plugin subdirectory which needs a .dll with the same name as the folder 
                    //name which should be the plugin name as well;                    
                    Assembly pluginAssembly = null;
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
                            
                        }
                    }
                    if (pluginAssembly == null) {
                        //malformed plugin directory, log, ignore and continue
                        MonkeyPaste.MpConsole.WriteLine(string.Format(@"Plugin directory {0} does not contain {0}.dll or {0}.dll cannot be loaded, ignoring plugin"));
                    }
                    for(int i = 0;i < pluginAssembly.GetTypes().Length;i++) {
                        var curType = pluginAssembly.GetTypes()[i];
                        if(curType.GetInterface("MonkeyPaste.Plugin.MpIPlugin") != null) {
                            var pluginObj = Activator.CreateInstance(curType);

                            if (pluginObj != null) {
                                var plugin = new MpPlugin();
                                plugin.Name = pluginObj.GetType().GetMethod("GetName").Invoke(pluginObj, null) as string;

                                var cl = pluginObj.GetType().GetMethod("GetComponents").Invoke(pluginObj, null);
                                if (cl != null && cl is object[]) {
                                    plugin.Components = cl as object[];
                                }
                                MonkeyPaste.MpConsole.WriteLine(@"Loaded " + plugin.Name + "...");
                                Plugins.Add(plugin);
                            }
                        }
                    }
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
