#if MAC
using Avalonia.Platform;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvMacHelpers {
        private static bool _isInitialized;

        public static void EnsureInitialized() {
            if (_isInitialized) {
                return;
            }
            _isInitialized = true;
            NSApplication.Init();
        }

        #region NSWindow
        public static IEnumerable<NSWindow> GetThisAppWindows() {
            int this_app_pid = GetThisAppPid();
            if (this_app_pid == default) {
                return null;
            }
            var this_running_app =
                NSWorkspace.SharedWorkspace
                .RunningApplications
                .FirstOrDefault(x => x.ProcessIdentifier == this_app_pid);
            if (this_running_app == default) {
                return null;
            }
            NSApplication app = new NSApplication(this_running_app.Handle);
            MpDebug.BreakAll();
            return app.Windows;
            //if (GetCGWindowObjsByProperty("kCGWindowOwnerPID", this_app_pid) is { } cg_window_objs) {
            //    foreach (var cg_win in cg_window_objs) {
            //        app.Win
            //        if (TryGetNSObjectProperty(cg_win, "kCGWindowNumber", out long win_long) &&
            //            app.WindowWithWindowNumber(win_long) is NSWindow win) {
            //            yield return win;
            //        }
            //    }
            //}
            //return null;
        }

        #endregion

        #region NSObject

        public static int GetThisAppPid() {
            // from https://stackoverflow.com/questions/52441936/macos-active-window-title-using-c-sharp
            var result = GetCGWindowObjsByProperty("kCGWindowOwnerName", "MonkeyPaste");
            if (result.FirstOrDefault() is { } win_obj) {
                if (TryGetNSObjectProperty<int>(win_obj, "kCGWindowOwnerPID", out int pid)) {
                    return pid;
                }
            }
            return default;
        }


        public static bool TryGetNSObjectProperty<T>(NSObject info_obj, string key, out T result) {
            result = default;
            NSObject cur_val_obj = info_obj.ValueForKey(new NSString(key));
            if (cur_val_obj == null) {
                return false;
            }
            if (typeof(T) == typeof(string)) {
                string cur_val_str = cur_val_obj.ToString();
                result = (T)(object)cur_val_str;
                return true;
            }
            if (typeof(T) == typeof(int)) {
                if (cur_val_obj is NSNumber cur_val_num &&
                    int.TryParse(cur_val_num.StringValue, out int intVal)) {
                    result = (T)(object)intVal;
                    return true;
                }
            }
            if (typeof(T) == typeof(long)) {
                if (cur_val_obj is NSNumber cur_val_num &&
                    long.TryParse(cur_val_num.StringValue, out long longVal)) {
                    result = (T)(object)longVal;
                    return true;
                }
            }
            if (typeof(T) == typeof(nint)) {
                if (cur_val_obj is NSNumber cur_val_num &&
                    nint.TryParse(cur_val_num.StringValue, out nint cur_val_nint)) {
                    result = (T)(object)cur_val_nint;
                    return true;
                }
            }
            return false;
        }

        public static bool TryGetNSObjectProperty<T>(NSObject info_obj, string key, T matchValue, out T result) {
            if (TryGetNSObjectProperty(info_obj, key, out result)) {
                return result.Equals(matchValue);
            }
            return false;
        }

        public static IEnumerable<NSObject> GetCGWindowObjsByProperty<T>(string key, T matchValue) {
            // from https://stackoverflow.com/questions/52441936/macos-active-window-title-using-c-sharp
            IntPtr windowInfo = CGWindowListCopyWindowInfo(CGWindowListOption.OnScreenOnly, 0);
            NSArray values = Runtime.GetNSObject(windowInfo) as NSArray;
            for (ulong i = 0, len = values.Count; i < len; i++) {
                NSObject info_obj = Runtime.GetNSObject(values.ValueAt(i));
                if (TryGetNSObjectProperty(info_obj, key, matchValue, out var result)) {
                    yield return info_obj;
                }
            }
            yield break;
        }

        public static bool IsPathExecutableUnderAppBundle(string path) {
            if (!string.IsNullOrEmpty(path) &&
                path.StartsWith(@"/Applications") &&
                path.Contains(".app") &&
                string.IsNullOrEmpty(Path.GetExtension(path))) {
                return true;
            }
            return false;
        }
        public static string GetAppBundlePathOrDefault(string path) {
            if (string.IsNullOrEmpty(path) ||
                path.SplitNoEmpty(".app") is not { } pathParts ||
                pathParts.Length <= 1) {
                return path;
            }
            // for exec path's within an app use app icon
            return pathParts[0] + ".app";
        }

        public static NSObject GetCGWindowByPid(nint pid, string filter_key = default) {
            // from https://stackoverflow.com/questions/52441936/macos-active-window-title-using-c-sharp
            string handle_str = pid.ToString();
            IntPtr windowInfo = CGWindowListCopyWindowInfo(CGWindowListOption.OnScreenOnly, 0);
            NSArray values = Runtime.GetNSObject(windowInfo) as NSArray;

            for (ulong i = 0, len = values.Count; i < len; i++) {
                nint val_handle = values.ValueAt(i);
                NSObject info_obj = Runtime.GetNSObject(val_handle);
                NSString pid_key = new NSString("kCGWindowOwnerPID");
                NSObject pid_val_obj = info_obj.ValueForKey(pid_key);
                if (pid_val_obj == null) {
                    continue;
                }
                if (filter_key != null && info_obj.ValueForKey(new NSString(filter_key)) is null) {
                    // ex. needed for windowName in terminal
                    continue;
                }
                if (pid_val_obj is NSNumber pid_val_num) {
                    string pid_val_str = pid_val_num.StringValue;
                    if (nint.TryParse(pid_val_str, out nint pid_val)) {
                        if (pid_val == pid) {
                            return info_obj;
                        }
                    }
                    if (pid_val_str == handle_str) {
                        return info_obj;
                    }
                }
            }
            return default;
        }

        #endregion


        #region Imports
        const string QuartzCore = @"/System/Library/Frameworks/QuartzCore.framework/QuartzCore";

        [DllImport(QuartzCore)]
        static extern IntPtr CGWindowListCopyWindowInfo(CGWindowListOption option, uint relativeToWindow);


        #endregion
    }
}
#endif
