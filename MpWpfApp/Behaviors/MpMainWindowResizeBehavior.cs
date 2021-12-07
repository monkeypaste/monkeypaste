using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {

    public class MpMainWindowResizeBehavior : MpSingletonBehavior<MpTitleBarView, MpMainWindowResizeBehavior> {
        #region Private Variables

        private Point _lastMousePosition;

        #endregion

        #region Properties

        #endregion

        protected override void OnLoad() {
            base.OnLoad();

            AssociatedObject.FrameGripButton.PreviewMouseDoubleClick += MpMainWindowResizeBehavior_MouseDoubleClick;
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        private void MpMainWindowResizeBehavior_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            double deltaHeight = MpMainWindowViewModel.Instance.MainWindowHeight - MpMeasurements.Instance.MainWindowDefaultHeight;
            Resize(-deltaHeight);
        }

        #region Manual Resize Event Handlers

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e) {
            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e) {
            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.ResizeNS;
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            AssociatedObject.ReleaseMouseCapture();
            MpMainWindowViewModel.Instance.IsResizing = false;

            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;

            MpMessenger.Instance.Send<MpMessageType>(MpMessageType.ResizeCompleted);
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            _lastMousePosition = e.GetPosition(Application.Current.MainWindow);
            AssociatedObject.CaptureMouse();
            MpMainWindowViewModel.Instance.IsResizing = true;
            e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (!AssociatedObject.IsMouseCaptured) {
                e.Handled = false;
                return;
            }

            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.ResizeNS;

            var mp = e.GetPosition(Application.Current.MainWindow);
            double deltaY = mp.Y - _lastMousePosition.Y;
            _lastMousePosition = mp;

            Resize(-deltaY);
            e.Handled = true;
        }

        #endregion

        public void Resize(double deltaHeight) {
            if(Math.Abs(deltaHeight) == 0) {
                return;
            }
            var mwvm = MpMainWindowViewModel.Instance;
            mwvm.IsResizing = true;

            if(mwvm.MainWindowHeight + deltaHeight < MpMeasurements.Instance.MainWindowMinHeight) {
                deltaHeight = mwvm.MainWindowHeight - MpMeasurements.Instance.MainWindowMinHeight;
            } else if (mwvm.MainWindowHeight + deltaHeight > MpMeasurements.Instance.MainWindowMaxHeight) {
                deltaHeight = MpMeasurements.Instance.MainWindowMaxHeight - mwvm.MainWindowHeight;
            }
            mwvm.MainWindowTop -= deltaHeight;
            mwvm.MainWindowHeight += deltaHeight;

            MpMeasurements.Instance.ClipTileMinSize += deltaHeight;
            MpClipTrayViewModel.Instance.Items.ForEach(x => x.TileBorderHeight = MpMeasurements.Instance.ClipTileMinSize);
            
            double boundAdjust = 0;

            mwvm.MainWindowHeight += boundAdjust;

            MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayScreenWidth));
            mwvm.OnPropertyChanged(nameof(mwvm.ClipTrayAndCriteriaListHeight));
            mwvm.IsResizing = false;

            MpMessenger.Instance.Send<MpMessageType>(MpMessageType.Resizing);

            Application.Current.MainWindow.UpdateLayout();
            Application.Current.MainWindow.GetVisualDescendents<MpUserControl>().ForEach(x => x.UpdateLayout());
        }
    }
}
