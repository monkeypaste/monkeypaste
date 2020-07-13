using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace MpWpfApp {
    public class MpLastWindowWatcher {
        public string ThisAppPath { get; set; }
        public IntPtr ThisAppHandle;
        private Timer _timer;
        private IntPtr _previousHandle;

        public MpLastWindowWatcher(IntPtr appHandle) {
            //Process.GetCurrentProcess().Invalidate();
            ThisAppHandle = appHandle;
            _previousHandle = IntPtr.Zero;
            ThisAppPath = MpHelperSingleton.Instance.GetProcessPath(ThisAppHandle);
            Console.WriteLine("This app's exe: " + ThisAppPath);
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
            IntPtr currentHandle = WinApi.GetForegroundWindow();
            if(ThisAppHandle == IntPtr.Zero) {
                ThisAppHandle = Process.GetCurrentProcess().MainWindowHandle;
            }
            if (currentHandle != _previousHandle && currentHandle != ThisAppHandle && ThisAppHandle != IntPtr.Zero && currentHandle != IntPtr.Zero) {
                _previousHandle = currentHandle;
            }
        }        
    }
}
