using Org.BouncyCastle.Utilities.Collections;
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
    /// Interaction logic for MpTagTileView.xaml
    /// </summary>
    public partial class MpTagTileView : MpUserControl<MpTagTileViewModel> {
        public MpTagTileView() {
            InitializeComponent();
        }

        private void TagTileBorder_Loaded(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
                //if tag is created at runtime show tbox w/ all selected
            if (ttvm.IsNew) {
                ttvm.RenameTagCommand.Execute(null);
            }
        }

        private void TagTileBorder_MouseEnter(object sender, MouseEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            ttvm.IsHovering = true;
        }

        private void TagTileBorder_MouseLeave(object sender, MouseEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            ttvm.IsHovering = false;
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
            var ttvm = DataContext as MpTagTileViewModel;
            if(ttvm.IsEditing) {
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
            } else if (e.Key == Key.Escape) {
                ttvm.CancelRenameTagCommand.Execute(null);
            }
        }

        private void TagTileBorder_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            var ttb = sender as FrameworkElement;
            var ttvm = DataContext as MpTagTileViewModel;
            ttb.ContextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void TagTile_ContextMenu_Loaded(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            var cm = (ContextMenu)sender;
            cm.DataContext = ttvm;
            foreach (var i in cm.Items) {
                if (i == null || i is Separator) {
                    continue;
                }
                MenuItem mi = i as MenuItem;
                if (mi.Name == "ClipTileColorContextMenuItem") {
                    MpHelpers.Instance.SetColorChooserMenuItem(
                        cm,
                        mi,
                        (s, e1) => {
                            ttvm.ChangeColorCommand.Execute((Brush)((Border)s).Tag);
                        }
                    );
                    continue;
                }
                if ((mi.Header.ToString() == "Rename" || mi.Header.ToString() == "Delete") && ttvm.IsTagReadOnly) {
                    mi.IsEnabled = false;
                }
            }
            
        }

        private void TagTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            var ttvm = BindingContext as MpTagTileViewModel;
            ttvm.IsContextMenuOpened = true;

            var cm = sender as ContextMenu;
            cm.Tag = ttvm;
            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(cm);
        }

        private void TagTile_ContextMenu_Closed(object sender, RoutedEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            var ttvm = BindingContext as MpTagTileViewModel;
            ttvm.IsContextMenuOpened = false;
        }

        private void RenameMenuItem_Click(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            ttvm.RenameTagCommand.Execute(null);
        }

        private void DeleteMenuItem_Clicked(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            ttvm.Parent.DeleteTagCommand.Execute(ttvm.TagId);
        }
    }
}
