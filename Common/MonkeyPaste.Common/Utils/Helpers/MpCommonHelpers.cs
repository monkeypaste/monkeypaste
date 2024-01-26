using MonkeyPaste.Common.Plugin;
using System;

namespace MonkeyPaste.Common {
    public static class MpCommonHelpers {
        //        public static string GetExecutingDir() {
        //            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        //        }
        //        public static string GetExecutingPath() {
        //#if DEBUG
        //            string ext = string.Empty;
        //            if (MpCommonTools.Services.OsInfo.OsType == MpUserDeviceType.Windows) {
        //                ext = ".exe";
        //            } else if (MpCommonTools.Services.OsInfo.OsType == MpUserDeviceType.Android) {
        //                ext = ".apk";
        //            } else if (MpCommonTools.Services.OsInfo.OsType == MpUserDeviceType.Mac) {
        //                ext = @"/";
        //            } else if (MpCommonTools.Services.OsInfo.OsType == MpUserDeviceType.Linux) {
        //                ext = @".deb";
        //            } else {
        //                // add
        //                MpDebug.Break();
        //            }
        //            string exe_name = MpCommonTools.Services.OsInfo.IsDesktop ?
        //                "MonkeyPaste.Desktop" :
        //                "MonkeyPaste.Avalonia.Android";
        //            exe_name += ext;
        //            return Path.Combine(GetExecutingDir(), exe_name);
        //#else
        //            //fix it
        //            MpDebug.Break();
        //#endif
        //        }
        public static string GetSolutionDir() {
            string solution_path = AppDomain.CurrentDomain.BaseDirectory.FindParentDirectory("MonkeyPaste");
            //string solution_path = MpCommonTools.Services.PlatformInfo.ExecutingDir.FindParentDirectory("MonkeyPaste");
            return solution_path;
        }


        public static string NewLineByEnv(MpUserDeviceType deviceType) {
            switch (deviceType) {
                case MpUserDeviceType.Windows:
                    return "\r\n";
                default:
                    return "\n";
            }
        }
    }
}
