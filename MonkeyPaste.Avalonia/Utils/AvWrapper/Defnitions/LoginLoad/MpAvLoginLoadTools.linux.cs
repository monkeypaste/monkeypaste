using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvLoginLoadTools {
        // from https://askubuntu.com/a/134369/1637790
        string AutoStartDir =>
            $"{Mp.Services.PlatformInfo.StorageDir}/../../../.config/autostart";

        string AutoStartLauncherPath =>
            Path.Combine(AutoStartDir, "monkeypaste.desktop");
        string LauncherPath =>
            "/usr/share/applications/monkeypaste.desktop";
        
        public bool IsLoadOnLoginEnabled =>
            AutoStartLauncherPath.IsFile();

        public async Task SetLoadOnLoginAsync(bool isLoadOnLogin, bool silent = false) {
            if(isLoadOnLogin == IsLoadOnLoginEnabled) {
                return;
            }
            await Task.Delay(1);

            if(isLoadOnLogin) {
                if(!AutoStartDir.IsDirectory()) {
                    MpFileIo.CreateDirectory(AutoStartDir);
                    //await $"mkdir {AutoStartDir}".ShellExecAsync();
                    if (!AutoStartDir.IsDirectory()) {
                        MpConsole.WriteLine($"Cannot enable login load cannot add auto start dir at '{AutoStartDir}'");
                        return;
                    }
                }
                if(!AutoStartLauncherPath.IsFile()) {
                    //await $"cp {LauncherPath} {AutoStartLauncherPath}".ShellExecAsync();
                    //MpFileIo.CopyFileOrDirectory(LauncherPath, AutoStartLauncherPath);
                    MpFileIo.WriteTextToFile(AutoStartLauncherPath, GetAutoStartEntry());
                }
            } else {
                if(AutoStartLauncherPath.IsFile()) {
                    MpFileIo.DeleteFile(AutoStartLauncherPath);
                }
            }
        }

        private string GetAutoStartEntry() {
            List<string> parts = default;
            if(LauncherPath.IsFile()) {
                string entry = MpFileIo.ReadTextFromFile(LauncherPath);
                parts = entry.SplitByLineBreak().ToList();
                if (parts.FirstOrDefault(x => x.StartsWith("Exec=")) is { } execLine) {
                    string new_exec = execLine + $" {App.LOGIN_LOAD_ARG}";
                    parts[parts.IndexOf(execLine)] = new_exec;
                }
                parts.Add("X-GNOME-Autostart-enabled=true");
            } else {
                parts = new List<string>() {
                    $"[Desktop Entry]",
                    $"Type=Application",
                    $"Exec={Mp.Services.PlatformInfo.ExecutingPath} {App.LOGIN_LOAD_ARG}",
                    $"Hidden=false",
                    $"Terminal=false",
                    $"NoDisplay=false",
                    $"X-GNOME-Autostart-enabled=true",
                    $"Name={Mp.Services.ThisAppInfo.ThisAppProductName}",
                    $"Comment=Login load for {Mp.Services.ThisAppInfo.ThisAppProductName}"
                };
            } 
            return string.Join(Environment.NewLine, parts);
        }
    }
}
