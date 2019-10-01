using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpScreenController {
        public static Rectangle GetScreenBoundsWithMouse() {
            foreach(Screen screen in Screen.AllScreens) {
                Point mp = MpCursorPosition.GetCursorPosition();
                if(screen.WorkingArea.Contains(mp)) {
                    return screen.WorkingArea;
                }
            }
            return Screen.FromHandle(Process.GetCurrentProcess().Handle).WorkingArea;
        }
    }
}
