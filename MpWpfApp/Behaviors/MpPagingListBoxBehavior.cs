using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpPagingListBoxBehavior : Behavior<ScrollViewer> {
        #region Private Variables
        private int _leftItemTotalIdx, _rightItemTotalIdx;
        private double _currentOffset = 0;

        private double _velocity = 0;
        private double _scrollTarget = 0;

        private DispatcherTimer _timer;

        #endregion

        #region Properties

        public double Friction { get; set; } = 0;
        public double WheelDampening { get; set; } = 0;

        public int RemainingItemsThreshold {
            get { return (int)GetValue(RemainingItemsThresholdProperty); }
            set { SetValue(RemainingItemsThresholdProperty, value); }
        }
        public static readonly DependencyProperty RemainingItemsThresholdProperty =
            DependencyProperty.Register(
                nameof(RemainingItemsThreshold),
                typeof(int),
                typeof(MpPagingListBoxBehavior),
                new PropertyMetadata(default(int)));

        public ICommand LoadMoreCommand {
            get { return (ICommand)GetValue(LoadMoreCommandProperty); }
            set { SetValue(LoadMoreCommandProperty, value); }
        }
        public static readonly DependencyProperty LoadMoreCommandProperty =
            DependencyProperty.Register(
                nameof(LoadMoreCommand), 
                typeof(ICommand), 
                typeof(MpPagingListBoxBehavior), 
                new PropertyMetadata(default(ICommand)));


        #endregion

        protected override void OnAttached() {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            AssociatedObject.PreviewMouseWheel += Sv_PreviewMouseWheel;
            AssociatedObject.GetScrollBar(Orientation.Horizontal).PreviewMouseDown += Sv_PreviewMouseDown;

            MpMessenger.Instance.Register<MpMessageType>(MpClipTrayViewModel.Instance, ReceivedClipTrayViewModelMessage);

            _timer = new DispatcherTimer(DispatcherPriority.Normal);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            _timer.Tick += HandleWorldTimerTick;
            _timer.Start();
        }


        private void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.JumpToIdxCompleted:
                case MpMessageType.RequeryCompleted:

                    AssociatedObject.UpdateLayout();

                    var hsb = AssociatedObject.GetScrollBar(Orientation.Horizontal);
                    hsb.Maximum = MpClipTrayViewModel.Instance.ClipTrayTotalWidth;                    
                    hsb.Minimum = 0;

                    hsb.UpdateLayout();

                    if (msg == MpMessageType.RequeryCompleted) {
                        MpConsole.WriteLine("Tray Width: " + AssociatedObject.ActualWidth);
                        hsb.Value = 0;
                        _leftItemTotalIdx = 0;
                        if(MpClipTrayViewModel.Instance.TotalItemsInQuery > MpMeasurements.Instance.TotalVisibleClipTiles) {
                            _rightItemTotalIdx = MpMeasurements.Instance.TotalVisibleClipTiles - 1;
                        } else {
                            _rightItemTotalIdx = MpClipTrayViewModel.Instance.TotalItemsInQuery;
                        }
                        //AssociatedObject.ScrollToHorizontalOffset(0);
                        //AssociatedObject.ScrollToLeftEnd();
                        //AssociatedObject.ScrollToHome();
                    } 
                    AssociatedObject.ScrollToHorizontalOffset(0);
                    AssociatedObject.InvalidateScrollInfo();
                    AssociatedObject.UpdateLayout();

                    _scrollTarget = 0;
                    break;
                case MpMessageType.Expand:
                    //TrayItemsPanel.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case MpMessageType.Unexpand:
                    //TrayItemsPanel.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
            }
        }

        private void HandleWorldTimerTick(object sender, EventArgs e) {
            if (Math.Abs(_velocity) > 0.1) { 
                // TODO Add Thumb check if at min/max based on velocity sign here
                //if(_velocity > 0 && )
                AssociatedObject.ScrollToHorizontalOffset(_scrollTarget);
                _scrollTarget += _velocity;
                _velocity *= Friction;
            }
        }

        private void Sv_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var hsb = sender as ScrollBar;
            var htrack = hsb.Track;
            var hthumb = htrack.Thumb;

            var htrack_mp = e.GetPosition(htrack);
            if (htrack_mp.Y < 0) {
                return;
            }
            var hthumb_rect = hthumb.Bounds();
            if (hthumb_rect.Contains(e.GetPosition(hthumb))) {
                return;
            }

            e.Handled = true;
            double norm_x = htrack_mp.X / htrack.ActualWidth;

            int targetTileIdx = (int)(norm_x * MpClipTrayViewModel.Instance.TotalItemsInQuery);            

            MpClipTrayViewModel.Instance.JumpToPageIdxCommand.Execute(targetTileIdx);

            double targetThumbValue = MpMeasurements.Instance.ClipTileBorderMinSize * targetTileIdx;
            if(MpClipTrayViewModel.Instance.TotalItemsInQuery - targetTileIdx <= MpMeasurements.Instance.TotalVisibleClipTiles) {
                //when target position is beyond total track width - half thumb width need to manually set thumb to max
                targetThumbValue = hsb.Maximum;
            }
            hsb.Value = targetThumbValue;
        }

        private void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (MpClipTrayViewModel.Instance.IsAnyTileFlipped ||
                MpClipTrayViewModel.Instance.IsAnyTileExpanded ||
                MpMainWindowViewModel.Instance.IsMainWindowOpening) {
                return;
            }
            _velocity -= e.Delta * WheelDampening;

            ApplyOffsetChange(e.Delta);
            e.Handled = true;
        }

        public void ApplyOffsetChange(double horizontalChange) {
            if (!LoadMoreCommand.CanExecute(0)) {
                return;
            }
            Rect svr = AssociatedObject.Bounds();
            ListBox lb = AssociatedObject.GetVisualDescendent<ListBox>();

            if (horizontalChange < 0) {
                //scrolling down towards end of list

                //get item under point in middle of right edge of listbox
                int r_target_idx = lb.GetItemIndexAtPoint(new Point(svr.Right, svr.Height / 2));
                if (r_target_idx < 0) {
                    return;
                }
                if (r_target_idx >= lb.Items.Count) {
                    r_target_idx = lb.Items.Count - 1;
                }
                //get item over right edge's rect
                var rlbir = lb.GetListBoxItemRect(r_target_idx);
                if (rlbir.Left < svr.Right) {
                    //when last visible item's right edge is past the listboxes edge
                    int itemsRemaining = lb.Items.Count - r_target_idx - 1;

                    if (itemsRemaining <= RemainingItemsThreshold) {
                        LoadMoreCommand.Execute(1);

                        AssociatedObject
                            .GetScrollBar(Orientation.Horizontal)
                                .Value += MpMeasurements.Instance.ClipTileBorderMinSize * RemainingItemsThreshold;
                    }
                }
            } else if (horizontalChange > 0) {
                //scrolling up towards beginning of list

                int l_lbi_idx = lb.GetItemIndexAtPoint(new Point(svr.Left, svr.Height / 2));
                if (l_lbi_idx < 0) {
                    l_lbi_idx = 0;
                }
                var llbir = lb.GetListBoxItemRect(l_lbi_idx);
                if (llbir.Left <= svr.Left + MpMeasurements.Instance.ClipTileMargin) {
                    //when last visible item's right edge is past the listboxes edge
                    int itemsRemaining = l_lbi_idx;
                    //MpConsole.WriteLine($"Scrolling left, right most idx: {l_lbi_idx} with remaining: {itemsRemaining}  and threshold: {thresholdRemainingItemCount}");

                    if (itemsRemaining <= RemainingItemsThreshold) {
                        LoadMoreCommand.Execute(-1);
                        AssociatedObject
                            .GetScrollBar(Orientation.Horizontal)
                                .Value -= MpMeasurements.Instance.ClipTileBorderMinSize * RemainingItemsThreshold;
                    }
                }
            }
        }
    }
}
