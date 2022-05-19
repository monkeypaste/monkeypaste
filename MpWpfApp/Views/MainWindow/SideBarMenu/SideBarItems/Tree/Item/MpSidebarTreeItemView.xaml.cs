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
using MonkeyPaste;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpSidebarTreeItemView.xaml
    /// </summary>
    public partial class MpSidebarTreeItemView : MpUserControl<MpIHierarchialViewModel> {
        private string _originalLabel;
        public bool IsPinnedTrayView { get; set; } = false;

        public MpSidebarTreeItemView() {
            InitializeComponent();
        }

        private void TagTileBorder_Loaded(object sender, RoutedEventArgs e) {            
                //if tag is created at runtime show tbox w/ all selected
            if (BindingContext.IsNew) {
                BeginLabelEdit();
            }

            if(!IsPinnedTrayView) {
                AddTagButtonPanel.Visibility = Visibility.Collapsed;
            }
        }


        private void TagTileBorder_LostFocus(object sender, RoutedEventArgs e) {
            if (!BindingContext.IsSelected) {
                EndLabelEdit();
            }
        }

        private void TagTileBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            if (BindingContext.IsReadOnly && e.ClickCount == 2) {
                BeginLabelEdit();
            } 
        }

        private void TagTextBox_LostFocus(object sender, RoutedEventArgs e) {
            EndLabelEdit();
        }

        private void TagTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                EndLabelEdit();
                e.Handled = true;
            } else if (e.Key == Key.Escape) {
                CancelLabelEdit();
                e.Handled = true;
            }
        }

        private void TagTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            //this.GetVisualAncestor<MpTagTrayView>()?.RefreshTray();
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

        private void BeginLabelEdit() {
            _originalLabel = BindingContext.Label;
            BindingContext.IsReadOnly = false;
            BindingContext.IsFocused = true;
        }

        private void EndLabelEdit() {
            BindingContext.IsReadOnly = true;
        }

        private void CancelLabelEdit() {
            BindingContext.Label = _originalLabel;
            EndLabelEdit();
        }
    }
}
