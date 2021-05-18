using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace MonkeyPaste {
    public class MpAsyncLazy<T> : Lazy<Task<T>> {
        readonly Lazy<Task<T>> instance;

        public MpAsyncLazy(Func<T> factory) {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        public MpAsyncLazy(Func<Task<T>> factory) {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        public TaskAwaiter<T> GetAwaiter() {
            return instance.Value.GetAwaiter();
        }
    }
}
