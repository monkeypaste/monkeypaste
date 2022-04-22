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
    /// Interaction logic for MpClipTileSourceIconContextMenu.xaml
    /// </summary>
    public partial class MpClipTileSourceIconContextMenu : ContextMenu {
        public MpClipTileSourceIconContextMenu() {
            InitializeComponent();
        }

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;

            if(civm.UrlViewModel != null) {
                string domainName = civm.UrlViewModel.UrlDomainPath;
                ExcludeSourceItem.Header = string.Format(@"Exclude '{0}'",domainName);
            } else {

                ExcludeSourceItem.Header = string.Format(@"Exclude '{0}'", civm.AppViewModel.AppName);
                ExcludeSourceDomainItem.Visibility = Visibility.Hidden;
            }

            // TODO Check for Paste macro to change PasteToPath Menu Item Title
            
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e) {

        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e) {

        }
    }
}
