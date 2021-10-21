using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpPagingListBox : DependencyObject {
        public static int GetRemainingItemsThreshold(DependencyObject obj) {
            return (int)obj.GetValue(RemainingItemsThresholdProperty);
        }
        public static void SetRemainingItemsThreshold(DependencyObject obj, int value) {
            obj.SetValue(RemainingItemsThresholdProperty, value);
        }
        public static readonly DependencyProperty RemainingItemsThresholdProperty =
          DependencyProperty.RegisterAttached(
            "RemainingItemsThreshold",
            typeof(int),
            typeof(MpPagingListBox),
            new FrameworkPropertyMetadata());

        public static ICommand GetRemainingItemsThresholdReachedCommand(DependencyObject obj) {
            return (ICommand)obj.GetValue(RemainingItemsThresholdReachedCommandProperty);
        }
        public static void SetRemainingItemsThresholdReachedCommand(DependencyObject obj, ICommand value) {
            obj.SetValue(RemainingItemsThresholdReachedCommandProperty, value);
        }
        public static readonly DependencyProperty RemainingItemsThresholdReachedCommandProperty =
          DependencyProperty.RegisterAttached(
            "RemainingItemsThresholdReachedCommand",
            typeof(ICommand),
            typeof(MpPagingListBox),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if(e.NewValue == null) {
                        return;
                    }
                    var lb = (ListBox)obj;
                    lb.Loaded += Lb_Loaded;
                    lb.Unloaded += Lb_Unloaded;
                }
            });

        private static void Lb_Unloaded(object sender, RoutedEventArgs e) {
            var lb = sender as ListBox;
            var sv = lb.GetScrollViewer();
            sv.ScrollChanged -= Sv_ScrollChanged;
            lb.Loaded -= Lb_Loaded;
            lb.Unloaded -= Lb_Unloaded;
        }

        private static void Lb_Loaded(object sender, RoutedEventArgs e) {
            var sv = (sender as ListBox).GetScrollViewer();
            sv.ScrollChanged += Sv_ScrollChanged;
        }

        private static void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if(e.HorizontalChange == 0) {
                return;
            }
            var sv = sender as ScrollViewer;
            if(sv == null) {
                return;
            }
            var lb = sv.GetVisualAncestor<ListBox>();
            if(lb == null) {
                return;
            }

            int remainingItemCount = (int)lb.GetValue(RemainingItemsThresholdProperty);
            var loadMoreCommand = (ICommand)lb.GetValue(RemainingItemsThresholdReachedCommandProperty);

            if(loadMoreCommand == null) {
                return;
            }

            int count = lb.Items.Count;
            if (count <= remainingItemCount) {
                return;
            }
            var lbr = lb.GetListBoxRect();

            if(e.HorizontalChange > 0) {
                //scrolling left
                if(!_isLeftScroll) {
                    _loadCount = 0;
                }
                _isLeftScroll = true;
                var r_lbi_idx = lb.GetItemIndexAtPoint(new Point(lbr.Right, lbr.Height / 2));
                MpConsole.WriteLine($"Scrolling left, right most idx: {r_lbi_idx} with remaining: {count-r_lbi_idx}  max remaining: {remainingItemCount}");
                if (count - r_lbi_idx <= remainingItemCount) {
                    //MpClipTrayViewModel.Instance.RecycleItemsCommand.Execute(1);
                    //StartOrContinueLoadTimer();

                    MpClipTrayViewModel.Instance.LoadAndRecycleMoreClipsCommand.Execute(1);
                }
            } else {
                //scrolling right
                if (_isLeftScroll) {
                    _loadCount = 0;
                }
                _isLeftScroll = false;
                var l_lbi_idx = lb.GetItemIndexAtPoint(new Point(lbr.Left, lbr.Height / 2));
                MpConsole.WriteLine($"Scrolling right, left most idx: {l_lbi_idx} with remaining: {l_lbi_idx}  max remaining: {remainingItemCount}");
                if (l_lbi_idx - remainingItemCount <= 0) {
                    //MpClipTrayViewModel.Instance.RecycleItemsCommand.Execute(-1);
                    //StartOrContinueLoadTimer();
                    MpClipTrayViewModel.Instance.LoadAndRecycleMoreClipsCommand.Execute(-1);
                }
            }         
        }

        private static CancellationTokenSource _cts;
        private static bool _isLeftScroll = false;
        private static int _loadCount = 0;
        private static DateTime _lastScrollTime;
        private static TimeSpan _maxWaitToLoadTime = TimeSpan.FromMilliseconds(500);
        private static DispatcherTimer _loadTimer;

        private static void StartOrContinueLoadTimer() {
            _lastScrollTime = DateTime.Now;
            _loadCount++;

            if(_loadTimer == null) {
                _loadTimer = new DispatcherTimer();
                _loadTimer.Interval = TimeSpan.FromMilliseconds(300);
                _loadTimer.Tick += _loadTimer_Tick;
            }
            if(!_loadTimer.IsEnabled) {
                _loadTimer.IsEnabled = true;
                _loadTimer.Start();
            }
        }

        private static void StopTimerAndLoad(ICommand loadCommand) {
            _loadTimer.IsEnabled = false;
            _loadTimer.Stop();

            _loadCount = _isLeftScroll ? _loadCount : -_loadCount;
            MpConsole.WriteLine("Load count: " + _loadCount);
            loadCommand.Execute(_loadCount);

            _loadCount = 0;
        }

        private static void _loadTimer_Tick(object sender, EventArgs e) {
            if(DateTime.Now - _lastScrollTime < _maxWaitToLoadTime) {
                return;
            }

            StopTimerAndLoad(MpClipTrayViewModel.Instance.LoadMoreClipsCommand);
        }
    }
}
