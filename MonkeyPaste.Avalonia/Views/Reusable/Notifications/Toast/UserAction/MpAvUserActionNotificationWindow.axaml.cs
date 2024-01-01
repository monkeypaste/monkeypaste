using Avalonia.Controls;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvUserActionNotificationWindow : MpAvNotificationWindow {

        public MpAvUserActionNotificationWindow() : this(null) { }
        public MpAvUserActionNotificationWindow(Window owner) : base(owner) {
            InitializeComponent();
        }
    }

}
