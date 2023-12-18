using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpTaskUtilities {
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public static async void FireAndForgetSafeAsync(this Task task, MpIErrorHandler handler = null, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
    {
            try {
                await task;
            }
            catch (Exception ex) {
                handler = handler == null ? MpDefaultErrorHandler.Instance : handler;
                handler?.HandleError(ex, callerName, callerFilePath, lineNum);
            }
        }

        public static async Task TimeoutAfter(this Task task, TimeSpan timeout, Func<bool> timeout_callback = null) {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource()) {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task) {
                    timeoutCancellationTokenSource.Cancel();
                    return;  // Very important in order to propagate exceptions
                } else {
                    timeout_callback?.Invoke();
                    if (timeout_callback == null) {
                        throw new TimeoutException("The operation has timed out.");
                    }
                }
            }
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout, Func<TResult> timeout_callback = null) {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource()) {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task) {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                } else {
                    if (timeout_callback == null) {
                        throw new TimeoutException("The operation has timed out.");
                    } else {
                        return timeout_callback.Invoke();
                    }
                }
            }
        }
    }
}
