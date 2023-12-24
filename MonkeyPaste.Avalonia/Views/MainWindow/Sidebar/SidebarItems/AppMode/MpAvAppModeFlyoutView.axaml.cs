using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvAppModeFlyoutView : MpAvUserControl<object> {
        public MpAvAppModeFlyoutView() {
            InitializeComponent();
        }


        private void MpAvAppModeFlyoutView_AttachedToVisualTree(object sender, global::Avalonia.VisualTreeAttachmentEventArgs e) {
            if (this.GetVisualRoot() is PopupRoot pur) {
                pur.TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
                pur.Background = Brushes.Transparent;

            }
        }


    }
}
