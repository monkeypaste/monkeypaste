
using MonkeyPaste.Common;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
    public class MpAvPlatformInfo : MpIPlatformInfo {
        public string OsMachineName =>
            Environment.MachineName;

        // TODO Add per env info here
        public string OsVersionInfo =>
            Environment.OSVersion.VersionString;

        public string ExecutingDir {
            get {
                return AppContext.BaseDirectory;
                //return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }
        public string ExecutableName {
            get {
                if (Environment.GetCommandLineArgs().Any()) {
                    string main_mod_path = Environment.GetCommandLineArgs()[0];
                    return Path.GetFileNameWithoutExtension(main_mod_path);
                }

                MpDebug.Break("is this android?");
                return null;
            }
        }
        public string ExecutingPath {
            get {
                return Path.Combine(ExecutingDir, ExecutableName + GetExecutableExt());
            }
        }

        public string StorageDir {
            get {
                if (OperatingSystem.IsAndroid()) {
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public string OsShortName {
            get {
                if (OperatingSystem.IsWindows()) {
                    return "win";
                }
                if (OperatingSystem.IsLinux()) {
                    return "x11";
                }
                if (OperatingSystem.IsMacOS()) {
                    return "mac";
                }
                if (OperatingSystem.IsAndroid()) {
                    return "android";
                }
                if (OperatingSystem.IsIOS()) {
                    return "ios";
                }
                if (OperatingSystem.IsBrowser()) {
                    return "browser";
                }
                throw new Exception("Unknown os");
            }
        }

        public bool IsDesktop =>
            OperatingSystem.IsWindows() ||
            OperatingSystem.IsMacOS() ||
            OperatingSystem.IsLinux();

        public string OsFileManagerPath {
            get {
                if (OperatingSystem.IsWindows()) {
                    return Path.Combine(Directory.GetParent(Environment.SystemDirectory).FullName, "explorer.exe");
                }
                if (OperatingSystem.IsLinux()) {
                    // TODO will likely need to have user specify path and name of file manager on linux. 
                    return @"/usr/bin/nautilus";
                }
                if (OperatingSystem.IsMacOS()) {
                    return @"/System/Library/CoreServices/finder.app";
                }
                return null;
            }
        }

        public string OsFileManagerName {
            get {
                if (OperatingSystem.IsWindows()) {
                    return "Explorer";
                }
                if (OperatingSystem.IsLinux()) {
                    // TODO will need to handle more than ubuntu here
                    return "Nautilus";
                }
                if (OperatingSystem.IsMacOS()) {
                    return @"Finder";
                }
                return null;
            }
        }

        public MpUserDeviceType OsType {
            get {
                if (OperatingSystem.IsWindows()) {
                    return MpUserDeviceType.Windows;
                }
                if (OperatingSystem.IsLinux()) {
                    return MpUserDeviceType.Linux;
                }
                if (OperatingSystem.IsMacOS()) {
                    return MpUserDeviceType.Mac;
                }
                if (OperatingSystem.IsAndroid()) {
                    return MpUserDeviceType.Android;
                }
                if (OperatingSystem.IsIOS()) {
                    return MpUserDeviceType.Ios;
                }
                if (OperatingSystem.IsBrowser()) {
                    return MpUserDeviceType.Browser;
                }
                return MpUserDeviceType.Unknown;
            }
        }

        #region Private Methods

        private string GetExecutableExt() {
            if (OsType == MpUserDeviceType.Windows) {
                return ".exe";
            } else if (OsType == MpUserDeviceType.Android) {
                // NOTE this may need OsVersionInfo too
                return ".apk";
            } else if (OsType == MpUserDeviceType.Mac) {
                return @"/";
            } else if (OsType == MpUserDeviceType.Linux) {
                // TODO this is a place OsVersionInfo will be needed
                return @".deb";
            }

            // add
            MpDebug.Break("missing executable exit");
            return null;
        }
        #endregion
    }
}

