using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpHorizontalScroll : DependencyObject {
        public static readonly DependencyProperty UseHorizontalScrollingProperty = DependencyProperty.RegisterAttached(
            "UseHorizontalScrolling", typeof(bool), typeof(MpHorizontalScroll), new PropertyMetadata(default(bool), UseHorizontalScrollingChangedCallback));

        private static void UseHorizontalScrollingChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            ItemsControl itemsControl = dependencyObject as ItemsControl;

            if (itemsControl == null) throw new ArgumentException("Element is not an ItemsControl");

            itemsControl.PreviewMouseWheel += delegate (object sender, MouseWheelEventArgs args) {
                ScrollViewer scrollViewer = itemsControl.GetVisualDescendent<ScrollViewer>();

                if (scrollViewer == null) return;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (args.Delta / 1));

                //if (args.Delta < 0) {
                    
                //} else {
                //    scrollViewer.LineLeft();
                //}
            };
        }


        public static void SetUseHorizontalScrolling(ItemsControl element, bool value) {
            element.SetValue(UseHorizontalScrollingProperty, value);
        }

        public static bool GetUseHorizontalScrolling(ItemsControl element) {
            return (bool)element.GetValue(UseHorizontalScrollingProperty);
        }
    }
}
