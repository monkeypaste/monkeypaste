﻿using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace MonkeyPaste.Avalonia {
    public class MpPluginAssemblyLoadContext : AssemblyLoadContext {
        // from https://learn.microsoft.com/en-us/dotnet/standard/assembly/unloadability#create-a-collectible-assemblyloadcontext

        private AssemblyDependencyResolver _resolver;
        public MpPluginAssemblyLoadContext() : base(nameof(MpPluginAssemblyLoadContext), isCollectible: true) {
        }
        public MpPluginAssemblyLoadContext(string mainAssemblyToLoadPath) : base(isCollectible: true) {
            _resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName) {
            //if (_resolver == null) {
            //    return Default.Assemblies.FirstOrDefault(x => x.FullName == name.FullName);
            //}

            //string? assemblyPath = _resolver.ResolveAssemblyToPath(name);
            //if (assemblyPath != null) {
            //    return LoadFromAssemblyPath(assemblyPath);
            //}

            //return null;
            // This will fallback to loading the assembly from default context.
            if (Default.Assemblies.Any(a => a.FullName == assemblyName.FullName))
                return null;

            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null) {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}