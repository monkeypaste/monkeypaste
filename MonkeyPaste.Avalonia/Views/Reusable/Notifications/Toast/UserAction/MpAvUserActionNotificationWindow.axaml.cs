using Avalonia.Controls;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvUserActionNotificationWindow : MpAvWindow<MpAvUserActionNotificationViewModel> {

        public MpAvUserActionNotificationWindow() : this(null) { }
        public MpAvUserActionNotificationWindow(Window owner) : base(owner) {
            InitializeComponent();
        }
    }

}
