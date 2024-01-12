using MonkeyPaste.Common;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using Avalonia.Platform;
using System.Runtime.InteropServices;
using static MonkeyPaste.Avalonia.NativeMethods;
using MonkeyPaste.Common.Plugin;




#if WINDOWS
using MonkeyPaste.Common.Wpf;
using static MonkeyPaste.Common.Wpf.WinApi;

#endif

namespace MonkeyPaste.Avalonia {
    public partial class MpAvProcessWatcher {

        #region Private Variable

        private string[] _ignoredProcessNames = new string[] {
            "csrss",
            "dwm",
            "mmc"
        };
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        public bool IsThisAppAdmin { get; private set; } = false;

        public IEnumerable<MpPortableProcessInfo> AllWindowProcessInfos =>
            GetOpenWindows()
            .GroupBy(x => (x.Key, x.Value))
            .Select(x => new MpPortableProcessInfo() {
                ApplicationName = GetAppNameByProessPath(x.Key.Key),
                ProcessPath = x.Key.Key,
                Handle = x.Key.Value
            });
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        protected virtual string GetAppNameByProessPath(string process_path) {
            string process_ext = Path.GetExtension(process_path);
            if (process_path.IsFile() &&
                FileVersionInfo.GetVersionInfo(process_path) is FileVersionInfo fvi) {

                if (!string.IsNullOrWhiteSpace(fvi.FileDescription)) {
                    return fvi.FileDescription.Replace(process_ext, string.Empty);
                }
                if (!string.IsNullOrWhiteSpace(fvi.ProductName)) {
                    return fvi.ProductName.Replace(process_ext, string.Empty);
                }
            }
            return Path.GetFileNameWithoutExtension(process_path);
        }
        protected nint GetParentHandleAtPoint(MpPoint p) {
            // Get the window/control that the mouse is hovering over...
            nint hwnd = WinApi.WindowFromPoint(new System.Drawing.Point((int)p.X, (int)p.Y));
            if (hwnd == nint.Zero) {
                return nint.Zero;
            }
            // Continue to get the parent until we reach the top-level window (with parent of NULL)...
            while (true) {
                nint p_hwnd = WinApi.GetParent(hwnd);
                if (p_hwnd == nint.Zero) {
                    return hwnd;
                }
                hwnd = p_hwnd;
            }
        }

        public nint SetActiveProcess(MpPortableProcessInfo p) {
            // details here https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow#remarks
            if (p.Handle == nint.Zero) {
                MpConsole.WriteLine("Warning cannot set active process to nint.Zero, ignoring");
                return nint.Zero;
            }
            //MpDebug.Assert(MpAvWindowManager.IsAnyActive, $"Must be active window to set fg window");
            //nint lastActive = WinApi.SetActiveWindow(ThisAppHandle);
            nint lastActive = GetActiveProcessHandle(); ;
            bool success = WinApi.SetForegroundWindow(p.Handle);
            MpConsole.WriteLine($"SetForegroundWindow to '{p.Handle}' from '{lastActive}' was {(success ? "SUCCESSFUL" : "FAILED")}");
            if (success) {
                return p.Handle;
            }
            return lastActive;
        }

        protected nint GetActiveProcessHandle() {
            return WinApi.GetForegroundWindow();
        }

        protected bool IsHandleWindowProcess(nint handle) {
            return
                WinApi.IsWindowVisible(handle) &&
                WinApi.IsWindow(handle) &&
                WinApi.GetWindowTextLength(handle) > 0;
        }

        protected string GetProcessTitle(nint hWnd) {
            try {
                if (hWnd == nint.Zero) {
                    return "Unknown Application";
                }
                int length = WinApi.GetWindowTextLength(hWnd);
                if (length == 0) {
                    return string.Empty;
                }

                StringBuilder builder = new StringBuilder(length);
                WinApi.GetWindowText(hWnd, builder, length + 1);
                return builder.ToString();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error getting process title for handle '{hWnd}'", ex);
                return "Unknown Application";
            }
        }

