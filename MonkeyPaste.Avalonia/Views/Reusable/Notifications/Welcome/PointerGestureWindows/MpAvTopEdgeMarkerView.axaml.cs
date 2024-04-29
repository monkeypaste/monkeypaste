using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using PropertyChanged;
namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvTopEdgeMarkerView : MpAvUserControl<MpAvPointerGestureWindowViewModel> {
        public MpAvTopEdgeMarkerView() : base() {
            InitializeComponent();
            this.EffectiveViewportChanged += MpAvTopEdgeMarkerView_EffectiveViewportChanged;

            var marker = this.FindControl<Control>("ScrollMarkerContainer");
            marker.EffectiveViewportChanged += MpAvTopEdgeMarkerView_EffectiveViewportChanged;

            InitDnd();
        }

        private void InitDnd() {
            //DragDrop.SetAllowDrop(this, true);
            //this.AddHandler(DragDrop.DragOverEvent, MpAvTopEdgeMarkerView_DragOver);
            if (TopLevel.GetTopLevel(this) is MpAvWindow w) {
                w.AddHandler(DragDrop.DragOverEvent, MpAvTopEdgeMarkerView_DragOver);
            }
        }
        private void MpAvTopEdgeMarkerView_DragOver(object sender, DragEventArgs e) {
            MpConsole.WriteLine($"TopEdge drag over...");
            if (BindingContext.FakeWindowViewModel.FakeWindowActionType == MpFakeWindowActionType.Open) {
                return;
            }
            BindingContext.FakeWindowViewModel.ToggleFakeWindowCommand.Execute(null);
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
