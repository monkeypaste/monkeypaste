using MonkeyPaste.Common;

using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginAssemblyHelpers {
        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static void Load(string manifestPath, MpPluginWrapper plugin, out Assembly component_assembly) {
            component_assembly = null;
            AssemblyLoadContext alc = null;
            plugin.manifestLastModifiedDateTime = File.GetLastWriteTime(manifestPath);
            string bundle_path = GetBundlePath(manifestPath, plugin);
            IEnumerable<MpIPluginComponentBase> components = null;
            switch (plugin.packageType) {
                case MpPluginPackageType.None:
                    throw new MpUserNotifiedException($"Error, Plugin '{plugin.title}' defined in '{manifestPath}' must specify a bundle type.");
                case MpPluginPackageType.Dll:
                    component_assembly = LoadDll(bundle_path, out alc);
                    components = component_assembly.FindSubTypes<MpIPluginComponentBase>();
                    break;
                case MpPluginPackageType.Nuget:
                    component_assembly = LoadNuget(bundle_path, out alc);
                    components = component_assembly.FindSubTypes<MpIPluginComponentBase>();
                    break;
                case MpPluginPackageType.Python:
                    component_assembly = Assembly.GetAssembly(typeof(MpPythonAnalyzerPlugin));
                    components = new MpIPluginComponentBase[] { new MpPythonAnalyzerPlugin(bundle_path) };
                    break;
                case MpPluginPackageType.Http:
                    component_assembly = Assembly.GetAssembly(typeof(MpHttpAnalyzerPlugin));
                    components = new MpIPluginComponentBase[] { new MpHttpAnalyzerPlugin(plugin.analyzer.http) };
                    break;
                default:
                    throw new MpUserNotifiedException($"Unhandled plugin bundle type for '{plugin.title}' defined at '{manifestPath}' with type '{plugin.packageType}'");
            }
            plugin.LoadContext = alc;
            plugin.Components = components.ToArray();
        }
        private static Assembly LoadDll(string dllPath, out AssemblyLoadContext alc) {
            alc = null;
            try {
                alc = new MpPluginAssemblyLoadContext(dllPath);
                return alc.LoadFromAssemblyPath(dllPath);
            }
            catch (Exception ex) {
                throw new MpUserNotifiedException($"Plugin Linking error '{dllPath}':{Environment.NewLine}{ex}");
            }
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

        private static string GetBundlePath(string manifestPath, MpPluginWrapper plugin) {
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
                    throw new MpUserNotifiedException($"Plugin activation error for plugin '{pluginAssembly.FullName}': {Environment.NewLine}{ex.Message}");
                }
            }
            return objs;
        }
        #endregion
    }
}
