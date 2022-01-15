using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {

    public class MpMainWindowResizeBehavior : MpBehavior<MpTitleBarView> {
        #region Private Variables

        private Point _lastMousePosition;

        private double _lastMainWindowHeight = 0;

        #endregion

        #region Properties

        #endregion

        protected override void OnLoad() {
            base.OnLoad();
            
            AssociatedObject.PreviewMouseDoubleClick += MpMainWindowResizeBehavior_MouseDoubleClick;
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
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.ResizeNS;
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            AssociatedObject.ReleaseMouseCapture();
            MpMainWindowViewModel.Instance.IsResizing = false;

            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;

            MpMessenger.Instance.Send<MpMessageType>(MpMessageType.ResizeCompleted);

            double mwDeltaHeight = MpMainWindowViewModel.Instance.MainWindowHeight - _lastMainWindowHeight;
            if(mwDeltaHeight < 0) {
                //when main window is sized smaller more items become visible but
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            _lastMousePosition = e.GetPosition(Application.Current.MainWindow);
            AssociatedObject.CaptureMouse();
            MpMainWindowViewModel.Instance.IsResizing = true;
            _lastMainWindowHeight = MpMainWindowViewModel.Instance.MainWindowHeight;
            e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (!AssociatedObject.IsMouseCaptured) {
                e.Handled = false;
                return;
            }

            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.ResizeNS;

            var mp = e.GetPosition(Application.Current.MainWindow);
            double deltaY = mp.Y - _lastMousePosition.Y;
            _lastMousePosition = mp;

            Resize(-deltaY);
            e.Handled = true;
        }

        #endregion

        public async Task ResizeForInitialLoad() {
            while(MpClipTrayViewModel.Instance.HeadItem == null) {
                await Task.Delay(10);
            }
            double deltaHeight = MpPreferences.Instance.MainWindowInitialHeight - MpMainWindowViewModel.Instance.MainWindowHeight;
            Resize(deltaHeight);
        }

        public void Resize(double deltaHeight) {
            if(Math.Abs(deltaHeight) == 0) {
                return;
            }

            var ctrvm = MpClipTrayViewModel.Instance;
            var msrmvm = MpMeasurements.Instance;

            double oldHeadTrayX = ctrvm.HeadItem.TrayX;
            double oldScrollOffset = ctrvm.ScrollOffset;

            //double oldScrollOffsetDiffWithHead = ctrvm.ScrollOffset - oldHeadTrayX;
            
            var mwvm = MpMainWindowViewModel.Instance;
            mwvm.IsResizing = true;

            if(mwvm.MainWindowHeight + deltaHeight < msrmvm.MainWindowMinHeight) {
                deltaHeight = mwvm.MainWindowHeight - msrmvm.MainWindowMinHeight;
            } else if (mwvm.MainWindowHeight + deltaHeight > msrmvm.MainWindowMaxHeight) {
                deltaHeight = msrmvm.MainWindowMaxHeight - mwvm.MainWindowHeight;
            }
            mwvm.MainWindowTop -= deltaHeight;
            mwvm.MainWindowHeight += deltaHeight;

            msrmvm.ClipTileMinSize += deltaHeight;
            msrmvm.OnPropertyChanged(nameof(msrmvm.ClipTileTitleHeight));

            MpClipTileViewModel.DefaultBorderWidth += deltaHeight;
            MpClipTileViewModel.DefaultBorderHeight += deltaHeight;
            ctrvm.Items.ForEach(x => x.TileBorderHeight = msrmvm.ClipTileMinSize);
            ctrvm.Items.ForEach(x => x.TileBorderWidth += deltaHeight);
            //ctrvm.PersistentUniqueWidthTileLookup.Values.ForEach(x => x += deltaHeight);

            ctrvm.OnPropertyChanged(nameof(ctrvm.ClipTrayTotalTileWidth));
            ctrvm.OnPropertyChanged(nameof(ctrvm.ClipTrayScreenWidth));
            ctrvm.OnPropertyChanged(nameof(ctrvm.ClipTrayTotalWidth));
            ctrvm.OnPropertyChanged(nameof(ctrvm.MaximumScrollOfset));
            ctrvm.Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));

            //adjust the scroll offset using the head items origin and size change as a ratio to update offset
            //double newHeadTrayX = ctrvm.HeadItem.TrayX;
            //double headOffsetRatio = newHeadTrayX / oldHeadTrayX;
            //headOffsetRatio = double.IsNaN(headOffsetRatio) ? 0 : headOffsetRatio;
            //double newScrollOfsetDiffWithHead = headOffsetRatio * oldScrollOffsetDiffWithHead;
            //double newScrollOfset = MpPagingListBoxBehavior.Instance.FindTileOffsetX(ctrvm.HeadQueryIdx) + newScrollOfsetDiffWithHead;

            //ctrvm.ScrollOffset = ctrvm.LastScrollOfset = newScrollOfset;

            MpClipTrayViewModel.Instance.AdjustScrollOffsetToResize(oldHeadTrayX, oldScrollOffset);

            mwvm.IsResizing = false;

            MpMessenger.Instance.Send<MpMessageType>(MpMessageType.Resizing);

            MpPreferences.Instance.MainWindowInitialHeight = mwvm.MainWindowHeight;
            //Application.Current.MainWindow.UpdateLayout();
            //Application.Current.MainWindow.GetVisualDescendents<MpUserControl>().ForEach(x => x.UpdateLayout());
        }
    }
}
