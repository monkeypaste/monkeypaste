using Avalonia.Controls;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvNotificationContainerView : MpAvUserControl<MpAvNotificationViewModelBase> {
        public MpAvNotificationContainerView() : base() {
            InitializeComponent();
        }
    }

}
