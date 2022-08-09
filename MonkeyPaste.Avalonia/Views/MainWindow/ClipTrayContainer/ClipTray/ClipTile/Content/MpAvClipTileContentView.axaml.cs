using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Input;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileContentView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileContentView() {
            InitializeComponent();
            var b = this.FindControl<Border>("ClipTileContainerBorder");
            //b.PointerPressed += MpAvClipTileContentView_PointerPressed;
            
            b.AddHandler(Control.PointerPressedEvent, MpAvClipTileContentView_PointerPressed, RoutingStrategies.Tunnel);
        }

        private void MpAvClipTileContentView_PointerPressed(object sender, PointerPressedEventArgs e) {
            if(!e.IsLeftPress()) {
                return;
            }

            if (e.ClickCount >= 2 &&
                BindingContext.IsContentReadOnly &&
                !BindingContext.IsSubSelectionEnabled) {
                BindingContext.IsSubSelectionEnabled = true;
                MpCursor.SetCursor(BindingContext, MpCursorType.IBeam);
                //UpdateAdorners();
                return;
            }
            if (!BindingContext.IsTitleReadOnly ||
                !BindingContext.IsContentReadOnly ||
                 BindingContext.Parent.IsAnyResizing ||
                 BindingContext.Parent.CanAnyResize) { // ||
                 //MpResizeBehavior.IsAnyResizing) {
                e.Handled = false;
                return;
            }
            if (BindingContext.IsSubSelectionEnabled) {
                // NOTE only check for drag when there is selected text AND
                // drag is from somewhere in the selection range.
                // If mouse down isn't in selection range reset selection to down position

                //if (Rtb.Selection.IsEmpty) {
                //    e.Handled = false;
                //    return;
                //}
                //if (!Rtb.Selection.IsPointInRange(e.GetPosition(Rtb))) {
                //    //var mptp = Rtb.GetPositionFromPoint(e.GetPosition(Rtb),true);
                //    //Rtb.Selection.Select(mptp, mptp);
                //    e.Handled = false;
                //    return;
                //}
            }

            //MpDragDropManager.StartDragCheck(BindingContext);

            e.Handled = true;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
