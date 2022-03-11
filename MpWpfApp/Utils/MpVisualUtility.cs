using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpVisualUtility {
        public static BitmapSource CreateBitmapSourceFromVisual(Double width,
            Double height,
            Visual visualToRender,
            Boolean undoTransformation) {
            if (visualToRender == null) {
                return null;
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap((Int32)Math.Ceiling(width),
                                                            (Int32)Math.Ceiling(height),
                                                            (Double)DeviceHelper.PixelsPerInch(Orientation.Horizontal),
                                                            (Double)DeviceHelper.PixelsPerInch(Orientation.Vertical),
                                                            PixelFormats.Pbgra32);
            if (undoTransformation) {
                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen()) {
                    VisualBrush vb = new VisualBrush(visualToRender);
                    dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
                }
                bmp.Render(dv);
            } else {
                bmp.Render(visualToRender);
            }

            return bmp;
        }

        public static BitmapSource CreateBitmapFromVisual(Visual visualToRender, Boolean undoTransformation) {
            if (visualToRender == null) {
                return null;
            }

            Rect bounds = VisualTreeHelper.GetContentBounds(visualToRender);
            return CreateBitmapSourceFromVisual(bounds.Width, bounds.Height, visualToRender, undoTransformation);
        }

        public static BitmapSource CreateBitmapFromVisual(Visual visualToRender) {
            return CreateBitmapFromVisual(visualToRender, false);
        }
    }

    public class DeviceHelper {
        public static Int32 PixelsPerInch(Orientation orientation) {
            Int32 capIndex = (orientation == Orientation.Horizontal) ? 0x58 : 90;
            using (DCSafeHandle handle = UnsafeNativeMethods.CreateDC("DISPLAY")) {
                return (handle.IsInvalid ? 0x60 : UnsafeNativeMethods.GetDeviceCaps(handle, capIndex));
            }
        }
    }

    internal sealed class DCSafeHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private DCSafeHandle() : base(true) { }

        protected override Boolean ReleaseHandle() {
            return UnsafeNativeMethods.DeleteDC(base.handle);
        }
    }

    [System.Security.SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern Boolean DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern Int32 GetDeviceCaps(DCSafeHandle hDC, Int32 nIndex);

        [DllImport("gdi32.dll", EntryPoint = "CreateDC", CharSet = CharSet.Auto)]
        public static extern DCSafeHandle IntCreateDC(String lpszDriver, String lpszDeviceName, String lpszOutput, IntPtr devMode);

        public static DCSafeHandle CreateDC(String lpszDriver) {
            return UnsafeNativeMethods.IntCreateDC(lpszDriver, null, null, IntPtr.Zero);
        }
    }
}
