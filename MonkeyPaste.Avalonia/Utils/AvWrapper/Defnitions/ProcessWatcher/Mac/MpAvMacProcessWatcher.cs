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
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Avalonia {
    public class MpAvMacProcessWatcher : MpAvProcessWatcherBase {
        public override IEnumerable<MpPortableProcessInfo> AllWindowProcessInfos =>
            NSWorkspace.SharedWorkspace.RunningApplications
            .Where(x => IsHandleWindowProcess(x.Handle))
            .Select(x => GetProcessInfoByHandle(x.Handle));

        protected override string GetProcessPath(nint handle) {
            if (new NSRunningApplication(handle) is not { } app ||
                app.ExecutableUrl is not { } url) {
                //}
                //if (NSWorkspace.SharedWorkspace.RunningApplications
                //    .FirstOrDefault(x => x.Handle == handle) is not { } app) {
                return string.Empty;
            }
            return url.AbsoluteString;
        }

        protected override nint GetActiveProcessHandle() {
            if (NSWorkspace.SharedWorkspace.FrontmostApplication
                    is not { } app) {
                return nint.Zero;
            }
            return app.Handle;
        }

        protected override bool IsHandleWindowProcess(nint handle) {
            if (NSWorkspace.SharedWorkspace.RunningApplications
                .FirstOrDefault(x => x.Handle == handle) is not { } app) {
                return false;
            }
            return !app.Terminated && app.ActivationPolicy != NSApplicationActivationPolicy.Prohibited;
            //var result = MpAvMacHelpers.GetCGWindowByHandle(handle);
            //return result != default;
        }


        public override nint SetActiveProcess(nint handle) {
            // trys to activate and returns actual active regardless of success
            // from https://stackoverflow.com/a/47264136/105028
            if (new NSRunningApplication(handle) is not { } app) {
                // not found
                nint last_active_handle = GetActiveProcessHandle();
                return last_active_handle;
            }
            app.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
            nint actual_active = GetActiveProcessHandle();
            return actual_active;
        }

        protected override nint GetParentHandleAtPoint(MpPoint ponint) {
            // see inner if here https://gist.github.com/matthewreagan/2f3a30b8b229e9e2aa7c
            throw new NotImplementedException();
        }


        protected override string GetProcessTitle(nint handle) {
            // from https://stackoverflow.com/a/55662306/105028
            if (MpAvMacHelpers.GetCGWindowByHandle(handle) is not { } win_obj) {
                return string.Empty;
            }
            NSString win_name_key = new NSString("kCGWindowName");
            NSString win_name_val = win_obj.ValueForKey(win_name_key) as NSString;
            return win_name_val.ToString();
        }

        #region Helpers



        #endregion
    }
}

#endif