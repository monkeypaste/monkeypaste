using MonkeyPaste;

namespace MpWpfApp {
    public class MpWpfContextMenuCloser : MpIContextMenuCloser {
        public void CloseMenu() {
            MpContextMenuView.Instance.CloseMenu();
        }
    }
}