        private Func<nint, string>[] _getProcessMeths;
        protected string GetProcessPath(nint hWnd) {
            if (hWnd == nint.Zero) {
                return string.Empty;
            }
            if (_getProcessMeths == null) {
                _getProcessMeths = new Func<nint, string>[] {
                    GetProcessPath3,
                    GetProcessPath4,
                    GetProcessPath1,
                    GetProcessPath2,
                };
            }


            foreach (var (getMeth, meth_idx) in _getProcessMeths.WithIndex()) {
                try {
                    string process_path = getMeth(hWnd);
                    if (process_path.IsFile()) {
                        return process_path;
                    }
                    if (!string.IsNullOrEmpty(process_path)) {
                        MpConsole.WriteLine($"GetProcessMethod{(meth_idx + 1)} is gave invalid process path: '{process_path}'");
                    } else {
                        MpConsole.WriteLine($"GetProcessMethod{(meth_idx + 1)} couldn't find process path");
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"GetProcessPath error on method #{_getProcessMeths.IndexOf(getMeth)}.", ex);
                }
            }

            return string.Empty;

        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods
        #region Helpers

        private string GetProcessPath1(nint hWnd) {

            StringBuilder sb = new StringBuilder(2000);

            /** Need to get the process ID from handle under cursor **/
            GetWindowThreadProcessId(hWnd, out uint pid);

            /** Hook into process **/
            nint pic = OpenProcess(ProcessAccessFlags.All, true, (int)pid);

            /** This gets the filename of the process image. Path is in device format **/
            GetProcessImageFileName(pic, sb, 2000);

            return MpWpfDevicePathMapper.FromDevicePath(sb.ToString());
        }

        private string GetProcessPath2(nint hwnd) {
            GetWindowThreadProcessId(hwnd, out uint pid);
            string Query = "SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = " + pid;

            using (ManagementObjectSearcher mos = new ManagementObjectSearcher(Query)) {
                using (ManagementObjectCollection moc = mos.Get()) {
                    if (moc.Count == 0) {
                        return null;
                    }
                    var proc_record = (from mo in moc.Cast<ManagementObject>() select mo["ExecutablePath"]).FirstOrDefault();
                    if (proc_record is string path) {
                        return path;
                    }
                }
            }
            return null;
        }
        private string GetProcessPath3(nint hwnd) {
            GetWindowThreadProcessId(hwnd, out uint pid);
            int capacity = 1024;
            StringBuilder sb = new StringBuilder(capacity);
            nint handle = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, pid);
            QueryFullProcessImageName(handle, 0, sb, ref capacity);
            string fullPath = sb.ToString();

            return fullPath;
        }
        private string GetProcessPath4(nint hwnd) {
            GetWindowThreadProcessId(hwnd, out uint pid);
            using (Process proc = Process.GetProcessById((int)pid)) {
                // TODO when user clicks eye (to hide it) icon on running apps it should add to a string[] pref
                // and if it contains proc.ProcessName return fallback (so choice persists
                if (_ignoredProcessNames.Contains(proc.ProcessName.ToLower())) {
                    //occurs with messageboxes and dialogs
                    //MpConsole.WriteTraceLine($"Active process '{proc.ProcessName}' is on ignored list, using fallback '{fallback}'");
                    return null;
                }
                if (proc.MainWindowHandle == nint.Zero) {
                    return null; //null;
                }


                if (!Environment.Is64BitProcess && Is64Bit(proc)) {
                    return null;
                }

                bool isProcElevated = IsProcessAdmin(proc.MainWindowHandle);

                if (!IsThisAppAdmin && isProcElevated) {
                    return null;
                }

                return proc.MainModule.FileName.ToString().ToLower();
            }
        }

        private int GetShowWindowState(ProcessWindowStyle pws) {
            switch (pws) {
                case ProcessWindowStyle.Hidden:
                    return (int)ShowWindowCommands.Hide;

                case ProcessWindowStyle.Minimized:
                    return (int)ShowWindowCommands.Minimized;

                case ProcessWindowStyle.Maximized:
                    return (int)ShowWindowCommands.Maximized;

                default:
                    return (int)ShowWindowCommands.Normal;
            }
        }

        private bool Is64Bit(Process process) {
            if (!Environment.Is64BitOperatingSystem) {
                return false;
            }
            // if this method is not available in your version of .NET, use GetNativeSystemInfo via P/Invoke instead

            bool isWow64;
            if (!WinApi.IsWow64Process(process.Handle, out isWow64))
                throw new Win32Exception();
            return !isWow64;
        }


