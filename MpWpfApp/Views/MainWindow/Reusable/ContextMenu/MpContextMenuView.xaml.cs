using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MpContentContextMenuView.xaml
    /// </summary>
    public partial class MpContextMenuView : MenuItem {
        public MpContextMenuView() {
            InitializeComponent();
        }

        private void ClipTile_ContextMenu_Loaded(object sender, RoutedEventArgs e) {
        }

        private async void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            await PrepareContextMenu();
        }

        private async Task PrepareContextMenu() {
            await Task.Delay(1);
        }

        private void ClipTile_ContextMenu_Closed(object sender, RoutedEventArgs e) {
        }
    }
}
