using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpPagingListBoxBehavior : Behavior<ListBox> {
        #region Private Variables
        #endregion

        #region Properties

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
                ScrollViewer sv = AssociatedObject.GetVisualDescendent<ScrollViewer>();
                while (sv == null) {
                    sv = AssociatedObject.GetVisualDescendent<ScrollViewer>();
                    await Task.Delay(100);
                }
                sv.PreviewMouseWheel += Sv_PreviewMouseWheel;

                sv.GetScrollBar(Orientation.Horizontal).PreviewMouseDown += Sv_PreviewMouseDown;

                MpMessenger.Instance.Register<MpMessageType>(MpClipTrayViewModel.Instance, ReceivedClipTrayViewModelMessage);
            });
        }

        private void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.JumpToIdxCompleted:
                case MpMessageType.RequeryCompleted:
                    AssociatedObject.UpdateLayout();
                    var sv = AssociatedObject.GetScrollViewer();
                    if (sv != null) {
                        double tw = MpMeasurements.Instance.ClipTileBorderMinSize;
                        double ttw = tw * MpClipTrayViewModel.Instance.TotalItemsInQuery;
                        var hsb = sv.GetScrollBar(Orientation.Horizontal);

                        hsb.Maximum = ttw;
                        hsb.Minimum = 0;
                        hsb.UpdateLayout();

                        if (msg == MpMessageType.RequeryCompleted) {
                            hsb.Value = 0;
                            sv.ScrollToHorizontalOffset(0);
                            sv.ScrollToLeftEnd();
                            sv.ScrollToHome();
                        } else {
                            MpClipTrayViewModel.Instance.IsScrollJumping = false;
                            sv.ScrollToHorizontalOffset(0);
                        }

                        sv.UpdateLayout();
                        AssociatedObject.UpdateLayout();
                    }
                    break;
                case MpMessageType.Expand:
                    //TrayItemsPanel.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case MpMessageType.Unexpand:
                    //TrayItemsPanel.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
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

            MpClipTrayViewModel.Instance.IsScrollJumping = true;

            e.Handled = true;
            double norm_x = htrack_mp.X / htrack.ActualWidth;

            int targetTileIdx = (int)(norm_x * MpClipTrayViewModel.Instance.TotalItemsInQuery);            

            MpClipTrayViewModel.Instance.JumpToPageIdxCommand.Execute(targetTileIdx);

            AssociatedObject
                .GetVisualDescendent<ScrollViewer>()
                    .GetScrollBar(Orientation.Horizontal)
                        .Value = MpMeasurements.Instance.ClipTileBorderMinSize * targetTileIdx;
        }

        private void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if(!MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                e.Handled = true;
            }

            ApplyOffsetChange(e.Delta);
        }

        public void ApplyOffsetChange(double horizontalChange) {
            if (!LoadMoreCommand.CanExecute(0)) {
                return;
            }
            Rect lbr = AssociatedObject.GetListBoxRect();

            if (horizontalChange < 0) {
                //scrolling down towards end of list

                //get item under point in middle of right edge of listbox
                int r_lbi_idx = AssociatedObject.GetItemIndexAtPoint(new Point(lbr.Right, lbr.Height / 2));
                if (r_lbi_idx < 0) {// || r_lbi_idx > AssociatedObject.Items.Count) {
                    return;
                }
                if (r_lbi_idx >= AssociatedObject.Items.Count) {
                    r_lbi_idx = AssociatedObject.Items.Count - 1;
                }
                //get item over right edge's rect
                var rlbir = AssociatedObject.GetListBoxItemRect(r_lbi_idx);
                if (rlbir.Right < lbr.Right) { // - MpMeasurements.Instance.ClipTileMargin) {
                    //when last visible item's right edge is past the listboxes edge
                    int itemsRemaining = AssociatedObject.Items.Count - r_lbi_idx - 1;

                    if (itemsRemaining <= RemainingItemsThreshold) {
                        LoadMoreCommand.Execute(1);

                        AssociatedObject
                            .GetVisualDescendent<ScrollViewer>()
                                .GetScrollBar(Orientation.Horizontal)
                                    .Value += MpMeasurements.Instance.ClipTileBorderMinSize * RemainingItemsThreshold;
                    }
                }
            } else if (horizontalChange > 0) {
                //scrolling up towards beginning of list

                int l_lbi_idx = AssociatedObject.GetItemIndexAtPoint(new Point(lbr.Left, lbr.Height / 2));
                if (l_lbi_idx < 0) {
                    l_lbi_idx = 0;
                }
                var llbir = AssociatedObject.GetListBoxItemRect(l_lbi_idx);
                if (llbir.Left <= lbr.Left + MpMeasurements.Instance.ClipTileMargin) {
                    //when last visible item's right edge is past the listboxes edge
                    int itemsRemaining = l_lbi_idx;
                    //MpConsole.WriteLine($"Scrolling left, right most idx: {l_lbi_idx} with remaining: {itemsRemaining}  and threshold: {thresholdRemainingItemCount}");

                    if (itemsRemaining <= RemainingItemsThreshold) {
                        LoadMoreCommand.Execute(-1);
                        AssociatedObject
                            .GetVisualDescendent<ScrollViewer>()
                                .GetScrollBar(Orientation.Horizontal)
                                    .Value -= MpMeasurements.Instance.ClipTileBorderMinSize * RemainingItemsThreshold;
                    }
                }
            }
        }
    }
}
