using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpTagTabsView.xaml
    /// </summary>
    public partial class MpTagTabsView : MpUserControl<MpTagTrayViewModel> {
        public MpTagTabsView() {
            InitializeComponent();
        }

        public void RefreshTray() {
            MpHelpers.RunOnMainThread(async () => {
                var sv = TagTray.GetVisualDescendent<ScrollViewer>();
                while (sv == null) {
                    await Task.Delay(100);
                    sv = TagTray.GetVisualDescendent<ScrollViewer>();
                }
                if (sv.ExtentWidth >= TagTray.MaxWidth) {
                    TagTrayNavLeftButton.Visibility = Visibility.Visible;
                    TagTrayNavRightButton.Visibility = Visibility.Visible;
                } else {
                    TagTrayNavLeftButton.Visibility = Visibility.Collapsed;
                    TagTrayNavRightButton.Visibility = Visibility.Collapsed;
                }
            });
        }


        private void TagTray_Loaded(object sender, RoutedEventArgs e) {
            TagTray.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            MpHelpers.RunOnMainThread(async () => {
                while (BindingContext == null || BindingContext.IsBusy) {
                    await Task.Delay(100);
                }

                //BindingContext.Items.CollectionChanged += TagTileViewModels_CollectionChanged;
                RefreshTray();
            });
        }

        private void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e) {
            RefreshTray();
        }


        private void TagTrayContainerGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            MpClipTrayViewModel.Instance.ResetClipSelection();
        }

        private void TagTrayNavLeftButton_Click(object sender, RoutedEventArgs e) {
            var sv = TagTray.GetVisualDescendent<ScrollViewer>();
            sv.ScrollToHorizontalOffset(TagTray.GetVisualDescendent<ScrollViewer>().HorizontalOffset - 20);
        }

        private void TagTrayNavRightButton_Click(object sender, RoutedEventArgs e) {
            var sv = TagTray.GetVisualDescendent<ScrollViewer>();
            sv.ScrollToHorizontalOffset(TagTray.GetVisualDescendent<ScrollViewer>().HorizontalOffset + 20);
        }

        private void TagTray_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems == null || e.AddedItems.Count == 0) {
                BindingContext.SelectTagCommand.Execute(null);
                return;
            }
            var sttvm = e.AddedItems[0] as MpTagTileViewModel;
            if(sttvm.IsSelected) {
                return;
            }
            BindingContext.SelectTagCommand.Execute(sttvm.TagId);
        }

    }

    public class ContentToMarginConverter : IValueConverter {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return new Thickness(0, 0, -((ContentPresenter)value).ActualHeight, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ContentToPathConverter : IValueConverter {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var ps = new PathSegmentCollection(4);
            ContentPresenter cp = (ContentPresenter)value;
            double h = cp.ActualHeight > 10 ? 1.4 * cp.ActualHeight : 10;
            double w = cp.ActualWidth > 10 ? 1.25 * cp.ActualWidth : 10;
            ps.Add(new LineSegment(new Point(1, 0.7 * h), true));
            ps.Add(new BezierSegment(new Point(1, 0.9 * h), new Point(0.1 * h, h), new Point(0.3 * h, h), true));
            ps.Add(new LineSegment(new Point(w, h), true));
            ps.Add(new BezierSegment(new Point(w + 0.6 * h, h), new Point(w + h, 0), new Point(w + h * 1.3, 0), true));
            return ps;


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
