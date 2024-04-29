using MonkeyPaste.Common;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public enum MpXdoCmdType {
        None = 0,
        WindowActivate,
        WindowFocus,
        GetWindowFocus,
        GetWindowPid,
        GetWindowName
    }
    public static class MpXdoCmd {
        static string[] _errorPrefixes = [
            "X Error",
            "Error for cmd"
            ];

        public static string Exec(MpXdoCmdType cmd, int handle = default) {
            string cmd_text = $"xdotool {cmd.ToString().ToLowerInvariant()}";
            switch(cmd) {
                case MpXdoCmdType.WindowFocus:
                case MpXdoCmdType.WindowActivate:
                case MpXdoCmdType.GetWindowPid:
                case MpXdoCmdType.GetWindowName:
                    if(handle == 0) {
                        return default;
                    }
                    cmd_text += $" {handle}";
                    break;
            }
            string result = cmd_text.ShellExec().Trim();
            if(_errorPrefixes.Any(x=>result.StartsWith(x))) {
                return default;
            }
            return result;
        }
        public static T Exec<T>(MpXdoCmdType cmd, int handle = default) where T : new() {
            string result = Exec(cmd, handle);
            if(_errorPrefixes.Any(x=>result.StartsWith(x))) {
                return default;
            }
            if(typeof(T) == typeof(string)) {
                return (T)(object)result;
            }
            if(typeof(T) == typeof(int) &&
                int.TryParse(result,out int intResult)) {
                return (T)(object)intResult;
            }
            if(typeof(T) == typeof(bool)) {
                return (T)(object)true;
            }
            return default;
        }
    }
}