        private IDictionary<string, nint> GetOpenWindows() {
            nint shellWindow = GetShellWindow();
            Dictionary<string, nint> windows = new Dictionary<string, nint>();

            EnumWindows(delegate (nint hWnd, int lParam) {
                try {
                    if (!IsHandleWindowProcess(hWnd)) {
                        return true;
                    }
                    string process_path = GetProcessPath(hWnd);
                    if (string.IsNullOrEmpty(process_path)) {
                        return true;
                    }
                    windows.AddOrReplace(process_path, hWnd);
                }
                catch (InvalidOperationException ex) {
                    // no graphical interface
                    MpConsole.WriteLine("OpenWindowGetter, ignoring non GUI window w/ error: " + ex.ToString());
                }

                return true;

            }, 0);

            return windows;
        }

        protected nint GetThisAppHandle() {
            if (MpAvWindowManager.MainWindow is not { } mw ||
                mw.TryGetPlatformHandle() is not IPlatformHandle ph) {
                return nint.Zero;
            }
            return ph.Handle;
        }
        #endregion

        #endregion


        #region Unused

        private bool IsProcessAdmin(nint handle) {
            if (handle == nint.Zero) {
                return false;
            }
            try {
                WinApi.GetWindowThreadProcessId(handle, out uint pid);
                using (Process proc = Process.GetProcessById((int)pid)) {
                    nint ph = nint.Zero;
                    WinApi.OpenProcessToken(proc.Handle, WinApi.TOKEN_ALL_ACCESS, out ph);
                    WindowsIdentity iden = new WindowsIdentity(ph);
                    bool result = false;

                    foreach (IdentityReference role in iden.Groups) {
                        if (role.IsValidTargetType(typeof(SecurityIdentifier))) {
                            SecurityIdentifier sid = role as SecurityIdentifier;
                            if (sid.IsWellKnown(WellKnownSidType.AccountAdministratorSid) || sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid)) {
                                result = true;
                                break;
                            }
                        }
                    }
                    WinApi.CloseHandle(ph);
                    return result;
                }
            }
            catch (Exception ex) {
                //if app is started using "Run as" is if you get "Access Denied" error. 
                //That means that running app has rights that your app does not have. 
                //in this case ADMIN rights
                MpConsole.WriteLine("IsProcessAdmin error: " + ex.ToString());
                return true;
            }
        }



        //protected override ProcessWindowStyle GetWindowStyle(object handleIdOrTitle) {
        //    nint handle = nint.Zero;
        //    if (handleIdOrTitle is nint) {
        //        handle = (nint)handleIdOrTitle;
        //    } else {
        //        throw new NotImplementedException();
        //    }
        //    ShowWindowCommands swc = (ShowWindowCommands)GetWindowLong(handle, GWL_STYLE);
        //    switch (swc) {
        //        case ShowWindowCommands.Hide:
        //            return ProcessWindowStyle.Hidden;

        //        case ShowWindowCommands.Minimized:
        //            return ProcessWindowStyle.Minimized;

        //        case ShowWindowCommands.Maximized:
        //            return ProcessWindowStyle.Maximized;

        //        default:
        //            return ProcessWindowStyle.Normal;
        //    }
        //}
        #endregion
    }
    public static class NativeMethods {
        public const Int32 MONITOR_DEFAULTTOPRIMERTY = 0x00000001;
        public const Int32 MONITOR_DEFAULTTONEAREST = 0x00000002;


        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr handle, Int32 flags);


        [DllImport("user32.dll")]
        public static extern Boolean GetMonitorInfo(IntPtr hMonitor, NativeMonitorInfo lpmi);


        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct NativeRectangle {
            public Int32 Left;
            public Int32 Top;
            public Int32 Right;
            public Int32 Bottom;


            public NativeRectangle(Int32 left, Int32 top, Int32 right, Int32 bottom) {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }



    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public sealed class NativeMonitorInfo {
        public Int32 Size = Marshal.SizeOf(typeof(NativeMonitorInfo));
        public NativeRectangle Monitor;
        public NativeRectangle Work;
        public Int32 Flags;
    }
}
