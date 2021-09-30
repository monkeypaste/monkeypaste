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
    public partial class MpTagTileView : UserControl {
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
                ttvm.SelectTagCommand.Execute(null);
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

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            var cm = (ContextMenu)sender;
            cm.DataContext = ttvm;
            MenuItem cmi = null;
            foreach (var mi in cm.Items) {
                if (mi == null || mi is Separator) {
                    continue;
                }
                if ((mi as MenuItem).Name == "ClipTileColorContextMenuItem") {
                    cmi = (MenuItem)mi;
                    break;
                }
            }
            MpHelpers.Instance.SetColorChooserMenuItem(
                    cm,
                    cmi,
                    (s, e1) => {
                        ttvm.ChangeColorCommand.Execute((Brush)((Border)s).Tag);
                    },
                    MpHelpers.Instance.GetColorColumn(ttvm.TagColor),
                    MpHelpers.Instance.GetColorRow(ttvm.TagColor)
                );
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            var cm = sender as ContextMenu;
            cm.Tag = ttvm;
            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(cm);
        }
    }
}
