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
    /// Interaction logic for MpSidebarContainerView.xaml
    /// </summary>
    public partial class MpSidebarContainerView : MpUserControl<MpSidebarViewModel> {
        public MpSidebarContainerView() {
            InitializeComponent();
        }

        private void ManageAnalyticItemsContainerView_Loaded(object sender, RoutedEventArgs e) {
            (sender as FrameworkElement).DataContext = MpAnalyticItemCollectionViewModel.Instance;
        }
    }
}
