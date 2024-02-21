using McMaster.NETCore.Plugins;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginAssemblyHelpers {
        private static Type[] _sharedTypes;
        //private static Type[] sharedTypes = [
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
                    _sharedTypes = typeof(MpIPluginComponentBase).Assembly.ExportedTypes.ToArray();
                }
                return _sharedTypes;
            }
        }

        private static Dictionary<string, PluginLoader> _loaders = [];

        public static bool UnloadPlugin(string plugin_guid) {
            if (!_loaders.TryGetValue(plugin_guid, out var pl)) {
                // not found
                return true;
            }
            if (!pl.IsUnloadable) {
                return false;
            }
            pl.Dispose();
            _loaders.Remove(plugin_guid);
            return true;
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static async Task LoadComponentsAsync(string manifestPath, MpRuntimePlugin plugin) {
            AssemblyLoadContext alc = null;
            plugin.manifestLastModifiedDateTime = File.GetLastWriteTime(manifestPath);
            string bundle_path = GetBundlePath(manifestPath, plugin);
            switch (plugin.packageType) {
                default:
                case MpPluginPackageType.Dll:
                    plugin.Components = LoadDll<MpIPluginComponentBase>(bundle_path, plugin.guid);
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
        public static IDisposable GetPluginContext(string plugin_guid) {
            if (!_loaders.TryGetValue(plugin_guid, out var pl)) {
                return null;
            }
            return pl.EnterContextualReflection();
        }
        private static IEnumerable<T> LoadDll<T>(string targetDllPath, string plugin_guid) {
            try {
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
                    _loaders.AddOrReplace(plugin_guid, pl);
                    return objs;
                }
            }
            catch (Exception ex) {
                throw new MpUserNotifiedException($"Plugin Linking error '{targetDllPath}':{Environment.NewLine}{ex}");
            }
            return [];
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

        public static IEnumerable<T> FindSubTypes<T>(this Assembly pluginAssembly) {
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
        public static IEnumerable<T> FindSubTypes2<T>(this Assembly pluginAssembly) {
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

    }
}
