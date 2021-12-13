using MonkeyPaste;
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

        public void RefreshTray() {
            //var sv = TagTray.GetScrollViewer();
            //while (sv == null) {
            //    sv = TagTray.GetScrollViewer();
            //}
            //if (sv.ExtentWidth >= TagTray.MaxWidth) {
            //    TagTrayNavLeftButton.Visibility = Visibility.Visible;
            //    TagTrayNavRightButton.Visibility = Visibility.Visible;
            //} else {
            //    TagTrayNavLeftButton.Visibility = Visibility.Collapsed;
            //    TagTrayNavRightButton.Visibility = Visibility.Collapsed;
            //}
        }


        private void TagTray_Loaded(object sender, RoutedEventArgs e) {
            TagTray.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            MpHelpers.Instance.RunOnMainThread(async () => {
                while (BindingContext == null || BindingContext.IsBusy) {
                    await Task.Delay(100);
                }

                //BindingContext.TagTileViewModels.CollectionChanged += TagTileViewModels_CollectionChanged;
                RefreshTray();
            });
        }

        private void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e) {
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

        private void TagTray_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if(BindingContext.SelectedTagTile.TagId != MpTag.RootTagId) {
                BindingContext.SelectTagCommand.Execute(null);
            }
        }

        private void MpTagTileView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if((sender as FrameworkElement).DataContext is MpTagTileViewModel ttvm) {
                e.Handled = ttvm.IsRootTagTile;
            }
        }
    }
}
