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
    public class MpPagingListBoxBehavior : MpSingletonBehavior<ScrollViewer,MpPagingListBoxBehavior> {
        private static readonly Lazy<MpPagingListBoxBehavior> _Lazy = new Lazy<MpPagingListBoxBehavior>(() => new MpPagingListBoxBehavior());
        public static MpPagingListBoxBehavior Instance { get { return _Lazy.Value; } }
        
        #region Private Variables

        private int _lastWheelDelta = 0;
        private static double _velocity = 0;

        private DispatcherTimer _timer;

        #endregion

        #region Properties

        public double Friction { get; set; } = 0;

        public double WheelDampening { get; set; } = 0;
        

        #region RemainingItemsThresholdProperty

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

        #endregion

        #region LoadMoreCommandProperty

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

        #endregion

        #region Constructors

        protected override void OnLoad() {
            base.OnLoad();
            MpHelpers.Instance.RunOnMainThread(async () => {
                AssociatedObject.PreviewMouseWheel += Sv_PreviewMouseWheel;

                var hScrollBar = AssociatedObject.GetScrollBar(Orientation.Horizontal);
                while (hScrollBar == null) {
                    hScrollBar = AssociatedObject.GetScrollBar(Orientation.Horizontal);
                    await Task.Delay(100);
                }

                while (hScrollBar.Track == null) {
                    await Task.Delay(100);
                }

                hScrollBar.Track.PreviewMouseDown += Track_PreviewMouseDown;
                hScrollBar.Track.MouseMove += Track_MouseMove;
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
                    MpMainWindowResizeBehavior.Instance,
                    ReceivedMainWindowResizeBehaviorMessage);

                _timer = new DispatcherTimer(DispatcherPriority.Normal);
                _timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                _timer.Tick += HandleWorldTimerTick;
                _timer.Start();
            });
            
        }
        #endregion

        private void ReceivedMainWindowResizeBehaviorMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.Resizing:
                case MpMessageType.ResizeCompleted:
                    
                    break;
            }
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
            if(Math.Abs(_velocity) < 0.1) {
                _velocity = 0;
                MpClipTrayViewModel.Instance.HasScrollVelocity = false;
            } else {
                MpClipTrayViewModel.Instance.ScrollOffset += _velocity;
                _velocity *= Friction;
                MpClipTrayViewModel.Instance.HasScrollVelocity = true;
            }
        }

        private void Track_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            e.Handled = PerformPageJump();
        }

        private void Track_MouseMove(object sender, MouseEventArgs e) {
            if(!MpDragDropManager.Instance.IsDragAndDrop || MpClipTrayViewModel.Instance.IsScrollJumping) {
                return;
            }
            PerformPageJump();
        }

        private bool PerformPageJump() {
            Track htrack = AssociatedObject.GetScrollBar(Orientation.Horizontal).Track;

            var htrack_mp = Mouse.GetPosition(htrack);
            if (htrack_mp.Y < 0) {
                return false;
            }

            if (htrack.Thumb.Bounds().Contains(Mouse.GetPosition(htrack.Thumb))) {
                return true;
            }

            _velocity = _lastWheelDelta = 0;

            double tileSize = MpMeasurements.Instance.ClipTileMinSize;
            if (MpClipTrayViewModel.Instance.Items.Count > 0) {
                //in case tiles have been resized get current size from one of em
                tileSize = MpClipTrayViewModel.Instance.Items[0].TileBorderHeight;
            }
            //int targetTileIdx = (int)(htrack.ValueFromPoint(htrack_mp) / tileSize);
            int targetTileIdx = FindJumpTileIdx(htrack.ValueFromPoint(htrack_mp));

            MpClipTrayViewModel.Instance.JumpToQueryIdxCommand.Execute(targetTileIdx);

            return true;
        }

        public double FindTileOffsetX(int queryOffsetIdx) {
            int totalTileCount = MpDataModelProvider.Instance.AllFetchedAndSortedCopyItemIds.Count;
            if(queryOffsetIdx < 0 || queryOffsetIdx >= totalTileCount) {
                throw new Exception($"HeadItemId {queryOffsetIdx} is out of item bounds of {totalTileCount}");
            }

            var headItemIds = MpDataModelProvider.Instance.AllFetchedAndSortedCopyItemIds;
            var uniqueWidthLookup = MpClipTrayViewModel.Instance.PersistentUniqueWidthTileLookup;

            double offsetX = 0;// MpMeasurements.Instance.ClipTileMargin;
            for (int i = 1; i <= queryOffsetIdx; i++) {
                offsetX += MpMeasurements.Instance.ClipTileMargin * 2;
                int tileHeadId = headItemIds[i-1];

                if (uniqueWidthLookup.ContainsKey(tileHeadId)) {
                    offsetX += uniqueWidthLookup[tileHeadId];
                    offsetX -= MpMeasurements.Instance.ClipTileMargin;
                } else {
                    offsetX += MpClipTileViewModel.DefaultBorderWidth;
                }
            }

            return offsetX;
        }

        public int FindJumpTileIdx(double trackValue) {
            int totalTileCount = MpDataModelProvider.Instance.AllFetchedAndSortedCopyItemIds.Count;
            var headItemIds = MpDataModelProvider.Instance.AllFetchedAndSortedCopyItemIds;
            var uniqueWidthLookup = MpClipTrayViewModel.Instance.PersistentUniqueWidthTileLookup;

            double offsetX = 0;
            for (int i = 0; i < totalTileCount; i++) {
                offsetX += MpMeasurements.Instance.ClipTileMargin * 3;
                int tileHeadId = headItemIds[i];

                if(offsetX >= trackValue) {
                    return i;
                }

                if(uniqueWidthLookup.ContainsKey(tileHeadId)) {
                    offsetX += uniqueWidthLookup[tileHeadId];
                    offsetX -= MpMeasurements.Instance.ClipTileMargin * 2;
                } else {
                    offsetX += MpClipTileViewModel.DefaultBorderWidth;
                }
            }

            return totalTileCount - 1;
        }

        private void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (//MpClipTrayViewModel.Instance.IsAnyTileFlipped ||
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
                int remainingItemsOnRight = lb.Items.Count - r_target_idx - 1;

                if (remainingItemsOnRight < RemainingItemsThreshold) {
                    LoadMoreCommand.Execute(1);
                }
            } else if (horizontalChange < 0) {
                //scrolling up towards beginning of list

                int l_lbi_idx = lb.GetItemIndexAtPoint(new Point(svr.Left, svr.Height / 2));
                if (l_lbi_idx < 0) {
                    l_lbi_idx = 0;
                }

                //when last visible item's right edge is past the listboxes edge
                int remainingItemsOnLeft = l_lbi_idx;
                //MpConsole.WriteLine($"Scrolling left, right most idx: {l_lbi_idx} with remaining: {itemsRemaining}  and threshold: {thresholdRemainingItemCount}");

                if (remainingItemsOnLeft < RemainingItemsThreshold) {
                    LoadMoreCommand.Execute(-1);
                }
            }
        }
    }
}
