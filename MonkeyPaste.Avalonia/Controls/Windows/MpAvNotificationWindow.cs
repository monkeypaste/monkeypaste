using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvNotificationWindow : MpAvWindow {
        public MpAvNotificationWindow(MpAvWindow owner = default) : base(owner) { }
    }
}
