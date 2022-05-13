using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpPagingListBoxBehavior : MpBehavior<ScrollViewer> {
        #region Private Variables

        private int _lastWheelDelta = 0;
        
        private double _velocity = 0;

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

                hScrollBar.Track.PreviewMouseLeftButtonDown += Track_PreviewMouseLeftButtonDown;
                hScrollBar.Track.PreviewMouseLeftButtonUp += Track_PreviewMouseUp;
                hScrollBar.Track.PreviewMouseMove += Track_PreviewMouseMove;

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


        public void ScrollIntoView(object obj) {
            MpHelpers.RunOnMainThread(() => {
                var ctrvm = AssociatedObject.DataContext as MpClipTrayViewModel;
                if (ctrvm.IsScrollingIntoView || ctrvm.IsAnyBusy) {
                    return;
                }

                if (obj is MpClipTileViewModel ctvm) {
                    if (!ctrvm.Items.Contains(ctvm) || ctvm.IsPinned) {
                        return;
                    }
                    ctrvm.IsScrollingIntoView = true;
                    _velocity = 0;
                    ctrvm.ScrollOffset += GetScrollIntoViewDeltaOffset(ctvm); ;
                    ctrvm.IsScrollingIntoView = false;
                    return;

                    //while (true) {
                    //    ctrvm.IsScrollingIntoView = true;
                    //    double deltaScrollOffset = GetScrollIntoViewDeltaOffset(ctvm);
                    //    MpConsole.WriteLine("Delta offset: " + deltaScrollOffset);
                    //    double targetScrollOffset = ctrvm.ScrollOffset + deltaScrollOffset;
                    //    double vel = 30;

                    //    if (Math.Abs(deltaScrollOffset) < 1) {
                    //        ctrvm.ScrollOffset = targetScrollOffset;
                    //        _velocity = 0;
                    //        break;
                    //    }

                    //    if (deltaScrollOffset > 0) {
                    //        if (ctrvm.ScrollOffset > targetScrollOffset) {
                    //            ctrvm.ScrollOffset = targetScrollOffset;
                    //            _velocity = 0;
                    //            break;
                    //        }
                    //        _velocity = vel;
                    //    } else {
                    //        if (ctrvm.ScrollOffset < targetScrollOffset) {
                    //            ctrvm.ScrollOffset = targetScrollOffset;
                    //            _velocity = 0;
                    //            break;
                    //        }
                    //        _velocity = -vel;
                    //    }
                    //    await Task.Delay(1000 / 30);
                    //}

                    //ctrvm.IsScrollingIntoView = false;
                }
            });
        }

        #endregion

        #region Private Methods
        private double GetScrollIntoViewDeltaOffset(MpClipTileViewModel ctvm) {
            var ctrvm = MpClipTrayViewModel.Instance;

            double pad = 0;
            var svr = new Rect(0, 0, ctrvm.ClipTrayScreenWidth, ctrvm.ClipTrayHeight);//AssociatedObject.Bounds();
            var ctvm_rect = new Rect(ctvm.TrayX - ctrvm.ScrollOffset, 0, ctvm.TileBorderWidth + (MpMeasurements.Instance.ClipTileMargin * 2), ctvm.TileBorderHeight);
            if (svr.Contains(ctvm_rect)) {
                return 0;
            }

            if (ctvm_rect.Left < svr.Left) {
                //item is outside on left
                return ctvm_rect.Left - svr.Left - pad;
            } else if (ctvm_rect.Right > svr.Right) {
                //item is outside on right
                return ctvm_rect.Right - svr.Right + pad;
            }

            return 0;
        }
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
            if(MpClipTrayViewModel.Instance.IsRequery || 
               !MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                return;
            }

            if(MpClipTrayViewModel.Instance.ScrollOffset < 0) {
                MpClipTrayViewModel.Instance.ScrollOffset = 0;
                _velocity = 0;
            } else if(MpClipTrayViewModel.Instance.ScrollOffset > MpClipTrayViewModel.Instance.MaximumScrollOfset) {
                MpClipTrayViewModel.Instance.ScrollOffset = MpClipTrayViewModel.Instance.MaximumScrollOfset;
                _velocity = 0;
            }

            if(MpClipTrayViewModel.Instance.IsThumbDragging) {
                return;
            }

            AssociatedObject.ScrollToHorizontalOffset(MpClipTrayViewModel.Instance.ScrollOffset);
            if(Math.Abs(_velocity) < 0.1){
                _velocity = 0;
                MpClipTrayViewModel.Instance.HasScrollVelocity = false;
            } else {
                MpClipTrayViewModel.Instance.ScrollOffset += _velocity;
                _velocity *= Friction;
                MpClipTrayViewModel.Instance.HasScrollVelocity = true;
            }
        }


        private void Track_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            e.Handled = true;

            Track htrack = AssociatedObject.GetScrollBar(Orientation.Horizontal).Track;
            htrack.CaptureMouse();
        }

        private void Track_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            Track htrack = AssociatedObject.GetScrollBar(Orientation.Horizontal).Track;
            htrack.ReleaseMouseCapture();

            e.Handled = true;
            PerformPageJump();
        }

        private void Track_PreviewMouseMove(object sender, MouseEventArgs e) {
            MpClipTrayViewModel.Instance.IsThumbDragging = Mouse.LeftButton == MouseButtonState.Pressed;

            if (MpClipTrayViewModel.Instance.IsThumbDragging) {
                e.Handled = true;

                MpClipTrayViewModel.Instance.ForceScrollOffset(GetTrackValue());
            }
        }


        private void PerformPageJump() {
            double newTrackVal = GetTrackValue();

            int targetTileIdx = MpClipTrayViewModel.Instance.FindJumpTileIdx(newTrackVal);
            MpClipTrayViewModel.Instance.QueryCommand.Execute(targetTileIdx);
        }

        private double GetTrackValue() {
            Track htrack = AssociatedObject.GetScrollBar(Orientation.Horizontal).Track;

            var htrack_mp = Mouse.GetPosition(htrack);
            _velocity = _lastWheelDelta = 0;
            double newTrackVal = (htrack_mp.X / htrack.RenderSize.Width) * htrack.Maximum;
            return Math.Min(Math.Max(htrack.Minimum, newTrackVal), htrack.Maximum);
        }

        private void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            ApplyVelocity(e.Delta);
        }

        private void ApplyVelocity(double v) {
            if (//MpClipTrayViewModel.Instance.IsAnyTileFlipped ||
                //MpClipTrayViewModel.Instance.IsAnyTileExpanded ||
                MpClipTrayViewModel.Instance.IsScrollingIntoView ||
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
            if (MpClipTrayViewModel.Instance.IsAnyBusy || 
                MpClipTrayViewModel.Instance.IsThumbDragging || 
                isChangeResize != MpResizeBehavior.IsAnyResizing) {
                return;
            }
            var ctrvm = MpClipTrayViewModel.Instance;
            var lb = AssociatedObject.GetVisualDescendent<ListBox>();
            var lbil = lb.GetListBoxItems();
            if(lbil == null || lbil.Count() == 0) {
                return;
            }
            lbil = lbil.Where(x => x.DataContext is MpClipTileViewModel ctvm && !ctvm.IsPlaceholder);
            Rect svr = new Rect(0, 0, ctrvm.ClipTrayScreenWidth, ctrvm.ClipTrayHeight);//AssociatedObject.Bounds();
            double horizontalChange = MpClipTrayViewModel.Instance.ScrollOffset - MpClipTrayViewModel.Instance.LastScrollOffset;            

            if(horizontalChange > 0) {
                var tail_lbi_origin = lbil.Aggregate((a, b) => a.GetRect(true).X > b.GetRect(true).X ? a : b).TranslatePoint(new Point(), AssociatedObject);
                if (tail_lbi_origin.X < svr.Right) {
                    ctrvm.QueryCommand.Execute(true);
                }
            } else if(horizontalChange < 0) {
                var head_lbi_origin = lbil.Aggregate((a, b) => a.GetRect(true).X < b.GetRect(true).X ? a : b).TranslatePoint(new Point(), AssociatedObject);
                if (head_lbi_origin.X > 0) {
                    ctrvm.QueryCommand.Execute(false);
                }
            }

            //return;
            //if (!LoadMoreCommand.CanExecute(0) || MpClipTrayViewModel.Instance.IsThumbDragging || isChangeResize != MpResizeBehavior.IsAnyResizing) {
            //    return;
            //}

            //double horizontalChange = MpClipTrayViewModel.Instance.ScrollOffset - MpClipTrayViewModel.Instance.LastScrollOfset;

            
            //ListBox lb = AssociatedObject.GetVisualDescendent<ListBox>();
            //Rect svr = AssociatedObject.Bounds();

            //if (horizontalChange > 0 || isChangeResize) {
            //    //scrolling down towards end of list

            //    //get item under point in middle of right edge of listbox
            //    int item_at_left_edge_idx = lb.GetItemIndexAtPoint(new Point(svr.Left, svr.Height / 2), AssociatedObject);
            //    //if (item_at_left_edge_idx < 0) {
            //    //    return;
            //    //}
            //    //if (item_at_left_edge_idx >= lb.Items.Count) {
            //    //    item_at_left_edge_idx = lb.Items.Count - 1;
                    
            //    //}
            //    ////when last visible item's right edge is past the listboxes edge
            //    //int remainingItemsOnRight = lb.Items.Count - item_at_left_edge_idx - 1;

            //    if (item_at_left_edge_idx >= RemainingItemsThreshold) {
            //        LoadMoreCommand.Execute(1);
            //    }

            //} 
            //if(isChangeResize) {
            //    if (!LoadMoreCommand.CanExecute(0)) {
            //        return;
            //    }
            //}
            //if (horizontalChange < 0 || isChangeResize) {
            //    //scrolling up towards beginning of list

            //    int item_at_right_edge_idx = lb.GetItemIndexAtPoint(new Point(svr.Right, svr.Height / 2), AssociatedObject);
            //    if (item_at_right_edge_idx < 0) {
            //        item_at_right_edge_idx = 0;
            //    }

            //    //when last visible item's right edge is past the listboxes edge
            //    int remainingItemsOnRight = lb.Items.Count - item_at_right_edge_idx - 1;
            //    //MpConsole.WriteLine($"Scrolling left, right most idx: {l_lbi_idx} with remaining: {itemsRemaining}  and threshold: {thresholdRemainingItemCount}");

            //    if (remainingItemsOnRight >= RemainingItemsThreshold) {
            //        LoadMoreCommand.Execute(-1);
            //    }


            //}
        }


        #endregion
    }
}
