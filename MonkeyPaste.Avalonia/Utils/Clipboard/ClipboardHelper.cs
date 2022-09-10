
using System;
using System.Runtime.InteropServices;


namespace MonkeyPaste.Avalonia {
    public class MpAvWin32HtmlClipboardHelper {

        //--------------------------------------------------------------------------------
        //http://metadataconsulting.blogspot.com/2019/06/How-to-get-HTML-from-the-Windows-system-clipboard-directly-using-PInvoke-Win32-Native-methods-avoiding-bad-funny-characters.html
        //--------------------------------------------------------------------------------

        #region Win32 Native PInvoke

        [DllImport("User32.dll", SetLastError = true)]
        private static extern uint RegisterClipboardFormatA(string lpszFormat);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GlobalSize(IntPtr hMem);

        [DllImport("User32.dll", SetLastError = true)]
        private static extern uint RegisterClipboardFormat(string lpszFormat);
        //or specifically - private static extern uint RegisterClipboardFormatA(string lpszFormat);

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("User32.dll", SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseClipboard();

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        //[DllImport("Kernel32.dll", SetLastError = true)]
        //private static extern int GlobalSize(IntPtr hMem);

        #endregion

        public static string GetHTMLWin32Native() {
            string strHTMLUTF8 = string.Empty;
            uint CF_HTML = RegisterClipboardFormatA("HTML Format");
            if (CF_HTML == 0)
                return null;

            if (!IsClipboardFormatAvailable(CF_HTML))
                return null;

            try {
                if (!OpenClipboard(IntPtr.Zero))
                    return null;

                IntPtr handle = GetClipboardData(CF_HTML);
                if (handle == IntPtr.Zero)
                    return null;

                IntPtr pointer = IntPtr.Zero;

                try {
                    pointer = GlobalLock(handle);
                    if (pointer == IntPtr.Zero)
                        return null;

                    uint size = GlobalSize(handle);
                    byte[] buff = new byte[size];

                    Marshal.Copy(pointer, buff, 0, (int)size);

                    strHTMLUTF8 = System.Text.Encoding.UTF8.GetString(buff);
                }
                finally {
                    if (pointer != IntPtr.Zero)
                        GlobalUnlock(handle);
                }
            }
            finally {
                CloseClipboard();
            }

            return strHTMLUTF8;
        }
    }
}
