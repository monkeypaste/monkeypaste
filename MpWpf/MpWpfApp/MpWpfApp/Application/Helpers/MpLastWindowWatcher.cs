using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace MpWpfApp
{
    public class MpLastWindowWatcher {
        public static string ThisAppPath { get; set; }
        private IntPtr _thisAppHandle;
        private Timer _timer;
        private IntPtr _previousHandle;

        public MpLastWindowWatcher(IntPtr appHandle) {
            //Process.GetCurrentProcess().Invalidate();
            _thisAppHandle = appHandle;
            _previousHandle = IntPtr.Zero;
            ThisAppPath = MpHelperSingleton.Instance.GetProcessPath(_thisAppHandle);
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
            if(_thisAppHandle == IntPtr.Zero) {
                _thisAppHandle = Process.GetCurrentProcess().MainWindowHandle;
            }
            if (currentHandle != _previousHandle && currentHandle != _thisAppHandle && _thisAppHandle != IntPtr.Zero && currentHandle != IntPtr.Zero) {
                _previousHandle = currentHandle;
            }
        }        
    }
}
