using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpIDragProcessWatcher {
        MpPortableProcessInfo DragProcess { get; }
        void StartWatcher();
        void StopWatcher();

        void Reset();
    }
}
