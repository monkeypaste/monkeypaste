using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppBuilder : MpIAppBuilder {
        public async Task<MpApp> CreateAsync(MpPortableProcessInfo pi) {
            if (pi == null) {
                // prob
                Debugger.Break();
                return null;
            }

            string processPath, appName, iconBase64;

            // GET APP PATH
            if (string.IsNullOrWhiteSpace(pi.ProcessPath)) {
                if (pi.Handle == IntPtr.Zero) {
                    // when does this happen? likely a problem
                    Debugger.Break();
                    return null;
                }
                // likely running application when no process path, find the path from the handle and use handle to query for mw title (if not already set)
                processPath = MpPlatform.Services.ProcessWatcher.GetProcessPath(pi.Handle);
            } else {
                processPath = pi.ProcessPath;
            }
            if (string.IsNullOrEmpty(processPath)) {
                MpConsole.WriteWarningLine($"could not find process path for info (using this app): ");
                MpConsole.WriteLine(pi.ToString());
                var this_app_fallback = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
                return this_app_fallback;
            }

            // GET APP NAME
            if (string.IsNullOrWhiteSpace(pi.MainWindowTitle)) {
                if (pi.Handle == IntPtr.Zero) {
                    appName = Path.GetFileNameWithoutExtension(pi.ProcessPath);
                } else {
                    appName = MpPlatform.Services.ProcessWatcher.GetProcessApplicationName(pi.Handle);
                }
            } else {
                appName = MpPlatform.Services.ProcessWatcher.ParseTitleForApplicationName(pi.MainWindowTitle);
            }

            // GET APP ICON
            iconBase64 =
                string.IsNullOrEmpty(pi.MainWindowIconBase64) ?
                    MpPlatform.Services.IconBuilder.GetApplicationIconBase64(processPath) :
                    pi.MainWindowIconBase64;

            if (string.IsNullOrEmpty(iconBase64)) {
                MpConsole.WriteWarningLine($" could not find icon for info (using question mark): ");
                MpConsole.WriteLine(pi.ToString());
                iconBase64 = MpBase64Images.QuestionMark;
            }

            var icon = await MpPlatform.Services.IconBuilder.CreateAsync(iconBase64);

            string args = pi.ArgumentList == null || pi.ArgumentList.Count == 0 ?
                null : string.Join(Environment.NewLine, pi.ArgumentList);

            var app = await MpApp.CreateAsync(
                appPath: processPath,
                appName: appName,
                arguments: args,
                iconId: icon.Id);

            return app;
        }
    }
}

