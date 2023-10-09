using MonkeyPaste.Common;
//using Avalonia.Win32;

namespace MonkeyPaste.Avalonia {
    public class MpAvDebugBreakHelper : MpIDebugBreakHelper {
        bool is_paused = false;
        public void HandlePreBreak() {
            MpAvShortcutCollectionViewModel.Instance.PauseGlobalHooks();
        }

        public void HandlePostBreak() {
            MpAvShortcutCollectionViewModel.Instance.ResumeGlobalHooks();
        }
        public void ToggleBreak() {
            if (is_paused) {
                HandlePostBreak();
            } else {
                HandlePreBreak();
            }
            is_paused = !is_paused;
        }
    }
}
