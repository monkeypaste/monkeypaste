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
using System.Windows.Controls.Primitives;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpTagTileTreeView.xaml
    /// </summary>
    public partial class MpTagTileTreeView : MpUserControl<MpTagTileViewModel> {
        public bool IsTagTreeTile { get; set; } = true;

        public MpTagTileTreeView() {
            InitializeComponent();
        }

        private void TagTileBorder_Loaded(object sender, RoutedEventArgs e) {
                //if tag is created at runtime show tbox w/ all selected
            if (BindingContext.IsNew) {
                BindingContext.RenameTagCommand.Execute(true);
            }

            if(!IsTagTreeTile) {
                AddTagButtonPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void TagTileBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) {
                BindingContext.RenameTagCommand.Execute(true);
            } 
        }

        private void TagTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                BindingContext.FinishRenameTagCommand.Execute(null);
            } else if (e.Key == Key.Escape) {
                e.Handled = true;
                BindingContext.CancelRenameTagCommand.Execute(null);
            }
        }

        private void StackPanel_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            e.Handled = true;
            var fe = sender as FrameworkElement;

            BindingContext.IsTreeContextMenuOpened = true;

            RoutedEventHandler onCloseHandler = null;
            onCloseHandler = (s, e1) => {
                BindingContext.IsTreeContextMenuOpened = false;
                fe.ContextMenu.Closed -= onCloseHandler;
            };

            MpContextMenuView.Instance.DataContext = BindingContext.MenuItemViewModel;
            fe.ContextMenu = MpContextMenuView.Instance;
            fe.ContextMenu.PlacementTarget = this;
            fe.ContextMenu.IsOpen = true;
        }
    }
}
