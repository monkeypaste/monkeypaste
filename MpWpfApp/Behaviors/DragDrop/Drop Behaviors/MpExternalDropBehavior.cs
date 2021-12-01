using MonkeyPaste;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpExternalDropBehavior : MpDropBehaviorBase<MpExternalDropView> {
        public override int DropPriority => int.MaxValue;

        public override FrameworkElement AdornedElement => AssociatedObject;
        public override Orientation AdornerOrientation => Orientation.Horizontal;

        public override bool IsEnabled { get; set; } = false;

        public override UIElement RelativeToElement => Application.Current.MainWindow;

        public override MpCursorType MoveCursor => MpCursorType.ContentMove;
        public override MpCursorType CopyCursor => MpCursorType.ContentCopy;

        protected override void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ExpandComplete:
                    IsEnabled = false;
                    UpdateAdorner();
                    break;
                case MpMessageType.UnexpandComplete:
                    IsEnabled = false;
                    UpdateAdorner();
                    break;
            }
        }

        public override List<Rect> GetDropTargetRects() {
            return new List<Rect> { new Rect() };// AssociatedObject.Bounds() };
        }

        public override async Task StartDrop() {
            // TODO create IDataObject and call DoDragDrop here, ignoring templates
            await Task.Delay(1);
        }

        public override async Task Drop(bool isCopy, object dragData) {
            // TODO when templates present trigger w/ HideWindow command
            await Task.Delay(10);
        }

        public override void AutoScrollByMouse(MouseEventArgs e) {
            return;
        }
    }

}
