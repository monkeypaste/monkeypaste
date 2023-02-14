using System;

namespace MonkeyPaste {
    public class MpTouchEventArgs<T> : EventArgs {
        public T EventData { get; private set; }

        public MpTouchEventArgs(T EventData) {
            this.EventData = EventData;
        }
    }
}
