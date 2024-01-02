using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpPluginWrapper : MpPluginFormat {
        [JsonIgnore]
        public IEnumerable<MpIPluginComponentBase> Components { private get; set; } = null;
        [JsonIgnore]
        public AssemblyLoadContext LoadContext { private get; set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task UnloadAsync() {
            // BUG plugin unload doesn't work don't know why 
            //if (Components.OfType<MpIUnloadPluginComponent>().Any()) {
            //    try {

            //        await IssueRequestAsync(nameof(MpIUnloadPluginComponent.Unload), typeof(MpIUnloadPluginComponent).FullName, null);
            //    }
            //    catch (Exception ex) {
            //        if (ex is TargetParameterCountException) {
            //            // expected when no unload implemented
            //        } else {

            //            MpConsole.WriteTraceLine($"Error unloading {this}", ex);
            //        }
            //    }
            //}
            await Task.Delay(1);

            Components = null;

            analyzer = null;
            oleHandler = null;
            headless = null;
            contactFetcher = null;
            backupCheckPluginFormat = null;

            WeakReference wr = new WeakReference(LoadContext);
            // from https://learn.microsoft.com/en-us/dotnet/standard/assembly/unloadability#use-a-custom-collectible-assemblyloadcontext
            LoadContext.Unload();
            for (int i = 0; wr.IsAlive && (i < 50); i++) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            LoadContext = null;
        }
        public bool ValidatePluginComponents() {
            foreach (var component in Components) {
                if (component is MpIAnalyzeComponentAsync || component is MpIAnalyzeComponent) {
                    if (analyzer == null) {
                        throw new MpUserNotifiedException($"Plugin error '{ManifestPath}' must define 'analyzer' definition for '{component.GetType()}' which implements analyzer interface.");
                    }
                } else if (component is MpIOlePluginComponent &&
                        (this is not MpPluginWrapper avpf ||
                        avpf.oleHandler == null)) {
                    throw new MpUserNotifiedException($"Plugin error '{ManifestPath}' must define 'oleHandler' definition for '{component.GetType()}' which implements analyzer interface.");
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task<object> IssueRequestAsync(string methodName, string typeName, MpPluginRequestFormatBase req, bool sync_only = false) {
            Type onType = typeof(MpPluginFormat).Assembly.GetType(typeName);
            if (onType == null) {
                MpDebug.Break($"Plugin type '{typeName}' not found");
                return null;
            }
            foreach (var comp in Components) {
                if (!comp.GetType().IsClassSubclassOfOrImplements(onType) ||
                     comp.GetType().GetMethod(methodName) is not { } mi) {
                    continue;
                }
                // NOTE this prevents exception since unload has NO args
                // NOTE2 this presumes all other components have ONE req arg

                object[] args = mi.GetParameters().Length == 0 ? null : new[] { req };
                if (methodName.EndsWith("Async")) {
                    dynamic task = mi.Invoke(comp, args);
                    object result = await task;
                    return result;
                } else {
                    object result = mi.Invoke(comp, args);
                    return result;
                }
            }
            if (!methodName.EndsWith("Async") && !sync_only) {
                // re issue async
                var async_result = await IssueRequestAsync(methodName + "Async", typeName + "Async", req);
                return async_result;
            }
            return null;
        }

        [JsonIgnore]
        public string ManifestPath { get; set; }

        [JsonIgnore]
        public string ManifestDir =>
            ManifestPath.IsFile() ? Path.GetDirectoryName(ManifestPath) : null;

        [JsonIgnore]
        public string RootDirectory {
            get {
                if (!ManifestDir.IsDirectory()) {
                    return null;
                }
                if (Path.GetDirectoryName(ManifestDir).EndsWith(guid)) {
                    // when plugin is installed it'll be in wrapper dir using guid as its name
                    return Path.GetDirectoryName(ManifestDir);
                }
                return ManifestDir;
            }
        }

        [JsonIgnore]
        public string CachePath {
            get {
                return Path.Combine(
                    MpPluginLoader.PluginCacheDir,
                    $"{guid}.{MpPluginLoader.MANIFEST_FILE_EXT}");
            }
        }
        [JsonIgnore]
        public DateTime manifestLastModifiedDateTime { get; set; }


        [JsonIgnore]
        public bool IsManifestChangedFromBackup { get; set; }

        [JsonIgnore]
        public virtual MpPluginWrapper backupCheckPluginFormat { get; set; }


        [JsonIgnore]
        public virtual IEnumerable<MpParameterHostBaseFormat> componentFormats {
            get {
                if (headless != null) {
                    yield return headless;
                }
                if (analyzer != null) {
                    yield return analyzer;
                }
                if (contactFetcher != null) {
                    yield return contactFetcher;
                }
                if (oleHandler != null) {
                    if (oleHandler.readers != null) {
                        foreach (var cr in oleHandler.readers) {
                            yield return cr;
                        }
                    }
                    if (oleHandler.writers != null) {
                        foreach (var cw in oleHandler.writers) {
                            yield return cw;
                        }
                    }
                }
            }
        }
    }
}
