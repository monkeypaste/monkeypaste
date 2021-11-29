using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpExternalDropBehavior : MpDropBehaviorBase<MpExternalDropView> {
        public override int DropPriority => int.MaxValue;

        protected override FrameworkElement AdornedElement => AssociatedObject;

        public override Orientation AdornerOrientation => Orientation.Horizontal;

        public override List<Rect> GetDropTargetRects() {
            return new List<Rect> { AssociatedObject.Bounds() };
        }

        public override async Task Drop(bool isCopy, object dragData) {
            await Task.Delay(10);
        }

        public override void AutoScrollByMouse(MouseEventArgs e) {
            return;
        }

        public override void StartDrop() {
            
        }
    }

}
