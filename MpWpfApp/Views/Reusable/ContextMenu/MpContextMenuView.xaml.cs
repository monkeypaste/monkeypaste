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
    
    public partial class MpContextMenuView : ContextMenu {
        public static MpContextMenuView CurrentContextMenu;

        public static void CloseMenu() {
            if(CurrentContextMenu == null) {
                return;
            }
            CurrentContextMenu.IsOpen = false;
        }
        public MpContextMenuView() {
            InitializeComponent();
        }

        private void ContextMenuView_Loaded(object sender, RoutedEventArgs e) {
            CurrentContextMenu = sender as MpContextMenuView;
            
        }

        private void ContextMenuView_Opened(object sender, RoutedEventArgs e) {
            ItemsSource = (DataContext as MpMenuItemViewModel).SubItems;
        }
    }
}
