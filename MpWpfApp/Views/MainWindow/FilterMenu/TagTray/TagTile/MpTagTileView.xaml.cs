using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    /// Interaction logic for MpTagTileView.xaml
    /// </summary>
    public partial class MpTagTileView : MpUserControl<MpTagTileViewModel> {
        public bool IsTagTreeTile { get; set; } = false;

        public MpTagTileView() {
            InitializeComponent();
        }

        private void TagTileBorder_Loaded(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
                //if tag is created at runtime show tbox w/ all selected
            if (ttvm.IsNew) {
                ttvm.RenameTagCommand.Execute(null);
            }

            if(!IsTagTreeTile) {
                AddTagButtonPanel.Visibility = Visibility.Collapsed;
            }
            ttvm.OnRequestSelectAll += Ttvm_OnRequestSelectAll;
        }


        private void TagTileBorder_Unloaded(object sender, RoutedEventArgs e) {
            if(BindingContext != null) {
                BindingContext.OnRequestSelectAll -= Ttvm_OnRequestSelectAll;
            }
        }

        private void Ttvm_OnRequestSelectAll(object sender, EventArgs e) {
            TagTextBox.Focus();
            TagTextBox.SelectAll();
        }

        private void TagTileBorder_LostFocus(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            if (!ttvm.IsSelected) {
                ttvm.IsEditing = false;
            }
        }

        private void TagTileBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            if (e.ClickCount == 2) {
                ttvm.RenameTagCommand.Execute(null);
            } else {
               // ttvm.SelectTagCommand.Execute(null);
            }
        }

        private void TagTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(BindingContext.IsEditing) {
                TagTextBox.Focus();
                TagTextBox.Focus();
                TagTextBox.SelectAll();
            } 
        }

        private void TagTextBox_LostFocus(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            ttvm.IsEditing = false;
        }

        private void TagTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            if (e.Key == Key.Enter) {
                ttvm.FinishRenameTagCommand.Execute(null);
                e.Handled = true;
            } else if (e.Key == Key.Escape) {
                ttvm.CancelRenameTagCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void TagTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            this.GetVisualAncestor<MpTagTrayView>()?.RefreshTray();
        }

        private void StackPanel_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            //if (!BindingContext.IsSelected) {
            //    BindingContext.IsSelected = true;
            //}

            e.Handled = true;
            var fe = sender as FrameworkElement;

            MpContextMenuView.Instance.DataContext = BindingContext.MenuItemViewModel;
            fe.ContextMenu = MpContextMenuView.Instance;
            fe.ContextMenu.PlacementTarget = this;
            fe.ContextMenu.IsOpen = true;
        }
    }
}
