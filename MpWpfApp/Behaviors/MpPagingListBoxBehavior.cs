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

                MpMessenger.Register<MpMessageType>(
                    MpClipTrayViewModel.Instance,
                    ReceivedClipTrayViewModelMessage);

                var mwrb = (Application.Current.MainWindow as MpMainWindow).MainWindowResizeBehvior;
                MpMessenger.Register<MpMessageType>(
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
            int totalTileCount = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.Count;
            var headItemIds = MpDataModelProvider.AllFetchedAndSortedCopyItemIds;
            var uniqueWidthLookup = MpClipTrayViewModel.Instance.PersistentUniqueWidthTileLookup;

            double offsetX = 0;
            for (int i = 0; i < totalTileCount; i++) {
                offsetX += MpMeasurements.Instance.ClipTileMargin;
                int tileHeadId = headItemIds[i];


                if (uniqueWidthLookup.ContainsKey(tileHeadId)) {
                    offsetX += uniqueWidthLookup[tileHeadId];
                    //offsetX -= MpMeasurements.Instance.ClipTileMargin * 2;
                } else {
                    offsetX += MpClipTileViewModel.DefaultBorderWidth;
                }

                if (offsetX >= trackValue) {
                    return i;
                }
                offsetX += MpMeasurements.Instance.ClipTileMargin;
            }

            return totalTileCount - 1;
        }

        public void ScrollIntoView(object obj) {
            return;
            var ctrvm = AssociatedObject.DataContext as MpClipTrayViewModel;
            if (ctrvm.HasScrollVelocity || ctrvm.IsBusy) {
                return;
            }
            if (obj is MpClipTileViewModel ctvm && ctvm.HeadItem != null) {
                //var ctcv = this.GetVisualDescendents<MpClipTileContainerView>().FirstOrDefault(x => x.DataContext == ctvm);
                //if(ctcv == null) {
                //    return;
                //}
                if(!ctrvm.Items.Contains(ctvm)) {
                    return;
                }
                double itemX = ctvm.TrayX;
                double itemWidth = ctvm.TileBorderWidth;
                double pad = 20;
                double deltaScrollOfset = 0;
                if (itemX < ctrvm.ScrollOffset) {
                    // tile is before current scroll location
                    double diff = ctrvm.ScrollOffset - itemX;
                    deltaScrollOfset = diff - pad;
                } else if (ctrvm.ScrollOffset + ctrvm.ClipTrayScreenWidth < itemX + itemWidth) {
                    // tile is either after current viewport or only partially visible
                    double diff = (itemX + itemWidth) - (ctrvm.ScrollOffset + ctrvm.ClipTrayScreenWidth);
                    deltaScrollOfset = diff + pad;
                }
                if (deltaScrollOfset != 0) {
                    double newOffset = ctrvm.ScrollOffset + deltaScrollOfset;

                    newOffset = Math.Max(0, Math.Min(newOffset, ctrvm.MaximumScrollOfset));

                    MpHelpers.RunOnMainThread(async () => {
                        while(true) {
                            double diff = Math.Abs(ctrvm.ScrollOffset - newOffset);
                            if (diff < 0.1) {
                                return;
                            }
                            double v = 0;
                            if(ctrvm.ScrollOffset < newOffset) {
                                v = 75;
                            } else {
                                v = -75;
                            }
                            v *= WheelDampening;
                            ApplyVelocity(v);
                            await Task.Delay(10);
                        }
                    });
                }
            }
        }

        #endregion

        #region Private Methods

        private void ReceivedMainWindowResizeBehaviorMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ResizingMainWindowComplete:
                case MpMessageType.ResizingContent:
                case MpMessageType.ResizeContentCompleted:
                    MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.MaximumScrollOfset));
                    //
                    if (msg == MpMessageType.ResizingMainWindowComplete ||
                       msg == MpMessageType.ResizeContentCompleted) {
                        //  MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                        ApplyOffsetChange(true);
                    }


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
                    _velocity = _lastWheelDelta = 0;
                    var cttvl = AssociatedObject.GetVisualDescendents<MpClipTileTitleView>();
                    if (cttvl != null) {
                        foreach (var cttv in cttvl) {
                            if (cttv.ClipTileTitleMarqueeCanvas != null) {
                                // BUG this is a workaround because marquee attached property
                                // doesn't recognize that the data context has changed
                                MpMarqueeExtension.SetIsEnabled(cttv.ClipTileTitleMarqueeCanvas, false);
                                MpMarqueeExtension.SetIsEnabled(cttv.ClipTileTitleMarqueeCanvas, true);
                            }
                        }
                    }
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
            if(!MpDragDropManager.IsDragAndDrop || 
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
                double newTrackVal = (htrack_mp.X / htrack.RenderSize.Width) * htrack.Maximum;
                //newTrackVal += htrack.Thumb.RenderSize.Width / 2;
                newTrackVal = Math.Min(Math.Max(htrack.Minimum, newTrackVal), htrack.Maximum);
                //MpClipTrayViewModel.Instance.ScrollOffset = newTrackVal;
                
                int targetTileIdx = FindJumpTileIdx(newTrackVal);

                MpClipTrayViewModel.Instance.JumpToQueryIdxCommand.Execute(targetTileIdx);
                //MpClipTrayViewModel.Instance.RequeryCommand.Execute(targetTileIdx);

                return;
            });
        }

        private void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            ApplyVelocity(e.Delta);
        }

        private void ApplyVelocity(double v) {
            if (//MpClipTrayViewModel.Instance.IsAnyTileFlipped ||
                //MpClipTrayViewModel.Instance.IsAnyTileExpanded ||
                MpMainWindowViewModel.Instance.IsMainWindowOpening ||
                !MpClipTrayViewModel.Instance.CanScroll) {
                //e.Handled = true;
                return;
            }


            if ((v < 0 && MpClipTrayViewModel.Instance.ScrollOffset >= MpClipTrayViewModel.Instance.ClipTrayTotalWidth) ||
                (v > 0 && MpClipTrayViewModel.Instance.ScrollOffset <= 0)) {
                _velocity = 0;
                return;
            }


            if ((_lastWheelDelta < 0 && v > 0) ||
               (_lastWheelDelta > 0 && v < 0)) {
                _velocity = 0;
            }

            _velocity -= v * WheelDampening;
            _lastWheelDelta = (int)v;
        }

        private void ApplyOffsetChange(bool isChangeResize = false) {
            if (!LoadMoreCommand.CanExecute(0) || MpClipTrayViewModel.Instance.IsThumbDragging || isChangeResize != MpResizeBehavior.IsAnyResizing) {
                return;
            }

            double horizontalChange = MpClipTrayViewModel.Instance.ScrollOffset - MpClipTrayViewModel.Instance.LastScrollOfset;

            
            ListBox lb = AssociatedObject.GetVisualDescendent<ListBox>();
            Rect svr = AssociatedObject.Bounds();

            if (horizontalChange > 0 || isChangeResize) {
                //scrolling down towards end of list

                //get item under point in middle of right edge of listbox
                int item_at_left_edge_idx = lb.GetItemIndexAtPoint(new Point(svr.Left, svr.Height / 2), AssociatedObject);
                //if (item_at_left_edge_idx < 0) {
                //    return;
                //}
                //if (item_at_left_edge_idx >= lb.Items.Count) {
                //    item_at_left_edge_idx = lb.Items.Count - 1;
                    
                //}
                ////when last visible item's right edge is past the listboxes edge
                //int remainingItemsOnRight = lb.Items.Count - item_at_left_edge_idx - 1;

                if (item_at_left_edge_idx >= RemainingItemsThreshold) {
                    LoadMoreCommand.Execute(1);
                }

            } 
            if(isChangeResize) {
                if (!LoadMoreCommand.CanExecute(0)) {
                    return;
                }
            }
            if (horizontalChange < 0 || isChangeResize) {
                //scrolling up towards beginning of list

                int item_at_right_edge_idx = lb.GetItemIndexAtPoint(new Point(svr.Right, svr.Height / 2), AssociatedObject);
                if (item_at_right_edge_idx < 0) {
                    item_at_right_edge_idx = 0;
                }

                //when last visible item's right edge is past the listboxes edge
                int remainingItemsOnRight = lb.Items.Count - item_at_right_edge_idx - 1;
                //MpConsole.WriteLine($"Scrolling left, right most idx: {l_lbi_idx} with remaining: {itemsRemaining}  and threshold: {thresholdRemainingItemCount}");

                if (remainingItemsOnRight >= RemainingItemsThreshold) {
                    LoadMoreCommand.Execute(-1);
                }


            }
        }
        #endregion
    }
}
