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
    public class MpPagingListBoxBehavior : Behavior<ListBox> {
        private static readonly Lazy<MpPagingListBoxBehavior> _Lazy = new Lazy<MpPagingListBoxBehavior>(() => new MpPagingListBoxBehavior());
        public static MpPagingListBoxBehavior Instance { get { return _Lazy.Value; } }
        
        #region Private Variables

        private int _lastWheelDelta = 0;
        private static double _velocity = 0;

        private double _scrollTarget {
            get {
                return MpClipTrayViewModel.Instance.ScrollOffset;
            }
            set {
                MpClipTrayViewModel.Instance.ScrollOffset = value;
            }
        }

        private DispatcherTimer _timer;

        private Thumb _hthumb;
        private Track _htrack;
        private ScrollBar _hsb;
        private ScrollViewer _sv;

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
            MpHelpers.Instance.RunOnMainThread(async () => {
                _sv = AssociatedObject.GetVisualDescendent<ScrollViewer>();
                while (_sv == null) {
                    _sv = AssociatedObject.GetVisualDescendent<ScrollViewer>();
                    await Task.Delay(100);
                }
                _sv.PreviewMouseWheel += Sv_PreviewMouseWheel;

                _hsb = _sv.GetScrollBar(Orientation.Horizontal);
                _hsb.PreviewMouseDown += Sv_PreviewMouseDown;

                while (_hsb.Track == null) {
                    await Task.Delay(100);
                }
                _htrack = _hsb.Track;
                _hthumb = _htrack.Thumb;

                MpMessenger.Instance.Register<MpMessageType>(MpClipTrayViewModel.Instance, ReceivedClipTrayViewModelMessage);

            });
            
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
                    
                    _hsb.Maximum = MpClipTrayViewModel.Instance.ClipTrayTotalWidth;                    
                    _hsb.Minimum = 0;

                    _hsb.UpdateLayout();

                    if (msg == MpMessageType.RequeryCompleted) {
                        MpConsole.WriteLine("Tray Width: " + AssociatedObject.ActualWidth);
                        _hsb.Value = 0;
                        //AssociatedObject.ScrollToHorizontalOffset(0);
                        //AssociatedObject.ScrollToLeftEnd();
                        //AssociatedObject.ScrollToHome();
                    }
                    //_sv.ScrollToHorizontalOffset(0);
                    //_sv.InvalidateScrollInfo();
                    _sv.UpdateLayout();

                    //_scrollTarget = _hsb.Value;
                    break;
                case MpMessageType.KeyboardNext:
                case MpMessageType.KeyboardPrev:
                    double offset = -MpMeasurements.Instance.ClipTileMinSize;
                    if(msg == MpMessageType.KeyboardPrev) {
                        offset = -offset;
                    }
                    
                    _htrack.Value -= offset;
                    _scrollTarget -= offset;
                    ApplyOffsetChange(offset);
                    break;
                case MpMessageType.KeyboardHome:
                    _htrack.Value = 0;
                    _scrollTarget = 0;
                    break;
                case MpMessageType.KeyboardEnd:
                    _htrack.Value = _htrack.Maximum;
                    _scrollTarget = ((MpClipTrayViewModel.Instance.InitialLoadCount - MpMeasurements.Instance.TotalVisibleClipTiles) * MpMeasurements.Instance.ClipTileMinSize);
                    break;
            }
        }

        private void HandleWorldTimerTick(object sender, EventArgs e) {
           // if (Math.Abs(_velocity) > 0.1) 
                {                 
                _sv.ScrollToHorizontalOffset(_scrollTarget);
                //MpMeasurements.Instance.ClipTileBorderMinSize * RemainingItemsThreshold;

                _scrollTarget += _velocity;
                _velocity *= Friction;
                _htrack.Value += _velocity;
            }
        }

        private void Sv_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var htrack_mp = e.GetPosition(_htrack);
            if (htrack_mp.Y < 0) {
                return;
            }
            var hthumb_rect = _hthumb.Bounds();
            if (hthumb_rect.Contains(e.GetPosition(_hthumb))) {
                return;
            }

            e.Handled = true;
            _lastWheelDelta = 0;
            double norm_x = htrack_mp.X / _htrack.ActualWidth;

            int targetTileIdx = (int)(norm_x * MpClipTrayViewModel.Instance.TotalItemsInQuery);            

            MpClipTrayViewModel.Instance.JumpToPageIdxCommand.Execute(targetTileIdx);

            double targetThumbValue = MpMeasurements.Instance.ClipTileMinSize * targetTileIdx;
            if(MpClipTrayViewModel.Instance.TotalItemsInQuery - targetTileIdx <= MpMeasurements.Instance.TotalVisibleClipTiles) {
                //when target position is beyond total track width - half thumb width need to manually set thumb to max
                targetThumbValue = _hsb.Maximum - ((MpClipTrayViewModel.Instance.InitialLoadCount - MpMeasurements.Instance.TotalVisibleClipTiles) * MpMeasurements.Instance.ClipTileMinSize);
            }
            _hsb.Value = targetThumbValue;
        }

        private void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (MpClipTrayViewModel.Instance.IsAnyTileFlipped ||
                MpClipTrayViewModel.Instance.IsAnyTileExpanded ||
                MpMainWindowViewModel.Instance.IsMainWindowOpening) {
                return;
            }
            if((_lastWheelDelta < 0 && e.Delta > 0) ||
               (_lastWheelDelta > 0 && e.Delta < 0)) {
                _velocity = 0;
            }

            if ((e.Delta < 0 && _hsb.Value >= _htrack.Maximum) ||
                   (e.Delta > 0 && _hsb.Value <= 0)) {
                _velocity = 0;
                return;
            }

            _velocity -= e.Delta * WheelDampening;
            ApplyOffsetChange(e.Delta);
            e.Handled = true;
            _lastWheelDelta = e.Delta;
        }


        public void ApplyOffsetChange(double horizontalChange) {

            if (!LoadMoreCommand.CanExecute(0)) {
                return;
            }
            Rect svr = AssociatedObject.Bounds();
            ListBox lb = AssociatedObject;

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
                //var rlbir = lb.GetListBoxItemRect(r_target_idx);
                //if (rlbir.Right <= svr.Right) 
                    {
                    //when last visible item's right edge is past the listboxes edge
                    int itemsRemaining = lb.Items.Count - r_target_idx - 1;

                    if (itemsRemaining < RemainingItemsThreshold) {
                        LoadMoreCommand.Execute(1);
                    }
                }
            } else if (horizontalChange > 0) {
                //scrolling up towards beginning of list

                int l_lbi_idx = lb.GetItemIndexAtPoint(new Point(svr.Left, svr.Height / 2));
                if (l_lbi_idx < 0) {
                    l_lbi_idx = 0;
                }
                //var llbir = lb.GetListBoxItemRect(l_lbi_idx);
                //if (llbir.Right <= svr.Left + MpMeasurements.Instance.ClipTileMargin) 
                 {
                    //when last visible item's right edge is past the listboxes edge
                    int itemsRemaining = l_lbi_idx;
                    //MpConsole.WriteLine($"Scrolling left, right most idx: {l_lbi_idx} with remaining: {itemsRemaining}  and threshold: {thresholdRemainingItemCount}");

                    if (itemsRemaining < RemainingItemsThreshold) {
                        LoadMoreCommand.Execute(-1);                        
                    }
                }
            }
        }
    }
}
