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
    public class MpPagingListBoxBehavior : MpBehavior<ScrollViewer> {
        
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

        public MpPagingListBoxBehavior() { }

        protected override void OnLoad() {
            base.OnLoad();
            MpHelpers.RunOnMainThread(async () => {
                AssociatedObject.PreviewMouseWheel += Sv_MouseWheel;

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
                //hScrollBar.Track.Thumb.MouseMove += Thumb_MouseMove;

                MpHelpers.CreateBinding(
                   MpClipTrayViewModel.Instance,
                   new PropertyPath(
                       nameof(MpClipTrayViewModel.Instance.ScrollOffset)),
                   hScrollBar.Track, Track.ValueProperty);

                MpHelpers.CreateBinding(
                    MpClipTrayViewModel.Instance,
                    new PropertyPath(
                        nameof(MpClipTrayViewModel.Instance.MaximumScrollOfset)),
                    hScrollBar.Track, Track.MaximumProperty);


                hScrollBar.Track.Minimum = 0;

                MpMessenger.Instance.Register<MpMessageType>(
                    MpClipTrayViewModel.Instance,
                    ReceivedClipTrayViewModelMessage);

                var mwrb = (Application.Current.MainWindow as MpMainWindow).TitleBarView.MainWindowResizeBehvior;
                MpMessenger.Instance.Register<MpMessageType>(
                    mwrb,
                    ReceivedMainWindowResizeBehaviorMessage);

                _timer = new DispatcherTimer(DispatcherPriority.Normal);
                _timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                _timer.Tick += HandleWorldTimerTick;
                _timer.Start();
            });
            
        }

        #endregion

        #region Public Methods

        public int FindJumpTileIdx(double trackValue) {
            int totalTileCount = MpDataModelProvider.Instance.AllFetchedAndSortedCopyItemIds.Count;
            var headItemIds = MpDataModelProvider.Instance.AllFetchedAndSortedCopyItemIds;
            var uniqueWidthLookup = MpClipTrayViewModel.Instance.PersistentUniqueWidthTileLookup;

            double offsetX = 0;
            for (int i = 0; i < totalTileCount; i++) {
                offsetX += MpMeasurements.Instance.ClipTileMargin * 3;
                int tileHeadId = headItemIds[i];

                if (offsetX >= trackValue) {
                    return i;
                }

                if (uniqueWidthLookup.ContainsKey(tileHeadId)) {
                    offsetX += uniqueWidthLookup[tileHeadId];
                    offsetX -= MpMeasurements.Instance.ClipTileMargin * 2;
                } else {
                    offsetX += MpClipTileViewModel.DefaultBorderWidth;
                }
            }

            return totalTileCount - 1;
        }

        #endregion

        #region Private Methods

        private void ReceivedMainWindowResizeBehaviorMessage(MpMessageType msg) {
            switch (msg) {
                //case MpMessageType.Resizing:
                case MpMessageType.ResizeCompleted:
                    //ApplyOffsetChange(true);
                    MpDataModelProvider.Instance.QueryInfo.NotifyQueryChanged(false);
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

        private async void Track_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            e.Handled = true;
            await PerformPageJump();
        }

        private async void Track_MouseMove(object sender, MouseEventArgs e) {
            //only used when dragging data onto trackbar
            if(!MpDragDropManager.Instance.IsDragAndDrop || 
                MpClipTrayViewModel.Instance.IsScrollJumping ||
                MpClipTrayViewModel.Instance.IsThumbDragging) {
                return;
            }
            await PerformPageJump();
        }


        private async Task PerformPageJump() {
            await MpHelpers.RunOnMainThreadAsync(() => {

                Track htrack = AssociatedObject.GetScrollBar(Orientation.Horizontal).Track;

                var htrack_mp = Mouse.GetPosition(htrack);
                if (htrack_mp.Y < 0) {
                    return;
                }

                //MpClipTrayViewModel.Instance.IsThumbDragging = true;

                _velocity = _lastWheelDelta = 0;

                //double deltaX = htrack.ValueFromPoint(htrack_mp);
                //while (Mouse.LeftButton == MouseButtonState.Pressed) {
                //    await Task.Delay(10);
                //    var new_mp = Mouse.GetPosition(htrack);
                //    deltaX += new_mp.X - htrack_mp.X;
                //    htrack_mp = new_mp;

                //    double newOffset = MpClipTrayViewModel.Instance.ScrollOffset + deltaX;
                //    MpClipTrayViewModel.Instance.ScrollOffset =
                //    MpClipTrayViewModel.Instance.LastScrollOfset =
                //        newOffset;
                //}

                //int targetTileIdx = FindJumpTileIdx(MpClipTrayViewModel.Instance.ScrollOffset);
                //MpClipTrayViewModel.Instance.IsThumbDragging = false;

                //if (htrack.Thumb.Bounds().Contains(Mouse.GetPosition(htrack.Thumb))) {



                //    MpClipTrayViewModel.Instance.JumpToQueryIdxCommand.Execute(dragTargetTileIdx);
                //    return ;
                //}
                int targetTileIdx = FindJumpTileIdx(htrack.ValueFromPoint(htrack_mp));

                MpClipTrayViewModel.Instance.JumpToQueryIdxCommand.Execute(targetTileIdx);

                return;
            });
        }

        private void Sv_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (//MpClipTrayViewModel.Instance.IsAnyTileFlipped ||
                MpClipTrayViewModel.Instance.IsAnyTileExpanded ||
                MpMainWindowViewModel.Instance.IsMainWindowOpening) {
                return;
            }


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

        private void ApplyOffsetChange(bool isChangeResize = false) {
            if (!LoadMoreCommand.CanExecute(0) || MpClipTrayViewModel.Instance.IsThumbDragging) {
                return;
            }

            double horizontalChange = MpClipTrayViewModel.Instance.ScrollOffset - MpClipTrayViewModel.Instance.LastScrollOfset;

            Rect svr = AssociatedObject.Bounds();
            ListBox lb = AssociatedObject.GetVisualDescendent<ListBox>();

            if (horizontalChange > 0 || isChangeResize) {
                //scrolling down towards end of list

                //get item under point in middle of right edge of listbox
                int r_target_idx = lb.GetItemIndexAtPoint(new Point(svr.Right, svr.Height / 2),AssociatedObject);
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
            } 
            if (horizontalChange < 0 || isChangeResize) {
                //scrolling up towards beginning of list

                int l_lbi_idx = lb.GetItemIndexAtPoint(new Point(svr.Left, svr.Height / 2),AssociatedObject);
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
        #endregion
    }
}
