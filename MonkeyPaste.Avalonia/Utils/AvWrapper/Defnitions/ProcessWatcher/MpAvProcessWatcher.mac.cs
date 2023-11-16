#if MAC
using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvProcessWatcher {
        public static NSRunningApplication GetRunningApp(nint value) {
            return
                NSWorkspace.SharedWorkspace
                .RunningApplications
                .FirstOrDefault(x => x.ProcessIdentifier == (int)value);
        }
        protected nint GetThisAppHandle() {
            var ra =
                NSWorkspace.SharedWorkspace
                .RunningApplications
                .FirstOrDefault(x => x.BundleIdentifier == "com.Monkey.MonkeyPaste");
            if (ra == default) {
                return nint.Zero;
            }
            return ra.ProcessIdentifier;
        }
        protected nint GetActiveProcessHandle() {
            if (NSWorkspace.SharedWorkspace.FrontmostApplication
                    is not { } app) {
                return nint.Zero;
            }
            return app.ProcessIdentifier;
        }
        protected nint GetActiveWindowNumber() {
            if (NSWorkspace.SharedWorkspace.FrontmostApplication
                    is not { } app) {
                return nint.Zero;
            }
            return app.ProcessIdentifier;
        }
        protected string GetProcessTitle(nint handle) {
            // from https://stackoverflow.com/a/55662306/105028
            if (MpAvMacHelpers.GetCGWindowByPid(handle, "kCGWindowName") is not { } win_obj) {
                return string.Empty;
            }
            NSString win_name_key = new NSString("kCGWindowName");
            NSString win_name_val = win_obj.ValueForKey(win_name_key) as NSString;
            return win_name_val.ToString();
        }
        public IEnumerable<MpPortableProcessInfo> AllWindowProcessInfos =>
            NSWorkspace.SharedWorkspace.RunningApplications
            .Where(x => IsHandleWindowProcess(x.ProcessIdentifier))
            .Select(x => GetProcessInfoByHandle(x.ProcessIdentifier));

        protected string GetProcessPath(nint handle) {
            if (GetRunningApp(handle) is not { } app ||
                app.ExecutableUrl is not { } url) {

                return string.Empty;
            }
            return url.Path;
        }
        protected virtual string GetAppNameByProessPath(string process_path) {
            if (NSWorkspace.SharedWorkspace.RunningApplications
                .FirstOrDefault(x => x.ExecutableUrl != null && x.ExecutableUrl.Path == process_path) is not { } app) {
                return Path.GetFileNameWithoutExtension(process_path);
            }
            return app.BundleIdentifier.SplitNoEmpty(".").LastOrDefault();
            //if (app.BundleIdentifier.StartsWith("com.apple.")) {
            //    return app.BundleIdentifier.Replace("com.apple.", string.Empty);
            //}
            //if (NSBundle.FromIdentifier(app.BundleIdentifier) is not NSBundle bundle) {
            //    MpConsole.WriteLine($"Process watcher error. Could not find bundle for bundle id: '{app.BundleIdentifier}' from path '{process_path}'");

            //    return Path.GetFileNameWithoutExtension(process_path);
            //}
            //MpDebug.BreakAll();
            //string[] keys = new[] { "CFBundleDisplayName", "CFBundleName", "CFBundleExecutable" };
            //foreach (string key in keys) {
            //    if (bundle.ObjectForInfoDictionary(key) is NSString nstr) {
            //        string result = nstr.ToString();
            //        return result;
            //    }
            //}
            //return Path.GetFileNameWithoutExtension(process_path);
        }
        protected bool IsHandleWindowProcess(nint handle) {
            if (GetRunningApp(handle) is not { } app) {
                return false;
            }
            return
                !app.Terminated &&
                app.ActivationPolicy != NSApplicationActivationPolicy.Prohibited;
            //var result = MpAvMacHelpers.GetCGWindowByHandle(handle);
            //return result != default;
        }

        public nint SetActiveProcess(MpPortableProcessInfo p) {
            // trys to activate and returns actual active regardless of success
            // from https://stackoverflow.com/a/47264136/105028
            //if (new NSRunningApplication(handle) is not { } app) {
            //    // not found
            //    nint last_active_handle = GetActiveProcessHandle();
            //    return last_active_handle;
            //}
            if (GetRunningApp(p.Handle) is not { } app) {
                // not found
                nint last_active_handle = GetActiveProcessHandle();
                return last_active_handle;
            }
            app.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
            nint actual_active = GetActiveProcessHandle();
            return actual_active;
        }

        protected nint GetParentHandleAtPoint(MpPoint ponint) {
            // see inner if here https://gist.github.com/matthewreagan/2f3a30b8b229e9e2aa7c
            throw new NotImplementedException();
        }


        #region Helpers



        #endregion
    }
}

#endif