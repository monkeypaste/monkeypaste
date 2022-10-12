using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppBuilder : MpIAppBuilder {
        public async Task<MpApp> CreateAsync(MpPortableProcessInfo pi) {
            if(pi == null) {
                // prob
                Debugger.Break();
                return null;
            }
            string processPath, appName, iconBase64;

            // GET APP PATH
            if (string.IsNullOrWhiteSpace(pi.ProcessPath)) {
                if(pi.Handle == IntPtr.Zero) {
                    // when does this happen? likely a problem
                    Debugger.Break();
                    return null;
                }
                // likely running application when no process path, find the path from the handle and use handle to query for mw title (if not already set)
                processPath = MpPlatformWrapper.Services.ProcessWatcher.GetProcessPath(pi.Handle);
            } else if(pi.Handle != IntPtr.Zero) {
                processPath = pi.ProcessPath;
            } else {
                // shouldn't happen or 
                Debugger.Break();

                // since source is unknown set to this app
                //processPath = Assembly.GetExecutingAssembly().Location;
                //// force app name and icon here (when not given) since this a fallback
                //appName = string.IsNullOrEmpty(pi.MainWindowTitle) ? MpPrefViewModel.Instance.ThisAppName : pi.MainWindowTitle;
                //iconBase64 = String.IsNullOrEmpty(pi.MainWindowIconBase64) ? MpBase64Images.AppIcon : pi.MainWindowIconBase64;
                return null;
            }

            // GET APP NAME
            if(string.IsNullOrWhiteSpace(pi.MainWindowTitle)) {
                if(pi.Handle == IntPtr.Zero) {
                    appName = Path.GetFileNameWithoutExtension(pi.ProcessPath);
                } else {
                    appName = MpPlatformWrapper.Services.ProcessWatcher.GetProcessApplicationName(pi.Handle);
                }
            } else {
                appName = MpPlatformWrapper.Services.ProcessWatcher.ParseTitleForApplicationName(pi.MainWindowTitle);
            }

            // GET APP ICON
            iconBase64 = String.IsNullOrEmpty(pi.MainWindowIconBase64) ? MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(processPath) : pi.MainWindowIconBase64;

            var icon = await MpPlatformWrapper.Services.IconBuilder.CreateAsync(iconBase64);

            var app = await MpApp.CreateAsync(
                appPath: processPath,
                appName: appName,
                arguments: string.Join(Environment.NewLine,pi.ArgumentList),
                iconId: icon.Id);

            return app;
        }
    }
}

