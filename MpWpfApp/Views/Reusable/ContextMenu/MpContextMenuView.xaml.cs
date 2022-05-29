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
    
    public partial class MpContextMenuView : ContextMenu, MpISingleton<MpContextMenuView> {
        private static MpContextMenuView _instance;
        public static MpContextMenuView Instance => _instance ?? (_instance = new MpContextMenuView());

        public async Task Init() {
            await Task.Delay(1);
        }

        public void CloseMenu() {
            if(IsLoaded) {
                IsOpen = false;
            }
        }
        public MpContextMenuView() {
            InitializeComponent();
        }

        private void ContextMenuView_Opened(object sender, RoutedEventArgs e) {
            
        }

        private void ContextMenuView_Closed(object sender, RoutedEventArgs e) {

        }
    }
}
