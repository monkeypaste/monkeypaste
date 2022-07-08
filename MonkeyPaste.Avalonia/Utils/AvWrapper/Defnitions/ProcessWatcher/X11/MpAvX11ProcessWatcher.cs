using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;
using Avalonia.X11.NativeDialogs;
using System.Diagnostics;
using System.Timers;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public class MpAvX11ProcessWatcher : MpIProcessWatcher {
        private Timer _timer;

        public IntPtr ThisAppHandle { get; set; }
        public IntPtr LastHandle { get; }
        public string LastMainWindowTitle { get; private set; }
        public string LastProcessPath { get; }
        public ConcurrentDictionary<string, ObservableCollection<IntPtr>> RunningProcessLookup { get; }

        public MpAvX11ProcessWatcher() {

            
            _timer = new Timer() {
                Interval = 300,
                AutoReset = true,
                Enabled = true
            };
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e) {
            var processes = Process.GetProcesses()
                            .Where(x => x.MainWindowHandle != IntPtr.Zero &&
                                      !string.IsNullOrEmpty(x.MainWindowTitle) &&
                                      x.Handle == x.MainWindowHandle);
            var curProcess = Process.GetCurrentProcess();

            MpConsole.WriteLine("Cur Process: " + curProcess.MainWindowTitle);
        }

        public IntPtr GetParentHandleAtPoint(MpPoint poIntPtr) {
            throw new NotImplementedException();
        }

        public IntPtr GetLastActiveInstance(string path) {
            throw new NotImplementedException();
        }

        public bool IsHandleRunningProcess(IntPtr handle) {
            throw new NotImplementedException();
        }

        public string GetProcessPath(IntPtr handle) {
            throw new NotImplementedException();
        }

        public string GetProcessApplicationName(IntPtr handle) {
            throw new NotImplementedException();
        }

        public string GetProcessMainWindowTitle(IntPtr handle) {
            throw new NotImplementedException();
        }

        public event EventHandler<MpProcessActivatedEventArgs> OnAppActivated;
    }
}
