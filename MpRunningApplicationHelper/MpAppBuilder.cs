using MonkeyPaste;
using System;
using System.Threading.Tasks;

namespace MpProcessHelper {
    public static class MpAppBuilder {

        public static async Task<object> Build(IntPtr hWnd) {
            string appPath = MpProcessManager.GetProcessPath(hWnd);
            string appName = GetProcessApplicationName(appPath);

            var iconStr = MpProcessIconBuilder.GetBase64BitmapFromPath(appPath);
            var icon = await MpIcon.Create(iconStr);
            var app = await MpApp.Create(appPath, appName, icon);

            return app;
        }

        public static string GetProcessApplicationName(object handleInfo) {
            if (handleInfo == null || handleInfo.GetType() != typeof(IntPtr)) {
                return null;
            }
            IntPtr hWnd = (IntPtr)handleInfo;
            string mwt = MpProcessManager.GetProcessMainWindowTitle(hWnd);
            if (string.IsNullOrEmpty(mwt)) {
                return mwt;
            }
            var mwta = mwt.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (mwta.Length == 1) {
                if (string.IsNullOrEmpty(mwta[0])) {
                    return "Explorer";
                }
                return mwta[0];
            }
            return mwta[mwta.Length - 1].Trim();
        }
    }

}
