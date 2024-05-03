using MonkeyPaste.Common;
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
#if ANDROID
#else
            _resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
#endif
        }

        protected override Assembly? Load(AssemblyName assemblyName) {
            if (_resolver == null ||
                Default.Assemblies.Any(a => a.FullName == assemblyName.FullName)) {
                // This will fallback to loading the assembly from default context.
                return null;
            }
#pragma warning disable CA1416 // Validate platform compatibility
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
#pragma warning restore CA1416 // Validate platform compatibility
            if (assemblyPath != null) {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}
