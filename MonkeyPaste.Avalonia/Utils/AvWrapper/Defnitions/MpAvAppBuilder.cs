using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppBuilder : MpIAppBuilder {
        public async Task<MpApp> CreateAsync(object handleOrAppPathOrProcessInfo, string appName = "") {
            string processPath, processIconImg64;

            if(handleOrAppPathOrProcessInfo is MpPortableProcessInfo ppi) {
                appName = ppi.MainWindowTitle;
                processIconImg64 = ppi.MainWindowIconBase64;

                if(string.IsNullOrWhiteSpace(ppi.ProcessPath)) {
                    handleOrAppPathOrProcessInfo = ppi.Handle;
                } else {

                    handleOrAppPathOrProcessInfo = ppi.ProcessPath;
                }
            } 

            if (handleOrAppPathOrProcessInfo is string) {
                processPath = (string)handleOrAppPathOrProcessInfo;
                appName = string.IsNullOrEmpty(appName) ? Path.GetFileNameWithoutExtension(processPath) : appName;
                processIconImg64 = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(processPath);
            } else if (handleOrAppPathOrProcessInfo is IntPtr processHandle) {
                //processPath = MpProcessManager.GetProcessPath(processHandle);
                processPath = MpPlatformWrapper.Services.ProcessWatcher.GetProcessPath(processHandle);

                appName = string.IsNullOrEmpty(appName) ? MpPlatformWrapper.Services.ProcessWatcher.GetProcessApplicationName(processHandle) : appName;
                processIconImg64 = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(processPath);
            } else {
                // this probably shouldn't happen
                Debugger.Break();
                // since source is unknown set to this app
                processPath = Assembly.GetExecutingAssembly().Location;
                appName = string.IsNullOrEmpty(appName) ? MpPrefViewModel.Instance.ThisAppName : appName;
                processIconImg64 = MpBase64Images.AppIcon;
            }
            var icon = await MpPlatformWrapper.Services.IconBuilder.CreateAsync(processIconImg64);

            var app = await MpApp.CreateAsync(
                appPath: processPath,
                appName: appName,
                iconId: icon.Id);

            return app;
        }
    }
}

