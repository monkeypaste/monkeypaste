using Avalonia.Controls;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMessageNotificationWindow : MpAvNotificationWindow {
        public MpAvMessageNotificationWindow() : this(null) { }
        public MpAvMessageNotificationWindow(Window owner = default) : base(owner) {
            InitializeComponent();
        }

    }

}
