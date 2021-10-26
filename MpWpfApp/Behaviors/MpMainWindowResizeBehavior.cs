using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpMainWindowResizeBehavior : Behavior<FrameworkElement> {
        #region Private Variables

        private Point _lastMousePosition;

        #endregion

        #region Statics
        #endregion

        protected override void OnAttached() {
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseDown;
            AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e) {
            //AssociatedObject.BindingContext.IsHovering = false;

            Application.Current.MainWindow.ForceCursor = true;
            Application.Current.MainWindow.Cursor = Cursors.Arrow;
        }

        private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e) {
            //AssociatedObject.BindingContext.IsHovering = true;

            Application.Current.MainWindow.ForceCursor = true;
            Application.Current.MainWindow.Cursor = Cursors.SizeNS;
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            AssociatedObject.ReleaseMouseCapture();
            MpMainWindowViewModel.Instance.IsResizing = false;
            Application.Current.MainWindow.ForceCursor = true;
            Application.Current.MainWindow.Cursor = Cursors.Arrow;
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {            
            if(!MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                return;
            }
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


            Application.Current.MainWindow.ForceCursor = true;
            Application.Current.MainWindow.Cursor = Cursors.SizeNS;
            var mp = e.GetPosition(Application.Current.MainWindow);
            double deltaY = mp.Y - _lastMousePosition.Y;
            _lastMousePosition = mp;

            Resize(-deltaY);
            e.Handled = true;
        }

        private void Resize(double deltaHeight) {
            var mwvm = MpMainWindowViewModel.Instance;

            mwvm.MainWindowTop -= deltaHeight;
            mwvm.ClipTrayHeight += deltaHeight;
            MpClipTrayViewModel.Instance.Items.ForEach(x => x.TileBorderHeight += deltaHeight);
            MpClipTrayViewModel.Instance.Items.ForEach(x => x.TileContentHeight += deltaHeight);

            double boundAdjust = 0;
            if (mwvm.MainWindowTop < MpMeasurements.Instance.ClipTileExpandedMaxHeightPadding) {
                boundAdjust = mwvm.MainWindowTop - MpMeasurements.Instance.ClipTileExpandedMaxHeightPadding;
            }
            //else if (mwvm.MainWindowTop > MpMeasurements.Instance.MainWindowDefaultHeight) {
            //    boundAdjust = mwvm.MainWindowTop - _initialExpandedMainWindowTop;
            //}

            mwvm.MainWindowTop -= boundAdjust;
            mwvm.ClipTrayHeight += boundAdjust;
            MpClipTrayViewModel.Instance.Items.ForEach(x => x.TileBorderHeight += boundAdjust);
            MpClipTrayViewModel.Instance.Items.ForEach(x => x.TileContentHeight += boundAdjust);

            Application.Current.MainWindow.GetVisualDescendents<MpUserControl>().ForEach(x => x.UpdateLayout());
            //var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            //clv.UpdateLayout();

            //var civl = AssociatedObject.GetVisualDescendents<MpContentContainerView>().ToList();
            //foreach (var civ in civl) {
            //    civ.UpdateLayout();
            //}
            //AssociatedObject.UpdateLayout();
        }
    }
}
