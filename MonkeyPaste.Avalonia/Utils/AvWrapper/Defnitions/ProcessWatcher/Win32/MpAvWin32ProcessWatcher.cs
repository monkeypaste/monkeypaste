using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Text;
using System.Management;
using System.Linq;
using System.ComponentModel;
using System.Security.Principal;
#if WINDOWS
using MonkeyPaste.Common.Wpf;
using static MonkeyPaste.Common.Wpf.WinApi;

#endif

namespace MonkeyPaste.Avalonia {
    public class MpAvWin32ProcessWatcher : MpAvProcessWatcherBase {

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
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        protected override IntPtr GetParentHandleAtPoint(MpPoint p) {
#if WINDOWS
            // Get the window/control that the mouse is hovering over...
            IntPtr hwnd = WinApi.WindowFromPoint(new System.Drawing.Point((int)p.X, (int)p.Y));
            if (hwnd == IntPtr.Zero) {
                return IntPtr.Zero;
            }
            // Continue to get the parent until we reach the top-level window (with parent of NULL)...
            while (true) {
                IntPtr p_hwnd = WinApi.GetParent(hwnd);
                if (p_hwnd == IntPtr.Zero) {
                    return hwnd;
                }
                hwnd = p_hwnd;
            }
#else
            return IntPtr.Zero;
#endif
        }

        public override IntPtr SetActiveProcess(IntPtr handle) {
            // details here https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow#remarks
            if (handle == IntPtr.Zero) {
                MpConsole.WriteLine("Warning cannot set active process to IntPtr.Zero, ignoring");
                return IntPtr.Zero;
            }
            //if (!MpAvMainView.Instance.IsVisible) {
            //    MpConsole.WriteLine("Warning cannot set active process mw is not visible");
            //    return IntPtr.Zero;
            //}
            IntPtr lastActive = WinApi.SetActiveWindow(ThisAppHandle);
            bool success = WinApi.SetForegroundWindow(handle);
            MpConsole.WriteLine($"SetForegroundWindow to '{handle}' from '{lastActive}' was {(success ? "SUCCESSFUL" : "FAILED")}");
            if (success) {
                return handle;
            }
            return lastActive;
        }
        protected override IntPtr SetActiveProcess(IntPtr handle, ProcessWindowStyle windowStyle) {
            if (!WinApi.ShowWindowAsync(handle, GetShowWindowState(windowStyle))) {
                MpConsole.WriteLine($"ShowWindowAsync failed for handle '{handle}' with window state '{windowStyle}'");
            }
            return SetActiveProcess(handle);
        }

        protected override bool IsAdmin(object handleIdOrTitle) {
            if (handleIdOrTitle is IntPtr handle) {
                return IsProcessAdmin(handle);
            }
            throw new NotImplementedException();
        }

        protected override ProcessWindowStyle GetWindowStyle(object handleIdOrTitle) {
            IntPtr handle = IntPtr.Zero;
            if (handleIdOrTitle is IntPtr) {
                handle = (IntPtr)handleIdOrTitle;
            } else {
                throw new NotImplementedException();
            }
            ShowWindowCommands swc = (ShowWindowCommands)GetWindowLong(handle, GWL_STYLE);
            switch (swc) {
                case ShowWindowCommands.Hide:
                    return ProcessWindowStyle.Hidden;

                case ShowWindowCommands.Minimized:
                    return ProcessWindowStyle.Minimized;

                case ShowWindowCommands.Maximized:
                    return ProcessWindowStyle.Maximized;

                default:
                    return ProcessWindowStyle.Normal;
            }
        }

        protected override nint GetActiveProcessHandle() {
            return WinApi.GetForegroundWindow();
        }
        protected override MpPortableProcessInfo GetProcessInfoByHandle(nint handle) {
            if (handle == nint.Zero) {
                return null;
            }
            var ppi = new MpPortableProcessInfo() {
                Handle = handle,
                ProcessPath = GetProcessPath(handle),
                ApplicationName = GetProcessApplicationName(handle),
                MainWindowTitle = GetProcessTitle(handle)
            };
            ppi.MainWindowIconBase64 = Mp.Services.IconBuilder.GetPathIconBase64(ppi.ProcessPath);
            return ppi;
        }
        protected override bool IsHandleWindowProcess(nint handle) {
            return
                WinApi.IsWindowVisible(handle) &&
                WinApi.IsWindow(handle) &&
                WinApi.GetWindowTextLength(handle) > 0;
        }

        private string GetProcessTitle(IntPtr hWnd) {
            try {
                if (hWnd == IntPtr.Zero) {
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

        protected override Process GetProcess(object handleIdOrTitle) {
            if (handleIdOrTitle is IntPtr handle) {
                GetWindowThreadProcessId(handle, out uint pid);
                return base.GetProcess((int)pid);
            }
            return base.GetProcess(handleIdOrTitle);
        }
        private Func<IntPtr, string>[] _getProcessMeths;
        protected override string GetProcessPath(IntPtr hWnd) {
            if (hWnd == IntPtr.Zero) {
                return string.Empty;
            }
            if (_getProcessMeths == null) {
                _getProcessMeths = new Func<IntPtr, string>[] {
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

        private string GetProcessPath1(IntPtr hWnd) {

            StringBuilder sb = new StringBuilder(2000);

            /** Need to get the process ID from handle under cursor **/
            GetWindowThreadProcessId(hWnd, out uint pid);

            /** Hook into process **/
            IntPtr pic = OpenProcess(ProcessAccessFlags.All, true, (int)pid);

            /** This gets the filename of the process image. Path is in device format **/
            GetProcessImageFileName(pic, sb, 2000);

            return MpWpfDevicePathMapper.FromDevicePath(sb.ToString());
        }

        private string GetProcessPath2(IntPtr hwnd) {
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
        private string GetProcessPath3(IntPtr hwnd) {
            GetWindowThreadProcessId(hwnd, out uint pid);
            int capacity = 1024;
            StringBuilder sb = new StringBuilder(capacity);
            IntPtr handle = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, pid);
            QueryFullProcessImageName(handle, 0, sb, ref capacity);
            string fullPath = sb.ToString();

            return fullPath;
        }
        private string GetProcessPath4(IntPtr hwnd) {
            GetWindowThreadProcessId(hwnd, out uint pid);
            using (Process proc = Process.GetProcessById((int)pid)) {
                // TODO when user clicks eye (to hide it) icon on running apps it should add to a string[] pref
                // and if it contains proc.ProcessName return fallback (so choice persists
                if (_ignoredProcessNames.Contains(proc.ProcessName.ToLower())) {
                    //occurs with messageboxes and dialogs
                    //MpConsole.WriteTraceLine($"Active process '{proc.ProcessName}' is on ignored list, using fallback '{fallback}'");
                    return null;
                }
                if (proc.MainWindowHandle == IntPtr.Zero) {
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

        private bool IsProcessAdmin(IntPtr handle) {
            if (handle == IntPtr.Zero) {
                return false;
            }
            try {
                WinApi.GetWindowThreadProcessId(handle, out uint pid);
                using (Process proc = Process.GetProcessById((int)pid)) {
                    IntPtr ph = IntPtr.Zero;
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

        #endregion

        #endregion
    }
}
