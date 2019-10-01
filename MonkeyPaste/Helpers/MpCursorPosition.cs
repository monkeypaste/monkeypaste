using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    internal static class MpCursorPosition {
        [StructLayout(LayoutKind.Sequential)]
        struct PointInter {
            public int X;
            public int Y;
            public static explicit operator Point(PointInter point) => new Point(point.X,point.Y);
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out PointInter lpPoint);

        // For your convenience
        public static Point GetCursorPosition() {
            PointInter lpPoint;
            GetCursorPos(out lpPoint);
            return (Point)lpPoint;
        }
    }
}
