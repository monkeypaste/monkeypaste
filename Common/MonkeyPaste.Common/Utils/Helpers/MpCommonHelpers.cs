using MonkeyPaste.Common.Plugin;
using System;
using System.IO;
using System.Linq;
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

        public static string GetStorageDir() {
            return typeof(MpCommonHelpers).Assembly.GetCustomAttribute<MpLocalStorageDirAttribute>().Value;
        }
        public static string GetPackageDir() {
#if !WINDOWS
            return GetStorageDir();
#endif
            string packages_dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Packages");
            var all_packages = Directory.GetDirectories(packages_dir);
            var possible_dirs = all_packages.Where(x => x.Contains("MonkeyPaste")).ToList();
            MpDebug.Assert(possible_dirs.Count == 1, $"Error mutliple package dirs found: {(string.Join(Environment.NewLine, possible_dirs))}");
            return possible_dirs.FirstOrDefault();
        }
        public static string LocalStoragePathToPackagePath(this string local_storage_path) {
#if !WINDOWS
            return local_storage_path;
#endif
            // example
            // source="C:\Users\tkefauver\AppData\Roaming\MonkeyPaste_DEBUG\Plugins\cf2ec03f-9edd-45e9-a605-2a2df71e03bd"
            // target="C:\Users\tkefauver\AppData\Local\Packages\10843MonkeyLLC.MonkeyPaste_gak2v2dkd2bkp\LocalCache\Roaming\MonkeyPaste_DEBUG\Plugins\cf2ec03f-9edd-45e9-a605-2a2df71e03bd"

            // gets "C:\Users\tkefauver\AppData"
            string app_data_dir = Path.GetDirectoryName(Path.GetDirectoryName(GetStorageDir()));
            // gets "C:\Users\tkefauver\AppData\Local\Packages\10843MonkeyLLC.MonkeyPaste_gak2v2dkd2bkp\LocalCache"
            string package_cache_dir = Path.Combine(
                GetPackageDir(),
                "LocalCache");
            // replace one for the other
            string package_cache_path = local_storage_path.Replace(app_data_dir, package_cache_dir);
            return package_cache_path;
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
}
