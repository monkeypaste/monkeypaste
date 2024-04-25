using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvLoginLoadTools {
        // from https://askubuntu.com/a/134369/1637790
        string AutoStartPath =>
            $"{Mp.Services.PlatformInfo.StorageDir}/../../../.config/autostart";
                    

        public bool IsLoadOnLoginEnabled {
            get {                
                if (!AutoStartPath.IsFile() ||
                    MpFileIo.ReadTextFromFile(AutoStartPath) is not string auto_start_contents) {
                    return false;
                }
                return auto_start_contents.Contains(GetAutoStartEntry(true));
            }
        }

        public async Task SetLoadOnLoginAsync(bool isLoadOnLogin, bool silent = false) {
            if(isLoadOnLogin == IsLoadOnLoginEnabled) {
                return;
            }
            await Task.Delay(1);

            if(isLoadOnLogin) {
                string contents;
                if (AutoStartPath.IsFile()) {
                    // autostart file exists
                    contents = MpFileIo.ReadTextFromFile(AutoStartPath);
                    if(contents.Contains(GetAutoStartEntry(false))) {
                        // disabled entry exists, replace
                        contents = contents.Replace(GetAutoStartEntry(false), GetAutoStartEntry(true));
                    } else {
                        // no entry exists
                        contents = contents + Environment.NewLine + GetAutoStartEntry(true);
                    }
                } else {
                    contents = GetAutoStartEntry(true);
                }
                MpFileIo.WriteTextToFile(AutoStartPath, contents);
            } else {
                string contents;
                if (AutoStartPath.IsFile()) {
                    // autostart file exists
                    contents = MpFileIo.ReadTextFromFile(AutoStartPath);
                    if (contents.Contains(GetAutoStartEntry(true))) {
                        // enabled entry exists, replace
                        contents = contents.Replace(GetAutoStartEntry(true), GetAutoStartEntry(false));
                    } 
                } else {
                    // file doesn't exist, shouldn't happen
                    return;
                }
                if(contents == null) {
                    // no entry or file doesn't exist, shouldn't happen
                    return;
                }
                MpFileIo.WriteTextToFile(AutoStartPath, contents);
            }
        }

        private string GetAutoStartEntry(bool isEnabled) {
            string[] parts = {
                    $"[Desktop Entry]",
                    $"Type=Application",
                    $"Exec={Mp.Services.PlatformInfo.ExecutingPath}",
                    $"Hidden=false",
                    $"NoDisplay=false",
                    $"X-GNOME-Autostart-enabled={isEnabled.ToString().ToLowerInvariant()}",
                    $"Name={Mp.Services.ThisAppInfo.ThisAppProductName}",
                    $"Comment=Login load for {Mp.Services.ThisAppInfo.ThisAppProductName}"
                };
            return string.Join(Environment.NewLine, parts);
        }
    }
}
