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

        //public async void RefreshTray() {
        //    var sv = TagTray.GetVisualDescendent<ScrollViewer>();
        //    while (sv == null) {
        //        await Task.Delay(100);
        //        sv = TagTray.GetVisualDescendent<ScrollViewer>();
        //    }

        //    if (sv.ExtentWidth >= TagTray.MaxWidth) {
        //        TagTrayNavLeftButton.Visibility = Visibility.Visible;
        //        TagTrayNavRightButton.Visibility = Visibility.Visible;
        //    } else {
        //        TagTrayNavLeftButton.Visibility = Visibility.Collapsed;
        //        TagTrayNavRightButton.Visibility = Visibility.Collapsed;
        //    }
        //}


        //private void TagTray_Loaded(object sender, RoutedEventArgs e) {
        //    TagTray.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
        //    MpHelpers.RunOnMainThread(async () => {
        //        while (BindingContext == null || BindingContext.IsBusy) {
        //            await Task.Delay(100);
        //        }

        //        //BindingContext.Items.CollectionChanged += TagTileViewModels_CollectionChanged;
        //        RefreshTray();
        //    });
        //}

        //private void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e) {
        //    RefreshTray();
        //}


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

        //private void MpTagTileView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
        //    RefreshTray();
        //}
    }
}
