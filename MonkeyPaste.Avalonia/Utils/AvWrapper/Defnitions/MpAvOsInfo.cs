using MonkeyPaste.Common;
using System;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public class MpAvOsInfo : MpIOsInfo {
        public string OsMachineName => Environment.MachineName;

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
                return MpUserDeviceType.Unknown;
            }
        }
    }
}

