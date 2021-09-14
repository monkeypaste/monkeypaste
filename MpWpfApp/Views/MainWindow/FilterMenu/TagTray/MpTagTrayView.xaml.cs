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
    /// Interaction logic for MpTagTrayView.xaml
    /// </summary>
    public partial class MpTagTrayView : UserControl {
        public MpTagTrayView() {
            InitializeComponent();
        }

        private void TagTray_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(DataContext != null && DataContext is MpTagTrayViewModel ttrvm) {
                ttrvm.TagTileViewModels.CollectionChanged += TagTileViewModels_CollectionChanged;
            }
        }

        private void TagTileViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (TagTray.ScrollViewer.ExtentWidth >= TagTray.MaxWidth) {
                TagTrayNavLeftButton.Visibility = Visibility.Visible;
                TagTrayNavRightButton.Visibility = Visibility.Visible;
            } else {
                TagTrayNavLeftButton.Visibility = Visibility.Collapsed;
                TagTrayNavRightButton.Visibility = Visibility.Collapsed;
            }
        }

        private void TagTrayContainerGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            MpClipTrayViewModel.Instance.ResetClipSelection();
        }

        private void TagTrayNavLeftButton_Click(object sender, RoutedEventArgs e) {
            TagTray.ScrollViewer.ScrollToHorizontalOffset(TagTray.ScrollViewer.HorizontalOffset - 20);
        }

        private void TagTrayNavRightButton_Click(object sender, RoutedEventArgs e) {
            TagTray.ScrollViewer.ScrollToHorizontalOffset(TagTray.ScrollViewer.HorizontalOffset + 20);
        }

        private void TagTrayContainerGrid_Loaded(object sender, RoutedEventArgs e) {

        }
    }
}
