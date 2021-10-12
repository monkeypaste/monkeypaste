using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpPagedItem : DependencyObject {        
        public static MpPagedItemViewModel GetPagedItem(DependencyObject obj) {
            return (MpPagedItemViewModel)obj.GetValue(PagedItemProperty);
        }
        public static void SetPagedItem(DependencyObject obj, MpPagedItemViewModel value) {
            obj.SetValue(PagedItemProperty, value);
        }
        public static readonly DependencyProperty PagedItemProperty =
          DependencyProperty.RegisterAttached(
            "PagedItem",
            typeof(MpPagedItemViewModel),
            typeof(MpPagedItem),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    var ctv = obj as MpClipTileView;
                    if(e.NewValue == null) {
                        ctv.DataContext = null;
                        ctv.Visibility = Visibility.Collapsed;
                    } else {
                        var pivm = (MpPagedItemViewModel)e.NewValue;
                        //ctv.
                    }
                }
            });
    }
}