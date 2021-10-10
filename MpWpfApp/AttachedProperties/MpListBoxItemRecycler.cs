using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpListBoxItemRecycler : DependencyObject {
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
            typeof(MpListBoxItemRecycler),
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
            typeof(MpListBoxItemRecycler),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if(e.NewValue == null) {
                        return;
                    }
                    var lb = (ListBox)obj;
                    lb.Loaded += (s, e1) => {
                        var sv = lb.GetScrollViewer();
                        sv.ScrollChanged += Sv_ScrollChanged;
                    };                    
                }
            });

        private static void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e) {
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

            if(lb.Items.Count <= remainingItemCount) {
                return;
            }
            var lbr = new Rect(new Point(0, 0), new Size(lb.ActualWidth, lb.ActualHeight));

            int checkIdx = lb.Items.Count - remainingItemCount - 1;
            if(checkIdx < lb.Items.Count && checkIdx >= 0) {
                var lbi = (ListBoxItem)lb.ItemContainerGenerator.ContainerFromIndex(checkIdx);
                var origin = new Point();
                if (sv.HorizontalOffset > 0 || sv.VerticalOffset > 0) {
                    origin = lbi.TranslatePoint(new Point(0, 0), sv);
                } else {
                    origin = lbi.TranslatePoint(new Point(0, 0), lb);
                }
                var lbir = new Rect(origin, new Size(lbi.ActualWidth, lbi.ActualHeight));

                if (lbir.Right < lbr.Right + sv.HorizontalOffset) {
                    loadMoreCommand.Execute(null);
                }
            }            
        }
    }
}
