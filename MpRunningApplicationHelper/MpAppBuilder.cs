using MonkeyPaste;
using System;
using System.Threading.Tasks;

namespace MpProcessHelper {
    public class MpAppBuilder : MonkeyPaste.MpIAppBuilder, MonkeyPaste.MpISingletonViewModel<MpAppBuilder> {
        #region MpISingleton Definition
        private static MpAppBuilder _instance;
        public static MpAppBuilder Instance => _instance ?? (_instance = new MpAppBuilder());

        public async Task Init() {
            await Task.Delay(1);
        }

        #region Constructor

        public MpAppBuilder() { }

        #endregion

        #endregion
        public async Task<MpApp> Build(object handleInfo, MpIProcessIconBuilder pib) {
            object result = await Build(new object[] { handleInfo, pib });
            return result == null ? null : result as MpApp;
        }

        public async Task<MpApp> Build(object args) {
            object result = await Build(args as object[]);
            return result == null ? null : result as MpApp;
        }

        public async Task<object> Build(object[] args) {
            object handleInfo = args[0];
            MpIProcessIconBuilder pib = args[1] as MpIProcessIconBuilder;

            if (handleInfo == null || handleInfo.GetType() != typeof(IntPtr)) {
                return null;
            }
            IntPtr hWnd = (IntPtr)handleInfo;
            string appPath = MpProcessManager.GetProcessPath(hWnd);
            string appName = GetProcessApplicationName(appPath);

            var iconStr = pib.GetBase64BitmapFromFilePath(appPath);
            var icon = await MpIcon.Create(iconStr);
            var app = await MpApp.Create(appPath, appName, icon);

            return app;
        }

        public string GetProcessApplicationName(object handleInfo) {
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
