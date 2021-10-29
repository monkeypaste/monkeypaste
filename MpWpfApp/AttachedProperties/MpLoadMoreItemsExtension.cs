using GongSolutions.Wpf.DragDrop.Utilities;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpLoadMoreItemsExtension : DependencyObject {
        #region Private Variables
        #endregion

        #region RemainingItemThreshold dep prop
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
            typeof(MpLoadMoreItemsExtension),
            new FrameworkPropertyMetadata());

        #endregion

        #region LoadMoreCommand dep prop

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
            typeof(MpLoadMoreItemsExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if (e.NewValue == null) {
                        return;
                    }
                    var lb = obj as ListBox;
                    lb.Loaded += Lb_Loaded;
                }
            });

        #endregion

        public static bool IsScrollJumping = false;
        private static double _accumHorizontalChange = 0;
        private static ListBox lb;

        private static void Lb_Loaded(object sender, RoutedEventArgs e) {
            lb = sender as ListBox;
            var sv = lb.GetScrollViewer();

            Task.Run(async() => {
                await Task.Delay(3000);
                MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseWheel += Sv_ScrollChanged;
            });
            
        }


        private static void Lb_Unloaded(object sender, RoutedEventArgs e) {
            var lb = sender as ListBox;
            var sv = lb.GetScrollViewer();

            MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseWheel -= Sv_ScrollChanged;
            lb.Loaded -= Lb_Loaded;
            lb.Unloaded -= Lb_Unloaded;
        }

        private static void Sv_ScrollChanged(object sender, System.Windows.Forms.MouseEventArgs e) {
            if(IsScrollJumping || /*e.HorizontalChange == 0 || */MpMainWindowViewModel.IsMainWindowLoading) {
                IsScrollJumping = false;
                return;
            }
            //var sv = sender as ScrollViewer;
            //if(sv == null) {
            //    MpConsole.WriteTraceLine("Scrollviewer is null");
            //    return;
            //}
            //var lb = sv.GetVisualAncestor<ListBox>();
            //if(lb == null) {
            //    MpConsole.WriteTraceLine("Listbox is null");
            //    return;
            //}

            int thresholdRemainingItemCount = (int)lb.GetValue(RemainingItemsThresholdProperty);
            var loadMoreCommand = (ICommand)lb.GetValue(RemainingItemsThresholdReachedCommandProperty);

            if (loadMoreCommand == null) {
                return;
            }

            var ctrvm = MpClipTrayViewModel.Instance;
            if (ctrvm == null) {
                MpConsole.WriteTraceLine("tray vm is null");
                return;
            }
            int itemCountInListbox = ctrvm.Items.Count;
            if (itemCountInListbox <= thresholdRemainingItemCount) {
                //list is shorter than remaining threshold so there will be no more to load
                return;
            }
            var lbr = lb.GetListBoxRect();

            _accumHorizontalChange += Math.Abs(e.Delta);
            if (_accumHorizontalChange >= 0) {
                //scrolling down through list

                //get item under point in middle of right edge of listbox
                var r_lbi_idx = lb.GetItemIndexAtPoint(new Point(lbr.Right, lbr.Height / 2));
                if (r_lbi_idx < 0 || r_lbi_idx > itemCountInListbox) {
                    return;
                }
                if (r_lbi_idx == itemCountInListbox) {
                    r_lbi_idx--;
                }
                //get item over right edge's rect
                var rlbir = lb.GetListBoxItemRect(r_lbi_idx);
                if (rlbir.Right >= lbr.Right - MpMeasurements.Instance.ClipTileMargin) {
                    //when last visible item's right edge is past the listboxes edge
                    int itemsRemaining = itemCountInListbox - r_lbi_idx;
                    MpConsole.WriteLine($"Scrolling left, right most idx: {r_lbi_idx} with remaining: {itemsRemaining}  and threshold: {thresholdRemainingItemCount}");

                    if (itemsRemaining <= thresholdRemainingItemCount) {
                        if(!ctrvm.IsBusy) {
                        }

                        _accumHorizontalChange = 0;
                        loadMoreCommand.Execute(1);
                    }
                }
            } else {
                //scrolling up
                //get item under point in middle of left edge of listbox
                var l_lbi_idx = lb.GetItemIndexAtPoint(new Point(lbr.Left, lbr.Height / 2));
                MpConsole.WriteLine($"Scrolling right, left most idx: {l_lbi_idx} with remaining: {l_lbi_idx}  max remaining: {thresholdRemainingItemCount}");
                if (l_lbi_idx - thresholdRemainingItemCount <= 0) {
                }
            }
        }
    }
}
