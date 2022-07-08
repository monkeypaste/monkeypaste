using System;
using MonkeyPaste;
namespace MonkeyPaste.Avalonia {
    public class MpAvOsInfo : MpIOsInfo {
        public string OsFileManagerPath {
            get {
                if(OperatingSystem.IsWindows()) {

                }
                if(OperatingSystem.IsLinux()) {

                }
                if(OperatingSystem.IsMacOS()) {
                    return @"/System/Library/CoreServices/finder.app";
                }
                return null;
            }
        }

        public string OsFileManagerName {
            get {
                if (OperatingSystem.IsWindows()) {

                }
                if (OperatingSystem.IsLinux()) {

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

