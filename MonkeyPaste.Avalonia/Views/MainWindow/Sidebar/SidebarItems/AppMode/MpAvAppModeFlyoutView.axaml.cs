using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvAppModeFlyoutView : UserControl {
        public MpAvAppModeFlyoutView() {
            AvaloniaXamlLoader.Load(this);
            //this.AttachedToVisualTree += MpAvAppModeFlyoutView_AttachedToVisualTree;

            //var abrb = this.FindControl<Control>("AppendBaseRadioButton");
            //var mmbb = this.FindControl<Control>("MouseBaseRadioButton");

            //abrb.PointerReleased += Abrb_PointerReleased;
            //mmbb.PointerReleased += Abrb_PointerReleased;
        }

        private void Abrb_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            if (sender is Control c) {
                //if (c.Name == "AppendBaseRadioButton") {
                //    this.FindControl<ToggleButton>("MouseBaseRadioButton").IsChecked = false;
                //} else {
                //    this.FindControl<ToggleButton>("AppendBaseRadioButton").IsChecked = false;
                //}
            }
        }

        private void MpAvAppModeFlyoutView_AttachedToVisualTree(object sender, global::Avalonia.VisualTreeAttachmentEventArgs e) {
            if (this.GetVisualRoot() is PopupRoot pur) {
#if DEBUG
                // pur.AttachDevTools();
#endif

                pur.TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
                pur.Background = Brushes.Transparent;

            }
        }


    }
}
