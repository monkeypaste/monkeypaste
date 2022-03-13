using MonkeyPaste;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpWpfMainThreadMarshal : MpIMainThreadMarshal {
        public void RunOnMainThread(Action action, MpDispatcherPriority priority = MpDispatcherPriority.Normal) {
            Application.Current.Dispatcher.Invoke(action, (DispatcherPriority)priority);
        }

        public TResult RunOnMainThread<TResult>(Func<TResult> action, MpDispatcherPriority priority = MpDispatcherPriority.Normal) {
            TResult result = Application.Current.Dispatcher.Invoke<TResult>(action, (DispatcherPriority)priority);
            return result;
        }

        public async Task RunOnMainThreadAsync(Action action, MpDispatcherPriority priority = MpDispatcherPriority.Normal) {
            await Application.Current.Dispatcher.InvokeAsync(action, (DispatcherPriority)priority);
        }

        public async Task<TResult> RunOnMainThreadAsync<TResult>(Func<TResult> action, MpDispatcherPriority priority = MpDispatcherPriority.Normal) {
            TResult result = await Application.Current.Dispatcher.InvokeAsync<TResult>(action, (DispatcherPriority)priority);
            return result;
        }
    }
}
