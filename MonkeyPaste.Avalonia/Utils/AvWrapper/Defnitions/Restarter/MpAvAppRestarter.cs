using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvAppRestarter {

        public static async Task ShutdownWithRestartTaskAsync(string detail) {
            string launcher_path =
#if WINDOWS
        Path.Combine(
                    Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
                    "MonkeyPaste.Desktop.Launcher",
                    "MonkeyPaste.Desktop.Launcher.exe"); 
#else
                Mp.Services.PlatformInfo.ExecutingPath;
#endif
            Process process = Process.Start(launcher_path, App.RESTART_ARG);
            Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.Restart, detail);
            await Task.Delay(1);
        }
    }
}
