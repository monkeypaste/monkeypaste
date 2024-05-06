using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace MonkeyPaste.Avalonia {
    public class MpPluginAssemblyLoadContext : AssemblyLoadContext {
        // from https://learn.microsoft.com/en-us/dotnet/standard/assembly/unloadability#create-a-collectible-assemblyloadcontext

        private AssemblyDependencyResolver _resolver;
        public MpPluginAssemblyLoadContext() : base(nameof(MpPluginAssemblyLoadContext), isCollectible: true) {
        }
        public MpPluginAssemblyLoadContext(string mainAssemblyToLoadPath) : base(isCollectible: true) {
            if (OperatingSystem.IsAndroid()) {
                return;
            }
            _resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName) {
            if (_resolver == null ||
                OperatingSystem.IsAndroid() ||
                Default.Assemblies.Any(a => a.FullName == assemblyName.FullName)) {
                // This will fallback to loading the assembly from default context.
                return null;
            }
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null) {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}
