﻿using MonkeyPaste.Common.Plugin;
using System.Diagnostics;

namespace MonkeyPaste.Common {

    public interface MpIPlatformInfo {
        bool IsAdmin { get; }
        string OsMachineName { get; }
        string OsVersion { get; }
        string OsFileManagerPath { get; }
        string RuntimeShortName { get; }
        string ExecutableName { get; }
        string ExecutingDir { get; }
        string ExecutingPath { get; }
        string StorageDir { get; }
        string LogDir { get; }
        string LogPath { get; }
        string LoggingEnabledCheckPath { get; }
        bool IsDesktop { get; }
        bool IsMobile { get; }
        bool IsBrowser { get; }
        bool IsTouchInputEnabled { get; }
        bool IsTraceEnabled { get; }
        string OsShortName { get; }
        string PlatformName { get; }
        string ResourcesDir { get; }
        string EditorPath { get; }
        string ThemesDir { get; }
        string TermsPath { get; }
        string EnumsPath { get; }
        string UiStringsPath { get; }
        string CreditsPath { get; }
        string CreditsPlatformPath { get; }

        TraceListener ConsoleTraceListener { get; }

        MpUserDeviceType OsType { get; }
    }
}
