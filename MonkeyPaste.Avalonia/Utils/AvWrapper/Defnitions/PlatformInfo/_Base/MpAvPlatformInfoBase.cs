
using MonkeyPaste.Common;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvPlatformInfoBase : MpIPlatformInfo {
        public virtual string OsMachineName =>
            Environment.MachineName;

        // TODO Add per env info here
        public virtual string OsVersionInfo =>
            Environment.OSVersion.VersionString;

        public virtual string ExecutingDir =>
            AppContext.BaseDirectory;
        public virtual string ExecutableName {
            get {
                if (Environment.GetCommandLineArgs().Any()) {
                    string main_mod_path = Environment.GetCommandLineArgs()[0];
                    return Path.GetFileNameWithoutExtension(main_mod_path);
                }

                MpDebug.Break("is this android?");
                return null;
            }
        }
        public virtual string ExecutingPath {
            get {
                return Path.Combine(ExecutingDir, ExecutableName + GetExecutableExt());
            }
        }

        private string _storageDir;
        public virtual string StorageDir {
            get {
                if (_storageDir == null) {
                    if (OperatingSystem.IsBrowser()) {
                        _storageDir = @"/tmp";
                    } else {
                        _storageDir = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            Mp.Services.ThisAppInfo.ThisAppProductName);
                        if (!_storageDir.IsDirectory()) {

                            MpFileIo.CreateDirectory(_storageDir);
                            MpConsole.WriteLine($"App Storage dir successfully created at: '{_storageDir}'");
                        }
                    }
                }
                return _storageDir;
            }
        }

        public virtual string OsShortName {
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

        public virtual bool IsDesktop =>
            OperatingSystem.IsWindows() ||
            OperatingSystem.IsMacOS() ||
            OperatingSystem.IsLinux();


        public bool IsMobile =>
            OperatingSystem.IsIOS() ||
            OperatingSystem.IsAndroid();
        public bool IsBrowser =>
            OperatingSystem.IsBrowser();

        public abstract bool IsTouchInputEnabled { get; }
        public virtual string OsFileManagerPath {
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

        public virtual string EditorPath {
            get {
                if (OperatingSystem.IsBrowser()) {
                    return Path.Combine("Editor", "index.html");
                }
                return Path.Combine(ExecutingDir, "Resources", "Editor", "index.html");
            }
        }

        #region Private Methods

        private string GetExecutableExt() {
            if (OsType == MpUserDeviceType.Windows) {
                return ".exe";
            }
            if (OsType == MpUserDeviceType.Android) {
                // NOTE this may need OsVersionInfo too
                return ".apk";
            }
            if (OsType == MpUserDeviceType.Mac) {
                return @"/";
            }
            if (OsType == MpUserDeviceType.Linux) {
                // TODO this is a place OsVersionInfo will be needed
                return @".deb";
            }
            if (OsType == MpUserDeviceType.Browser) {
                return string.Empty;
            }
            // add
            MpDebug.Break("missing executable exit");
            return null;
        }

        #endregion
    }
}

