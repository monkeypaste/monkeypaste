using Avalonia.Controls;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvUserActionNotificationWindow : MpAvNotificationWindow {

        public MpAvUserActionNotificationWindow() {
            InitializeComponent();
        }
        //public MpAvUserActionNotificationWindow(MpAvWindow owner) : base(owner) {
        //    InitializeComponent();
        //}
    }

}
