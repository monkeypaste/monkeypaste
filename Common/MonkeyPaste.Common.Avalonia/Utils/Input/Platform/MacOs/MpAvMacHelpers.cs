
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

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvMacHelpers {
        static MpAvMacHelpers() {
            EnsureInitialized();
        }

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
                path.ToLowerInvariant().Contains(@".app/") &&
                string.IsNullOrEmpty(Path.GetExtension(path))) {
                // only return true for extensionless files under some .app/ dir
                return true;
            }
            return false;
        }
        public static string GetAppBundlePathOrDefault(string path) {
            if (string.IsNullOrEmpty(path) ||
                path.ToLowerInvariant().SplitNoEmpty(".app") is not { } pathParts ||
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

        #region Images

        public static string ConvertToBase64(NSImage nsimage) {
            // from https://stackoverflow.com/a/52110970/105028
            /*
             var base64String: String? {
        guard let rep = NSBitmapImageRep(
            bitmapDataPlanes: nil,
            pixelsWide: Int(size.width),
            pixelsHigh: Int(size.height),
            bitsPerSample: 8,
            samplesPerPixel: 4,
            hasAlpha: true,
            isPlanar: false,
            colorSpaceName: .calibratedRGB,
            bytesPerRow: 0,
            bitsPerPixel: 0
            ) else {
                print("Couldn't create bitmap representation")
                return nil
        }
        
        NSGraphicsContext.saveGraphicsState()
        NSGraphicsContext.current = NSGraphicsContext(bitmapImageRep: rep)
        draw(at: NSZeroPoint, from: NSZeroRect, operation: .sourceOver, fraction: 1.0)
        NSGraphicsContext.restoreGraphicsState()
        
        guard let data = rep.representation(using: NSBitmapImageRep.FileType.png, properties: [NSBitmapImageRep.PropertyKey.compressionFactor: 1.0]) else {
            print("Couldn't create PNG")
            return nil
        }
        
        // With prefix
        // return "data:image/png;base64,\(data.base64EncodedString(options: []))" 
        // Without prefix
        return data.base64EncodedString(options: [])
    }
             public NSBitmapImageRep(IntPtr planes, long width, long height, long bps, long spp, bool alpha, bool isPlanar, string colorSpaceName, NSBitmapFormat bitmapFormat, long rBytes, long pBits)
            */
            //var rep = new NSBitmapImageRep(
            //    planes: nint.Zero,
            //    width: (long)nsimage.Size.Width,
            //    height: (long)nsimage.Size.Height,
            //    bps: (long)8,
            //    spp: (long)4,
            //    alpha: true,
            //    isPlanar: false,
            //    colorSpaceName: NSColorSpace.CalibratedRGB.ToString(),
            //    rBytes: 0,
            //    pBits: 0);
            //NSGraphicsContext
            return null;
        }
        #endregion

        #region Rtf
        public static string RtfToHtml(string rtf) {
            // from https://stackoverflow.com/a/20925575/105028
            /*
            NSTask *task = [[NSTask alloc] init];
            [task setLaunchPath: @"/usr/bin/textutil"];
            [task setArguments: @[@"-format", @"rtf", @"-convert", @"html", @"-stdin", @"-stdout"]];
            [task setStandardInput:[NSPipe pipe]];
            [task setStandardOutput:[NSPipe pipe]];
            NSFileHandle *taskInput = [[task standardInput] fileHandleForWriting];
            [taskInput writeData:[NSData dataWithBytes:cString length:cStringLength]];
            [task launch];
            [taskInput closeFile];

            // sync
            NSData *outData = [[[task standardOutput] fileHandleForReading] readDataToEndOfFile];
            NSString *outStr = [[NSString alloc] initWithData:outData encoding:NSUTF8StringEncoding];
            */
            NSData rtf_data = NSData.FromString(rtf);
            NSTask task = new NSTask();
            task.LaunchPath = @"/usr/bin/textutil";
            task.Arguments = ["-format", "rtf", "-convert", "html", "-stdin", "-stdout" ];
            task.StandardInput = new NSPipe();
            task.StandardOutput = new NSPipe();
            NSFileHandle taskInput = (task.StandardInput as NSPipe).WriteHandle;
            taskInput.WriteData(rtf_data);
            task.Launch();
            taskInput.CloseFile();

            NSData outData = (task.StandardOutput as NSPipe).ReadHandle.ReadDataToEndOfFile();
            NSString outStr = NSString.FromData(outData, NSStringEncoding.UTF8);
            string html = outStr.ToString();

            var pm = new PreMailer.Net.PreMailer(html);
            var result = pm.MoveCssInline();
            // BUG somewhere in this process (source rtf, rtf2html or inlining css) a trailing newline(s) is added
            // so linearizing the html
            return result.Html.StripLineBreaks();
        }

        public static string Html2Rtf(string html) {
            // from https://stackoverflow.com/a/20925575/105028
            NSData rtf_data = NSData.FromString(html);
            NSTask task = new NSTask();
            task.LaunchPath = @"/usr/bin/textutil";
            task.Arguments = new[] { "-format", "html", "-convert", "rtf", "-stdin", "-stdout" };
            task.StandardInput = new NSPipe();
            task.StandardOutput = new NSPipe();
            NSFileHandle taskInput = (task.StandardInput as NSPipe).WriteHandle;
            taskInput.WriteData(rtf_data);
            task.Launch();
            taskInput.CloseFile();

            NSData outData = (task.StandardOutput as NSPipe).ReadHandle.ReadDataToEndOfFile();
            NSString outStr = NSString.FromData(outData, NSStringEncoding.UTF8);
            string rtf = outStr.ToString();

            return rtf;
        }
        #endregion

        #region Pasteboard


        #endregion

        #region Imports
        const string QuartzCore = @"/System/Library/Frameworks/QuartzCore.framework/QuartzCore";

        [DllImport(QuartzCore)]
        static extern IntPtr CGWindowListCopyWindowInfo(CGWindowListOption option, uint relativeToWindow);


        #endregion
    }
}