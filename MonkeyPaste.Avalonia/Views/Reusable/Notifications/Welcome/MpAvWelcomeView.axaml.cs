using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeView : MpAvUserControl<MpAvWelcomeNotificationViewModel> {

        public MpAvWelcomeView() {
            InitializeComponent();
        }

    }

}
