using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvTopEdgeMarkerView : MpAvUserControl<MpAvPointerGestureWindowViewModel> {
        public MpAvTopEdgeMarkerView() : base() {
            AvaloniaXamlLoader.Load(this);
            this.EffectiveViewportChanged += MpAvTopEdgeMarkerView_EffectiveViewportChanged;

            var marker = this.FindControl<Control>("ScrollMarkerContainer");
            marker.EffectiveViewportChanged += MpAvTopEdgeMarkerView_EffectiveViewportChanged;
        }

        private void MpAvTopEdgeMarkerView_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            // NOTE also called 
            PositionControls();
        }

        private void PositionControls() {
            var marker = this.FindControl<Control>("ScrollMarkerContainer");
            // center marker horizontally once actual width is known
            Canvas.SetLeft(marker, (this.Bounds.Width / 2) - (marker.Bounds.Width / 2));

            if (TopLevel.GetTopLevel(this) is not Window w) {
                return;
            }
            w.Position = new PixelPoint();
        }
    }
}
