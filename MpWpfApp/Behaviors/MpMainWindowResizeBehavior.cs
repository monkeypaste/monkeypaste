using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {

    public class MpMainWindowResizeBehavior : MpBehavior<MpMainWindow> {
        private Point _lastMousePosition;
        private FrameworkElement _mainWindowTitlePanel;

        protected override void OnLoad() {
            base.OnLoad();

            _mainWindowTitlePanel = (Application.Current.MainWindow as MpMainWindow).TitleMenu;
            _mainWindowTitlePanel.PreviewMouseLeftButtonDown += AssociatedObject_MouseDown;
            _mainWindowTitlePanel.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            _mainWindowTitlePanel.PreviewMouseMove += AssociatedObject_MouseMove;
            _mainWindowTitlePanel.MouseEnter += AssociatedObject_MouseEnter;
            _mainWindowTitlePanel.MouseLeave += AssociatedObject_MouseLeave;
        }

        #region Manual Resize Event Handlers

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e) {
            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e) {
            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.ResizeNS;
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            _mainWindowTitlePanel.ReleaseMouseCapture();
            MpMainWindowViewModel.Instance.IsResizing = false;

            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;

            var rtbvl = AssociatedObject.GetVisualDescendents<MpRtbView>();
            rtbvl.ForEach(x => x.Rtb.FitDocToRtb());
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            _lastMousePosition = e.GetPosition(Application.Current.MainWindow);
            _mainWindowTitlePanel.CaptureMouse();
            MpMainWindowViewModel.Instance.IsResizing = true;
            e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (!_mainWindowTitlePanel.IsMouseCaptured) {
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

            mwvm.MainWindowTop -= deltaHeight;
            mwvm.MainWindowHeight += deltaHeight;
            MpClipTrayViewModel.Instance.Items.ForEach(x => x.TileBorderHeight += deltaHeight);
            //mwvm.ClipTrayHeight += deltaHeight;
            //ctvm.TileBorderHeight += deltaHeight;
            //ctvm.TileContentHeight += deltaHeight;

            double boundAdjust = 0;
            //if (mwvm.MainWindowTop < MpMeasurements.Instance.ClipTileExpandedMaxHeightPadding) {
            //    boundAdjust = mwvm.MainWindowContainerTop - MpMeasurements.Instance.ClipTileExpandedMaxHeightPadding;
            //} else if (mwvm.MainWindowTop > mwvm.MainWindowHeight - MpMeasurements.Instance.MainWindowMinHeight) {
            //    boundAdjust = mwvm.MainWindowContainerTop - (mwvm.MainWindowHeight - MpMeasurements.Instance.MainWindowMinHeight);
            //}

            mwvm.MainWindowHeight += boundAdjust;
            //mwvm.ClipTrayHeight += boundAdjust;
            //ctvm.TileBorderHeight += boundAdjust;
            //ctvm.TileContentHeight += boundAdjust;

            MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayScreenWidth));
            mwvm.OnPropertyChanged(nameof(mwvm.ClipTrayAndCriteriaListHeight));
            mwvm.IsResizing = false;


            Application.Current.MainWindow.UpdateLayout();
            Application.Current.MainWindow.GetVisualDescendents<MpUserControl>().ForEach(x => x.UpdateLayout());

            //MpConsole.WriteLine($"MainWindowContainerTop: {mwvm.MainWindowContainerTop}");
            MpConsole.WriteLine($"MainWindowTop: {mwvm.MainWindowTop}");
            MpConsole.WriteLine($"MainWindowHeight: {mwvm.MainWindowHeight}");
        }
    }
}
