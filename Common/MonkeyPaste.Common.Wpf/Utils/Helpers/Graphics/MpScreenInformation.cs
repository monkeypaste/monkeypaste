using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace MonkeyPaste.Common.Wpf {

    public enum MpTaskbarLocation {
        None,
        Bottom,
        Right,
        Top,
        Left
    }
    // note this class considers dpix = dpiy
    public static class DpiUtilities {
        // you should always use this one and it will fallback if necessary
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdpiforwindow
        public static int GetDpiForWindow(IntPtr hwnd) {
            var h = LoadLibrary("user32.dll");
            var ptr = GetProcAddress(h, "GetDpiForWindow"); // Windows 10 1607
            if (ptr == IntPtr.Zero)
                return GetDpiForNearestMonitor(hwnd);

            return Marshal.GetDelegateForFunctionPointer<GetDpiForWindowFn>(ptr)(hwnd);
        }

        public static int GetDpiForNearestMonitor(IntPtr hwnd) => GetDpiForMonitor(GetNearestMonitorFromWindow(hwnd));
        public static int GetDpiForNearestMonitor(int x, int y) => GetDpiForMonitor(GetNearestMonitorFromPoint(x, y));
        public static int GetDpiForMonitor(IntPtr monitor, MonitorDpiType type = MonitorDpiType.Effective) {
            var h = LoadLibrary("shcore.dll");
            var ptr = GetProcAddress(h, "GetDpiForMonitor"); // Windows 8.1
            if (ptr == IntPtr.Zero)
                return GetDpiForDesktop();

            int hr = Marshal.GetDelegateForFunctionPointer<GetDpiForMonitorFn>(ptr)(monitor, type, out int x, out int y);
            if (hr < 0)
                return GetDpiForDesktop();

            return x;
        }

        public static int GetDpiForDesktop() {
            int hr = D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED, typeof(ID2D1Factory).GUID, IntPtr.Zero, out ID2D1Factory factory);
            if (hr < 0)
                return 96; // we really hit the ground, don't know what to do next!

            factory.GetDesktopDpi(out float x, out float y); // Windows 7
            Marshal.ReleaseComObject(factory);
            return (int)x;
        }

        public static IntPtr GetDesktopMonitor() => GetNearestMonitorFromWindow(GetDesktopWindow());
        public static IntPtr GetShellMonitor() => GetNearestMonitorFromWindow(GetShellWindow());
        public static IntPtr GetNearestMonitorFromWindow(IntPtr hwnd) => MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        public static IntPtr GetNearestMonitorFromPoint(int x, int y) => MonitorFromPoint(new POINT { x = x, y = y }, MONITOR_DEFAULTTONEAREST);

        private delegate int GetDpiForWindowFn(IntPtr hwnd);
        private delegate int GetDpiForMonitorFn(IntPtr hmonitor, MonitorDpiType dpiType, out int dpiX, out int dpiY);

        private const int MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("user32")]
        private static extern IntPtr MonitorFromPoint(POINT pt, int flags);

        [DllImport("user32")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);

        [DllImport("user32")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32")]
        private static extern IntPtr GetShellWindow();

        [StructLayout(LayoutKind.Sequential)]
        private partial struct POINT {
            public int x;
            public int y;
        }

        [DllImport("d2d1")]
        private static extern int D2D1CreateFactory(D2D1_FACTORY_TYPE factoryType, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, IntPtr pFactoryOptions, out ID2D1Factory ppIFactory);

        private enum D2D1_FACTORY_TYPE {
            D2D1_FACTORY_TYPE_SINGLE_THREADED = 0,
            D2D1_FACTORY_TYPE_MULTI_THREADED = 1,
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("06152247-6f50-465a-9245-118bfd3b6007")]
        private interface ID2D1Factory {
            int ReloadSystemMetrics();

            [PreserveSig]
            void GetDesktopDpi(out float dpiX, out float dpiY);

            // the rest is not implemented as we don't need it
        }
    }

    public enum MonitorDpiType {
        Effective = 0,
        Angular = 1,
        Raw = 2,
    }
    public static class MpScreenInformation {
        public static uint DefaultDpi = 96;

        public static uint RawDpi { get; private set; }
        public static uint DpiX { get; private set; }
        public static uint DpiY { get; private set; }

        public static double ThisAppDip { get; private set; } = 1.0d;

        public static void Init() {
            uint dpiX;
            uint dpiY;
            GetDpi(DpiType.EFFECTIVE, out dpiX, out dpiY);
            RawDpi = Math.Max(dpiX, dpiY);
            DpiX = dpiX;
            DpiY = dpiY;
            ThisAppDip = VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip;
        }

        public static Point ConvertWinFormsScreenPointToWpf(System.Drawing.Point winFormsPoint) {
            double wpf_x = Math.Max(0, 96.0d * winFormsPoint.X / DpiX);
            double wpf_y = Math.Max(0, 96.0d * winFormsPoint.Y / DpiY);
            return new Point(wpf_x, wpf_y);
        }

        public static System.Drawing.Point ConvertWpfScreenPointToWinForms(Point wpfPoint) {
            double winforms_x = Math.Max(0, wpfPoint.X * DpiX / 96.0d);
            double winforms_y = Math.Max(0, wpfPoint.Y * DpiY / 96.0d);
            return new System.Drawing.Point((int)winforms_x, (int)winforms_y);
        }

        public static MpTaskbarLocation TaskbarLocation {
            get {
                if (SystemParameters.WorkArea.Top == 0) {
                    return MpTaskbarLocation.Bottom;
                } else if (SystemParameters.WorkArea.Left != 0) {
                    return MpTaskbarLocation.Right;
                } else if (SystemParameters.WorkArea.Right != SystemParameters.PrimaryScreenWidth) {
                    return MpTaskbarLocation.Left;
                }
                return MpTaskbarLocation.Top;
            }
        }

        /// <summary>
        /// Returns the scaling of the given screen.
        /// </summary>
        /// <param name="dpiType">The type of dpi that should be given back..</param>
        /// <param name="dpiX">Gives the horizontal scaling back (in dpi).</param>
        /// <param name="dpiY">Gives the vertical scaling back (in dpi).</param>
        private static void GetDpi(DpiType dpiType, out uint dpiX, out uint dpiY) {
            var point = new System.Drawing.Point(1, 1);
            var hmonitor = MonitorFromPoint(point, _MONITOR_DEFAULTTONEAREST);

            switch (GetDpiForMonitor(hmonitor, dpiType, out dpiX, out dpiY).ToInt32()) {
                case _S_OK: return;
                case _E_INVALIDARG:
                    throw new ArgumentException("Unknown error. See https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510.aspx for more information.");
                default:
                    throw new COMException("Unknown error. See https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510.aspx for more information.");
            }
        }

        //https://msdn.microsoft.com/en-us/library/windows/desktop/dd145062.aspx
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint([In] System.Drawing.Point pt, [In] uint dwFlags);

        //https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510.aspx
        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

        const int _S_OK = 0;
        const int _MONITOR_DEFAULTTONEAREST = 2;
        const int _E_INVALIDARG = -2147024809;
    }

    /// <summary>
    /// Represents the different types of scaling.
    /// </summary>
    /// <seealso cref="https://msdn.microsoft.com/en-us/library/windows/desktop/dn280511.aspx"/>
    public enum DpiType {
        EFFECTIVE = 0,
        ANGULAR = 1,
        RAW = 2,
    }
}
