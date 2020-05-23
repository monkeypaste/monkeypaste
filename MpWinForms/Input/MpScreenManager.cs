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
        private static readonly Lazy<MpScreenManager> lazy = new Lazy<MpScreenManager>(() => new MpScreenManager());
        public static MpScreenManager Instance { get { return lazy.Value; } }

        private Screen _curScreen = null;

        public MpScreenManager() {

        }
        public Rectangle GetScreenBoundsWithMouse() {
            foreach(Screen screen in Screen.AllScreens) {
                //get cursor pos
                WinApi.PointInter lpPoint;
                WinApi.GetCursorPos(out lpPoint);
                Point mp = (Point)lpPoint;
                if(screen.WorkingArea.Contains(mp)) {
                    _curScreen = screen;
                    return screen.Bounds;
                }
            }
            return Screen.FromHandle(Process.GetCurrentProcess().Handle).Bounds;
        }
    }
}
