using MonkeyPaste.Common;
//using Avalonia.Win32;

namespace MonkeyPaste.Avalonia {
    public class MpAvDebugBreakHelper : MpIDebugBreakHelper {
        public void HandlePreBreak() {
            MpAvShortcutCollectionViewModel.Instance.ToggleGlobalHooksCommand.Execute(true);
        }

        public void HandlePostBreak() {
            MpAvShortcutCollectionViewModel.Instance.ToggleGlobalHooksCommand.Execute(false);
        }
    }
}
