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
    public partial class MpTagTrayView : MpUserControl<MpTagTrayViewModel> {
        public MpTagTrayView() {
            InitializeComponent();
        }

        private void TagTray_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(DataContext != null && DataContext is MpTagTrayViewModel ttrvm) {
                ttrvm.TagTileViewModels.CollectionChanged += TagTileViewModels_CollectionChanged;
            }
        }

        private async void TagTileViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            var sv = TagTray.GetScrollViewer();
            while(sv == null) {
                await Task.Delay(50);
                sv = TagTray.GetScrollViewer();
            }
            if (sv.ExtentWidth >= TagTray.MaxWidth) {
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
            TagTray.GetScrollViewer().ScrollToHorizontalOffset(TagTray.GetScrollViewer().HorizontalOffset - 20);
        }

        private void TagTrayNavRightButton_Click(object sender, RoutedEventArgs e) {
            TagTray.GetScrollViewer().ScrollToHorizontalOffset(TagTray.GetScrollViewer().HorizontalOffset + 20);
        }

        private void TagTray_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(e.AddedItems == null || e.AddedItems.Count == 0) {
                BindingContext.SelectTagCommand.Execute(null);
                return;
            }
            var sttvm = e.AddedItems[0] as MpTagTileViewModel;
            BindingContext.SelectTagCommand.Execute(sttvm.TagId);
        }
    }
}
