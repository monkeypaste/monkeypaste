﻿using Avalonia.Threading;
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
            
            if((int)dp < (int)DispatcherPriority.MinValue) {
                return DispatcherPriority.MinValue;
            }
            if((int)dp > (int)DispatcherPriority.Render) {
                dp++;
            }
            return (DispatcherPriority)((int)dp);
        }
    }


}