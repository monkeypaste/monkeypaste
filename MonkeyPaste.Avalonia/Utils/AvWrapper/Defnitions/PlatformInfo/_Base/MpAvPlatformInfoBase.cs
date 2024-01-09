
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvPlatformInfoBase : MpIPlatformInfo {
        #region Properties

        #region State
        public virtual bool IsTraceEnabled {
            get {
                bool do_trace = App.HasStartupArg(App.TRACE_ARG);
#if DEBUG
                return true;
#else
                return do_trace;
#endif
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
        #endregion

        #region Info

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

        public virtual string RuntimeShortName {
            get {
                var arch = RuntimeInformation.ProcessArchitecture;

                if (OperatingSystem.IsWindows()) {
                    if (arch == Architecture.Arm64) {
                        return "win-arm64";
                    }
                    if (arch == Architecture.Arm) {
                        return "win-arm";
                    }
                    if (arch == Architecture.X64) {
                        return "win-x64";
                    }
                    if (arch == Architecture.X86) {
                        return "win-x86";
                    }
                }
                if (OperatingSystem.IsLinux()) {
                    if (arch == Architecture.Arm64) {
                        return "linux-arm64";
                    }
                    if (arch == Architecture.Arm) {
                        return "linux-arm";
                    }
                    if (arch == Architecture.X64) {
                        return "linux-x64";
                    }
                    if (arch == Architecture.X86) {
                        return "linux-x86";
                    }
                }
                if (OperatingSystem.IsMacOS()) {
                    if (arch == Architecture.Arm64) {
                        return "osx-arm64";
                    }
                    if (arch == Architecture.Arm) {
                        return "osx-arm";
                    }
                    if (arch == Architecture.X64) {
                        return "osx-x64";
                    }
                    if (arch == Architecture.X86) {
                        return "osx-x86";
                    }
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
                return string.Empty;
            }
        }

        public virtual string OsMachineName =>
            Environment.MachineName;

        // TODO Add per env info here
        public virtual string OsVersion =>
            Environment.OSVersion.VersionString;

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

        #endregion

        #region Paths

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
                        string suffix = string.Empty;
#if DEBUG
                        suffix = "_DEBUG";
#endif
                        MpIThisAppInfo tai =
                            Mp.Services == null ||
                            Mp.Services.PlatformInfo == null ?
                                new MpAvThisAppInfo() :
                                Mp.Services.ThisAppInfo;
                        _storageDir = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            tai.ThisAppProductName + suffix);
                        if (!_storageDir.IsDirectory()) {

                            MpFileIo.CreateDirectory(_storageDir);
                            MpConsole.WriteLine($"App Storage dir successfully created at: '{_storageDir}'");
                        }
                    }
                }
                return _storageDir;
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

        public virtual string TermsPath {
            get {
                return Path.Combine(ExecutingDir, "Resources", "Legal", "terms", "terms.html");
            }
        }

        public virtual string HelpPath {
            get {
                if (OperatingSystem.IsBrowser()) {
                    return Path.Combine("Editor", "index.html");
                }
                return Path.Combine(ExecutingDir, "Resources", "Help", "index.html");
            }
        }

        public string LogDir =>
            Path.Combine(StorageDir, "logs");
        string LogFileName =>
            "mp.log";
        public string LogPath =>
            Path.Combine(LogDir, LogFileName);

        #endregion

        #region Console

        private TraceListener _ctl;
        public TraceListener ConsoleTraceListener {
            get {
                if (_ctl == null) {
                    _ctl = new ConsoleTraceListener();
                }
                return _ctl;
            }
        }
        #endregion
        #endregion

        #region Constructors
        public MpAvPlatformInfoBase() {
#if RELEASE
            IsTraceEnabled = App.HasStartupArg(App.TRACE_ARG);
#endif
        }
        #endregion

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

