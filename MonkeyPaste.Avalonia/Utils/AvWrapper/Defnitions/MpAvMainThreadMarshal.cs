using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvMainThreadMarshal : MpIMainThreadMarshal {
        public void RunOnMainThread(Action action, MpDispatcherPriority priority = MpDispatcherPriority.Normal) {
            Dispatcher.UIThread.Post(action, ConvertPriority(priority));
        }

        public TResult RunOnMainThread<TResult>(Func<TResult> action, MpDispatcherPriority priority = MpDispatcherPriority.Normal) {
            TResult result = default;
            Dispatcher.UIThread.Post(
                () => {
                    result = action.Invoke();
                }, ConvertPriority(priority));

            return result;
        }

        public async Task RunOnMainThreadAsync(Action action, MpDispatcherPriority priority = MpDispatcherPriority.Normal) {
            await Dispatcher.UIThread.InvokeAsync(action, ConvertPriority(priority));
        }

        public async Task<TResult> RunOnMainThreadAsync<TResult>(Func<TResult> action, MpDispatcherPriority priority = MpDispatcherPriority.Normal) {
            return await Dispatcher.UIThread.InvokeAsync<TResult>(action, ConvertPriority(priority));
        }

        public DispatcherPriority ConvertPriority(MpDispatcherPriority dp) {
            if (dp == MpDispatcherPriority.Normal) {
                return DispatcherPriority.Normal;
            }
            if (dp == MpDispatcherPriority.Background) {
                return DispatcherPriority.Background;
            }
            throw new Exception("Add converter");
        }
    }


}
