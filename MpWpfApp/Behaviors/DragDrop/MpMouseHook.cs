using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public static class MpMouseHook {
        private static IntPtr _hookID = IntPtr.Zero;
        private static LowLevelMouseProc _proc = HookCallback;

        private const int WH_MOUSE_LL = 14;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        public static long LeftMouseClicks { get; private set; }
        public static long RightMouseClicks { get; private set; }

        public static void Init() {
            _hookID = SetHook(_proc);
        }

        public static void Dispose() {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc) {
            using (Process currentProcess = Process.GetCurrentProcess()) {
                using (ProcessModule currentModule = currentProcess.MainModule)
                    return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(currentModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && MouseMessage.WM_LBUTTONDOWN == (MouseMessage)wParam)
                /*{
                    MSLLHOOK hookStruct = (MSLLHOOK)Marshal.PtrToStructure(lParam, typeof(MSLLHOOK));
                    Console.WriteLine(hookStruct.pt.x + ", " + hookStruct.pt.y);
                }*/
                LeftMouseClicks++;
            else if (nCode >= 0 && MouseMessage.WM_RBUTTONDOWN == (MouseMessage)wParam)
                RightMouseClicks++;

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private enum MouseMessage {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOK {
            POINT pt;
            uint mouseData;
            uint flags;
            uint time;
            IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT {
            int x;
            int y;
        }

        /// <summary>
        /// Enumerates the valid hook types passed as the idHook parameter into a call to SetWindowsHookEx.
        /// </summary>
        enum HookType : int {
            /// <summary>
            /// Installs a hook procedure that monitors messages generated as a result of an input event in a dialog box,
            /// message box, menu, or scroll bar. For more information, see the MessageProc hook procedure.
            /// </summary>
            WH_MSGFILTER = -1,
            /// <summary>
            /// Installs a hook procedure that records input messages posted to the system message queue. This hook is
            /// useful for recording macros. For more information, see the JournalRecordProc hook procedure.
            /// </summary>
            WH_JOURNALRECORD = 0,
            /// <summary>
            /// Installs a hook procedure that posts messages previously recorded by a WH_JOURNALRECORD hook procedure.
            /// For more information, see the JournalPlaybackProc hook procedure.
            /// </summary>
            WH_JOURNALPLAYBACK = 1,
            /// <summary>
            /// Installs a hook procedure that monitors keystroke messages. For more information, see the KeyboardProc
            /// hook procedure.
            /// </summary>
            WH_KEYBOARD = 2,
            /// <summary>
            /// Installs a hook procedure that monitors messages posted to a message queue. For more information, see the
            /// GetMsgProc hook procedure.
            /// </summary>
            WH_GETMESSAGE = 3,
            /// <summary>
            /// Installs a hook procedure that monitors messages before the system sends them to the destination window
            /// procedure. For more information, see the CallWndProc hook procedure.
            /// </summary>
            WH_CALLWNDPROC = 4,
            /// <summary>
            /// Installs a hook procedure that receives notifications useful to a CBT application. For more information,
            /// see the CBTProc hook procedure.
            /// </summary>
            WH_CBT = 5,
            /// <summary>
            /// Installs a hook procedure that monitors messages generated as a result of an input event in a dialog box,
            /// message box, menu, or scroll bar. The hook procedure monitors these messages for all applications in the
            /// same desktop as the calling thread. For more information, see the SysMsgProc hook procedure.
            /// </summary>
            WH_SYSMSGFILTER = 6,
            /// <summary>
            /// Installs a hook procedure that monitors mouse messages. For more information, see the MouseProc hook
            /// procedure.
            /// </summary>
            WH_MOUSE = 7,
            /// <summary>
            ///
            /// </summary>
            WH_HARDWARE = 8,
            /// <summary>
            /// Installs a hook procedure useful for debugging other hook procedures. For more information, see the
            /// DebugProc hook procedure.
            /// </summary>
            WH_DEBUG = 9,
            /// <summary>
            /// Installs a hook procedure that receives notifications useful to shell applications. For more information,
            /// see the ShellProc hook procedure.
            /// </summary>
            WH_SHELL = 10,
            /// <summary>
            /// Installs a hook procedure that will be called when the application's foreground thread is about to become
            /// idle. This hook is useful for performing low priority tasks during idle time. For more information, see the
            /// ForegroundIdleProc hook procedure.
            /// </summary>
            WH_FOREGROUNDIDLE = 11,
            /// <summary>
            /// Installs a hook procedure that monitors messages after they have been processed by the destination window
            /// procedure. For more information, see the CallWndRetProc hook procedure.
            /// </summary>
            WH_CALLWNDPROCRET = 12,
            /// <summary>
            /// Installs a hook procedure that monitors low-level keyboard input events. For more information, see the
            /// LowLevelKeyboardProc hook procedure.
            /// </summary>
            WH_KEYBOARD_LL = 13,
            /// <summary>
            /// Installs a hook procedure that monitors low-level mouse input events. For more information, see the
            /// LowLevelMouseProc hook procedure.
            /// </summary>
            WH_MOUSE_LL = 14
        }
    }
}
