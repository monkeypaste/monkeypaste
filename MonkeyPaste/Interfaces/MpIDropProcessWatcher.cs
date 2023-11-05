using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpIDropProcessWatcher {
        MpPortableProcessInfo DropProcess { get; }
        void StartWatcher();
        void StopWatcher();

        void Reset();
    }
}
