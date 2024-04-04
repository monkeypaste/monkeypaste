using MonkeyPaste.Common.Plugin;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MonkeyPaste.Common {
    public static class MpPlatformHelpers {
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
        public static bool IsRunningAsStoreApp() {
            return Assembly.GetExecutingAssembly().Location.Contains(@"Program Files\WindowsApps");

        }
        public static Version GetAppVersion() {
            string ver_str = typeof(MpPlatformHelpers).Assembly.GetCustomAttribute<MpAppVersionAttribute>().Value;
            var ver = ver_str.ToVersion();
            return ver;
        }

        public static string GetSolutionDir() {
            string sln_path = typeof(MpPlatformHelpers).Assembly.GetCustomAttribute<MpSolutionPathAttribute>().Value;
            return Path.GetDirectoryName(sln_path);
        }

        public static string GetStorageDir() {
            string app_name =
#if DEBUG
                "MonkeyPaste_DEBUG";
#else
                            "MonkeyPaste";
#endif
            string storage_dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                app_name);
            return storage_dir;
            //if(MpCommonTools.Services.PlatformInfo.IsAdmin) {

            //}
            //return typeof(MpPlatformHelpers).Assembly.GetCustomAttribute<MpLocalStorageDirAttribute>().Value;
        }
        private static string GetPackageDir() {
#if WINDOWS
            string packages_dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Packages");
            var all_packages = Directory.GetDirectories(packages_dir);
            var possible_dirs = all_packages.Where(x => x.Contains("MonkeyPaste")).ToList();
            string package_dir =
                    possible_dirs
                    .OrderByDescending(x => new DirectoryInfo(x).LastWriteTimeUtc)
                    .FirstOrDefault();
            if (possible_dirs.Count > 1) {
                MpConsole.WriteLine($"Warning, multiple possible package dirs found, selecting most recently modified");
            }

            return package_dir;
#else
            return GetStorageDir();
#endif

        }

        public static string LocalStoragePathToPackagePath(this string local_storage_path, bool must_exist = true) {
#if WINDOWS
            // example
            // source="C:\Users\tkefauver\AppData\Roaming\MonkeyPaste_DEBUG\Plugins\cf2ec03f-9edd-45e9-a605-2a2df71e03bd"
            // target="C:\Users\tkefauver\AppData\Local\Packages\10843MonkeyLLC.MonkeyPaste_gak2v2dkd2bkp\LocalCache\Roaming\MonkeyPaste_DEBUG\Plugins\cf2ec03f-9edd-45e9-a605-2a2df71e03bd"

            if (GetPackageDir() is not string root_package_dir ||
                !root_package_dir.IsDirectory()) {
                // probably not using WAP
                return local_storage_path;
            }
            // gets "C:\Users\tkefauver\AppData"
            string app_data_dir = Path.GetDirectoryName(Path.GetDirectoryName(GetStorageDir()));
            // gets "C:\Users\tkefauver\AppData\Local\Packages\10843MonkeyLLC.MonkeyPaste_gak2v2dkd2bkp\LocalCache"
            string package_cache_dir = Path.Combine(
                root_package_dir,
                "LocalCache");
            MpDebug.Assert(package_cache_dir.IsDirectory(), $"Storage error can't find package dir '{package_cache_dir}'");
            // replace one for the other
            string package_cache_path = local_storage_path.Replace(app_data_dir, package_cache_dir);
            if (!package_cache_path.IsFileOrDirectory() &&
                must_exist) {
                // not found fallback 
                return local_storage_path;
            }

            return package_cache_path;
#else
            return local_storage_path;
#endif            
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
