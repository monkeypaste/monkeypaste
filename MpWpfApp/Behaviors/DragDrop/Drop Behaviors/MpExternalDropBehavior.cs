using MonkeyPaste;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpExternalDropBehavior : MpDropBehaviorBase<MpExternalDropView> {
        private IDataObject _ido;

        public override int DropPriority => int.MaxValue;

        public override FrameworkElement AdornedElement => AssociatedObject;
        public override Orientation AdornerOrientation => Orientation.Horizontal;

        public override bool IsEnabled { get; set; } = true;

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
                    IsEnabled = true;
                    UpdateAdorner();
                    break;
            }
        }

        public override List<Rect> GetDropTargetRects() {
            Rect extRect = new Rect(
                0, 
                -(MpMeasurements.Instance.ScreenHeight - MpMainWindowViewModel.Instance.MainWindowTop), 
                MpMeasurements.Instance.ScreenWidth, 
                MpMeasurements.Instance.ScreenHeight - MpMainWindowViewModel.Instance.MainWindowTop);

            return new List<Rect> { extRect };
        }

        public override async Task StartDrop() {
            // TODO create IDataObject and call DoDragDrop here, ignoring templates
            //await Task.Delay(1);

            Application.Current.MainWindow.IsEnabled = false;
            _ido = await MpClipTrayViewModel.Instance.GetDataObjectFromSelectedClips(true, true);
            DragDrop.DoDragDrop(AssociatedObject, _ido, DragDropEffects.Copy);
        }

        public override async Task Drop(bool isCopy, object dragData) {
            // TODO when templates present trigger w/ HideWindow command
            await Task.Delay(10);
            Application.Current.MainWindow.IsEnabled = true;
        }

        public override void AutoScrollByMouse() {
            return;
        }
    }

}
