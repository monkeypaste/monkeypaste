using MonkeyPaste;
using MpProcessHelper;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpWpfAppBuilder : MpIAppBuilder {
        public async Task<MpApp> CreateAsync(object handleOrAppPath, string appName = "") {

            string processPath, processIconImg64;

            if (handleOrAppPath is string) {
                processPath = (string)handleOrAppPath;
                appName = string.IsNullOrEmpty(appName) ? Path.GetFileNameWithoutExtension(processPath) : appName;
                processIconImg64 = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(processPath);
            } else if(handleOrAppPath is IntPtr processHandle){
                processPath = MpProcessManager.GetProcessPath(processHandle);
                appName = string.IsNullOrEmpty(appName) ? MpProcessManager.GetProcessApplicationName(processHandle) : appName;
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

            var app = await MpApp.Create(
                appPath: processPath, 
                appName: appName, 
                iconId: icon.Id);

            return app;
        }
    }
}
