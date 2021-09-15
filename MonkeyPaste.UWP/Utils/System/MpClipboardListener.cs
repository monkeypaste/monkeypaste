using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace MonkeyPaste.UWP {
    public class MpClipboardListener {
        #region Singleton
        private static readonly Lazy<MpClipboardListener> _Lazy = new Lazy<MpClipboardListener>(() => new MpClipboardListener());
        public static MpClipboardListener Instance { get { return _Lazy.Value; } }
        #endregion

        #region Private Varibles
        //private System.Timers.Timer _timer;
        private bool _isStopped = false;
        private Dictionary<string,object> _lastCbo;
        //private ManualResetEvent _resetEvent = new ManualResetEvent(true);
        private Thread workThread;
        #endregion

        #region Properties
        public bool IgnoreNextClipboardChange = false;
        #endregion

        #region Events
        public event EventHandler<object> OnClipboardChanged;
        #endregion

        private MpClipboardListener() {
            workThread = new Thread(new ThreadStart(CheckClipboard));
            workThread.SetApartmentState(ApartmentState.STA);
            workThread.IsBackground = true;

            //_timer = new System.Timers.Timer();
            //_timer.Interval = 100;
            //_timer.AutoReset = false;
            //_timer.Elapsed += _timer_Elapsed;
        }

        public void Start() {
            if(workThread.IsAlive) {
                _isStopped = false;
            } else {
                workThread.Start();
            }
        }

        public void Stop() {
            _isStopped = false;
        }

        private void CheckClipboard() {
            while(true) {
                while(_isStopped) {
                    Thread.Sleep(100);
                } 
                var cbo = ConvertDpv(Clipboard.GetContent());
                if (HasChanged(cbo)) {
                    _lastCbo = cbo;
                    if (IgnoreNextClipboardChange) {
                        IgnoreNextClipboardChange = false;
                       // _timer.Start();
                        return;
                    }

                    OnClipboardChanged?.Invoke(this, _lastCbo);
                } 
                Thread.Sleep(1000);
            }    
        }

        private Dictionary<string,object> ConvertDpv(DataPackageView dpv) {
            var formats = new string[] { StandardDataFormats.Text, StandardDataFormats.Html, StandardDataFormats.Rtf, StandardDataFormats.Bitmap, StandardDataFormats.StorageItems };
            var cbDict = new Dictionary<string, object>();
            if(dpv == null) {
                return cbDict;
            }
            foreach (var af in formats) {
                if (dpv.Contains(af)) {

                    // TODO add checks for files and Images and convert: files to string seperated by NewLine, images to base 64
                    var data = AsyncHelpers.RunSync<object>(() => dpv.GetDataAsync(af).AsTask());
                    cbDict.Add(af, data);
                }
                //var cbe = await cbo.GetDataAsync(af);
                //cbDict.Add(af, cbe);
            }
            return cbDict;
        }

        private bool HasChanged(Dictionary<string, object> nco) {
            if(_lastCbo == null && nco != null) {
                return true;
            }
            if(_lastCbo != null && nco == null) {
                return true;
            }
            if(_lastCbo.Count != nco.Count) {
                return true;
            }
            foreach(var nce in nco) {
                if(!_lastCbo.ContainsKey(nce.Key)) {
                    return true;
                }
                if(!_lastCbo[nce.Key].ToString().Equals(nce.Value)) {
                    return true;
                }
            }
            return false;
        }
    }

    public static class AsyncHelpers {
        /// <summary>
        /// Execute's an async Task<T> method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task<T> method to execute</param>
        public static void RunSync(Func<Task> task) {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try {
                    await task();
                }
                catch (Exception e) {
                    synch.InnerException = e;
                    throw;
                }
                finally {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task) {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
            synch.Post(async _ =>
            {
                try {
                    ret = await task();
                }
                catch (Exception e) {
                    synch.InnerException = e;
                    throw;
                }
                finally {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state) {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state) {
                lock (items) {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop() {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop() {
                while (!done) {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items) {
                        if (items.Count > 0) {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null) {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    } else {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy() {
                return this;
            }
        }
    }
}
