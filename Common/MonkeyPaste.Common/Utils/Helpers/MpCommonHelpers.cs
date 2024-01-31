using MonkeyPaste.Common.Plugin;
using System;
using System.IO;
using System.Reflection;

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
            //string solution_path = MpCommonTools.Services.PlatformInfo.ExecutingDir.FindParentDirectory("MonkeyPaste");
            //string solution_dir = AppDomain.CurrentDomain.BaseDirectory.FindParentDirectory("MonkeyPaste");
            string sln_path = typeof(MpCommonHelpers).Assembly.GetCustomAttribute<MpSolutionPathAttribute>().Value;
            return Path.GetDirectoryName(sln_path);
        }

        public static string GetTargetDatDir() {
            return typeof(MpCommonHelpers).Assembly.GetCustomAttribute<MpTargetDatDirAttribute>().Value;
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
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class MpSolutionPathAttribute : Attribute {
        public string Value { get; set; }
        public MpSolutionPathAttribute(string value) {
            Value = value;
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class MpTargetDatDirAttribute : Attribute {
        public string Value { get; set; }
        public MpTargetDatDirAttribute(string value) {
            Value = value;
        }
    }
}
