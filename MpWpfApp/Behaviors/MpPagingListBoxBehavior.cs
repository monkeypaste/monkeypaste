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
        private static readonly Lazy<MpPagingListBoxBehavior> _Lazy = new Lazy<MpPagingListBoxBehavior>(() => new MpPagingListBoxBehavior());
        public static MpPagingListBoxBehavior Instance { get { return _Lazy.Value; } }
        
        #region Private Variables

        private int _lastWheelDelta = 0;
        private static double _velocity = 0;

        private DispatcherTimer _timer;

        //private Thumb _hthumb;
        //private Track _htrack;
        //private ScrollBar _hsb;
        //private ScrollViewer _sv;

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
                AssociatedObject.PreviewMouseWheel += Sv_PreviewMouseWheel;

                var hScrollBar = AssociatedObject.GetScrollBar(Orientation.Horizontal);
                while(hScrollBar == null) {
                    hScrollBar = AssociatedObject.GetScrollBar(Orientation.Horizontal);
                    await Task.Delay(100);
                }

                while (hScrollBar.Track == null) {
                    await Task.Delay(100);
                }

                hScrollBar.Track.PreviewMouseDown += Sv_PreviewMouseDown;
                MpHelpers.Instance.CreateBinding(
                   MpClipTrayViewModel.Instance,
                   new PropertyPath(
                       nameof(MpClipTrayViewModel.Instance.ScrollOffset)),
                   hScrollBar.Track, Track.ValueProperty);

                MpHelpers.Instance.CreateBinding(
                    MpClipTrayViewModel.Instance,
                    new PropertyPath(
                        nameof(MpClipTrayViewModel.Instance.MaximumScrollOfset)),
                    hScrollBar.Track, Track.MaximumProperty);


                hScrollBar.Track.Minimum = 0;

                MpMessenger.Instance.Register<MpMessageType>(
                    MpClipTrayViewModel.Instance, 
                    ReceivedClipTrayViewModelMessage);

                MpMessenger.Instance.Register<MpMessageType>(
                    AssociatedObject.GetVisualAncestor<MpClipTrayView>().ClipTrayDropBehavior, 
                    ReceivedClipTrayViewModelMessage);
            });
            
            _timer = new DispatcherTimer(DispatcherPriority.Normal);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            _timer.Tick += HandleWorldTimerTick;
            _timer.Start();
        }

        private void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TrayScrollChanged:
                    ApplyOffsetChange();
                    break;
                case MpMessageType.RequeryCompleted:
                case MpMessageType.JumpToIdxCompleted:
                    AssociatedObject.UpdateLayout();
                    break;
            }
        }

        private void HandleWorldTimerTick(object sender, EventArgs e) {
            if(MpClipTrayViewModel.Instance.IsScrollJumping) {
                return;
            }
            AssociatedObject.ScrollToHorizontalOffset(MpClipTrayViewModel.Instance.ScrollOffset);
            MpClipTrayViewModel.Instance.ScrollOffset += _velocity;
            _velocity *= Friction;
        }

        private void Sv_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            Track htrack = AssociatedObject.GetScrollBar(Orientation.Horizontal).Track;

            var htrack_mp = e.GetPosition(htrack);
            if (htrack_mp.Y < 0) {
                return;
            }

            e.Handled = true;
            if (htrack.Thumb.Bounds().Contains(e.GetPosition(htrack.Thumb))) {
                return;
            }

            _velocity = _lastWheelDelta = 0;

            int targetTileIdx = (int)(htrack.ValueFromPoint(htrack_mp) / MpMeasurements.Instance.ClipTileMinSize);

            MpClipTrayViewModel.Instance.JumpToPageIdxCommand.Execute(targetTileIdx);
        }

        private void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (MpClipTrayViewModel.Instance.IsAnyTileFlipped ||
                MpClipTrayViewModel.Instance.IsAnyTileExpanded ||
                MpMainWindowViewModel.Instance.IsMainWindowOpening) {
                return;
            }

            e.Handled = true;

            if ((e.Delta < 0 && MpClipTrayViewModel.Instance.ScrollOffset >= MpClipTrayViewModel.Instance.ClipTrayTotalWidth) ||
                (e.Delta > 0 && MpClipTrayViewModel.Instance.ScrollOffset <= 0)) {
                _velocity = 0;
                return;
            }


            if ((_lastWheelDelta < 0 && e.Delta > 0) ||
               (_lastWheelDelta > 0 && e.Delta < 0)) {
                _velocity = 0;
            }           

            _velocity -= e.Delta * WheelDampening;
            _lastWheelDelta = e.Delta;
        }

        private void ApplyOffsetChange() {
            if (!LoadMoreCommand.CanExecute(0)) {
                return;
            }

            double horizontalChange = MpClipTrayViewModel.Instance.ScrollOffset - MpClipTrayViewModel.Instance.LastScrollOfset;
            
            if(Math.Abs(horizontalChange) > 0 && MpClipTrayViewModel.Instance.IsScrollJumping) {
                Debugger.Break();
            }

            Rect svr = AssociatedObject.Bounds();
            ListBox lb = AssociatedObject.GetVisualDescendent<ListBox>();

            if (horizontalChange > 0) {
                //scrolling down towards end of list

                //get item under point in middle of right edge of listbox
                int r_target_idx = lb.GetItemIndexAtPoint(new Point(svr.Right, svr.Height / 2));
                if (r_target_idx < 0) {
                    return;
                }
                if (r_target_idx >= lb.Items.Count) {
                    r_target_idx = lb.Items.Count - 1;
                }
                //when last visible item's right edge is past the listboxes edge
                int itemsRemainingOnRight = lb.Items.Count - r_target_idx - 1;

                if (itemsRemainingOnRight < RemainingItemsThreshold) {
                    LoadMoreCommand.Execute(1);
                }
            } else if (horizontalChange < 0) {
                //scrolling up towards beginning of list

                int l_lbi_idx = lb.GetItemIndexAtPoint(new Point(svr.Left, svr.Height / 2));
                if (l_lbi_idx < 0) {
                    l_lbi_idx = 0;
                }
                //when last visible item's right edge is past the listboxes edge
                int itemsRemainingOnLeft = l_lbi_idx;
                //MpConsole.WriteLine($"Scrolling left, right most idx: {l_lbi_idx} with remaining: {itemsRemaining}  and threshold: {thresholdRemainingItemCount}");

                if (itemsRemainingOnLeft < RemainingItemsThreshold) {
                    LoadMoreCommand.Execute(-1);
                }
            }
        }
    }
}
