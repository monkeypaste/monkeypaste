using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace MonkeyPaste
{
    public class MpLastWindowWatcher {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private IntPtr _thisAppHandle;
        private Timer _timer;
        private IntPtr _previousHandle;

        public MpLastWindowWatcher() {
            //Process.GetCurrentProcess().Refresh();
            _thisAppHandle = GetForegroundWindow();// Process.GetCurrentProcess().MainWindowHandle;
            _previousHandle = IntPtr.Zero;

            _timer = new Timer(100);
            _timer.Elapsed += new ElapsedEventHandler(SetLastActive);
            _timer.Start();
        }
        public IntPtr LastHandle {
            get {
                return _previousHandle;
            }
        }
        private void SetLastActive(object sender,ElapsedEventArgs e) {
            IntPtr currentHandle = GetForegroundWindow();
            if(_thisAppHandle == IntPtr.Zero) {
                //Process.GetCurrentProcess().Refresh();
                _thisAppHandle = Process.GetCurrentProcess().MainWindowHandle;
            }
            if (currentHandle != _previousHandle && currentHandle != _thisAppHandle && _thisAppHandle != IntPtr.Zero) {
                _previousHandle = currentHandle;
            }
        }        
    }
}
