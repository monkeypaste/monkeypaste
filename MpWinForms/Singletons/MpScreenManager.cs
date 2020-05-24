using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpScreenManager {
        public Rectangle GetScreenWorkingAreaWithMouse() {
            foreach (Screen screen in Screen.AllScreens) {
                //get cursor pos
                WinApi.PointInter lpPoint;
                WinApi.GetCursorPos(out lpPoint);
                Point mp = (Point)lpPoint;
                if (screen.WorkingArea.Contains(mp)) {
                    return screen.WorkingArea;
                }
            }
            return Screen.FromHandle(Process.GetCurrentProcess().Handle).WorkingArea;
        }
        public Rectangle GetScreenBoundsWithMouse() {
            foreach(Screen screen in Screen.AllScreens) {
                //get cursor pos
                WinApi.PointInter lpPoint;
                WinApi.GetCursorPos(out lpPoint);
                Point mp = (Point)lpPoint;
                if(screen.WorkingArea.Contains(mp)) {
                    return screen.Bounds;
                }
            }
            return Screen.FromHandle(Process.GetCurrentProcess().Handle).Bounds;
        }

    }
}
