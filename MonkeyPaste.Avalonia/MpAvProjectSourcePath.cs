using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    internal static class MpAvProjectSourcePath {
        private const string myRelativePath = nameof(MpAvProjectSourcePath) + ".cs";
        private static string? lazyValue;
        public static string Value => lazyValue ??= calculatePath();

        private static string calculatePath([CallerFilePath] string? callerFilePath = null) {
            string pathName = callerFilePath;
            return pathName.Substring(0, pathName.Length - myRelativePath.Length);
        }
    }
}
